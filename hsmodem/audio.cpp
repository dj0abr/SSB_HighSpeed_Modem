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
* audio.c ... functions to handle audio in/out via a soundcard
*             uses the "BASS" library
* 
* captures samples from the sound card.
* Samples are 32-bit floats in a range of -1 to +1
* get these samples from the thread safe fifo: cap_read_fifo(&floatvariable)
* 
* plays samples to the sound card
* Samples are 32-bit floats in a range of -1 to +1
* play the samples by calling the thread save function: pb_write_fifo(floatsample)
* 
*/

#include "hsmodem.h"

BOOL CALLBACK RecordingCallback(HRECORD handle, const void *buffer, DWORD length, void *user);
DWORD CALLBACK WriteStream(HSTREAM handle, float *buffer, DWORD length, void *user);
int pb_read_fifo(float *data, int elements);
void close_audio();
void cap_write_fifo(float sample);
int pb_fifo_freespace(int nolock);
void init_pipes();

HRECORD rchan = 0;		// recording channel
BASS_INFO info;
HSTREAM stream = 0;	

int openpbdev = -1;
int opencapdev = -1;

/*void showDeviceInfo(BASS_DEVICEINFO info)
{
    if (info.flags & BASS_DEVICE_ENABLED)	printf("%s\n","BASS_DEVICE_ENABLED	");
    if (info.flags & BASS_DEVICE_DEFAULT)	printf("%s\n","BASS_DEVICE_DEFAULT	");
    if (info.flags & BASS_DEVICE_INIT)	printf("%s\n","BASS_DEVICE_INIT	");
    if (info.flags & BASS_DEVICE_LOOPBACK)	printf("%s\n","BASS_DEVICE_LOOPBACK	");
    if (info.flags & BASS_DEVICE_TYPE_DIGITAL)	printf("%s\n","BASS_DEVICE_TYPE_DIGITAL	");
    if (info.flags & BASS_DEVICE_TYPE_DISPLAYPORT)	printf("%s\n","BASS_DEVICE_TYPE_DISPLAYPORT	");
    if (info.flags & BASS_DEVICE_TYPE_HANDSET)	printf("%s\n","BASS_DEVICE_TYPE_HANDSET	");
    if (info.flags & BASS_DEVICE_TYPE_HDMI)	printf("%s\n","BASS_DEVICE_TYPE_HDMI	");
    if (info.flags & BASS_DEVICE_TYPE_HEADPHONES)	printf("%s\n","BASS_DEVICE_TYPE_HEADPHONES	");
    if (info.flags & BASS_DEVICE_TYPE_HEADSET)	printf("%s\n","BASS_DEVICE_TYPE_HEADSET	");
    if (info.flags & BASS_DEVICE_TYPE_LINE)	printf("%s\n","BASS_DEVICE_TYPE_LINE	");
    if (info.flags & BASS_DEVICE_TYPE_MICROPHONE)	printf("%s\n","BASS_DEVICE_TYPE_MICROPHONE	");
    if (info.flags & BASS_DEVICE_TYPE_NETWORK)	printf("%s\n","BASS_DEVICE_TYPE_NETWORK	");
    if (info.flags & BASS_DEVICE_TYPE_SPDIF)	printf("%s\n","BASS_DEVICE_TYPE_SPDIF	");
    if (info.flags & BASS_DEVICE_TYPE_SPEAKERS)	printf("%s\n","BASS_DEVICE_TYPE_SPEAKERS	");

}*/

uint8_t devstring[MAXDEVSTRLEN +100];
char PBdevs[100][256]; // stores the device names, just for diagnosis, has no real fuction
char CAPdevs[100][256];

// audio device description table
typedef struct {
    int bassdev;    // bass (basswasapi) dev no
    char name[256]; // DEV name
} AUDIODEVS;

// index is enumerated number, 0=default
AUDIODEVS audioPBdevs[100];
AUDIODEVS audioCAPdevs[100];
int pbanz = 0, capanz = 0;

