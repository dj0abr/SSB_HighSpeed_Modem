# makefile for dv_serial

CXXFLAGS = -Wall -O3 -std=c++0x -Wno-write-strings -Wno-narrowing
LDFLAGS = -lpthread -lrt -lsndfile -lasound -lm -lbass -lbassflac -lfftw3 -lfftw3_threads -lliquid
OBJ = hsmodem.o constellation.o crc16.o frame_packer.o main_helper.o scrambler.o speed.o fec.o audio.o udp.o fft.o liquid_if.o

default: $(OBJ)
	g++ $(CXXFLAGS) -o ../LinuxRelease/hsmodem $(OBJ) $(LDFLAGS)

clean:
	rm -f *.o

