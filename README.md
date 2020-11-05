# QO-100-modem
The purpose of this project is to transfer data (pictures...) via a 2,7kHz SSB channel on the narrow band transponder as fast as possible.

# this is work in progress
Version 0.2 is working on:
Windows 10 (should work on Win7, not tested)
linux Desktop PC, 
Odroid SBC
Raspberry 4 (3B+)

# Prerequisites
* Windopws 10 Desktop PC ... working
* LINUX Desktop PC ... working
* Raspberry PI 4 ... working
* Raspberry PI 3B+ ... working, but not 100% error free in fullduplex mode (RX only or TX only is working)
* Odroid N2 ... working
* Odroid C2 ... working
* Odroid C4 ... working

* Raspberry: Raspian OS ist NOT working, instead Ubuntu 64bit is required

* Application Software "oscardata.exe" running on Windows, Linux, (possibly MAC-OS, not tested)

# 3rd Party Libraries
this software uses these programs:
* Shifra Reed Solomon code: https://www.schifra.com/
* liquid-SDR: https://github.com/jgaeddert/liquid-dsp
* BASS Audio: https://www.un4seen.com/
* fftw3: http://www.fftw.org

# building the software
* Linux
1. go into the folder "hsmodem"
2. run "make"
3. the executable is in folder LinuxRelease
* Windows
1. load hsmodem.sln in VisualStudio-19 and build Release version.
2. the executable is in folder WinRelease

# starting the modem and application

you need to run 2 programs, the first one is "hsmodem" which runs in a termimal without GUI. This is the modem doing all modulation and demodulation staff.
The second program is the user interface "oscardata.exe".

1. go into the folder "WinRelease" or "LinuxRelease"
2. run the software:  ./hs100modem.exe or ./hsmodem
optional command line parameter:
no parameter ... normal usage
-m IP ... specify the V4 IP adress of the device where the application software is running. This is useful if you have more than one qo100modem running simultaneously. Without this parameter the app will search the modem automatically.

3. start the user application on any PC in your home network. It will find the modem automatically
The file is located in oscardata/oscardata/bin/Release
On windows just start oscardata.exe
On Linux start it with:  mono  oscardata.exe

# tested scenarious

* QO-100 via IC-9700, IC-7300 or IC-7100 ... working
* Short Wave 6m band via IC-7300, IC-7100 ... working. In case of significant noise, use the lowest bit rate (3000 bit/s)

# TODOs
the current version V0.2 runs very fine on Linux but shows a higher bit error rate on Windows. This has to do with the initialisation of the sound card. The default sound bitrate setting in the Windows-Sound-Settings implement some kind of "filtering". This is currently under evaluation.

# usage

In the IC-9700 activate the DATA mode and the RX filter FIL1 to full range of 3.6kHz.

In oscardata.exe go to the "BER" tab. Then click START. If you change the bitrate, wait a few seconds before starting again.

The program is now sending test data frames to the default sound card. If your sound card is properly connected to the transceiver then switch the transceiver to TX and the data will be sent to QO-100.
Receive your transmission, feed it to the default soundcard. As soon as oscardata.exe detects a correct data frame it will display status messages on the screen.

(For testing purposes you can just connect Line-Out of your soundcard with Line-IN with a cable.)

To assign the soundcard to the modem I recommend to use pavucontrol. Using the TX volume set a signal level of about 20 to 24 dB over noise floor. You will need about -10dB compared to the BPSK400 beacon. The received audio volume can be adjusted with help of the spectrum display in oscardata.exe-

Now as the transmission is OK, you can go to the "Image RX/TX" tab. First, select a picture quality then load a picture and finally press SEND to send it to QO-100. When you correctly receive your own transmission the RX picture will be displayed line by line.

vy 73, DJ0ABR

