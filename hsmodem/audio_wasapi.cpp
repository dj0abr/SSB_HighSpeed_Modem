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
* audio_wasabi.c ... functions to handle audio in/out via a soundcard uses the "BASSWASAPI" library
* wasapi is needed because we need exclusive access to the sound card which is not provided for Windows with the normal bass.lib
*/


#include "hsmodem.h"

#ifdef _WIN32_

#define WASAPI_CHANNELS 2   // wasapi works with 2 only

void init_pipes();
void cap_write_fifo(float sample);
int pb_read_fifo(float* data, int elements);
void close_wasapi();
DWORD CALLBACK PBcallback_wasapi(void* buffer, DWORD length, void* user);
DWORD CALLBACK CAPcallback_wasapi(void* buffer, DWORD length, void* user);

float minPBvol = 0;
float maxPBvol = 99;
float minCAPvol = 0;
float maxCAPvol = 99;

extern int openpbdev;
extern int opencapdev;

float softwareCAPvolume = 0.5;

int use_wasapi = -1;

int init_wasapi(int pbdev, int capdev)
{
    close_wasapi();

    use_wasapi = -1;

    // ======= init PLAYBACK device ========

    // initialize default output device
    if (!BASS_WASAPI_Init(pbdev, caprate, WASAPI_CHANNELS, BASS_WASAPI_EXCLUSIVE, 0.1f/*buffer in seconds*/, 0, PBcallback_wasapi, NULL))
    {
        printf("Can't initialize output device: %d err:%d\n", pbdev, BASS_ErrorGetCode());
        return -1;
    }

    // read real device number since a -1 cannot be started
    int ret = BASS_WASAPI_GetDevice();
    if (ret == -1)
    {
        printf("BASS_WASAPI_GetDevice: %d err:%d\n", pbdev, BASS_ErrorGetCode());
        return -1;
    }
    pbdev = ret;

    // read the possible volume settings
    BASS_WASAPI_INFO info;
    if (!BASS_WASAPI_GetInfo(&info))
    {
        printf("BASS_WASAPI_GetInfo: %d err:%d\n", pbdev, BASS_ErrorGetCode());
        return -1;
    }
    minPBvol = info.volmin;
    maxPBvol = info.volmax;

    // start playback
    if (!BASS_WASAPI_Start())
    {
        printf("BASS_WASAPI_Start: %d err:%d\n", pbdev, BASS_ErrorGetCode());
        return -1;
    }

    // ======= init CAPTURE device ========

    // initalize default recording device
    if (capdev == -1) capdev = -2;  // cap: -2 is the default device for input
    if (!BASS_WASAPI_Init(capdev, caprate, WASAPI_CHANNELS, BASS_WASAPI_EXCLUSIVE, 0.1f/*buffer in seconds*/, 0, CAPcallback_wasapi, NULL))
    {
        printf("Can't initialize recording device: %d err:%d\n", capdev, BASS_ErrorGetCode());
        return -1;
    }

    // read real device number since a -2 cannot be started
    ret = BASS_WASAPI_GetDevice();
    if (ret == -1)
    {
        printf("BASS_WASAPI_GetDevice: %d err:%d\n", capdev, BASS_ErrorGetCode());
        return -1;
    }
    capdev = ret;

    // read the possible volume settings
    if (!BASS_WASAPI_GetInfo(&info))
    {
        printf("BASS_WASAPI_GetInfo: %d err:%d\n", pbdev, BASS_ErrorGetCode());
        return -1;
    }
    minCAPvol = info.volmin;
    maxCAPvol = info.volmax;

    // start recording
    if (!BASS_WASAPI_Start())
    {
        printf("BASS_WASAPI_Start: %d err:%d\n", capdev, BASS_ErrorGetCode());
        return -1;
    }

    printf("WASAPI started successfully for PBdev:%d and CAPdev:%d\n", pbdev, capdev);

    openpbdev = pbdev;
    opencapdev = capdev;

    use_wasapi = 0;

    return 0;
}

void selectPBdevice_wasapi()
{
    if (!BASS_WASAPI_SetDevice(openpbdev))
        printf("BASS_WASAPI_SetDevice: %d err:%d\n", openpbdev, BASS_ErrorGetCode());
}

