
#ifdef _WIN32
#define _WIN32_
	// ignore senseless warnings invented by M$ to confuse developers
	#pragma warning( disable : 4091 )
	#pragma warning( disable : 4003 )
#else
#define _LINUX_
#endif

#include <stdio.h>
#include <ctype.h>
#include <string.h>
#include <errno.h>
#include <sys/types.h>
#include <fcntl.h>
#include <signal.h>
#include <stdlib.h>
#include <stdarg.h>
#include <stdint.h>
#include <wchar.h>

#ifdef _WIN32_
#include "Winsock2.h"
#include "io.h"
#include <Windows.h>
#include <iostream>
#include <process.h>
#include <Tlhelp32.h>
#include <winbase.h>
#include <Shlobj.h>
#define _USE_MATH_DEFINES
#include <math.h>

#pragma comment(lib, "bass.lib")
#pragma comment(lib, "basswasapi.lib")
#pragma comment(lib, "libliquid.lib")
#pragma comment(lib, "fftw_lib/libfftw3-3.lib")
#endif

#ifdef _LINUX_
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
#endif

#include "bass.h"
#include "basswasapi.h"
#include "liquid.h"
#include "frameformat.h"
#include "fec.h"
#include "udp.h"

#define jpg_tempfilename "rxdata.jpg"

#define CRC16TX 0
#define CRC16RX 1
#define CRC16FILE 2

// definitions for audio
#define MAXDEVSTRLEN    2000
#define CHANNELS 1      // no of channels used

void init_packer();
uint8_t* Pack(uint8_t* payload, int type, int status, int* plen);
uint8_t* unpack_data(uint8_t* rxd, int len);

void convertBytesToSyms_QPSK(uint8_t* bytes, uint8_t* syms, int bytenum);
void convertBytesToSyms_8PSK(uint8_t* bytes, uint8_t* syms, int bytenum);

uint8_t* convertQPSKSymToBytes(uint8_t* rxsymbols);
uint8_t* convert8PSKSymToBytes(uint8_t* rxsymbols, int len);


void rotateQPSKsyms(uint8_t* src, uint8_t* dst, int len);
void rotate8PSKsyms(uint8_t* src, uint8_t* dst, int len);
void rotate8APSKsyms(uint8_t* src, uint8_t* dst, int len);

uint8_t* rotateBackQPSK(uint8_t* buf, int len, int rotations);
uint8_t* rotateBack8PSK(uint8_t* buf, int len, int rotations);
uint8_t* rotateBack8APSK(uint8_t* buf, int len, int rotations);

void TX_Scramble(uint8_t* data, int len);
uint8_t* RX_Scramble(uint8_t* data, int len);
uint16_t Crc16_messagecalc(int rxtx, uint8_t* data, int len);

void showbytestring(char* title, uint8_t* data, int anz);
void measure_speed_syms(int len);
void measure_speed_bps(int len);

void initFEC();
void GetFEC(uint8_t* txblock, int len, uint8_t* destArray);
int cfec_Reconstruct(uint8_t* darr, uint8_t* destination);

int init_audio(int pbdev, int capdev);
int pb_fifo_freespace(int nolock);
void pb_write_fifo_clear();
void pb_write_fifo(float sample);
int cap_read_fifo(float* data);
uint8_t* getAudioDevicelist(int* len);
void setPBvolume(int v);
void setCAPvolume(int v); 
void setVolume(int pbcap, int v);
int init_wasapi(int pbdev, int capdev);

void sleep_ms(int ms);
void GRdata_rxdata(uint8_t* pdata, int len, struct sockaddr_in* rxsock);

void modulator(uint8_t sym_in);
int pb_fifo_usedBlocks();
void init_dsp();
int demodulator();
void sendToModulator(uint8_t* d, int len);
void resetModem();
void close_dsp();
void init_fft();
void exit_fft();
void showbytestringf(char* title, float* data, int anz);
uint16_t* make_waterfall(float fre, int* retlen);


extern int speedmode;
extern int bitsPerSymbol;
extern int constellationSize;
extern int speed;
extern int keeprunning;
extern int caprate;
extern int BC_sock_AppToModem;
extern int UdpDataPort_ModemToApp;
extern int txinterpolfactor;
extern int rxPreInterpolfactor;
extern char appIP[20];
extern float softwareCAPvolume;

#ifdef _LINUX_
int isRunning(char* prgname);
void install_signal_handler();
int isRunning(char* prgname);
#endif
