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
#include <fftw3.h>
#include <math.h>

#define AUDIOSAMPLERATE 8000

double *din = NULL;		// input data for  fft
fftw_complex *cpout = NULL;	    // ouput data from fft
fftw_plan plan = NULL;
#define fft_rate  (AUDIOSAMPLERATE / 10)       // resolution: 10 Hz
int fftidx = 0;
int fftcnt = fft_rate/2+1;      // number of output values
uint16_t fftout[AUDIOSAMPLERATE / 10/2+1];

uint16_t *make_waterfall(uint8_t *pdata, int len, int *retlen)
{
    int fftrdy = 0;
    // get the real sample in float (imag is not required for the FFT)
    int re=0;
    
    // GR sends 8 Bytes containing 4x 0x000003e8 (marker) and 4x input-samples (real integer)
    #define dlen  8
    static uint8_t rbuf[dlen];
    
    for(int i=0; i<len; i++)
    {
        // insert new byte in rbuf
        for(int sh = (dlen-1); sh > 0; sh--)
            rbuf[sh] = rbuf[sh - 1];
        rbuf[0] = pdata[i];
        
        // check for BIG/LITTLE endian
        if(rbuf[0] == 0 && rbuf[1] == 0 && rbuf[2] == 3 && rbuf[3] == 0xe8)
        {
            re = rbuf[4];
            re <<= 24;
            re += rbuf[5];
            re <<= 16;
            re += rbuf[6];
            re <<= 8;
            re += rbuf[7];
        }
        else if(rbuf[0] == 0xe8 && rbuf[1] == 3 && rbuf[2] == 0 && rbuf[3] == 0)
        {
            re = rbuf[7];
            re <<= 24;
            re += rbuf[6];
            re <<= 16;
            re += rbuf[5];
            re <<= 8;
            re += rbuf[4];
        }
        else
            continue;
        
        // the value was scaled in GR by 2^24 = 16777216
        // in order to send it in an INT
        // undo this scaling
        float fre = (float)re / 16777216;
        
        // fre are the float samples
        // fill into the fft input buffer
        din[fftidx++] = fre;
        
        if(fftidx == fft_rate)
        {
            fftidx = 0;
            
            // the fft buffer is full, execute the FFT 
            fftw_execute(plan);
            
            for (int j = 0; j < fftcnt; j++)
            {
                // calculate absolute value (magnitute without phase)
                float fre = cpout[j][0];
                float fim = cpout[j][1];
                float mag = sqrt((fre * fre) + (fim * fim));

                fftout[j] = (uint16_t)mag;
                
                fftrdy = 1;
            }
        }
    }
    
    if(fftrdy == 1)
    {
        *retlen = fftcnt;
        return fftout;
    }
    
    return NULL;
}

void init_fft()
{
char fn[300];

	sprintf(fn, "capture_fft_%d", fft_rate);	// wisdom file for each capture rate

	fftw_import_wisdom_from_filename(fn);

	din = (double *)fftw_malloc(sizeof(double) * fft_rate);
	cpout = (fftw_complex *)fftw_malloc(sizeof(fftw_complex) * fft_rate);

	plan = fftw_plan_dft_r2c_1d(fft_rate, din, cpout, FFTW_MEASURE);
	
	fftw_export_wisdom_to_filename(fn);
}

void exit_fft()
{
    if(plan) fftw_destroy_plan(plan);
	if(din) fftw_free(din);
	if(cpout) fftw_free(cpout);
}
