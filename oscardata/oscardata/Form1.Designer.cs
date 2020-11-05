namespace oscardata
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.timer_udpTX = new System.Windows.Forms.Timer(this.components);
            this.timer_udprx = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ts_ip = new System.Windows.Forms.ToolStripStatusLabel();
            this.RXstatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel_constel = new System.Windows.Forms.Panel();
            this.timer_qpsk = new System.Windows.Forms.Timer(this.components);
            this.panel_txspectrum = new System.Windows.Forms.Panel();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.button_stopBERtest = new System.Windows.Forms.Button();
            this.button_startBERtest = new System.Windows.Forms.Button();
            this.rtb = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.Panel();
            this.label_nextimage = new System.Windows.Forms.Label();
            this.cb_loop = new System.Windows.Forms.CheckBox();
            this.bt_rximages = new System.Windows.Forms.Button();
            this.button_loadimage = new System.Windows.Forms.Button();
            this.comboBox_quality = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_big = new System.Windows.Forms.CheckBox();
            this.button_cancelimg = new System.Windows.Forms.Button();
            this.button_sendimage = new System.Windows.Forms.Button();
            this.label_rximage = new System.Windows.Forms.Label();
            this.label_tximage = new System.Windows.Forms.Label();
            this.pictureBox_rximage = new System.Windows.Forms.PictureBox();
            this.pictureBox_tximage = new System.Windows.Forms.PictureBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.button2 = new System.Windows.Forms.Button();
            this.bt_openrxfile = new System.Windows.Forms.Button();
            this.label_rxfile = new System.Windows.Forms.Label();
            this.label_txfile = new System.Windows.Forms.Label();
            this.rtb_RXfile = new System.Windows.Forms.RichTextBox();
            this.rtb_TXfile = new System.Windows.Forms.RichTextBox();
            this.bt_file_send = new System.Windows.Forms.Button();
            this.bt_sendBinaryFile = new System.Windows.Forms.Button();
            this.bt_file_html = new System.Windows.Forms.Button();
            this.bt_file_ascii = new System.Windows.Forms.Button();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cb_audioCAP = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cb_audioPB = new System.Windows.Forms.ComboBox();
            this.bt_resetmodem = new System.Windows.Forms.Button();
            this.tb_shutdown = new System.Windows.Forms.TextBox();
            this.bt_shutdown = new System.Windows.Forms.Button();
            this.cb_savegoodfiles = new System.Windows.Forms.CheckBox();
            this.cb_stampcall = new System.Windows.Forms.CheckBox();
            this.tb_callsign = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_speed = new System.Windows.Forms.ComboBox();
            this.label_speed = new System.Windows.Forms.Label();
            this.timer_searchmodem = new System.Windows.Forms.Timer(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.statusStrip1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_rximage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_tximage)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer_udpTX
            // 
            this.timer_udpTX.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer_udprx
            // 
            this.timer_udprx.Tick += new System.EventHandler(this.timer_udprx_Tick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.ts_ip,
            this.RXstatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 669);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1304, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(39, 17);
            this.toolStripStatusLabel.Text = "Status";
            // 
            // ts_ip
            // 
            this.ts_ip.Name = "ts_ip";
            this.ts_ip.Size = new System.Drawing.Size(12, 17);
            this.ts_ip.Text = "?";
            // 
            // RXstatus
            // 
            this.RXstatus.Name = "RXstatus";
            this.RXstatus.Size = new System.Drawing.Size(58, 17);
            this.RXstatus.Text = "RX-Status";
            // 
            // panel_constel
            // 
            this.panel_constel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(220)))));
            this.panel_constel.Location = new System.Drawing.Point(11, 590);
            this.panel_constel.Name = "panel_constel";
            this.panel_constel.Size = new System.Drawing.Size(75, 75);
            this.panel_constel.TabIndex = 5;
            this.panel_constel.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_constel_Paint);
            // 
            // timer_qpsk
            // 
            this.timer_qpsk.Enabled = true;
            this.timer_qpsk.Interval = 500;
            this.timer_qpsk.Tick += new System.EventHandler(this.timer_qpsk_Tick);
            // 
            // panel_txspectrum
            // 
            this.panel_txspectrum.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel_txspectrum.Location = new System.Drawing.Point(92, 590);
            this.panel_txspectrum.Name = "panel_txspectrum";
            this.panel_txspectrum.Size = new System.Drawing.Size(441, 76);
            this.panel_txspectrum.TabIndex = 6;
            this.panel_txspectrum.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_txspectrum_Paint);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button_stopBERtest);
            this.tabPage1.Controls.Add(this.button_startBERtest);
            this.tabPage1.Controls.Add(this.rtb);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1291, 553);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "BER Test";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // button_stopBERtest
            // 
            this.button_stopBERtest.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_stopBERtest.Location = new System.Drawing.Point(113, 13);
            this.button_stopBERtest.Name = "button_stopBERtest";
            this.button_stopBERtest.Size = new System.Drawing.Size(101, 32);
            this.button_stopBERtest.TabIndex = 4;
            this.button_stopBERtest.Text = "STOP";
            this.button_stopBERtest.UseVisualStyleBackColor = true;
            this.button_stopBERtest.Click += new System.EventHandler(this.button_stopBERtest_Click);
            // 
            // button_startBERtest
            // 
            this.button_startBERtest.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_startBERtest.Location = new System.Drawing.Point(6, 13);
            this.button_startBERtest.Name = "button_startBERtest";
            this.button_startBERtest.Size = new System.Drawing.Size(101, 32);
            this.button_startBERtest.TabIndex = 3;
            this.button_startBERtest.Text = "START";
            this.button_startBERtest.UseVisualStyleBackColor = true;
            this.button_startBERtest.Click += new System.EventHandler(this.button_startBERtest_Click);
            // 
            // rtb
            // 
            this.rtb.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtb.Location = new System.Drawing.Point(6, 51);
            this.rtb.Name = "rtb";
            this.rtb.Size = new System.Drawing.Size(1270, 494);
            this.rtb.TabIndex = 0;
            this.rtb.Text = "";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Controls.Add(this.label_rximage);
            this.tabPage2.Controls.Add(this.label_tximage);
            this.tabPage2.Controls.Add(this.pictureBox_rximage);
            this.tabPage2.Controls.Add(this.pictureBox_tximage);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1291, 553);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Image";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_nextimage);
            this.groupBox1.Controls.Add(this.cb_loop);
            this.groupBox1.Controls.Add(this.bt_rximages);
            this.groupBox1.Controls.Add(this.button_loadimage);
            this.groupBox1.Controls.Add(this.comboBox_quality);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.checkBox_big);
            this.groupBox1.Controls.Add(this.button_cancelimg);
            this.groupBox1.Controls.Add(this.button_sendimage);
            this.groupBox1.Location = new System.Drawing.Point(3, 508);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1277, 42);
            this.groupBox1.TabIndex = 12;
            // 
            // label_nextimage
            // 
            this.label_nextimage.AutoSize = true;
            this.label_nextimage.Location = new System.Drawing.Point(618, 19);
            this.label_nextimage.Name = "label_nextimage";
            this.label_nextimage.Size = new System.Drawing.Size(81, 13);
            this.label_nextimage.TabIndex = 12;
            this.label_nextimage.Text = "next image in ...";
            // 
            // cb_loop
            // 
            this.cb_loop.AutoSize = true;
            this.cb_loop.Location = new System.Drawing.Point(621, 5);
            this.cb_loop.Name = "cb_loop";
            this.cb_loop.Size = new System.Drawing.Size(167, 17);
            this.cb_loop.TabIndex = 11;
            this.cb_loop.Text = "loop (send all images in folder)";
            this.cb_loop.UseVisualStyleBackColor = true;
            // 
            // bt_rximages
            // 
            this.bt_rximages.Location = new System.Drawing.Point(534, 5);
            this.bt_rximages.Name = "bt_rximages";
            this.bt_rximages.Size = new System.Drawing.Size(75, 23);
            this.bt_rximages.TabIndex = 10;
            this.bt_rximages.Text = "RX Images";
            this.bt_rximages.UseVisualStyleBackColor = true;
            this.bt_rximages.Click += new System.EventHandler(this.bt_rximages_Click);
            // 
            // button_loadimage
            // 
            this.button_loadimage.Location = new System.Drawing.Point(265, 5);
            this.button_loadimage.Name = "button_loadimage";
            this.button_loadimage.Size = new System.Drawing.Size(75, 23);
            this.button_loadimage.TabIndex = 0;
            this.button_loadimage.Text = "Load Image";
            this.button_loadimage.UseVisualStyleBackColor = true;
            this.button_loadimage.Click += new System.EventHandler(this.button_loadimage_Click);
            // 
            // comboBox_quality
            // 
            this.comboBox_quality.FormattingEnabled = true;
            this.comboBox_quality.Items.AddRange(new object[] {
            "low, 30s",
            "medium, 1min",
            "high, 2min",
            "very high, 4min"});
            this.comboBox_quality.Location = new System.Drawing.Point(57, 7);
            this.comboBox_quality.Name = "comboBox_quality";
            this.comboBox_quality.Size = new System.Drawing.Size(109, 21);
            this.comboBox_quality.TabIndex = 6;
            this.comboBox_quality.Text = "medium, 1min";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Quality:";
            // 
            // checkBox_big
            // 
            this.checkBox_big.AutoSize = true;
            this.checkBox_big.Checked = true;
            this.checkBox_big.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_big.Location = new System.Drawing.Point(187, 9);
            this.checkBox_big.Name = "checkBox_big";
            this.checkBox_big.Size = new System.Drawing.Size(75, 17);
            this.checkBox_big.TabIndex = 8;
            this.checkBox_big.Text = "big picture";
            this.checkBox_big.UseVisualStyleBackColor = true;
            this.checkBox_big.CheckedChanged += new System.EventHandler(this.checkBox_small_CheckedChanged);
            // 
            // button_cancelimg
            // 
            this.button_cancelimg.Location = new System.Drawing.Point(443, 5);
            this.button_cancelimg.Name = "button_cancelimg";
            this.button_cancelimg.Size = new System.Drawing.Size(75, 23);
            this.button_cancelimg.TabIndex = 9;
            this.button_cancelimg.Text = "Cancel";
            this.button_cancelimg.UseVisualStyleBackColor = true;
            this.button_cancelimg.Click += new System.EventHandler(this.button_cancelimg_Click);
            // 
            // button_sendimage
            // 
            this.button_sendimage.Location = new System.Drawing.Point(346, 5);
            this.button_sendimage.Name = "button_sendimage";
            this.button_sendimage.Size = new System.Drawing.Size(75, 23);
            this.button_sendimage.TabIndex = 1;
            this.button_sendimage.Text = "Send Image";
            this.button_sendimage.UseVisualStyleBackColor = true;
            this.button_sendimage.Click += new System.EventHandler(this.button_sendimage_Click);
            // 
            // label_rximage
            // 
            this.label_rximage.AutoSize = true;
            this.label_rximage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_rximage.Location = new System.Drawing.Point(648, 7);
            this.label_rximage.Name = "label_rximage";
            this.label_rximage.Size = new System.Drawing.Size(61, 13);
            this.label_rximage.TabIndex = 5;
            this.label_rximage.Text = "RX image";
            // 
            // label_tximage
            // 
            this.label_tximage.AutoSize = true;
            this.label_tximage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_tximage.Location = new System.Drawing.Point(6, 7);
            this.label_tximage.Name = "label_tximage";
            this.label_tximage.Size = new System.Drawing.Size(60, 13);
            this.label_tximage.TabIndex = 4;
            this.label_tximage.Text = "TX image";
            // 
            // pictureBox_rximage
            // 
            this.pictureBox_rximage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(250)))), ((int)(((byte)(240)))));
            this.pictureBox_rximage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox_rximage.Location = new System.Drawing.Point(642, 27);
            this.pictureBox_rximage.Name = "pictureBox_rximage";
            this.pictureBox_rximage.Size = new System.Drawing.Size(640, 480);
            this.pictureBox_rximage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_rximage.TabIndex = 3;
            this.pictureBox_rximage.TabStop = false;
            // 
            // pictureBox_tximage
            // 
            this.pictureBox_tximage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(240)))));
            this.pictureBox_tximage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox_tximage.Location = new System.Drawing.Point(1, 27);
            this.pictureBox_tximage.Name = "pictureBox_tximage";
            this.pictureBox_tximage.Size = new System.Drawing.Size(640, 480);
            this.pictureBox_tximage.TabIndex = 2;
            this.pictureBox_tximage.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Location = new System.Drawing.Point(5, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1299, 579);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.button2);
            this.tabPage3.Controls.Add(this.bt_openrxfile);
            this.tabPage3.Controls.Add(this.label_rxfile);
            this.tabPage3.Controls.Add(this.label_txfile);
            this.tabPage3.Controls.Add(this.rtb_RXfile);
            this.tabPage3.Controls.Add(this.rtb_TXfile);
            this.tabPage3.Controls.Add(this.bt_file_send);
            this.tabPage3.Controls.Add(this.bt_sendBinaryFile);
            this.tabPage3.Controls.Add(this.bt_file_html);
            this.tabPage3.Controls.Add(this.bt_file_ascii);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1291, 553);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "File";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(17, 218);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(137, 23);
            this.button2.TabIndex = 12;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button_cancelimg_Click);
            // 
            // bt_openrxfile
            // 
            this.bt_openrxfile.Location = new System.Drawing.Point(17, 306);
            this.bt_openrxfile.Name = "bt_openrxfile";
            this.bt_openrxfile.Size = new System.Drawing.Size(137, 33);
            this.bt_openrxfile.TabIndex = 11;
            this.bt_openrxfile.Text = "Open RX file folder";
            this.bt_openrxfile.UseVisualStyleBackColor = true;
            this.bt_openrxfile.Click += new System.EventHandler(this.bt_openrxfile_Click);
            // 
            // label_rxfile
            // 
            this.label_rxfile.AutoSize = true;
            this.label_rxfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_rxfile.Location = new System.Drawing.Point(749, 10);
            this.label_rxfile.Name = "label_rxfile";
            this.label_rxfile.Size = new System.Drawing.Size(48, 13);
            this.label_rxfile.TabIndex = 7;
            this.label_rxfile.Text = "RX File";
            // 
            // label_txfile
            // 
            this.label_txfile.AutoSize = true;
            this.label_txfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_txfile.Location = new System.Drawing.Point(209, 10);
            this.label_txfile.Name = "label_txfile";
            this.label_txfile.Size = new System.Drawing.Size(47, 13);
            this.label_txfile.TabIndex = 6;
            this.label_txfile.Text = "TX File";
            // 
            // rtb_RXfile
            // 
            this.rtb_RXfile.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtb_RXfile.Location = new System.Drawing.Point(736, 31);
            this.rtb_RXfile.Name = "rtb_RXfile";
            this.rtb_RXfile.Size = new System.Drawing.Size(526, 508);
            this.rtb_RXfile.TabIndex = 5;
            this.rtb_RXfile.Text = "";
            // 
            // rtb_TXfile
            // 
            this.rtb_TXfile.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtb_TXfile.Location = new System.Drawing.Point(204, 31);
            this.rtb_TXfile.Name = "rtb_TXfile";
            this.rtb_TXfile.Size = new System.Drawing.Size(526, 508);
            this.rtb_TXfile.TabIndex = 4;
            this.rtb_TXfile.Text = "";
            // 
            // bt_file_send
            // 
            this.bt_file_send.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_file_send.ForeColor = System.Drawing.Color.Red;
            this.bt_file_send.Location = new System.Drawing.Point(17, 157);
            this.bt_file_send.Name = "bt_file_send";
            this.bt_file_send.Size = new System.Drawing.Size(137, 37);
            this.bt_file_send.TabIndex = 3;
            this.bt_file_send.Text = "SEND";
            this.bt_file_send.UseVisualStyleBackColor = true;
            this.bt_file_send.Click += new System.EventHandler(this.bt_file_send_Click);
            // 
            // bt_sendBinaryFile
            // 
            this.bt_sendBinaryFile.Location = new System.Drawing.Point(17, 89);
            this.bt_sendBinaryFile.Name = "bt_sendBinaryFile";
            this.bt_sendBinaryFile.Size = new System.Drawing.Size(137, 23);
            this.bt_sendBinaryFile.TabIndex = 2;
            this.bt_sendBinaryFile.Text = "Load Binary File";
            this.bt_sendBinaryFile.UseVisualStyleBackColor = true;
            this.bt_sendBinaryFile.Click += new System.EventHandler(this.bt_sendBinaryFile_Click);
            // 
            // bt_file_html
            // 
            this.bt_file_html.Location = new System.Drawing.Point(17, 60);
            this.bt_file_html.Name = "bt_file_html";
            this.bt_file_html.Size = new System.Drawing.Size(137, 23);
            this.bt_file_html.TabIndex = 1;
            this.bt_file_html.Text = "Load HTML File";
            this.bt_file_html.UseVisualStyleBackColor = true;
            this.bt_file_html.Click += new System.EventHandler(this.button2_Click);
            // 
            // bt_file_ascii
            // 
            this.bt_file_ascii.Location = new System.Drawing.Point(17, 31);
            this.bt_file_ascii.Name = "bt_file_ascii";
            this.bt_file_ascii.Size = new System.Drawing.Size(137, 23);
            this.bt_file_ascii.TabIndex = 0;
            this.bt_file_ascii.Text = "Load ASCII Text File";
            this.bt_file_ascii.UseVisualStyleBackColor = true;
            this.bt_file_ascii.Click += new System.EventHandler(this.bt_file_ascii_Click);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.groupBox4);
            this.tabPage5.Controls.Add(this.groupBox3);
            this.tabPage5.Controls.Add(this.groupBox2);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(1291, 553);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Setup";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // textBox3
            // 
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox3.ForeColor = System.Drawing.Color.Black;
            this.textBox3.Location = new System.Drawing.Point(138, 73);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(177, 19);
            this.textBox3.TabIndex = 12;
            this.textBox3.Text = "(HDMI is usually not used)";
            // 
            // textBox2
            // 
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.ForeColor = System.Drawing.Color.Black;
            this.textBox2.Location = new System.Drawing.Point(189, 48);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(126, 50);
            this.textBox2.TabIndex = 11;
            this.textBox2.Text = "in case the RX has sync\r\nproblems, it can be\r\nre-initialized here.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(112, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Audio Record Device:";
            // 
            // cb_audioCAP
            // 
            this.cb_audioCAP.FormattingEnabled = true;
            this.cb_audioCAP.Location = new System.Drawing.Point(138, 46);
            this.cb_audioCAP.Name = "cb_audioCAP";
            this.cb_audioCAP.Size = new System.Drawing.Size(230, 21);
            this.cb_audioCAP.TabIndex = 9;
            this.cb_audioCAP.Text = "Default";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Audio Playback Device:";
            // 
            // cb_audioPB
            // 
            this.cb_audioPB.FormattingEnabled = true;
            this.cb_audioPB.Location = new System.Drawing.Point(138, 19);
            this.cb_audioPB.Name = "cb_audioPB";
            this.cb_audioPB.Size = new System.Drawing.Size(230, 21);
            this.cb_audioPB.TabIndex = 7;
            this.cb_audioPB.Text = "Default";
            // 
            // bt_resetmodem
            // 
            this.bt_resetmodem.Location = new System.Drawing.Point(189, 19);
            this.bt_resetmodem.Name = "bt_resetmodem";
            this.bt_resetmodem.Size = new System.Drawing.Size(117, 23);
            this.bt_resetmodem.TabIndex = 6;
            this.bt_resetmodem.Text = "Reset RX Modem";
            this.bt_resetmodem.UseVisualStyleBackColor = true;
            this.bt_resetmodem.Click += new System.EventHandler(this.bt_resetmodem_Click);
            // 
            // tb_shutdown
            // 
            this.tb_shutdown.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tb_shutdown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_shutdown.ForeColor = System.Drawing.Color.Red;
            this.tb_shutdown.Location = new System.Drawing.Point(17, 48);
            this.tb_shutdown.Multiline = true;
            this.tb_shutdown.Name = "tb_shutdown";
            this.tb_shutdown.Size = new System.Drawing.Size(155, 50);
            this.tb_shutdown.TabIndex = 5;
            this.tb_shutdown.Text = "before switching off the \r\nmodem SBC click here to \r\navoid defective SD-cards.\r\n";
            // 
            // bt_shutdown
            // 
            this.bt_shutdown.Location = new System.Drawing.Point(17, 19);
            this.bt_shutdown.Name = "bt_shutdown";
            this.bt_shutdown.Size = new System.Drawing.Size(155, 23);
            this.bt_shutdown.TabIndex = 4;
            this.bt_shutdown.Text = "Shutdown Modem-SBC";
            this.bt_shutdown.UseVisualStyleBackColor = true;
            this.bt_shutdown.Click += new System.EventHandler(this.bt_shutdown_Click);
            // 
            // cb_savegoodfiles
            // 
            this.cb_savegoodfiles.AutoSize = true;
            this.cb_savegoodfiles.Checked = true;
            this.cb_savegoodfiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_savegoodfiles.Location = new System.Drawing.Point(71, 90);
            this.cb_savegoodfiles.Name = "cb_savegoodfiles";
            this.cb_savegoodfiles.Size = new System.Drawing.Size(159, 17);
            this.cb_savegoodfiles.TabIndex = 3;
            this.cb_savegoodfiles.Text = "Save good files/images only";
            this.cb_savegoodfiles.UseVisualStyleBackColor = true;
            // 
            // cb_stampcall
            // 
            this.cb_stampcall.AutoSize = true;
            this.cb_stampcall.Checked = true;
            this.cb_stampcall.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_stampcall.Location = new System.Drawing.Point(71, 67);
            this.cb_stampcall.Name = "cb_stampcall";
            this.cb_stampcall.Size = new System.Drawing.Size(146, 17);
            this.cb_stampcall.TabIndex = 2;
            this.cb_stampcall.Text = "Insert Callsign into picture";
            this.cb_stampcall.UseVisualStyleBackColor = true;
            // 
            // tb_callsign
            // 
            this.tb_callsign.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.tb_callsign.Location = new System.Drawing.Point(71, 28);
            this.tb_callsign.Name = "tb_callsign";
            this.tb_callsign.Size = new System.Drawing.Size(151, 20);
            this.tb_callsign.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Callsign:";
            // 
            // cb_speed
            // 
            this.cb_speed.FormattingEnabled = true;
            this.cb_speed.Items.AddRange(new object[] {
            "3000 QPSK BW: 1700 Hz ",
            "3150 QPSK BW: 1800 Hz ",
            "3675 QPSK BW: 2100 Hz ",
            "4000 QPSK BW: 2400 Hz ",
            "4410 QPSK BW: 2500 Hz (QO-100)",
            "4800 QPSK BW: 2700 Hz",
            "5500 8PSK BW: 2300 Hz",
            "6000 8PSK BW: 2500 Hz (QO-100)",
            "6600 8PSK BW: 2600 Hz",
            "7200 8PSK BW: 2700 Hz"});
            this.cb_speed.Location = new System.Drawing.Point(636, 644);
            this.cb_speed.Name = "cb_speed";
            this.cb_speed.Size = new System.Drawing.Size(324, 21);
            this.cb_speed.TabIndex = 11;
            this.cb_speed.Text = "4410 QPSK BW: 2500 Hz (QO-100)";
            this.cb_speed.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label_speed
            // 
            this.label_speed.AutoSize = true;
            this.label_speed.Location = new System.Drawing.Point(545, 647);
            this.label_speed.Name = "label_speed";
            this.label_speed.Size = new System.Drawing.Size(71, 13);
            this.label_speed.TabIndex = 12;
            this.label_speed.Text = "Speed [bit/s]:";
            // 
            // timer_searchmodem
            // 
            this.timer_searchmodem.Interval = 1000;
            this.timer_searchmodem.Tick += new System.EventHandler(this.timer_searchmodem_Tick);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tb_callsign);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.cb_stampcall);
            this.groupBox2.Controls.Add(this.cb_savegoodfiles);
            this.groupBox2.Location = new System.Drawing.Point(12, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(384, 126);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Personal Settings";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cb_audioPB);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.textBox3);
            this.groupBox3.Controls.Add(this.cb_audioCAP);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(12, 146);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(384, 107);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Transceiver Audio";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.bt_shutdown);
            this.groupBox4.Controls.Add(this.tb_shutdown);
            this.groupBox4.Controls.Add(this.bt_resetmodem);
            this.groupBox4.Controls.Add(this.textBox2);
            this.groupBox4.Location = new System.Drawing.Point(12, 259);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(384, 105);
            this.groupBox4.TabIndex = 15;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Maintenance";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1304, 691);
            this.Controls.Add(this.cb_speed);
            this.Controls.Add(this.label_speed);
            this.Controls.Add(this.panel_txspectrum);
            this.Controls.Add(this.panel_constel);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControl1);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "QO-100 NB Transponder HS Transmission AMSAT-DL V0.2 by DJ0ABR";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_rximage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_tximage)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer_udpTX;
        private System.Windows.Forms.Timer timer_udprx;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Panel panel_constel;
        private System.Windows.Forms.Timer timer_qpsk;
        private System.Windows.Forms.Panel panel_txspectrum;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button button_stopBERtest;
        private System.Windows.Forms.Button button_startBERtest;
        private System.Windows.Forms.RichTextBox rtb;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ComboBox comboBox_quality;
        private System.Windows.Forms.Button button_loadimage;
        private System.Windows.Forms.Button button_cancelimg;
        private System.Windows.Forms.Button button_sendimage;
        private System.Windows.Forms.CheckBox checkBox_big;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_rximage;
        private System.Windows.Forms.Label label_tximage;
        private System.Windows.Forms.PictureBox pictureBox_rximage;
        private System.Windows.Forms.PictureBox pictureBox_tximage;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ToolStripStatusLabel ts_ip;
        private System.Windows.Forms.Panel groupBox1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.RichTextBox rtb_TXfile;
        private System.Windows.Forms.Button bt_file_send;
        private System.Windows.Forms.Button bt_sendBinaryFile;
        private System.Windows.Forms.Button bt_file_html;
        private System.Windows.Forms.Button bt_file_ascii;
        private System.Windows.Forms.RichTextBox rtb_RXfile;
        private System.Windows.Forms.Label label_rxfile;
        private System.Windows.Forms.Label label_txfile;
        private System.Windows.Forms.ToolStripStatusLabel RXstatus;
        private System.Windows.Forms.ComboBox cb_speed;
        private System.Windows.Forms.Label label_speed;
        private System.Windows.Forms.Timer timer_searchmodem;
        private System.Windows.Forms.Button bt_rximages;
        private System.Windows.Forms.Button bt_openrxfile;
        private System.Windows.Forms.CheckBox cb_loop;
        private System.Windows.Forms.Label label_nextimage;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TextBox tb_callsign;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cb_stampcall;
        private System.Windows.Forms.CheckBox cb_savegoodfiles;
        private System.Windows.Forms.TextBox tb_shutdown;
        private System.Windows.Forms.Button bt_shutdown;
        private System.Windows.Forms.Button bt_resetmodem;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cb_audioCAP;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cb_audioPB;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}