void selectCAPdevice_wasapi()
{
    if (!BASS_WASAPI_SetDevice(opencapdev))
        printf("BASS_WASAPI_SetDevice: %d err:%d\n", opencapdev, BASS_ErrorGetCode());
}

void setPBvolume(int v)
{
    // the volume comes in % 0..99
    // map to min/maxPBvol
    float vf = v * (maxPBvol - minPBvol) / 100 + minPBvol;

    if (vf < minPBvol) vf = minPBvol;
    if (vf > maxPBvol) vf = maxPBvol;

    //printf("set PB volume to:%d / %f [%f..%f]\n", v, vf, minPBvol, maxPBvol);

    selectPBdevice_wasapi();
    if (!BASS_WASAPI_SetVolume(BASS_WASAPI_CURVE_DB, vf))
        printf("setPBvolume: %d err:%d\n", openpbdev, BASS_ErrorGetCode());
}

void setCAPvolume(int v)
{
    // non of the BASS input level functions are working in WASAPI exclusive mode
    // so we adjust the input level by software
    softwareCAPvolume = (float)v;
    softwareCAPvolume /= 50;
}

void close_wasapi()
{
    printf("close WASAPI Audio Devices\n");

    if (openpbdev != -1)
    {
        selectPBdevice_wasapi();
        if (!BASS_WASAPI_Free()) printf("BASS_WASAPI_Free: dev:%d err:%d\n", openpbdev, BASS_ErrorGetCode());
    }

    if (opencapdev != -1)
    {
        selectCAPdevice_wasapi();
        if (!BASS_WASAPI_Free()) printf("BASS_WASAPI_Free: dev:%d err:%d\n", opencapdev, BASS_ErrorGetCode());
    }
}

DWORD CALLBACK PBcallback_wasapi(void* buffer, DWORD length, void* user)
{
    float* fbuffer = (float*)buffer;

    // requested number of stereo samples: length/sizeof(float)
    // requested real number of samples
    int req_samples = length / sizeof(float) / WASAPI_CHANNELS;
    // prepare a buffer to store the mono samples from the fifo
    float* fdata = (float*)malloc(sizeof(float) * req_samples);
    // read mono samples from fifo
    int ret = pb_read_fifo(fdata, req_samples);
    if (ret == 0)
    {
        // fifo empty, send 00
        memset(fdata, 0, sizeof(float) * req_samples);
    }
    // copy the mono samples into the stereo output buffer
    int didx = 0;
    for (int i = 0; i < req_samples; i++)
    {
        fbuffer[didx++] = fdata[i];
        fbuffer[didx++] = fdata[i];
    }

    free(fdata);
    return length;
}
/*
#define MCHECK 10
void nullChecker(float fv, float *pbuf, DWORD len)
{
    static float farr[MCHECK];
    static int idx = 0;
    static int f = 1;
    static int anz = 0;

    if (f)
    {
        f = 0;
        for (int i = 0; i < MCHECK; i++)
            farr[i] = 1;
    }

    farr[idx] = fv;
    idx++;
    if (idx == MCHECK) idx = 0;

    float nu = 0;
    for (int i = 0; i < MCHECK; i++)
    {
        nu += farr[i];
    }

    if (nu == 0)
    {
        // how many 00s ar in the current buffer
        int a = 0;
        for (unsigned int i = 0; i < len-1; i++)
        {
            if (pbuf[i] == 0 && pbuf[i+1] == 0) a++;
        }
        printf("=============== null sequence detected: %d len:%d nullanz:%d\n",anz++,len,a);
    }
}
*/
DWORD CALLBACK CAPcallback_wasapi(void* buffer, DWORD length, void* user)
{
    //printf("CAP callback, len:%d\n",length);
    //measure_speed_bps(length/sizeof(float)/ WASAPI_CHANNELS);

    float* fbuffer = (float*)buffer;
    //showbytestringf((char*)"rx: ", fbuffer, 10);
    //printf("%10.6f\n", fbuffer[0]);
    for (unsigned int i = 0; i < (length / sizeof(float)); i += WASAPI_CHANNELS)
    {
        //nullChecker(fbuffer[i],fbuffer, length / sizeof(float));
        cap_write_fifo(fbuffer[i]);
    }

    return TRUE; // continue recording
}

#endif // _WIN32_
