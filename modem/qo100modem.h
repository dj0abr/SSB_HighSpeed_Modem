#include <stdio.h>
#include <ctype.h>
#include <assert.h>
#include <string.h>
#include <errno.h>
#include <sys/types.h>
#include <fcntl.h>
#include <signal.h>
#include <time.h>
#include <stdlib.h>
#include <stdarg.h>
#include <stdint.h>
#include <wchar.h>
#include <sys/socket.h>
#include <unistd.h>
#include <sys/time.h>
#include <sys/ioctl.h>
#include <termios.h>
#include <sys/file.h>
#include <pthread.h>
#include <netinet/in.h>
#include <netdb.h>
#include <arpa/inet.h>
#include <pwd.h>
#include <math.h>
#include <ifaddrs.h>
#include <zip.h> 
#include "frameformat.h"
#include "main_helper.h"
#include "udp.h"

#define jpg_tempfilename "rxdata.jpg"

#define CRC16TX 0
#define CRC16RX 1
#define CRC16FILE 2

void stopModem();
uint8_t *unpack_data(uint8_t *rxd, int len);
void TX_Scramble(uint8_t *data, int len);
uint8_t *RX_Scramble(uint8_t *data, int len);
uint16_t Crc16_messagecalc(int rxtx, uint8_t *data,int len);
uint32_t crc32_messagecalc(int txrx, unsigned char *data, int len);
int cfec_Reconstruct(uint8_t *darr, uint8_t *destination);
uint8_t *Pack(uint8_t *payload, int type, int status, int *plen);
void GetFEC(uint8_t *txblock, int len, uint8_t *destArray);
void initFEC();
void toGR_Preamble();
void toGR_sendData(uint8_t *data, int type, int status);
uint16_t *make_waterfall(uint8_t *pdata, int len, int *retlen);
void init_fft();
void exit_fft();
uint8_t *convertQPSKSymToBytes(uint8_t *rxsymbols);
uint8_t *convert8PSKSymToBytes(uint8_t *rxsymbols, int len);
uint8_t *getPayload(uint8_t *rxb);
void showbytestring(char *title, uint8_t *data, int anz);
void init_packer();
void convertBytesToSyms_QPSK(uint8_t *bytes, uint8_t *syms, int bytenum);
void rotateQPSKsyms(uint8_t *src, uint8_t *dst, int len);
uint8_t * rotateBackQPSK(uint8_t *buf, int len, int rotations);
void convertBytesToSyms_8PSK(uint8_t *bytes, uint8_t *syms, int bytenum);
void rotate8PSKsyms(uint8_t *src, uint8_t *dst, int len);
uint8_t * rotateBack8PSK(uint8_t *buf, int len, int rotations);
void setSending(uint8_t onoff);
void toGR_Preamble();
int getSending();
void doArraySend();
int arraySend(uint8_t *data, int length, uint8_t type, char *filename);
void shiftleft(uint8_t *data, int shiftnum, int len);
void showbytestring16(char *title, uint16_t *data, int anz);


extern int keeprunning;
extern int BC_sock_AppToModem;
extern int speed;
extern int speedmode;
extern int bitsPerSymbol;
extern int constellationSize;


/*
 * Constellation as produced by the GR Constellation Decoder:
 * 
 * 0 ... +1+1j
 * 1 ... -1+1j
 * 2 ... -1-1j
 * 3 ... +1-1j
 * 
 * 
 * */
