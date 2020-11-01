/*
* High Speed modem to transfer data in a 2,7kHz SSB channel
* =========================================================
* Author: DJ0ABR
*
*   (c) DJ0ABR
*   www.dj0abr.de
*
*   This program is free software; you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation; either version 2 of the License, or
*   (at your option) any later version.
*
*   This program is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU General Public License for more details.
*
*   You should have received a copy of the GNU General Public License
*   along with this program; if not, write to the Free Software
*   Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
* 
*/

#include "qo100modem.h"

int Open_BC_Socket();
void startModem();
void stopModem();
void getMyIP();
void bc_rxdata(uint8_t *pdata, int len, struct sockaddr_in* rxsock);
void appdata_rxdata(uint8_t *pdata, int len, struct sockaddr_in* rxsock);
void GRdata_rxdata(uint8_t *pdata, int len, struct sockaddr_in* rxsock);
void GRdata_FFTdata(uint8_t *pdata, int len, struct sockaddr_in* rxsock);
void GRdata_I_Qdata(uint8_t *pdata, int len, struct sockaddr_in* rxsock);

// threads will exit if set to 0
int keeprunning = 1;

// UDP I/O
int BC_sock_AppToModem = -1;
int DATA_sock_AppToModem = -1;
int DATA_sock_from_GR = -1;
int DATA_sock_FFT_from_GR = -1;
int DATA_sock_I_Q_from_GR = -1;

int UdpBCport_AppToModem = 40131;
int UdpDataPort_AppToModem = 40132;
int UdpDataPort_ModemToApp = 40133;

int UdpDataPort_toGR = 40134;
int UdpDataPort_fromGR = 40135;
int UdpDataPort_fromGR_FFT = 40136;
int UdpDataPort_fromGR_I_Q = 40137;

// op mode depending values
// default mode if not set by the app
int speedmode = 4;
int bitsPerSymbol = 2;      // QPSK=2, 8PSK=3
int constellationSize = 4;  // QPSK=4, 8PSK=8

char localIP[]={"127.0.0.1"};
char ownfilename[]={"qo100modem"};
char myIP[20];
char appIP[20] = {0};
int fixappIP = 0;
int restart_modems = 0;
int doNotLoadModems = 0;

int caprate = 44100;
int txinterpolfactor = 20;

int main(int argc, char *argv[])
{
int opt = 0;
char *modemip = NULL;

    while ((opt = getopt(argc, argv, "m:e:")) != -1) 
    {
        switch(opt) 
        {
            case 'e':
                doNotLoadModems = 1;
                break;
            case 'm':
                modemip = optarg;
                memset(appIP,0,20);
                int len = strlen(modemip);
                if(len < 16)
                {
                    memcpy(appIP,modemip,len);
                    fixappIP = 1;
                    printf("Application IP set to: %s\n",modemip);
                }
                else
                {
                    printf("invalid Application IP: %s\n",modemip);
                    exit(0);
                }
                break;
        }
    }

    if(doNotLoadModems == 0 && isRunning(ownfilename) == 1)
        exit(0);
    
    install_signal_handler();

    init_packer();
    initFEC();
    init_fft();
    init_audio();
    
    // start udp RX to listen for broadcast search message from Application
    UdpRxInit(&BC_sock_AppToModem, UdpBCport_AppToModem, &bc_rxdata, &keeprunning);
        
    // start udp RX for data from application
    UdpRxInit(&DATA_sock_AppToModem, UdpDataPort_AppToModem, &appdata_rxdata, &keeprunning);
    
    // start udp RX to listen for data from GR Receiver
    UdpRxInit(&DATA_sock_from_GR, UdpDataPort_fromGR, &GRdata_rxdata, &keeprunning);
    
    // start udp RX to listen for Audio-Samples (FFT) data from GR Receiver
    UdpRxInit(&DATA_sock_FFT_from_GR, UdpDataPort_fromGR_FFT, &GRdata_FFTdata, &keeprunning);
    
    // start udp RX to listen for IQ data from GR Receiver
    UdpRxInit(&DATA_sock_I_Q_from_GR, UdpDataPort_fromGR_I_Q, &GRdata_I_Qdata, &keeprunning);
    
    getMyIP();
    
    printf("QO100modem initialised and running\n");
    
    while (keeprunning)
    {
        if(restart_modems == 1)
        {
            stopModem();
            startModem();
            restart_modems = 0;
        }
        
        doArraySend();
        
        usleep(100);
    }
    printf("stopped: %d\n",keeprunning);
    
    close(BC_sock_AppToModem);
    
    return 0;
}