// populate audio device list
void readAudioDevs()
{
    int a;

    // enter default device manually
    audioPBdevs[pbanz].bassdev = -1;
    strcpy(audioPBdevs[pbanz].name, "Default");
    pbanz++;

    audioCAPdevs[capanz].bassdev = -1;
    strcpy(audioCAPdevs[capanz].name, "Default");
    capanz++;

#ifdef _LINUX_
    BASS_DEVICEINFO info;
    for (a = 1; BASS_GetDeviceInfo(a, &info); a++)
    {
        if (info.flags & BASS_DEVICE_ENABLED)
        {
            if (!strstr(info.name, "HDMI") && !strstr(info.name, "hdmi") && !strstr(info.name, "efault"))
            {
                audioPBdevs[pbanz].bassdev = a;
                strncpy(audioPBdevs[pbanz].name, info.name, 255);
                audioPBdevs[pbanz].name[255] = 0;
                pbanz++;
            }
        }
    }

    for (a = 1; BASS_RecordGetDeviceInfo(a, &info); a++)
    {
        if (info.flags & BASS_DEVICE_ENABLED)
        {
            if (!strstr(info.name, "HDMI") && !strstr(info.name, "hdmi") && !strstr(info.name, "efault"))
            {
                audioCAPdevs[capanz].bassdev = a;
                strncpy(audioCAPdevs[capanz].name, info.name, 255);
                audioCAPdevs[capanz].name[255] = 0;
                capanz++;
            }
        }
    }
#endif

#ifdef _WIN32_
    BASS_WASAPI_DEVICEINFO info;
    for (a = 0; BASS_WASAPI_GetDeviceInfo(a, &info); a++)
    {
        if (!(info.flags & BASS_DEVICE_INPUT) && (info.flags & BASS_DEVICE_ENABLED))
        {
            if (!strstr(info.name, "HDMI") && !strstr(info.name, "hdmi") && !strstr(info.name, "efault"))
            {
                audioPBdevs[pbanz].bassdev = a;
                strncpy(audioPBdevs[pbanz].name, info.name, 255);
                audioPBdevs[pbanz].name[255] = 0;
                pbanz++;
            }
        }

        if ((info.flags & BASS_DEVICE_INPUT) && (info.flags & BASS_DEVICE_ENABLED))
        {
            if (!strstr(info.name, "HDMI") && !strstr(info.name, "hdmi") && !strstr(info.name, "efault"))
            {
                audioCAPdevs[capanz].bassdev = a;
                strncpy(audioCAPdevs[capanz].name, info.name, 255);
                audioCAPdevs[capanz].name[255] = 0;
                capanz++;
            }
        }
    }
#endif
}

void printAudioDevs()
{
    printf("PB devices:\n");
    for (int i = 0; i < pbanz; i++)
        printf("idx:%d bass:%d name:%s\n", i, audioPBdevs[i].bassdev, audioPBdevs[i].name);

    printf("CAP devices:\n");
    for (int i = 0; i < capanz; i++)
        printf("idx:%d bass:%d name:%s\n", i, audioCAPdevs[i].bassdev, audioCAPdevs[i].name);
}

// build string of audio device name, to be sent to application as response to Broadcast search
// starting with PB devices, sperarator ^, capture devices
// separator between devices: ~
void buildUdpAudioList()
{
    memset(devstring, 0, sizeof(devstring));
    devstring[0] = ' ';     // placeholder for ID for this UDP message

    // playback devices
    for (int i = 0; i < pbanz; i++)
    {
        strcat((char*)devstring, audioPBdevs[i].name);
        strcat((char*)devstring, "~");    // audio device separator
    }

    strcat((char*)(devstring + 1), "^");    // PB, CAP separator

    // capture devices
    for (int i = 0; i < capanz; i++)
    {
        strcat((char*)devstring, audioCAPdevs[i].name);
        strcat((char*)devstring, "~");    // audio device separator
    }

    devstring[0] = 3;   // ID for this UDP message
}

uint8_t* getAudioDevicelist(int *len)
{
    *len = strlen((char*)(devstring+1))+1;
    return devstring;
}

