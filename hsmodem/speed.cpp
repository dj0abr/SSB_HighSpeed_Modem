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

#include "hsmodem.h"

int speed = 0;

#define MAXSPDARR   5
int spdarr[MAXSPDARR];
int spdarrbps[MAXSPDARR];

#ifdef _LINUX_
int getus()
{
    struct timeval  tv;
 	gettimeofday(&tv, NULL);
	return tv.tv_sec * 1000000 + tv.tv_usec;
}
#endif

#ifdef _WIN32_
int getus()
{
    int actms;

    SYSTEMTIME st;
    GetSystemTime(&st);

    actms = st.wSecond * 1000 + (int)st.wMilliseconds;
    return actms * 1000;
}
#endif

int meanval(int v)
{
static int f=1;

    if(f)
    {
        for(int i=0; i<MAXSPDARR; i++) spdarr[i] = -1;
        f=0;
    }
    
    for(int i=(MAXSPDARR-1); i>0; i--)
        spdarr[i] = spdarr[i-1];
    spdarr[0] = v;
    
    int ssum=0;
    int cnt = 0;
    for(int i=0; i<MAXSPDARR; i++)
    {
        if(spdarr[i] != -1)
        {
            ssum += spdarr[i];
            cnt++;
        }
    }
    ssum /= cnt;
    return ssum;
}

int meanvalbps(int v)
{
    static int f = 1;

    if (f)
    {
        for (int i = 0; i < MAXSPDARR; i++) spdarrbps[i] = -1;
        f = 0;
    }

    for (int i = (MAXSPDARR - 1); i > 0; i--)
        spdarrbps[i] = spdarrbps[i - 1];
    spdarrbps[0] = v;

    int ssum = 0;
    int cnt = 0;
    for (int i = 0; i < MAXSPDARR; i++)
    {
        if (spdarrbps[i] != -1)
        {
            ssum += spdarrbps[i];
            cnt++;
        }
    }
    ssum /= cnt;
    return ssum;
}

// len ... number of symbols
// measures and calculates the speed in bit / s
void measure_speed_syms(int len)
{
    static int lasttim = 0;
    static int elems = 0;
    
    int tim = getus();
    int timespan = tim - lasttim;
    if(timespan < 0) 
    {
        lasttim = tim;
        return;
    }
    
    
    elems += len;
    if(timespan < 1000000) return;
    
    double dspd = elems;
    dspd = dspd * 1e6 / timespan;
    speed = meanval((int)dspd) * bitsPerSymbol;
    
    // here we have number of elements after 1s
    //printf("%d sym/s\n",speed);
    
    elems=0;
    lasttim = tim;
}

void measure_speed_bps(int len)
{
    static int lasttim = 0;
    static int elems = 0;

    int tim = getus();
    int timespan = tim - lasttim;
    if (timespan < 0)
    {
        lasttim = tim;
        return;
    }


    elems += len;
    if (timespan < 1000000) return;

    double dspd = elems;
    dspd = dspd * 1e6 / timespan;
    speed = meanvalbps((int)dspd);

    // here we have number of elements after 1s
    printf("%d bit/s\n", speed);

    elems = 0;
    lasttim = tim;
}