typedef struct {
    int audio;
    int tx;
    int rx;
} SPEEDRATE;

SPEEDRATE sr[9] = {
    // QPSK modes
    {48000, 32, 8}, // AudioRate, TX-Resampler, RX-Resampler/4
    {44100, 28, 7}, // see samprate.ods
    {44100, 24, 6},
    {48000, 24, 6},
    {44100, 20, 5},
    {48000, 20, 5},
    
    // 8PSK modes
    {44100, 24, 6},
    {48000, 24, 6}
};

void startModem()
{
char srx[512];

    if(speedmode >= 0 && speedmode <=5)
    {
        bitsPerSymbol = 2;      // QPSK=2, 8PSK=3
        constellationSize = (1<<bitsPerSymbol); // QPSK=4, 8PSK=8
    }
    else if(speedmode >= 6 && speedmode <=7)
    {
        bitsPerSymbol = 3;                      // QPSK=2, 8PSK=3
        constellationSize = (1<<bitsPerSymbol); // QPSK=4, 8PSK=8
    }
    
    caprate = sr[speedmode].audio;
    txinterpolfactor = sr[speedmode].tx;
    
    if(doNotLoadModems == 1) return;

    if(speedmode >= 0 && speedmode <=5)
        sprintf(srx,"python3 qpsk_rx.py -r %d -s %d &",sr[speedmode].rx,sr[speedmode].audio);
    else if(speedmode >= 6 && speedmode <=7)
        sprintf(srx,"python3 rx_8psk.py -r %d -s %d &",sr[speedmode].rx,sr[speedmode].audio);
    else
    {
        printf("wrong modem number\n");
        exit(0);
    }

    // int TX audio and modulator
    init_dsp();
    init_audio();

    // the RX modem needs the app's IP address as a parameter -i ip
    if(run_console_program(srx) == -1)
    {
        printf("cannot start RX modem\n");
        exit(0);
    }
}

void stopModem()
{
    if(doNotLoadModems == 1) return;
    printf("stop modem\n");
    int ret = system("killall python3");
    if(ret){}
    // wait until stop job is done
    sleep(1);
}

void getMyIP()
{
    struct ifaddrs *ifaddr, *ifa;
    int s;
    char host[NI_MAXHOST];

    if (getifaddrs(&ifaddr) == -1)
    {
        printf("getifaddrs error\n");
        exit(0);
    }


    ifa = ifaddr;
    while(ifa)
    {
        s=getnameinfo(ifa->ifa_addr,sizeof(struct sockaddr_in),host, NI_MAXHOST, NULL, 0, NI_NUMERICHOST);

        if(ifa->ifa_addr->sa_family==AF_INET)
        {
            if (s != 0)
            {
                printf("getnameinfo() failed: %s\n", gai_strerror(s));
                exit(0);
            }
            strcpy(myIP, host);
            if(strncmp(host,"127",3) != 0)
                break;
        }
        
        ifa = ifa->ifa_next;
    }

    freeifaddrs(ifaddr);
    
    return;
}

// called from UDP RX thread for Broadcast-search from App
void bc_rxdata(uint8_t *pdata, int len, struct sockaddr_in* rxsock)
{
    if (len > 0 && pdata[0] == 0x3c)
    {
        char rxip[20];
        strcpy(rxip,inet_ntoa(rxsock->sin_addr));
        
        if(fixappIP == 0)
        {
            if(strcmp(appIP,rxip))
            {
                printf("new app IP: %s, restarting modems\n",rxip);
                restart_modems = 1;
            }
            strcpy(appIP,rxip);
            //printf("app (%s) is searching modem. Sending modem IP to the app\n",appIP);
            // App searches for the modem IP, mirror the received messages
            // so the app gets an UDP message with this local IP
            pdata[0] = 3;
            sendUDP(appIP,UdpDataPort_ModemToApp,pdata,1);
        }
        else
        {
            // appIP is fixed, answer only to this IP
            if(!strcmp(appIP,rxip))
            {
                //printf("app (%s) is searching modem. Sending modem IP to the app\n",appIP);
                restart_modems = 1;
                // App searches for the modem IP, mirror the received messages
                // so the app gets an UDP message with this local IP
                pdata[0] = 3;
                sendUDP(appIP,UdpDataPort_ModemToApp,pdata,1);
            }
        }
    }
}

