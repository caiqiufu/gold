
namespace NetworkTest
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelIP1 = new System.Windows.Forms.Label();
            this.textBoxIP1 = new System.Windows.Forms.TextBox();
            this.buttonBind = new System.Windows.Forms.Button();
            this.textBoxBindIP = new System.Windows.Forms.TextBox();
            this.listBoxSendMsg = new System.Windows.Forms.ListBox();
            this.timerSendMsg = new System.Windows.Forms.Timer(this.components);
            this.buttonStartTest = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelIP1
            // 
            this.labelIP1.AutoSize = true;
            this.labelIP1.Location = new System.Drawing.Point(37, 22);
            this.labelIP1.Name = "labelIP1";
            this.labelIP1.Size = new System.Drawing.Size(53, 12);
            this.labelIP1.TabIndex = 0;
            this.labelIP1.Text = "数据源IP";
            // 
            // textBoxIP1
            // 
            this.textBoxIP1.Location = new System.Drawing.Point(110, 19);
            this.textBoxIP1.Name = "textBoxIP1";
            this.textBoxIP1.Size = new System.Drawing.Size(194, 21);
            this.textBoxIP1.TabIndex = 1;
            this.textBoxIP1.Text = "64.74.232.69";
            // 
            // buttonBind
            // 
            this.buttonBind.Location = new System.Drawing.Point(15, 48);
            this.buttonBind.Name = "buttonBind";
            this.buttonBind.Size = new System.Drawing.Size(75, 23);
            this.buttonBind.TabIndex = 4;
            this.buttonBind.Text = "Bind";
            this.buttonBind.UseVisualStyleBackColor = true;
            this.buttonBind.Click += new System.EventHandler(this.buttonBind_Click);
            // 
            // textBoxBindIP
            // 
            this.textBoxBindIP.Location = new System.Drawing.Point(110, 50);
            this.textBoxBindIP.Name = "textBoxBindIP";
            this.textBoxBindIP.Size = new System.Drawing.Size(195, 21);
            this.textBoxBindIP.TabIndex = 5;
            this.textBoxBindIP.Text = "tcp://127.0.0.1:5556";
            // 
            // listBoxSendMsg
            // 
            this.listBoxSendMsg.FormattingEnabled = true;
            this.listBoxSendMsg.ItemHeight = 12;
            this.listBoxSendMsg.Location = new System.Drawing.Point(331, 8);
            this.listBoxSendMsg.Name = "listBoxSendMsg";
            this.listBoxSendMsg.Size = new System.Drawing.Size(530, 244);
            this.listBoxSendMsg.TabIndex = 6;
            // 
            // timerSendMsg
            // 
            this.timerSendMsg.Enabled = true;
            this.timerSendMsg.Interval = 60000;
            this.timerSendMsg.Tick += new System.EventHandler(this.timerSendMsg_Tick);
            // 
            // buttonStartTest
            // 
            this.buttonStartTest.Location = new System.Drawing.Point(15, 87);
            this.buttonStartTest.Name = "buttonStartTest";
            this.buttonStartTest.Size = new System.Drawing.Size(75, 23);
            this.buttonStartTest.TabIndex = 7;
            this.buttonStartTest.Text = "Start Test";
            this.buttonStartTest.UseVisualStyleBackColor = true;
            this.buttonStartTest.Click += new System.EventHandler(this.buttonStartTest_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(110, 87);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 8;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(11, 89);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(79, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "Start Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(111, 58);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(57, 21);
            this.textBox1.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(57, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 11;
            this.label1.Text = "时差";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(174, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 12;
            this.label2.Text = "小时";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxIP1);
            this.groupBox1.Controls.Add(this.labelIP1);
            this.groupBox1.Controls.Add(this.buttonBind);
            this.groupBox1.Controls.Add(this.textBoxBindIP);
            this.groupBox1.Controls.Add(this.buttonStartTest);
            this.groupBox1.Controls.Add(this.buttonStop);
            this.groupBox1.Location = new System.Drawing.Point(4, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(311, 118);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "消息发送端信息";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.textBox2);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.textBox1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(4, 132);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(311, 125);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "消息接收端信息";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(15, 24);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 13;
            this.button2.Text = "Bind";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(110, 25);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(194, 21);
            this.textBox2.TabIndex = 14;
            this.textBox2.Text = "tcp://127.0.0.1:5556";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(111, 88);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 15;
            this.button3.Text = "Stop";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(873, 269);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listBoxSendMsg);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelIP1;
        private System.Windows.Forms.TextBox textBoxIP1;
        private System.Windows.Forms.Button buttonBind;
        private System.Windows.Forms.TextBox textBoxBindIP;
        private System.Windows.Forms.ListBox listBoxSendMsg;
        private System.Windows.Forms.Timer timerSendMsg;
        private System.Windows.Forms.Button buttonStartTest;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button button1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button2;
    }
}

