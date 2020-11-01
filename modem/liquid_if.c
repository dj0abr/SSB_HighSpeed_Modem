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
* liquid_dsp_interface.c ... functions using liquid-dsp
* 
* liquid-dsp must be previously installed by running ./liquid-dsp-install (under linux)
* 
*/

#include "qo100modem.h"

void modulator(uint8_t sym_in);
void close_dsp();

// modem objects
modulation_scheme ms    = LIQUID_MODEM_QPSK;
modem mod = NULL;

// NCOs for mixing baseband <-> 1500 Hz
#define FREQUENCY           1500            
int      type        = LIQUID_NCO;                  // nco type
nco_crcf upnco = NULL;

// TX-Interpolator Filter Parameters
// 44100 input rate for 2205 Sym/s = 20
// change for other rates
firinterp_crcf TX_interpolator = NULL;
unsigned int k_SampPerSymb =            20;         // 44100 / (4410/2)
unsigned int m_filterDelay_Symbols =    15;         // not too short for good filter
float        beta_excessBW  =           0.3f;      // filter excess bandwidth factor
float        tau_FracSymbOffset   =     -0.2f;      // fractional symbol offset

int init_dsp()
{
    close_dsp();
    printf("create DSP\n");
    
    k_SampPerSymb = txinterpolfactor;
    if(bitsPerSymbol == 2)
        ms = LIQUID_MODEM_QPSK;
    else
        ms = LIQUID_MODEM_DPSK8;
    
    // create modulator
    mod = modem_create(ms);
    
    // create NCO for upmixing to 1500 Hz
    double RADIANS_PER_SAMPLE   = ((2.0*M_PI*(double)FREQUENCY)/(float)caprate);

    upnco = nco_crcf_create(LIQUID_NCO);
    nco_crcf_set_phase(upnco, 0.0f);
    nco_crcf_set_frequency(upnco, RADIANS_PER_SAMPLE);
    
    // TX: Interpolator Filter
    // compute delay
    while (tau_FracSymbOffset < 0) tau_FracSymbOffset += 1.0f;  // ensure positive tau
    float g = k_SampPerSymb*tau_FracSymbOffset;                 // number of samples offset
    int ds=floorf(g);               // additional symbol delay
    float dt = (g - (float)ds);     // fractional sample offset
    // force dt to be in [0.5,0.5]
    if (dt > 0.5f) 
    {                
        dt -= 1.0f;
        ds++;
    }
    
    // calculate filter coeffs
    unsigned int h_len_NumFilterCoeefs = 2 * k_SampPerSymb * m_filterDelay_Symbols + 1;
    float h[h_len_NumFilterCoeefs];
    liquid_firdes_prototype(    LIQUID_FIRFILT_RRC,
                                k_SampPerSymb,
                                m_filterDelay_Symbols,
                                beta_excessBW,
                                dt,
                                h);
    // create the filter
    TX_interpolator = firinterp_crcf_create(k_SampPerSymb,h,h_len_NumFilterCoeefs);
    
    printf("DSP created\n");
    return 1;
}

void close_dsp()
{
    if(mod != NULL) modem_destroy(mod);
    if(upnco != NULL) nco_crcf_destroy(upnco);
    if(TX_interpolator != NULL) firinterp_crcf_destroy(TX_interpolator);
    mod = NULL;
    upnco = NULL;
    TX_interpolator = NULL;
}

// d ... symbols to send
// len ... number of symbols in d
void sendToModulator(uint8_t *d, int len)
{

    //printf("sendToModulator %d bytes\n",len);
    
    int symanz = len * 8 / bitsPerSymbol;
    uint8_t syms[symanz]; 
    if(bitsPerSymbol == 2)
        convertBytesToSyms_QPSK(d, syms, len);
    else
        convertBytesToSyms_8PSK(d, syms, len);
    
    for(int i=0; i<symanz; i++)
    {
        // remove gray code
        // this adds gray code, liquid adds it again which removes it
        syms[i] ^= (syms[i]>>1);
        
        modulator(syms[i]);
    }
}

// call for every symbol
// modulates, filters and upmixes symbols and send it to soundcard
void modulator(uint8_t sym_in)
{
    liquid_float_complex sample;
    modem_modulate(mod, sym_in, &sample);
    
    //printf("TX ================= sample: %f + i%f\n", sample.real, sample.imag);
    
    // interpolate by k_SampPerSymb
    liquid_float_complex y[k_SampPerSymb];
    firinterp_crcf_execute(TX_interpolator, sample, y);

    for(unsigned int i=0; i<k_SampPerSymb; i++)
    {
        // move sample to 1,5kHz carrier
        nco_crcf_step(upnco);
        float minus_sine = -nco_crcf_sin(upnco);
        float cosinus = nco_crcf_cos(upnco);
        float re = y[i].real * cosinus;
        float im = y[i].imag * minus_sine;
        float usb = re + im;
        // value is -1 .. +1
        
        // adapt speed to soundcard samplerate
        int fs;
        while(1)
        {
            fs = pb_fifo_freespace(0);
            if(fs) break;
            usleep(10000);
        }
        
        pb_write_fifo(usb * 0.2); // reduce volume and send to soundcard
    }
}