// pbdev, capdev: -1=default device
int init_audio(int setpbdev, int setcapdev)
{
static int f = 1;

    if (f == 1)
    {
        // do only once after program start
        f = 0;
        readAudioDevs();
        printAudioDevs();
        buildUdpAudioList();
        init_pipes();
    }

    // translate requested device numbers to bass device numbers
    if (setpbdev < 0 || setpbdev >= pbanz) setpbdev = 0;
    if (setcapdev < 0 || setcapdev >= capanz) setcapdev = 0;

    int pbdev = -1;
    if (setpbdev >=0 && setpbdev < pbanz) pbdev = audioPBdevs[setpbdev].bassdev;
    int capdev = -1;
    if (setcapdev >= 0 && setcapdev < capanz) capdev = audioCAPdevs[setcapdev].bassdev;

    printf("init audio, caprate:%d\n",caprate);
    printf("requested PB  device: %d bassno:%d name:%s\n", setpbdev, pbdev, audioPBdevs[setpbdev].name);
    printf("requested CAP device: %d bassno:%d name:%s\n", setcapdev, capdev, audioCAPdevs[setcapdev].name);

    // check the correct BASS was loaded
    if (HIWORD(BASS_GetVersion()) != BASSVERSION)
    {
        printf("An incorrect version of BASS was loaded\n");
        return -1;
    }

#ifdef _WIN32_
    // use WASAPI for Windows to get exclusive access to sound card
    return init_wasapi(pbdev, capdev);
#endif

#ifdef _LINUX_
    close_audio();

    // ===== PLAYBACK ======

    // initialize default output device
    if (!BASS_Init(pbdev, caprate, 0, NULL, NULL))
    {
        printf("Can't initialize output device\n");
        return -1;
    }

    // read real device number
    int ret = BASS_GetDevice();
    if (ret == -1)
    {
        printf("BASS_GetDevice: %d err:%d\n", pbdev, BASS_ErrorGetCode());
        return -1;
    }
    pbdev = ret;

    // set play callback
    BASS_GetInfo(&info);
    stream = BASS_StreamCreate(info.freq, CHANNELS, BASS_SAMPLE_FLOAT, (STREAMPROC*)WriteStream, 0); // sample: 32 bit float
    BASS_ChannelSetAttribute(stream, BASS_ATTRIB_BUFFER, 0); // no buffering for minimum latency
    BASS_ChannelPlay(stream, FALSE); // start it

    // ===== CAPTURE ====

    // initalize default recording device
    if (!BASS_RecordInit(capdev))
    {
        printf("Can't initialize recording device: %d\n", BASS_ErrorGetCode());
        return -1;
    }

    // read real device number
    ret = BASS_GetDevice();
    if (ret == -1)
    {
        printf("BASS_GetDevice: %d err:%d\n", capdev, BASS_ErrorGetCode());
        return -1;
    }
    capdev = ret;

    // set capture callback
    rchan = BASS_RecordStart(caprate, CHANNELS, BASS_SAMPLE_FLOAT, RecordingCallback, 0);
    if (!rchan) {
        printf("Can't start capturing: %d\n", BASS_ErrorGetCode());
        return -1;
    }

    printf("audio initialized\n");

    openpbdev = pbdev;
    opencapdev = capdev;
    
    return 0;
#endif
}

#ifdef _LINUX_

void close_audio()
{
    if(stream != 0)
    {
        printf("close Audio Devices\n");
        BASS_ChannelStop(rchan);
        int rr = BASS_RecordFree();
        if (!rr) printf("Bass_RecordFree error: %d\n", BASS_ErrorGetCode());

        BASS_StreamFree(stream);
        int r = BASS_Free();
        if(!r) printf("Bass_Free error: %d\n", BASS_ErrorGetCode());
        stream = 0;
    }
}

void selectPBdevice()
{
    if (!BASS_SetDevice(openpbdev))
        printf("BASS_SetDevice: %d err:%d\n", openpbdev, BASS_ErrorGetCode());
}

