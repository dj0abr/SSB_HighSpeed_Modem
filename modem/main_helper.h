#include <stdio.h>
#include <ctype.h>
#include <string.h>
#include <errno.h>
#include <stdlib.h>
#include <stdint.h>
#include <wchar.h>
#include <unistd.h>
#include <sys/time.h>
#include <sys/ioctl.h>
#include <termios.h>
#include <sys/file.h>
#include <signal.h>

int isRunning(char *prgname);
void install_signal_handler();
void sighandler(int signum);
int run_console_program(char *cmd);
