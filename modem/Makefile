# Makefile for qo100modem
# =======================

CFLAGS=-O3 -Wall
LDLIBS= -L. -lpthread -lfftw3 -lm -lzip -lfftw3_threads -lsndfile -lasound -lbass -lliquid
CC=c++
PROGNAME=qo100modem
OBJ=qo100modem.o main_helper.o udp.o frame_packer.o scrambler.o crc16.o fec.o fft.o constellation.o arraysend.o audio.o liquid_if.o

all: qo100modem

qo100modem: $(OBJ)
	$(CC) -g -o $@ $^ $(LDFLAGS) $(LDLIBS)

clean:
	rm -f *.o qo100modem