void selectCAPdevice()
{
    if (!BASS_SetDevice(opencapdev))
        printf("BASS_SetDevice: %d err:%d\n", opencapdev, BASS_ErrorGetCode());
}

void setPBvolume(int v)
{
    // the volume comes in % 0..99
    // map to 0..1
    float vf = v;
    vf /= 100;

    printf("set PB volume to:%d / %f [0..1]\n", v, vf );

    selectPBdevice();
    if (!BASS_SetVolume(vf))
        printf("setPBvolume: %d err:%d\n", openpbdev, BASS_ErrorGetCode());
}

void setCAPvolume(int v)
{
    // the volume comes in % 0..99
    // map to min/maxPBvol
    float vf = v;
    vf /= 100;

    printf("set CAP volume to:%d / %f [0..1]\n", v, vf);

    selectCAPdevice();
    if (!BASS_RecordSetInput(-1,BASS_INPUT_ON,vf))
        printf("setCAPvolume: %d err:%d\n", opencapdev, BASS_ErrorGetCode());
}

// capture callback
BOOL CALLBACK RecordingCallback(HRECORD handle, const void *buffer, DWORD length, void *user)
{
    //printf("captured %ld samples, channels:%d\n",length/sizeof(float),CHANNELS);
    //measure_speed(length/sizeof(float));
    
    float *fbuffer = (float *)buffer;
    //showbytestringf((char*)"cap:", fbuffer, 10);
    //printf("w:%ld ",length/sizeof(float));
    for(unsigned int i=0; i<(length/sizeof(float)); i+=CHANNELS)
    {
        //printf("%f\n",fbuffer[i]);
        cap_write_fifo(fbuffer[i]);
    }

    return TRUE; // continue recording
}

// play callback
// length: bytes. float=4byte, 2channels, so it requests samples*8
DWORD CALLBACK WriteStream(HSTREAM handle, float *buffer, DWORD length, void *user)
{
    //printf("requested %ld samples\n", length / sizeof(float));
    int ret = pb_read_fifo(buffer, length / sizeof(float));
    if(ret == 0)
    {
        // fifo empty, send 00
        memset(buffer,0,length);
    }
	return length;
}

#endif // _LINUX_

// set volume 
void setVolume(int pbcap, int v)
{
    if (pbcap == 0) setPBvolume(v);
    else setCAPvolume(v);
 }

// ================ thread safe fifo for audio callback routines ===============

#ifdef _WIN32_
CRITICAL_SECTION cap_crit_sec;
CRITICAL_SECTION pb_crit_sec;
#define CAP_LOCK	EnterCriticalSection(&cap_crit_sec)
#define PB_LOCK	    EnterCriticalSection(&pb_crit_sec)
void CAP_UNLOCK()
{
    if (&cap_crit_sec != NULL)
        LeaveCriticalSection(&cap_crit_sec);
}
void PB_UNLOCK()
{
    if (&pb_crit_sec != NULL)
        LeaveCriticalSection(&pb_crit_sec);
}
#endif

#ifdef _LINUX_
pthread_mutex_t cap_crit_sec;
pthread_mutex_t pb_crit_sec;
#define CAP_LOCK	pthread_mutex_lock(&cap_crit_sec)
void CAP_UNLOCK() { pthread_mutex_unlock(&cap_crit_sec); }
#define PB_LOCK	pthread_mutex_lock(&pb_crit_sec)
void PB_UNLOCK() { pthread_mutex_unlock(&pb_crit_sec); }
#endif

#define AUDIO_BUFFERMAXTIME 2  // fifo can buffer this time in [s]
#define AUDIO_PLAYBACK_BUFLEN (48000 * 10) // space for 10 seconds of samples
#define AUDIO_CAPTURE_BUFLEN  (48000 * 10)

int cap_wridx=0;
int cap_rdidx=0;
float cap_buffer[AUDIO_CAPTURE_BUFLEN];

int pb_wridx=0;
int pb_rdidx=0;
float pb_buffer[AUDIO_PLAYBACK_BUFLEN];