// called by UDP RX thread for data from App
void appdata_rxdata(uint8_t *pdata, int len, struct sockaddr_in* rxsock)
{
    uint8_t type = pdata[0];
    uint8_t minfo = pdata[1];
    
    if(len != (PAYLOADLEN+2))
    {
        printf("data from app: wrong length:%d (should be %d)\n",len-2,PAYLOADLEN);
        return;
    }
    
    // type values: see oscardata config.cs: frame types
    if(type == 16)
    {
        // Byte 1 contains the resampler ratio for TX and RX modem
        speedmode = pdata[1];
        printf("set speedmode to %d\n",speedmode);
        restart_modems = 1;
        return;
    }
    
    if(type == 17)
    {
        // auto send file
        // TODO
        
        // for testing only:
        // simulate sending a text file with 1kB length
        int testlen = 100000;
        uint8_t arr[testlen];
        char c = 'A';
        for(int i=0; i<testlen; i++)
        {
            arr[i] = c;
            if(++c>'Z') c='A';
        }
        arraySend(arr, testlen, 3, (char *)"testfile.txt");
        return;
    }
    if(type == 18)
    {
        // auto send folder
        // TODO
    }

    if(type == 19)
    {
        // shut down this modem PC
        int r = system("sudo shutdown now");
        exit(r);
    }
    
    if(getSending() == 1) return;   // already sending (Array sending)
    
    if(minfo == 0)
    {
        // this is the first frame of a larger file
        // send it multiple times, like a preamble, to give the
        // receiver some time for synchronisation
        // duration: 3 seconds
        // caprate: samples/s. This are symbols: caprate/txinterpolfactor
        // and bits: symbols * bitsPerSymbol
        // and bytes/second: bits/8 = (caprate/txinterpolfactor) * bitsPerSymbol / 8
        // one frame has 258 bytes, so we need for 5s: 5* ((caprate/txinterpolfactor) * bitsPerSymbol / 8) /258 + 1 frames
        int numframespreamble = 3* ((caprate/txinterpolfactor) * bitsPerSymbol / 8) /258 + 1;
        for(int i=0; i<numframespreamble; i++)
            toGR_sendData(pdata+2, type, minfo);
    }
    else if((len-2) < PAYLOADLEN)
    {
        // if not enough data for a full payload add Zeros
        uint8_t payload[PAYLOADLEN];
        memset(payload,0,PAYLOADLEN);
        memcpy(payload,pdata+2,len-2);
        toGR_sendData(payload, type, minfo);
    }
    else
    {
        toGR_sendData(pdata+2, type, minfo);
    }
}

void toGR_sendData(uint8_t *data, int type, int status)
{
    int len = 0;
    uint8_t *txdata = Pack(data,type,status,&len);
    
    //showbytestring((char *)"BERtx: ", txdata, len);
    
    if(txdata != NULL)
         sendToModulator(txdata,len);
}

#define SPEEDMEAN 3
int speedmean = SPEEDMEAN;
int meansumbytes = 0;
int speed = 0;
void measure_speed(int len)
{
    struct timeval  tv;
    static uint64_t lastus = 0;
    uint64_t us;
    static int sparr[SPEEDMEAN] = {-1};

	gettimeofday(&tv, NULL);

	us = tv.tv_sec * 1000000 + tv.tv_usec;
    if(lastus == 0)
    {
        lastus = us;
        return;
    }
    uint64_t ts = us-lastus;
    
    // make measurement only if time >= 1s
    meansumbytes += len;
    if(ts < 5000000)
    {
        // do not measure
        return;
    }
    
    // ts ... time in us since last measurement
    // divide by the number of bits
    ts /= (meansumbytes*8);  // time for one bit
    int tbit = (int)ts;
    int sp1 = 1000000/tbit;
    // convert speed of symbols to speed of bits
    speed = sp1 * bitsPerSymbol / 8;
    
    int mean = 0;
    if(sparr[0] == -1)
    {
        for(int i=0; i<speedmean; i++)
            sparr[i] = speed;
    }
    else
    {
        // make mean value
        for(int i=speedmean-1; i>0; i--)
            sparr[i] = sparr[i-1];
        sparr[0] = speed;
    }
    
    for(int i=0; i<speedmean; i++)
        mean += sparr[i];
    mean /= speedmean;
    
    //printf("Speed %d bit/s\n",mean);
    lastus = us;
    meansumbytes = 0;
}

