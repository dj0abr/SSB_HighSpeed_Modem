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

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.IO;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace oscardata
{
    public partial class Form1 : Form
    {
        Imagehandler ih = new Imagehandler();
        int txcommand = 0;      // commands what to send
        Byte frameinfo = (Byte)statics.FirstFrame;
        String TXfilename;
        int rxbytecounter = 0;
        DateTime starttime;
        String old_tsip = "";
        bool modemrunning = false;

        public Form1()
        {
            // init GUI
            InitializeComponent();

            // test OS type
            OperatingSystem osversion = System.Environment.OSVersion;
            statics.OSversion = osversion.Platform.ToString();
            if (osversion.VersionString.Contains("indow"))
            {
                statics.ostype = 0;
                tb_shutdown.Visible = false;
                bt_shutdown.Visible = false;
            }
            else
                statics.ostype = 1; // Linux

            // start hsmodem (.exe)
            modemrunning = statics.StartHSmodem();

            // set temp paths
            statics.zip_TXtempfilename = statics.addTmpPath(statics.zip_TXtempfilename);
            statics.zip_RXtempfilename = statics.addTmpPath(statics.zip_RXtempfilename);
            statics.jpg_tempfilename = statics.addTmpPath(statics.jpg_tempfilename);

            load_Setup();

            checkBox_small_CheckedChanged(null, null);

            // init speed
            comboBox1_SelectedIndexChanged(null,null);

            // create Udp Communication ports and init UDP system
            Udp.InitUdp();
            search_modem();
            ArraySend.ArraySendInit();

            // enable processing
            timer_udpTX.Enabled = true;
            timer_udprx.Enabled = true;
            timer_searchmodem.Enabled = true;

            //pictureBox_rximage.BackgroundImage = Image.FromFile("/tmp/temp293.jpg");
        }

        // TX timer
        int loopdelay = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            // BER testdata
            if (txcommand == statics.BERtest)
            {
                if (Udp.GetBufferCount() < 2)
                {
                    Byte[] txdata = new byte[statics.PayloadLen + 2];

                    txdata[0] = (Byte)statics.BERtest; // BER Test Marker
                    txdata[1] = frameinfo;

                    Byte tb = (Byte)'A';
                    for (int i = 2; i < txdata.Length; i++)
                    {
                        txdata[i] = tb;
                        tb++;
                        if (tb == 'z') tb = (Byte)'A';
                    }

                    // and transmit it
                    Udp.UdpSendData(txdata);

                    frameinfo = (Byte)statics.NextFrame;
                }
            }

            if (ArraySend.getSending())
            {
                button_loadimage.Enabled = false;
                button_sendimage.Enabled = false;
            }
            else
            {
                button_loadimage.Enabled = true;
                if (TXimagefilename != "")
                    button_sendimage.Enabled = true;
                else
                    button_sendimage.Enabled = false;
            }

            if (TXfoldername == "" || lastFullName == "")
                cb_loop.Enabled = false;
            else
                cb_loop.Enabled = true;

            ShowTXstatus();

            if (txcommand == statics.Image)
            {
                // if "loop" is selected send the next image in folder
                if (cb_loop.Checked)
                {
                    // check if we are ready with any transmission
                    if (ArraySend.getSending() == false)
                    {
                        // this timer runs with 10ms
                        // after an image was finished, wait before starting the new one
                        // this helps cleaning any buffer
                        int spacetime = 20000;  // ms
                        label_nextimage.Text = "next image in " + ((spacetime / timer_udpTX.Interval - loopdelay) / 10).ToString() + " s";
                        if (++loopdelay > (spacetime / timer_udpTX.Interval))
                        {
                            // start sending a new picture
                            startNextImage();
                        }
                    }
                    else
                    {
                        loopdelay = 0;
                        label_nextimage.Text = "transmitting";
                    }
                }
                else
                    label_nextimage.Text = "";
            }
            else
                label_nextimage.Text = "";

            if (ts_ip.Text.Contains("?") || ts_ip.Text.Contains("1.2.3.4") || old_tsip != statics.ModemIP)
            {
                if (statics.ModemIP == "1.2.3.4")
                {
                    ts_ip.Text = "Modem-IP: ?";
                    ts_ip.ForeColor = Color.Red;
                }
                else
                {
                    ts_ip.Text = "Modem-IP: " + statics.ModemIP;
                    ts_ip.ForeColor = Color.Black;
                    old_tsip = statics.ModemIP;
                    comboBox1_SelectedIndexChanged(null, null); // send speed to modem
                }
            }

            if (statics.GotAudioDevices == 1)
            {
                statics.GotAudioDevices = 2;
                // populate combo boxes
                foreach (String s in statics.AudioPBdevs)
                {
                    if(s.Length > 1)
                        cb_audioPB.Items.Add(s);
                }
                foreach (String s in statics.AudioCAPdevs)
                {
                    if (s.Length > 1)
                        cb_audioCAP.Items.Add(s);
                }
            }


            if (setPBvolume >= 0)
            {
                Byte[] txdata = new byte[2];
                txdata[0] = (Byte)statics.SetPBvolume;
                txdata[1] = (Byte)setPBvolume;
                Udp.UdpSendCtrl(txdata);
                setPBvolume = -1;
            }

            if (setCAPvolume != -1)
            {
                Byte[] txdata = new byte[2];
                txdata[0] = (Byte)statics.SetCAPvolume;
                txdata[1] = (Byte)setCAPvolume;
                Udp.UdpSendCtrl(txdata);
                setCAPvolume = -1;
            }

            /*if(modemrunning == false)
            {
                // start hsmodem (.exe)
                modemrunning = statics.StartHSmodem();
            }*/
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            save_Setup();
            statics.killall("hsmodem");
            statics.killall("hsmodem.exe");
            // exit the threads
            statics.running = false;
            Udp.Close();
        }

        // RX timer
        int rxstat = 0;
        int speed;
        int tmpnum = 0;
        int file_lostframes = 0;
        int last_fileid = 0;
        private void timer_udprx_Tick(object sender, EventArgs e)
        {
            while (true)
            {
                Byte[] rxd = Udp.UdpReceive();
                if (rxd == null) break;

                // these status information are added by the unpack routine
                int rxtype = rxd[0];
                int rxfrmnum = rxd[1];
                rxfrmnum <<= 8;
                rxfrmnum += rxd[2];
                int minfo = rxd[3];
                rxstat = rxd[4];
                speed = rxd[5];
                speed <<= 8;
                speed += rxd[6];
                int dummy3 = rxd[7];
                int dummy4 = rxd[8];
                int dummy5 = rxd[9];

                if (rxstat == 4)
                {
                    framelost++;
                    file_lostframes++;
                }
                calcBer(rxfrmnum);

                if (minfo == statics.FirstFrame)
                    file_lostframes = 0;

                    Byte[] rxdata = new byte[rxd.Length - 10];
                Array.Copy(rxd, 10, rxdata, 0, rxd.Length - 10);

                //Console.WriteLine("minfo:" + minfo + " data:" + rxdata[0].ToString("X2") + " " + rxdata[1].ToString("X2"));

                if (minfo == statics.FirstFrame)
                {
                    rxbytecounter = rxdata.Length;
                    starttime = DateTime.UtcNow;
                }
                else
                {
                    rxbytecounter += rxdata.Length;
                }
                TimeSpan ts = DateTime.UtcNow - starttime;
                ts += new TimeSpan(0, 0, 0, 1);

                // ===== ASCII RX ================================================
                if (rxtype == statics.AsciiFile)
                {
                    // if this is the first frame of a file transfer
                    // then read and remove the file info header
                    if (minfo == statics.FirstFrame || minfo == statics.SingleFrame)
                    {
                        //Console.WriteLine("first, single");
                        rxdata = ArraySend.GetAndRemoveHeader(rxdata);
                        if (rxdata == null) return;
                        if (last_fileid == ArraySend.FileID) return;    // got first frame for this ID already
                        last_fileid = ArraySend.FileID;
                    }
                    else
                        last_fileid = 0;

                    // collect all received data into zip_RXtempfilename
                    Byte[] ba = null;
                    Byte[] nba;
                    try
                    {
                        ba = File.ReadAllBytes(statics.zip_RXtempfilename);
                    }
                    catch { }
                    
                    if (ba != null)
                    {
                        //Console.WriteLine("write next");
                        nba = new Byte[ba.Length + rxdata.Length];
                        Array.Copy(ba, nba, ba.Length);
                        Array.Copy(rxdata, 0, nba, ba.Length, rxdata.Length);
                    }
                    else
                    {
                        //Console.WriteLine("write first");
                        nba = new Byte[rxdata.Length];
                        Array.Copy(rxdata, nba, rxdata.Length);
                    }
                    File.WriteAllBytes(statics.zip_RXtempfilename, nba);
                    long filesize = 0;

                    // check if transmission is finished
                    if (minfo == statics.LastFrame || minfo == statics.SingleFrame)
                    {
                        // statics.zip_RXtempfilename has the received data, but maybee too long (multiple of payload length)
                        // reduce for the real file length
                        Byte[] fc = File.ReadAllBytes(statics.zip_RXtempfilename);
                        Byte[] fdst = new byte[ArraySend.FileSize];
                        if(fc.Length < ArraySend.FileSize)
                        {
                            Console.WriteLine("len=" + fc.Length + " fz=" + ArraySend.FileSize);
                            return;
                        }
                        Array.Copy(fc, 0, fdst, 0, ArraySend.FileSize);
                        File.WriteAllBytes(statics.zip_RXtempfilename, fdst);

                        //Console.WriteLine("size:"+ ArraySend.FileSize.ToString());

                        //Console.WriteLine("last");
                        // unzip received data and store result in file: unzipped_RXtempfilename
                        rtb_RXfile.Text = "";
                        ZipStorer zs = new ZipStorer();
                        String fl = zs.unzipFile(statics.zip_RXtempfilename);
                        if (fl != null)
                        {
                            // save file
                            int idx = fl.LastIndexOf('/');
                            if (idx == -1) idx = fl.LastIndexOf('\\');
                            String fdest = fl.Substring(idx + 1);
                            fdest = statics.getHomePath("", fdest);
                            try { File.Delete(fdest); } catch { }
                            File.Move(fl, fdest);
                            filesize = statics.GetFileSize(fdest);

                            String serg = File.ReadAllText(fdest);
                            printText(rtb_RXfile, serg);
                        }
                        else
                            printText(rtb_RXfile, "unzip failed");
                        File.Delete(statics.zip_RXtempfilename);
                    }

                    int rest = ArraySend.FileSize - rxbytecounter;
                    if (rest < 0) rest = 0;
                    if (rest > 0)
                        label_rxfile.Text = "RX file: " + ArraySend.rxFilename + " " + rest.ToString() + " bytes";
                    else
                        label_rxfile.Text = "RX file: " + ArraySend.rxFilename + " " + filesize + " bytes";

                    if (minfo == statics.LastFrame)
                        ShowStatus((int)filesize, (int)ts.TotalSeconds);
                    else
                        ShowStatus(rxbytecounter, (int)ts.TotalSeconds);
                }

                // ===== HTML File RX ================================================
                if (rxtype == statics.HTMLFile)
                {
                    // if this is the first frame of a file transfer
                    // then read and remove the file info header
                    if (minfo == statics.FirstFrame)
                    {
                        rxdata = ArraySend.GetAndRemoveHeader(rxdata);
                        if (last_fileid == ArraySend.FileID) return;    // got first frame for this ID already
                        last_fileid = ArraySend.FileID;
                    }
                    else
                        last_fileid = 0;

                    Byte[] ba = null;
                    Byte[] nba;
                    try
                    {
                        ba = File.ReadAllBytes(statics.zip_RXtempfilename);
                    }
                    catch { }

                    if (ba != null)
                    {
                        nba = new Byte[ba.Length + rxdata.Length];
                        Array.Copy(ba, nba, ba.Length);
                        Array.Copy(rxdata, 0, nba, ba.Length, rxdata.Length);
                    }
                    else
                    {
                        nba = new Byte[rxdata.Length];
                        Array.Copy(rxdata, nba, rxdata.Length);
                    }
                    File.WriteAllBytes(statics.zip_RXtempfilename, nba);
                    long filesize = 0;
                    if (minfo == statics.LastFrame)
                    {
                        // unzip received data
                        rtb_RXfile.Text = "";
                        ZipStorer zs = new ZipStorer();
                        // unzip returns filename+path of unzipped file
                        String fl = zs.unzipFile(statics.zip_RXtempfilename);
                        if (fl != null)
                        {
                            // save file
                            int idx = fl.LastIndexOf('/');
                            if (idx == -1) idx = fl.LastIndexOf('\\');
                            String fdest = fl.Substring(idx + 1);
                            fdest = statics.getHomePath("", fdest);
                            try { File.Delete(fdest); } catch { }
                            File.Move(fl, fdest);
                            filesize = statics.GetFileSize(fdest);

                            rxbytecounter = (int)statics.GetFileSize(fdest);
                            String serg = File.ReadAllText(fdest);
                            printText(rtb_RXfile, serg);
                            try
                            {
                                OpenUrl(fdest);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                        else
                            printText(rtb_RXfile, "unzip failed");
                    }

                    int rest = ArraySend.FileSize - rxbytecounter;
                    if (rest < 0) rest = 0;
                    if (rest > 0)
                        label_rxfile.Text = "RX file: " + ArraySend.rxFilename + " " + rest.ToString() + " bytes";
                    else
                        label_rxfile.Text = "RX file: " + ArraySend.rxFilename + " " + filesize + " bytes";

                    if (minfo == statics.LastFrame)
                        ShowStatus(ArraySend.FileSize, (int)ts.TotalSeconds);
                    else
                        ShowStatus(rxbytecounter, (int)ts.TotalSeconds);
                }

                // ===== Binary File RX ================================================
                if (rxtype == statics.BinaryFile)
                {
                    // if this is the first frame of a file transfer
                    // then read and remove the file info header
                    if (minfo == statics.FirstFrame || minfo == statics.SingleFrame)
                    {
                        //Console.WriteLine("first, single");
                        rxdata = ArraySend.GetAndRemoveHeader(rxdata);
                        if (last_fileid == ArraySend.FileID) return;    // got first frame for this ID already
                        last_fileid = ArraySend.FileID;
                    }
                    else
                        last_fileid = 0;

                    // collect all received data into zip_RXtempfilename
                    Byte[] ba = null;
                    Byte[] nba;
                    try
                    {
                        ba = File.ReadAllBytes(statics.zip_RXtempfilename);
                    }
                    catch { }

                    if (ba != null)
                    {
                        //Console.WriteLine("write next");
                        nba = new Byte[ba.Length + rxdata.Length];
                        Array.Copy(ba, nba, ba.Length);
                        Array.Copy(rxdata, 0, nba, ba.Length, rxdata.Length);
                    }
                    else
                    {
                        //Console.WriteLine("write first");
                        nba = new Byte[rxdata.Length];
                        Array.Copy(rxdata, nba, rxdata.Length);
                    }
                    File.WriteAllBytes(statics.zip_RXtempfilename, nba);
                    long filesize = 0;

                    // check if transmission is finished
                    if (minfo == statics.LastFrame || minfo == statics.SingleFrame)
                    {
                        // statics.zip_RXtempfilename has the received data, but maybee too long (multiple of payload length)
                        // reduce for the real file length
                        Byte[] fc = File.ReadAllBytes(statics.zip_RXtempfilename);
                        Byte[] fdst = new byte[ArraySend.FileSize];
                        if (fc.Length < ArraySend.FileSize)
                        {
                            Console.WriteLine("len=" + fc.Length + " fz=" + ArraySend.FileSize);
                            return;
                        }
                        Console.WriteLine("copy final binary file");
                        Array.Copy(fc, 0, fdst, 0, ArraySend.FileSize);
                        File.WriteAllBytes(statics.zip_RXtempfilename, fdst);

                        //Console.WriteLine("last");
                        // unzip received data and store result in file: unzipped_RXtempfilename
                        rtb_RXfile.Text = "";
                        ZipStorer zs = new ZipStorer();
                        String fl = zs.unzipFile(statics.zip_RXtempfilename);
                        if (fl != null)
                        {
                            int idx = fl.LastIndexOf('/');
                            if(idx == -1) idx = fl.LastIndexOf('\\');
                            String fdest = fl.Substring(idx + 1);
                            fdest = statics.getHomePath("", fdest);
                            try { File.Delete(fdest); } catch { }
                            File.Move(fl, fdest);
                            filesize = statics.GetFileSize(fdest);
                            //File.WriteAllBytes(fl, nba);
                            printText(rtb_RXfile, "binary file received\r\n");
                            printText(rtb_RXfile, "--------------------\r\n\r\n");
                            printText(rtb_RXfile, "file size         : " + filesize + " byte\r\n\r\n");
                            printText(rtb_RXfile, "stored in         : " + fdest + "\r\n\r\n");
                            printText(rtb_RXfile, "transmission time : " + ((int)ts.TotalSeconds).ToString() + " seconds" + "\r\n\r\n");
                            printText(rtb_RXfile, "transmission speed: " + ((int)(filesize*8/ts.TotalSeconds)).ToString() + " bit/s" + "\r\n\r\n");
                        }
                        else
                            printText(rtb_RXfile, "unzip failed");
                        File.Delete(statics.zip_RXtempfilename);
                    }

                    int rest = ArraySend.FileSize - rxbytecounter;
                    if (rest < 0) rest = 0;
                    if (rest > 0)
                        label_rxfile.Text = "RX file: " + ArraySend.rxFilename + " " + rest.ToString() + " bytes";
                    else
                        label_rxfile.Text = "RX file: " + ArraySend.rxFilename + " " + filesize + " bytes";

                    if (minfo == statics.LastFrame)
                        ShowStatus((int)filesize, (int)ts.TotalSeconds);
                    else
                        ShowStatus(rxbytecounter, (int)ts.TotalSeconds);
                }

                // ===== IMAGE RX ================================================
                if (rxtype == statics.Image)
                {
                    // if this is the first frame of a file transfer
                    // then read and remove the file info header
                    if (minfo == statics.FirstFrame)
                    {
                        rxdata = ArraySend.GetAndRemoveHeader(rxdata);
                        if (rxdata == null) return;
                    }

                    ih.receive_image(rxdata, minfo);

                    // show currect contents of rxtemp.jpg in RX picturebox
                    try
                    {
                        String fn = statics.addTmpPath("temp" + tmpnum.ToString() + ".jpg");
                        try
                        {
                            File.Delete(fn);
                        }
                        catch { }
                        tmpnum++;
                        fn = statics.addTmpPath("temp" + tmpnum.ToString() + ".jpg");
                        File.Copy(statics.jpg_tempfilename, fn);

                        try
                        {
                            if(statics.GetFileSize(fn) > 1200)
                                pictureBox_rximage.BackgroundImage = Image.FromFile(fn);
                        }
                        catch {
                        }

                        if (minfo == statics.LastFrame)
                        {
                            // file is complete, save in RX storage
                            // remove possible path from filename
                            String fname = ArraySend.rxFilename;
                            int idx = fname.IndexOfAny(new char[] {'\\','/' });
                            if (idx != -1)
                            {
                                try
                                {
                                    fname = fname.Substring(idx + 1);
                                } catch{ }
                            }
                            if (!cb_savegoodfiles.Checked || (file_lostframes == 0 && cb_savegoodfiles.Checked))
                            {
                                // add home path and RXstorage path
                                String fnx = statics.getHomePath(statics.RXimageStorage, fname);
                                File.Copy(fn, fnx);
                            }
                        }
                    }
                    catch { }

                    int rest = ArraySend.FileSize - rxbytecounter;
                    if (rest < 0) rest = 0;
                    if(rest > 0)
                        label_rximage.Text = "RX image: " + ArraySend.rxFilename + " " + rest.ToString() + " bytes";
                    else
                        label_rximage.Text = "RX image: " + ArraySend.rxFilename;
                    ShowStatus(rxbytecounter, (int)ts.TotalSeconds);
                }

                // ===== BER Test ================================================
                if (rxtype == statics.BERtest)
                {
                    RXstatus.Text = "BER: " + ber.ToString("E3") + " " + rxframecounter.ToString() + " frames received OK";

                    BERcheck(rxdata);
                }
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (statics.ostype == 0)
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else
                {
                    Process.Start("xdg-open", url);
                }
            }
        }

        private void timer_qpsk_Tick(object sender, EventArgs e)
        {
            if(Udp.IQavail())
                panel_constel.Invalidate();

            panel_txspectrum.Invalidate();

            progressBar_fifo.Value = statics.PBfifousage;
        }

        private void panel_constel_Paint(object sender, PaintEventArgs e)
        {
            Bitmap bm = Udp.UdpBitmap();
            if (bm != null)
            {
                Pen pen = new Pen(Brushes.LightGray);
                e.Graphics.DrawEllipse(pen, 0, 0, panel_constel.Size.Width - 1, panel_constel.Size.Height - 1);
                e.Graphics.DrawLine(pen, panel_constel.Size.Width / 2, 0, panel_constel.Size.Width / 2, panel_constel.Size.Height);
                e.Graphics.DrawLine(pen, 0, panel_constel.Size.Height / 2, panel_constel.Size.Width, panel_constel.Size.Height / 2);

                e.Graphics.DrawImage(bm, 0, 0);
                bm.Dispose();
            }
        }

        static Brush brred = new SolidBrush(Color.FromArgb(255, (byte)255, (byte)220, (byte)220));
        static Brush brgreen = new SolidBrush(Color.FromArgb(255, (byte)240, (byte)255, (byte)240));
        static Brush brgray = new SolidBrush(Color.FromArgb(255, (byte)220, (byte)220, (byte)220));
        static Pen pen = new Pen(Brushes.Black);
        static Pen penblue = new Pen(Brushes.Blue, 2);
        static Pen pengrey = new Pen(brgray, 1);
        Font fnt = new Font("Verdana", 8.0f);
        Font smallfnt = new Font("Verdana", 6.0f);

        private void panel_txspectrum_Paint(object sender, PaintEventArgs e)
        {
            int miny = 200;
            int maxy = 2800;

            // horizontal level markers
            Point ps = GetFFTPos(0, 0);
            Point pe = GetFFTPos(maxxval, maxyval);
            int pw = pe.X - ps.X;
            int ph = ps.Y - pe.Y;
            e.Graphics.FillRectangle(brred, ps.X, pe.Y, pw, ph);

            ps = GetFFTPos(miny/10, 700);
            pe = GetFFTPos(maxy/10, 2300);
            pw = pe.X - ps.X;
            ph = ps.Y - pe.Y;
            e.Graphics.FillRectangle(brgreen, ps.X, pe.Y, pw, ph);

            // Coordinates
            e.Graphics.DrawLine(pen, GetFFTPos(0, 0), GetFFTPos(maxxval, 0));
            e.Graphics.DrawLine(pen, GetFFTPos(0, 0), GetFFTPos(0, maxyval));

            // vertical frequency markers for 2.7kHz
            for (int i = miny; i <= maxy; i+=100)
            {
                e.Graphics.DrawLine(pengrey, GetFFTPos(i / 10, 0), GetFFTPos(i / 10, maxyval));
            }

            // Title
            e.Graphics.DrawString("Tuning Window", fnt, Brushes.Black, GetFFTPos(110, 3000));
            e.Graphics.DrawString(miny.ToString() + " Hz", smallfnt, Brushes.Black, GetFFTPos(5, 2800));
            e.Graphics.DrawString("1500 Hz", smallfnt, Brushes.Black, GetFFTPos(138, 680));
            e.Graphics.DrawString(maxy.ToString() + " Hz", smallfnt, Brushes.Black, GetFFTPos(270, 2800));

            e.Graphics.DrawString("min Level", smallfnt, Brushes.Black, GetFFTPos(290, 1000));
            e.Graphics.DrawString("max", smallfnt, Brushes.Black, GetFFTPos(290, 2450));

            while (true)
            {
                UInt16[] da = Udp.UdpGetFFT();
                if (da == null) break;
                if (da.Length < maxxval) return;
                Fftmean(da);
            }

            // da are the FFT data
            // from 0 Hz to 4410 Hz with a resolution of 10 Hz
            // so we get 441 values
            // there may be 442, just ignore the last one
            GraphicsPath gp = new GraphicsPath();

            // calculate mean value and calc mean value over all values
            UInt16[] su = new UInt16[maxxval+1];
            for (int i = 0; i < maxxval; i++)
            {
                su[i] = 0;
                for(int j=0; j< meansize; j++)
                    su[i] += dam[j, i];
                su[i] /= (UInt16)meansize;
            }

            // scale and X-mean
            int lastu = 0;
            for (int i = 0; i < maxxval; i++)
            {
                UInt16 u = 0;
                if (i >= 1 && i < maxxval - 1)
                    u = (UInt16)((su[i - 1] + su[i] + su[i + 1]) / 3);
                else
                    u = su[i];

                u *= 3;
                gp.AddLine(GetFFTPos(i, lastu), GetFFTPos(i + 1, u));
                lastu = u;
            }

            e.Graphics.DrawPath(penblue, gp);
        }

        private UInt16[,] dam = new UInt16[meansize, maxxval];

        private void Fftmean(UInt16[] v)
        {
            for (int sh = meansize - 1; sh > 0; sh--)
                for (int i = 0; i < maxxval; i++)
                    dam[sh, i] = dam[sh - 1, i];

            for (int i = 0; i < maxxval; i++)
                dam[0, i] = v[i];
        }

        readonly static int meansize = 20;
        readonly static int maxxval = (statics.real_datarate / 10) * 6 / 10;
        readonly int maxyval = 3000;

        private Point GetFFTPos(int x, int y)
        {
            int leftMargin = 2;
            int rightMargin = 2;
            int topMargin = 2;
            int bottomMargin =2;

            int xsize = panel_txspectrum.Size.Width;
            int newx = (x * (xsize - leftMargin - rightMargin)) / maxxval;
            newx += leftMargin;

            int ysize = panel_txspectrum.Size.Height;
            int newy = (y * (ysize - topMargin - bottomMargin)) / maxyval;
            newy += bottomMargin;
            newy = ysize - newy;

            Point p = new Point(newx, newy);
            return p;
        }

        void printText(RichTextBox rtb, String s)
        {
            AppendTextOnce(rtb, new Font("Courier New", (float)8), Color.Blue, Color.White, s);
        }

        void AppendTextOnce(RichTextBox rtb, Font selfont, Color color, Color bcolor, string text)
        {
            try
            {
                if (text.Contains("\n"))
                {
                    char[] ca = new char[] { '\n', '\r' };

                    text = text.Trim(ca);
                    text += "\n";
                }

                // max. xxx Zeilen, wenn mehr dann lösche älteste
                if (rtb.Lines.Length > 200)
                {
                    rtb.SelectionStart = 0;
                    rtb.SelectionLength = rtb.Text.IndexOf("\n", 0) + 1;
                    rtb.SelectedText = "";
                }

                int start = rtb.TextLength;
                rtb.AppendText(text);
                int end = rtb.TextLength;

                // Textbox may transform chars, so (end-start) != text.Length
                rtb.Select(start, end - start);
                rtb.SelectionColor = color;
                rtb.SelectionFont = selfont;
                rtb.SelectionBackColor = bcolor;
                rtb.Select(end, 0);

                rtb.ScrollToCaret();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        String TXimagefilename = "";
        String TXRealFilename = "";
        long TXRealFileSize = 0;
        String TXfoldername = "";
        String lastFullName = "";

        // prepare an image file for transmission
        void prepareImage(String fullfn)
        {
            if (statics.checkImage(fullfn) == false) return;

            // all images are converted to jpg, make the new filename
            TXfoldername = statics.purePath(fullfn);
            TXRealFilename = statics.pureFilename(fullfn);
            TXRealFilename = statics.AddReplaceFileExtension(TXRealFilename,"jpg");
            lastFullName = fullfn;

            // random filename for picturebox control (picturebox cannot reload image from actual filename)
            try { File.Delete(TXimagefilename); } catch { }
            Random randNum = new Random();
            TXimagefilename = statics.addTmpPath("tempTX" + randNum.Next(0, 65000).ToString() + ".jpg");

            // get the quality selected by the user
            String qual = comboBox_quality.Text;
            long max_size = 22500;
            if (qual.Contains("30s")) max_size = 12000;
            if (qual.Contains("2min")) max_size = 45000;
            if (qual.Contains("4min")) max_size = 90000;

            // resize image and save it according the quality settings
            Image img = new Bitmap(fullfn);
            String cs = tb_callsign.Text;
            if (cb_stampcall.Checked == false) cs = "";
            if (!checkBox_big.Checked)
            {
                img = ih.ResizeImage(img, 320, 240, cs);
                // set quality by reducing the file size and save under default name
                ih.SaveJpgAtFileSize(img, TXimagefilename, max_size / 2);
            }
            else
            {
                img = ih.ResizeImage(img, 640, 480, cs);
                // set quality by reducing the file size and save under default name
                ih.SaveJpgAtFileSize(img, TXimagefilename, max_size);
            }
            pictureBox_tximage.Load(TXimagefilename);
            TXRealFileSize = statics.GetFileSize(TXimagefilename);
            ShowTXstatus();
            txcommand = statics.Image;
        }

        void ShowTXstatus()
        {
            if(txcommand == statics.Image)
                label_tximage.Text = "TX image: " + TXRealFilename + ". Sent: " + (ArraySend.txpos / 1000).ToString() + " of " + (TXRealFileSize / 1000).ToString() + " kB";
            else
                label_txfile.Text = "TX file: " + TXRealFilename + ". Sent: " + (ArraySend.txpos / 1000).ToString() + " of " + (TXRealFileSize / 1000).ToString() + " kB";
        }

        // in loop mode only: send the next picture in current image folder
        void startNextImage()
        {
            if (TXfoldername == "" || lastFullName == "") return;

            // read all file from folder
            String[] files = Directory.GetFiles(TXfoldername);
            Array.Sort(files);
            int i;
            bool found = false;
            for(i=0; i<files.Length; i++)
            {
                // look for the last transmitted file
                if(files[i] == lastFullName)
                {
                    // choose the next file
                    if (++i == files.Length) i = 0;
                    // check if the file is a valid image
                    try
                    {
                        Image dummy = new Bitmap(files[i]);
                        dummy.Dispose();
                        found = true;
                        break;
                    }
                    catch
                    {
                        lastFullName = files[i];
                    }
                }
            }
            if (!found) return;
            prepareImage(files[i]);
            button_sendimage_Click(null, null);
        }

        private void button_loadimage_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.png; *.gif; *.bmp)|*.jpg; *.jpeg; *.png; *.gif; *.bmp";
            if (open.ShowDialog() == DialogResult.OK)
            {
                prepareImage(open.FileName);
            }
        }

        private void button_sendimage_Click(object sender, EventArgs e)
        {
            txcommand = statics.Image;
            rxbytecounter = 0;
            pictureBox_rximage.Image = null;
            Byte[] imgarr = File.ReadAllBytes(TXimagefilename); // compress temp file name
            ArraySend.Send(imgarr, statics.Image, TXimagefilename, TXRealFilename);    // compress temp file name and real file name
        }

        private void button_startBERtest_Click(object sender, EventArgs e)
        {
            ber = 0;
            framelost = 0;
            totallostframes = 0;
            last_rxfrmnum = -1;
            rtb.Text = "";
            frameinfo = (Byte)statics.FirstFrame;
            txcommand = statics.BERtest;
        }

        private void button_stopBERtest_Click(object sender, EventArgs e)
        {
            txcommand = statics.noTX;
        }

        DateTime dt = DateTime.Now;
        int rxframecounter = 0;
        int framelost = 0;
        int last_rxfrmnum = -1;
        double ber = 0;
        int totallostframes = 0;

        void calcBer(int rxfrmnum)
        {
            if (last_rxfrmnum == -1)
            {
                last_rxfrmnum = rxfrmnum;
                return;
            }

            // calc gap
            int gap = ((rxfrmnum+1024) - last_rxfrmnum) % 1024;
            rxframecounter += gap;
            totallostframes += (gap - 1);

            int totalbits = rxframecounter * 258 * 8;
            int errorbits = totallostframes * 258 * 8;
            ber = (double)totallostframes / (double)rxframecounter;

            last_rxfrmnum = rxfrmnum;
        }

        private void BERcheck(Byte[] rxdata)
        {
            String line =   "RX: " + rxframecounter.ToString().PadLeft(6, ' ') + " ";

            // print payload (must be printable chars)
            line += Encoding.UTF8.GetString(rxdata).Substring(0, 50) + " ...";

            // show RX status for this frame
            if (rxstat == 4)
                line += " frame lost";
            else
                line += " sequence OK";

            int bits = rxframecounter * 258 * 8;
            int bytes = rxframecounter * 258;
            String sbit = "b";
            String sbyt = "B";

            if (bits > 1000)
            {
                bits /= 1000;
                sbit = "kb";
            }
            if (bits > 1000)
            {
                bits /= 1000;
                sbit = "Mb";
            }

            if (bytes > 1000)
            {
                bytes /= 1000;
                sbyt = "kB";
            }
            if (bytes > 1000)
            {
                bytes /= 1000;
                sbyt = "MB";
            }

            line += " " + bits.ToString() + " " + sbit + "  " + bytes.ToString() + " " + sbyt;

            line += " BER: " + string.Format("{0:#.##E+0}", ber);  // ber.ToString("E3");

            line += "\r\n";
            printText(rtb,line);

            int fl = framelost;
            if (fl <= 1) fl = 0;
            String s = "Speed: " + speed.ToString() + " bit/s,  Lost Frames: " + fl.ToString();
            toolStripStatusLabel.Text = s;
        }

        private void ShowStatus(int rxbytecounter, int totalseconds)
        {
            int fl = framelost;
            if (fl <= 1) fl = 0;
            String s = "Speed: " + speed.ToString() + " bit/s,  Lost Frames: " + fl.ToString();
            toolStripStatusLabel.Text = s;

            int rspeed = 0;
            if (totalseconds >= 1)
                rspeed = rxbytecounter * 8 / totalseconds;
            RXstatus.Text = "received " + rxbytecounter + " byte " + totalseconds + " s, " + rspeed + " bit/s";
        }

        private void button_cancelimg_Click(object sender, EventArgs e)
        {
            //txcommand = statics.noTX;    // finished
            label_rximage.ForeColor = Color.Black;
            pictureBox_rximage.Image = null;
            ArraySend.stopSending();
        }

        private void checkBox_small_CheckedChanged(object sender, EventArgs e)
        {
            // scale all elements
            // this is required if a scaled screen resolution is used für large 4k monitor, important under mono
            // since mono fails in automatic scaling if the screen resolution is different from 1:1
            label_rximage.Location = new Point(6, 7);
            label_rximage.Location = new Point(650, 7);
            pictureBox_tximage.Size = new Size(640,480);
            pictureBox_rximage.Size = new Size(640,480);
            int yPicBoxes = label_rximage.Location.Y + label_rximage.Size.Height + 3;
            pictureBox_tximage.Location = new Point(1, yPicBoxes);
            pictureBox_rximage.Location = new Point(642, yPicBoxes);

            int gb_yloc = pictureBox_tximage.Location.Y + pictureBox_tximage.Size.Height + 3;
            groupBox1.Location = new Point(3, gb_yloc);

            tabControl1.Size = new Size(pictureBox_tximage.Size.Width + pictureBox_rximage.Size.Width + 10, 
                                        label_rximage.Size.Height + 3 + pictureBox_rximage.Size.Height + 3 + groupBox1.Size.Height + 3 + 20);

            int rxpan_yloc = tabControl1.Location.Y + tabControl1.Size.Height + 3;
            panel_constel.Location = new Point(11, rxpan_yloc);
            panel_constel.Size = new Size(75,75);

            panel_txspectrum.Location = new Point(92, rxpan_yloc);
            panel_txspectrum.Size = new Size(441,75);

            rtb.Size = new Size(tabControl1.Size.Width - 30, tabControl1.Size.Height - button_startBERtest.Location.Y - button_startBERtest.Size.Height - 44);

            this.Size = new Size(tabControl1.Size.Width + 23, rxpan_yloc + panel_constel.Size.Height + statusStrip1.Size.Height + 42);

            int xf = bt_file_ascii.Location.X + bt_file_ascii.Size.Width + 20;
            int yf = bt_file_ascii.Location.Y;
            rtb_TXfile.Location = new Point(xf, yf);

            int mw = tabControl1.Size.Width - bt_file_ascii.Size.Width - 80;
            mw /= 2;
            int mh = tabControl1.Size.Height - bt_file_ascii.Size.Height - 50;
            rtb_TXfile.Size = new Size(mw, mh);

            xf += mw + 5;
            rtb_RXfile.Location = new Point(xf, yf);
            rtb_RXfile.Size = new Size(mw, mh);

            int ly = rtb_TXfile.Location.Y / 4;
            label_txfile.Location = new Point(rtb_TXfile.Location.X, ly);
            label_rxfile.Location = new Point(rtb_RXfile.Location.X, ly);

            label_speed.Location = new Point(panel_txspectrum.Location.X + panel_txspectrum.Size.Width + 20,panel_txspectrum.Location.Y+10);
            cb_speed.Location = new Point(label_speed.Location.X + label_speed.Size.Width + 10, label_speed.Location.Y-5);

            label_fifo.Location = new Point(label_speed.Location.X, label_speed.Location.Y + 35);
            progressBar_fifo.Location = new Point(cb_speed.Location.X, cb_speed.Location.Y + 35);
        }

        public String GetMyBroadcastIP()
        {
            String ip = "255.255.255.255";
            String[] myips = statics.getOwnIPs();
            //Console.WriteLine("BClen: " + myips.Length.ToString());
            // nur wenn der PC eine IP hat
            // hat er mehr, dann wissen wir nicht in welchem Netz wird broadcasten sollen, nehmen also die 255.255.255.255
            if (myips.Length == 1)
            {
                int idx = myips[0].LastIndexOf('.');
                if (idx >= 0)
                {
                    ip = myips[0].Substring(0, idx);
                    ip += ".255";
                    //Console.WriteLine("BCip: " + ip);
                }
            }
            return ip;
        }

        Byte getPBaudioDevice()
        {
            String s = cb_audioPB.Text;
            Byte x = (Byte)cb_audioPB.Items.IndexOf(s);
            //if (s.ToUpper() == "DEFAULT") x = 255;
            return x;
        }

        Byte getCAPaudioDevice()
        {
            String s = cb_audioCAP.Text;
            Byte x = (Byte)cb_audioCAP.Items.IndexOf(s);
            //if (s.ToUpper() == "DEFAULT") x = 255;
            return x;
        }

        /*
         * search for the modem IP:
         * send a search message via UDP to port UdpBCport
         * if a modem receives this message, it returns with an
         * UDP message to UdpBCport containing a String with it's IP address
         * this message also contains the selected Audio Devices
        */

        private void search_modem()
        {
            Byte[] txb = new byte[5];
            txb[0] = 0x3c;  // ID of this message
            txb[1] = getPBaudioDevice();
            txb[2] = getCAPaudioDevice();
            txb[3] = (Byte)tb_PBvol.Value;
            txb[4] = (Byte)tb_CAPvol.Value;

            Udp.UdpBCsend(txb, GetMyBroadcastIP(), statics.UdpBCport_AppToModem);

            Udp.searchtimeout++;
            if (Udp.searchtimeout >= 3)
                statics.ModemIP = "1.2.3.4";
        }

        private void bt_file_ascii_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Text Files(*.txt*; *.*)|*.txt; *.*";
            if (open.ShowDialog() == DialogResult.OK)
            {
                TXfilename = open.FileName;
                TXRealFilename = open.SafeFileName;
                String text = File.ReadAllText(TXfilename);
                rtb_TXfile.Text = text;
                txcommand = statics.AsciiFile;
                // compress file
                ZipStorer zs = new ZipStorer();
                zs.zipFile(statics.zip_TXtempfilename,open.SafeFileName,open.FileName);

                TXRealFileSize = statics.GetFileSize(statics.zip_TXtempfilename);
                ShowTXstatus();
            }
        }

        private void bt_file_send_Click(object sender, EventArgs e)
        {
            rtb_RXfile.Text = "";
            File.Delete(statics.zip_RXtempfilename);

            Byte[] textarr = File.ReadAllBytes(statics.zip_TXtempfilename);
            ArraySend.Send(textarr, (Byte)txcommand, TXfilename, TXRealFilename);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "HTML Files(*.html; *.htm; *.*)|*.html; *.htm; *.*";
            if (open.ShowDialog() == DialogResult.OK)
            {
                TXfilename = open.FileName;
                TXRealFilename = open.SafeFileName;
                String text = File.ReadAllText(TXfilename);
                rtb_TXfile.Text = text;
                txcommand = statics.HTMLFile;
                // compress file
                ZipStorer zs = new ZipStorer();
                zs.zipFile(statics.zip_TXtempfilename, open.SafeFileName, open.FileName);

                TXRealFileSize = statics.GetFileSize(statics.zip_TXtempfilename);
                ShowTXstatus();
            }
        }

        private void bt_sendBinaryFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "All Files(*.*)|*.*";
            if (open.ShowDialog() == DialogResult.OK)
            {
                TXfilename = open.FileName;
                TXRealFilename = open.SafeFileName;
                rtb_TXfile.Text = "Binary file " + TXfilename + " loaded";
                txcommand = statics.BinaryFile;
                // compress file
                ZipStorer zs = new ZipStorer();
                zs.zipFile(statics.zip_TXtempfilename, open.SafeFileName, open.FileName);

                TXRealFileSize = statics.GetFileSize(statics.zip_TXtempfilename);
                ShowTXstatus();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = cb_speed.SelectedIndex;
            int real_rate=4000;

            switch (idx)
            {
                case 0: real_rate = 3000; break;
                case 1: real_rate = 3150; break;
                case 2: real_rate = 3675; break;
                case 3: real_rate = 4000; break;
                case 4: real_rate = 4410; break;
                case 5: real_rate = 4800; break;
                case 6: real_rate = 5525; break;
                case 7: real_rate = 6000; break;
                case 8: real_rate = 6615; break;
                case 9: real_rate = 7200; break;
            }

            statics.setDatarate(real_rate);

            Byte[] txdata = new byte[statics.PayloadLen + 2];
            txdata[0] = (Byte)statics.ResamplingRate; // BER Test Marker
            txdata[1] = (Byte)idx;

            // and send info to modem
            Udp.UdpSendCtrl(txdata);

            //txcommand = statics.noTX;
            // stop any ongoing transmission
            button_cancelimg_Click(null, null);
        }

        
        private void timer_searchmodem_Tick(object sender, EventArgs e)
        {
            search_modem();
        }

        private void bt_rximages_Click(object sender, EventArgs e)
        {
            if (statics.ostype == 0)
            {
                try
                {
                    String s = "file://" + statics.getHomePath(statics.RXimageStorage, "");
                    Process.Start(s);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                try
                {
                    Process.Start("xdg-open", statics.getHomePath(statics.RXimageStorage, ""));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

        }

        private void bt_openrxfile_Click(object sender, EventArgs e)
        {
            if (statics.ostype == 0)
            {
                try
                {
                    String s = "file://" + statics.getHomePath("", "");
                    Process.Start(s);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                try
                {
                    Process.Start("xdg-open", statics.getHomePath("", ""));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private String ReadString(StreamReader sr)
        {
            String s = sr.ReadLine();
            if (s != null)
            {
                return s;
            }
            return " ";
        }

        private int ReadInt(StreamReader sr)
        {
            int v;

            try
            {
                String s = sr.ReadLine();
                if (s != null)
                {
                    v = Convert.ToInt32(s);
                    return v;
                }
            }
            catch { }
            return 0;
        }

        void load_Setup()
        {
            try
            {
                using (StreamReader sr = new StreamReader(statics.getHomePath("", "od.cfg")))
                {
                    tb_callsign.Text = ReadString(sr);
                    cb_speed.Text = ReadString(sr);
                    String s = ReadString(sr);
                    cb_stampcall.Checked = (s == "1");
                    s = ReadString(sr);
                    cb_savegoodfiles.Checked = (s == "1");
                    cb_audioPB.Text = ReadString(sr);
                    cb_audioCAP.Text = ReadString(sr);
                    tb_PBvol.Value = ReadInt(sr);
                    tb_CAPvol.Value = ReadInt(sr);
                }
            }
            catch
            {
                tb_callsign.Text = "";
                cb_speed.Text = "4000 QPSK BW: 2400 Hz (default QO-100)";
            }

            if (cb_audioPB.Text.Length <= 1) cb_audioPB.Text = "Default";
            if (cb_audioCAP.Text.Length <= 1) cb_audioCAP.Text = "Default";
        }

        void save_Setup()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(statics.getHomePath("", "od.cfg")))
                {
                    sw.WriteLine(tb_callsign.Text);
                    sw.WriteLine(cb_speed.Text);
                    sw.WriteLine(cb_stampcall.Checked?"1":"0");
                    sw.WriteLine(cb_savegoodfiles.Checked ? "1" : "0");
                    sw.WriteLine(cb_audioPB.Text);
                    sw.WriteLine(cb_audioCAP.Text);
                    sw.WriteLine(tb_PBvol.Value.ToString());
                    sw.WriteLine(tb_CAPvol.Value.ToString());
                }
            }
            catch { }
        }

        private void bt_shutdown_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Do you want to shut down the Modem-Computer ?", "Shut Down Modem", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                Byte[] txdata = new byte[1];
                txdata[0] = (Byte)statics.Modem_shutdown;
                Udp.UdpSendCtrl(txdata);

                MessageBox.Show("Please wait abt. 1 minute before powering OFF the modem", "Shut Down Modem", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        // TEST ONLY: tell modem to send a file
        private void button1_Click(object sender, EventArgs e)
        {
            Byte[] txdata = new byte[1];
            txdata[0] = (Byte)statics.AutosendFile;

            // and transmit it
            Udp.UdpSendCtrl(txdata);
        }

        private void bt_resetmodem_Click(object sender, EventArgs e)
        {
            Byte[] txdata = new byte[1];
            txdata[0] = (Byte)statics.ResetModem;

            // and transmit it
            Udp.UdpSendCtrl(txdata);
        }

        int setPBvolume = -1;
        int setCAPvolume = -1;

        private void tb_PBvol_Scroll(object sender, EventArgs e)
        {
            setPBvolume = tb_PBvol.Value;
        }

        private void tb_CAPvol_Scroll(object sender, EventArgs e)
        {
            setCAPvolume = tb_CAPvol.Value;
        }
    }
}
