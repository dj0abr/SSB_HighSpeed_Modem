# makefile for hsmodem

CXXFLAGS = -Wall -O3 -std=c++0x -Wno-write-strings -Wno-narrowing
LDFLAGS = -lpthread -lrt -lsndfile -lasound -lm -lopus -lbassopus -lbass -lbassenc_opus -lbassenc -lfftw3 -lfftw3_threads -lliquid -lcodec2
OBJ = hsmodem.o constellation.o crc16.o frame_packer.o main_helper.o scrambler.o speed.o fec.o audio.o udp.o fft.o liquid_if.o symboltracker.o audio_voice.o voiceprocessor.o codec2.o

default: $(OBJ)
	g++ $(CXXFLAGS) -o ../LinuxRelease/hsmodem $(OBJ) $(LDFLAGS)

clean:
	rm -f *.o