// called by UDP RX thread for data from GnuRadio Receiver
void GRdata_rxdata(uint8_t *pdata, int len, struct sockaddr_in* rxsock)
{
    // raw symbols
    measure_speed(len);
    uint8_t *pl = unpack_data(pdata, len);
    if(pl != NULL)
    {
        // complete frame received
        // send payload to app
        uint8_t txpl[PAYLOADLEN+10+1];
        memcpy(txpl+1,pl,PAYLOADLEN+10);
        txpl[0] = 1;    // type 1: payload data follows
        sendUDP(appIP,UdpDataPort_ModemToApp,txpl,PAYLOADLEN+10+1);
    }
}

// called by UDP RX thread for Audio-Samples (FFT) data from GR
void GRdata_FFTdata(uint8_t *pdata, int len, struct sockaddr_in* rxsock)
{
    // send IQ data to FFT for waterfall calculation
    int fftlen = 0;
    uint16_t *fft = make_waterfall(pdata,len, &fftlen);
    if(fft != NULL)
    {
        uint8_t txpl[10000];
        if(fftlen > (10000*2+1))
        {
            printf("txpl too small !!!\n");
            return;
        }

        int bidx = 0;
        txpl[bidx++] = 4;    // type 4: FFT data follows
        
        for(int i=0; i<fftlen; i++)
        {
            txpl[bidx++] = fft[i] >> 8;
            txpl[bidx++] = fft[i];
        }
        sendUDP(appIP,UdpDataPort_ModemToApp,txpl,bidx);
    }
}

uint8_t lastb[12];

void display_IQ(uint8_t *pdata, int len)
{
    for (int i = 0; i < len; i++)
    {
        // insert new byte in lastb
        for (int sh = 12 - 1; sh > 0; sh--)
            lastb[sh] = lastb[sh - 1];
        lastb[0] = pdata[i];

        // test if aligned
        // for PC
        if (lastb[0] == 0 && lastb[1] == 0 && lastb[2] == 3 && lastb[3] == 0xe8)
        {
            // we are aligned to a re value
            int re = lastb[4];
            re <<= 8;
            re += lastb[5];
            re <<= 8;
            re += lastb[6];
            re <<= 8;
            re += lastb[7];

            int im = lastb[8];
            im <<= 8;
            im += lastb[9];
            im <<= 8;
            im += lastb[10];
            im <<= 8;
            im += lastb[11];

            double fre = (double)re / 16777216;
            double fim = (double)im / 16777216;
            printf("re: %f im: %f\n",fre,fim);
            
        }
        // and for ARM
        else if (lastb[0] == 0xe8 && lastb[1] == 3 && lastb[2] == 0 && lastb[3] == 0)
        {
            // we are aligned to a re value
            int re = lastb[7];
            re <<= 8;
            re += lastb[6];
            re <<= 8;
            re += lastb[5];
            re <<= 8;
            re += lastb[4];

            int im = lastb[11];
            im <<= 8;
            im += lastb[10];
            im <<= 8;
            im += lastb[9];
            im <<= 8;
            im += lastb[8];

            double fre = (double)re / 16777216;
            double fim = (double)im / 16777216;
            printf("ARM re: %f im: %f\n",fre,fim);
        }
    }
}

// called by UDP RX thread for IQ data from GR
void GRdata_I_Qdata(uint8_t *pdata, int len, struct sockaddr_in* rxsock)
{
    // these data are floats multiplied by 2^24 and then converted to int 
    // for testing convert it back and display it
    //display_IQ(pdata,len);
    
    // send the data "as is" to app 
    uint8_t txpl[len+1];
    memcpy(txpl+1,pdata,len);
    txpl[0] = 5;    // type 5: IQ data follows
    sendUDP(appIP,UdpDataPort_ModemToApp,txpl,len+1);
}
