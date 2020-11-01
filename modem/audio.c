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

#include "qo100modem.h"

//BOOL CALLBACK RecordingCallback(HRECORD handle, const void *buffer, DWORD length, void *user);
DWORD CALLBACK WriteStream(HSTREAM handle, float *buffer, DWORD length, void *user);
int pb_read_fifo(float *data, int elements);
void close_audio();

#define CHANNELS 1      // no of channels used

HRECORD rchan = 0;		// recording channel
BASS_INFO info;
HSTREAM stream = 0;		

int init_audio()
{
    close_audio();
    printf("init audio, caprate:%d\n",caprate);
    
    // check the correct BASS was loaded
	if (HIWORD(BASS_GetVersion()) != BASSVERSION) 
    {
		printf("An incorrect version of BASS was loaded\n");
        return -1;
	}
	/*
	// initalize default recording device
	if (!BASS_RecordInit(-1)) 
    {
		printf("Can't initialize recording device\n");
        return -1;
	}
	*/
	// initialize default output device
	if (!BASS_Init(-1, caprate, 0, NULL, NULL)) 
    {
		printf("Can't initialize output device\n");
        return -1;
    }
    /*
    // set capture callback
    rchan = BASS_RecordStart(caprate, CHANNELS, BASS_SAMPLE_FLOAT, RecordingCallback, 0);
	if (!rchan) {
		printf("Can't start capturing\n");
		return -1;
	}
	*/
	// set play callback
	BASS_GetInfo(&info);
	stream = BASS_StreamCreate(info.freq, CHANNELS, BASS_SAMPLE_FLOAT, (STREAMPROC*)WriteStream, 0); // sample: 32 bit float
	BASS_ChannelSetAttribute(stream, BASS_ATTRIB_BUFFER, 0); // no buffering for minimum latency
	BASS_ChannelPlay(stream, FALSE); // start it
    
    return 0;
}

void close_audio()
{
    if(stream != 0)
    {
        //BASS_RecordFree();
        int r = BASS_Free();
        if(!r)
            printf("Bass_free error: %d\n", BASS_ErrorGetCode());
        stream = 0;
    }
}
/*
// capture callback
// length: bytes. short=2byte, 2channels, so it requests samples*4
BOOL CALLBACK RecordingCallback(HRECORD handle, const void *buffer, DWORD length, void *user)
{
    //printf("captured %ld samples\n",length/sizeof(float));
    
    float *fbuffer = (float *)buffer;
    //printf("w:%ld ",length/sizeof(float));
    for(int i=0; i<(length/sizeof(float)); i+=CHANNELS)
    {
        //printf("%f\n",fbuffer[i]);
        cap_write_fifo(fbuffer[i]);
    }

    return TRUE; // continue recording
}
*/
// play callback
// length: bytes. float=4byte, 2channels, so it requests samples*8
DWORD CALLBACK WriteStream(HSTREAM handle, float *buffer, DWORD length, void *user)
{
    int ret = pb_read_fifo(buffer,length/sizeof(float));
    if(ret == 0)
    {
        // fifo empty, send 00
        memset(buffer,0,length);
    }
	return length;
}

// ================ thread safe fifo for audio callback routines ===============

pthread_mutex_t cap_crit_sec;
pthread_mutex_t pb_crit_sec;
#define CAP_LOCK	pthread_mutex_lock(&cap_crit_sec)
#define CAP_UNLOCK	pthread_mutex_unlock(&cap_crit_sec)
#define PB_LOCK	pthread_mutex_lock(&pb_crit_sec)
#define PB_UNLOCK	pthread_mutex_unlock(&pb_crit_sec)

#define AUDIO_BUFFERMAXTIME 2  // fifo can buffer this time in [s]
#define AUDIO_PLAYBACK_BUFLEN (48000 * 10) // space for 10 seconds of samples
/*#define AUDIO_CAPTURE_BUFLEN  (48000*CHANNELS*AUDIO_BUFFERMAXTIME*UPSAMPLING)

int cap_wridx=0;
int cap_rdidx=0;
float cap_buffer[AUDIO_CAPTURE_BUFLEN];
*/
int pb_wridx=0;
int pb_rdidx=0;
float pb_buffer[AUDIO_PLAYBACK_BUFLEN];

/*
// write one sample into the fifo
// overwrite old data if the fifo is full
void cap_write_fifo(float sample)
{
	CAP_LOCK;
    cap_buffer[cap_wridx] = sample;
    if(++cap_wridx >= AUDIO_CAPTURE_BUFLEN) cap_wridx = 0;
	CAP_UNLOCK;
}

int cap_read_fifo(float *data)
{
	CAP_LOCK;

	if (cap_rdidx == cap_wridx)
	{
		// Fifo empty, no data available
		CAP_UNLOCK;
		return 0;
	}
	
	*data = cap_buffer[cap_rdidx];
    if(++cap_rdidx >= AUDIO_CAPTURE_BUFLEN) cap_rdidx = 0;
    CAP_UNLOCK;

	return 1;
}
*/
void pb_write_fifo(float sample)
{
	PB_LOCK;
    
    // check if there is free space in fifo
    if(pb_fifo_freespace(1) == 0)
    {
        PB_UNLOCK;
        printf("************* pb fifo full\n");
        return;
    }
    
    pb_buffer[pb_wridx] = sample;
    if(++pb_wridx >= AUDIO_PLAYBACK_BUFLEN) pb_wridx = 0;
	PB_UNLOCK;
    //printf("write: pbw:%d pbr:%d\n",pb_wridx,pb_rdidx);
}

int pb_fifo_freespace(int nolock)
{
int freebuf = 0;

    if(nolock == 0) PB_LOCK;
    
    /*freebuf = AUDIO_PLAYBACK_BUFLEN - ((pb_wridx+AUDIO_PLAYBACK_BUFLEN+1 - pb_rdidx)%AUDIO_PLAYBACK_BUFLEN);
    freebuf %= AUDIO_PLAYBACK_BUFLEN;*/
    
    int elemInFifo = (pb_wridx + AUDIO_PLAYBACK_BUFLEN - pb_rdidx) % AUDIO_PLAYBACK_BUFLEN;
    freebuf = AUDIO_PLAYBACK_BUFLEN - elemInFifo;
    
	if(nolock == 0) PB_UNLOCK;
    
    //printf("fifolen:%d check: pbw:%d pbr:%d freebuf:%d\n",AUDIO_PLAYBACK_BUFLEN,pb_wridx,pb_rdidx,freebuf);
    
    return freebuf;
}

// read elements floats from fifo or return 0 if not enough floats are available
int pb_read_fifo(float *data, int elements)
{
    //printf("pb read fifo: %d\n",elements);
	PB_LOCK;
    
    int e = AUDIO_PLAYBACK_BUFLEN - pb_fifo_freespace(1);
    //if (pb_rdidx == pb_wridx)
    if(e < elements)
	{
		// Fifo empty, no data available
		PB_UNLOCK;
        //printf("pb fifo empty, need:%d have:%d size:%d\n",elements,e,AUDIO_PLAYBACK_BUFLEN);
		return 0;
	}
	
    for(int i=0; i<elements; i++)
    {
        data[i] = pb_buffer[pb_rdidx];
        if(++pb_rdidx >= AUDIO_PLAYBACK_BUFLEN) pb_rdidx = 0;
    }
    //printf("read %d floats\n",elements);
    
    PB_UNLOCK;
    return 1;
}
