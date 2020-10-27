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

int AddHeader(uint8_t *data, int len, char *filename);
uint8_t *zipArray(uint8_t *data, int length, int *ziplen);

#define ZIPPED
#define TXMAXSIZE 200000
uint8_t TXarray[TXMAXSIZE];
int txlen;              // total length of TXarray
int txpos;              // current position in TXarray
uint8_t txtype = 0;     // file type (from GUI)
uint8_t filestat = 0;   // 0=first frame, 1=next frame, 2=last frame

/*
* start sending a named byte array
* data ... contents of the Byte array
* length ... length of the Byte array
* type ... type of the file (see statics)
* filename ... description of the file or its name which is send with the data
*/


int arraySend(uint8_t *data, int length, uint8_t type, char *filename)
{
    if((length+55) >= TXMAXSIZE)
    {
        printf("file TOO long. Max is %d byte\n",TXMAXSIZE);
        return 0;
    }
    
    txtype = type;
    txpos = 0;
    filestat = 0;
    
    // if it is an ASCII, HTML or binary file, zip it
    if(type == 3 || type == 4 || type == 5)
    {
        #ifdef ZIPPED
        int ziplen = 0;
        printf("orig len:%d\n",length);
        uint8_t *zipdata = zipArray(data,length,&ziplen);
        if(zipdata==NULL) return 0;
        printf("zipped len:%d\n",ziplen);
        // add a file header and copy to txdata for transmission
        txlen = AddHeader(zipdata,ziplen,filename);
        #else
        txlen = AddHeader(data,length,filename);
        #endif
        printf("txlen:%d\n",txlen);
    }
    else
    {
        // add a file header and copy to txdata for transmission
        txlen = AddHeader(data,length,filename);
    }

    // marker, we are sending
    setSending(1);
    return 1;
}

int AddHeader(uint8_t *data, int len, char *filename)
{
    // make a unique ID number for this file
    // we simply calc the CRC16 of the filename
    uint16_t fncrc = Crc16_messagecalc(CRC16FILE, (uint8_t *)filename,strlen(filename));
    
    // create the file header
    // 50 bytes ... Filename (or first 50 chars of the filename)
    // 2 bytes .... CRC16 od the filename, this is used as a file ID
    // 3 bytes .... size of file

    int flen = strlen(filename);
    if (flen > 50) flen = 50;
    memcpy(TXarray,filename,flen);
    
    TXarray[50] = (uint8_t)((fncrc >> 8)&0xff);
    TXarray[51] = (uint8_t)(fncrc&0xff);

    TXarray[52] = len >> 16;
    TXarray[53] = len >> 8;
    TXarray[54] = len;
    
    memcpy(TXarray+55,data,len);
    
    return len+55;
}

// called from main() in a loop
// sends an array if specified by arraySend(..)
void doArraySend()
{
    if(getSending() == 0) return;
    
    if(filestat == 0)
    {
        // send first frame
        printf("Start Array Send %d\n",getSending());
        toGR_Preamble();
        if(txlen <= PAYLOADLEN)
        {
            // we just need to send one frame
            printf("send last frame only\n");
            toGR_sendData(TXarray, txtype, 3);
            toGR_sendData(TXarray, txtype, 3);
            setSending(0);
        }
        else
        {
            printf("send first frame\n");
            // data is longer than one PAYLOAD
            toGR_sendData(TXarray, txtype, filestat);
            txpos += PAYLOADLEN;
            filestat = 1;
        }
        return;
    }
    
    if(filestat == 1)
    {
        // check if this is the last frame
        int restlen = txlen - txpos;
        if(restlen <= PAYLOADLEN)
        {
            // send as the last frame
            printf("send last frame\n");
            toGR_sendData(TXarray+txpos, txtype, 2);
            toGR_sendData(TXarray+txpos, txtype, 2);
            setSending(0);  // transmission complete
        }
        else
        {
            // additional frame follows
            printf("send next frame\n");
            // from txdata send one chunk of length PAYLOADLEN
            toGR_sendData(TXarray+txpos, txtype, filestat);
            txpos += PAYLOADLEN;
        }
        return;
    }
}

// make _arraySending flag thread safe
// it is called from main() and from udp-RX
pthread_mutex_t     as_crit_sec;
#define AS_LOCK	    pthread_mutex_lock(&as_crit_sec)
#define AS_UNLOCK	pthread_mutex_unlock(&as_crit_sec)

int __arraySending = 0;   // 1 ... Array transmission in progress

void setSending(uint8_t onoff)
{
    AS_LOCK;
    __arraySending = onoff;
    AS_UNLOCK;
}

int getSending()
{
    int as;
    AS_LOCK;
    if(__arraySending != 0)
        printf("__arraySending: %d\n",__arraySending);
    as = __arraySending;
    AS_UNLOCK;
    return as;
}

#define defaultTXzipFN  "tmp.zip"

uint8_t *zipArray(uint8_t *data, int length, int *ziplen)
{
    int err = 0;
    unlink(defaultTXzipFN); // delete existing zip file
    struct zip *zp = zip_open(defaultTXzipFN, ZIP_CREATE, &err);
    
    zip_source_t *s;
    if ((s=zip_source_buffer(zp, data, length, 0)) == NULL ||
        zip_file_add(zp, "my2databuffer", s, ZIP_FL_ENC_UTF_8) < 0) 
    {
        zip_source_free(s);
        printf("error adding file: %s\n", zip_strerror(zp));
        return NULL;
    }
    
    zip_close(zp);
    
    // zip file is done
    // now read the file and return the buffer
    #define TXMAXSIZE 200000
    static uint8_t ZIPdata[TXMAXSIZE];
    FILE *fp=fopen(defaultTXzipFN,"rb");
    if(fp)
    {
        *ziplen = fread(ZIPdata,1,TXMAXSIZE,fp);
        fclose(fp);
        return ZIPdata;
    }
    
    return NULL;
}