void init_pipes()
{
#ifdef _WIN32_
    if (&cap_crit_sec != NULL) DeleteCriticalSection(&cap_crit_sec);
    InitializeCriticalSection(&cap_crit_sec);

    if (&pb_crit_sec != NULL) DeleteCriticalSection(&pb_crit_sec);
    InitializeCriticalSection(&pb_crit_sec);
#endif
}

// write one sample into the fifo
// overwrite old data if the fifo is full
void cap_write_fifo(float sample)
{
	CAP_LOCK;
    cap_buffer[cap_wridx] = sample;
    if(++cap_wridx >= AUDIO_CAPTURE_BUFLEN) cap_wridx = 0;
	CAP_UNLOCK();
}

int cap_read_fifo(float *data)
{
	CAP_LOCK;

	if (cap_rdidx == cap_wridx)
	{
		// Fifo empty, no data available
		CAP_UNLOCK();
		return 0;
	}
	
	*data = cap_buffer[cap_rdidx];
    if(++cap_rdidx >= AUDIO_CAPTURE_BUFLEN) cap_rdidx = 0;
    CAP_UNLOCK();

	return 1;
}

void pb_write_fifo(float sample)
{
	PB_LOCK;
    
    // check if there is free space in fifo
    if(pb_fifo_freespace(1) == 0)
    {
        PB_UNLOCK();
        printf("************* pb fifo full\n");
        return;
    }
    
    pb_buffer[pb_wridx] = sample;
    if(++pb_wridx >= AUDIO_PLAYBACK_BUFLEN) pb_wridx = 0;
	PB_UNLOCK();
    //printf("write: pbw:%d pbr:%d\n",pb_wridx,pb_rdidx);
}

void pb_write_fifo_clear()
{
    pb_wridx = pb_rdidx = 0;
}

int pb_fifo_usedBlocks()
{
static int old_fill = 0;
int fill = 0;

    int fs = pb_fifo_freespace(0);
    int used = AUDIO_PLAYBACK_BUFLEN - fs;
    used /= (txinterpolfactor * UDPBLOCKLEN * 8 / bitsPerSymbol);

    if (used > 0) fill = 1; else fill = 0;
    if (fill == 1 && old_fill == 0)
        printf("fifo has data to send\n");
    if (fill == 0 && old_fill == 1)
        printf("fifo now empty\n");
    old_fill = fill;

    //printf("free:%d used blocks:%d\n", fs, used);
    return used;
}

int pb_fifo_freespace(int nolock)
{
int freebuf = 0;

    if(nolock == 0) PB_LOCK;
    
    int elemInFifo = (pb_wridx + AUDIO_PLAYBACK_BUFLEN - pb_rdidx) % AUDIO_PLAYBACK_BUFLEN;
    freebuf = AUDIO_PLAYBACK_BUFLEN - elemInFifo;
    
	if(nolock == 0) PB_UNLOCK();
    
    //printf("fifolen:%d check: pbw:%d pbr:%d freebuf:%d\n",AUDIO_PLAYBACK_BUFLEN,pb_wridx,pb_rdidx,freebuf);
    
    return freebuf;
}

// read elements floats from fifo or return 0 if not enough floats are available
int pb_read_fifo(float *data, int elements)
{
    //printf("pb read fifo: %d\n",elements);
    PB_LOCK;
    
    int e = AUDIO_PLAYBACK_BUFLEN - pb_fifo_freespace(1);
    if(e < elements)
	{
		// Fifo empty, no data available
		PB_UNLOCK();
        //printf("pb fifo empty, need:%d have:%d size:%d\n",elements,e,AUDIO_PLAYBACK_BUFLEN);
		return 0;
	}
	
    for(int i=0; i<elements; i++)
    {
        data[i] = pb_buffer[pb_rdidx];
        if(++pb_rdidx >= AUDIO_PLAYBACK_BUFLEN) pb_rdidx = 0;
    }
    //printf("read %d floats\n",elements);
    
    PB_UNLOCK();
    return 1;
}

