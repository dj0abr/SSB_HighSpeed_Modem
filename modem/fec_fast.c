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
#include "liquid.h"

fec fecobjTX;
fec fecobjRX;
fec_scheme fs = LIQUID_FEC_SECDED3932;   // error-correcting scheme
uint8_t encoded_message[UdpBlocklen];
uint8_t decoded_message[PayloadLen+framenumlen+CRClen];

uint8_t *cfec_Reconstruct(uint8_t *darr)
{
    memset(decoded_message,0,(PayloadLen+framenumlen+CRClen));
    fec_decode(fecobjRX, (PayloadLen+framenumlen+CRClen), darr, decoded_message);
       
    return decoded_message;
}

uint8_t *GetFEC(uint8_t *txblock, int len)
{
    if(len != (PayloadLen+framenumlen+CRClen))
    {
        printf("wrong FEC encode length, len:%d Payloadlen:%d\n",len,PayloadLen);
        exit(0);
    }
    
    fec_encode(fecobjTX, len, txblock, encoded_message);
       
    return encoded_message;
}

void initFEC()
{
    int n_enc = fec_get_enc_msg_length(fs,(PayloadLen+framenumlen+CRClen));
    if(n_enc != UdpBlocklen)
    {
        printf("wrong FEC init length\n");
        exit(0);
    }
    
    fecobjTX = fec_create(fs,NULL);
    fecobjRX = fec_create(fs,NULL);
}

