
namespace V4Quote
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
            this.label0 = new System.Windows.Forms.Label();
            this.textBoxUserCode = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.buttonLogin = new System.Windows.Forms.Button();
            this.buttonLogout = new System.Windows.Forms.Button();
            this.buttonBind = new System.Windows.Forms.Button();
            this.textBoxIp = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this._LstLog = new System.Windows.Forms.ListBox();
            this.comboBoxAccountType = new System.Windows.Forms.ComboBox();
            this.bgwQuote = new System.ComponentModel.BackgroundWorker();
            this.bgwLog = new System.ComponentModel.BackgroundWorker();
            this.comboBox_Broker = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label0
            // 
            this.label0.AutoSize = true;
            this.label0.Location = new System.Drawing.Point(177, 15);
            this.label0.Name = "label0";
            this.label0.Size = new System.Drawing.Size(29, 12);
            this.label0.TabIndex = 0;
            this.label0.Text = "账户";
            this.label0.UseMnemonic = false;
            // 
            // textBoxUserCode
            // 
            this.textBoxUserCode.Location = new System.Drawing.Point(209, 12);
            this.textBoxUserCode.Name = "textBoxUserCode";
            this.textBoxUserCode.Size = new System.Drawing.Size(108, 21);
            this.textBoxUserCode.TabIndex = 1;
            this.textBoxUserCode.TextChanged += new System.EventHandler(this.textBoxUserCode_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(321, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "密码";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(356, 12);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(102, 21);
            this.textBoxPassword.TabIndex = 3;
            this.textBoxPassword.TextChanged += new System.EventHandler(this.textBoxPassword_TextChanged);
            // 
            // buttonLogin
            // 
            this.buttonLogin.Location = new System.Drawing.Point(62, 58);
            this.buttonLogin.Name = "buttonLogin";
            this.buttonLogin.Size = new System.Drawing.Size(75, 23);
            this.buttonLogin.TabIndex = 4;
            this.buttonLogin.Text = "登录";
            this.buttonLogin.UseVisualStyleBackColor = true;
            this.buttonLogin.Click += new System.EventHandler(this.buttonLogin_Click);
            // 
            // buttonLogout
            // 
            this.buttonLogout.Location = new System.Drawing.Point(185, 58);
            this.buttonLogout.Name = "buttonLogout";
            this.buttonLogout.Size = new System.Drawing.Size(75, 23);
            this.buttonLogout.TabIndex = 5;
            this.buttonLogout.Text = "退出";
            this.buttonLogout.UseVisualStyleBackColor = true;
            this.buttonLogout.Click += new System.EventHandler(this.buttonLogout_Click);
            // 
            // buttonBind
            // 
            this.buttonBind.Location = new System.Drawing.Point(298, 102);
            this.buttonBind.Name = "buttonBind";
            this.buttonBind.Size = new System.Drawing.Size(75, 23);
            this.buttonBind.TabIndex = 6;
            this.buttonBind.Text = "Bind";
            this.buttonBind.UseVisualStyleBackColor = true;
            this.buttonBind.Click += new System.EventHandler(this.buttonBind_Click);
            // 
            // textBoxIp
            // 
            this.textBoxIp.Location = new System.Drawing.Point(62, 102);
            this.textBoxIp.Name = "textBoxIp";
            this.textBoxIp.Size = new System.Drawing.Size(208, 21);
            this.textBoxIp.TabIndex = 7;
            this.textBoxIp.TextChanged += new System.EventHandler(this.textBoxIp_TextChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(379, 102);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // _LstLog
            // 
            this._LstLog.FormattingEnabled = true;
            this._LstLog.ItemHeight = 12;
            this._LstLog.Location = new System.Drawing.Point(32, 142);
            this._LstLog.Name = "_LstLog";
            this._LstLog.Size = new System.Drawing.Size(383, 88);
            this._LstLog.TabIndex = 9;
            // 
            // comboBoxAccountType
            // 
            this.comboBoxAccountType.FormattingEnabled = true;
            this.comboBoxAccountType.Items.AddRange(new object[] {
            "Live",
            "Demo"});
            this.comboBoxAccountType.Location = new System.Drawing.Point(91, 12);
            this.comboBoxAccountType.Name = "comboBoxAccountType";
            this.comboBoxAccountType.Size = new System.Drawing.Size(63, 20);
            this.comboBoxAccountType.TabIndex = 10;
            this.comboBoxAccountType.Text = "Live";
            this.comboBoxAccountType.SelectedIndexChanged += new System.EventHandler(this.comboBoxAccountType_SelectedIndexChanged);
            // 
            // bgwQuote
            // 
            this.bgwQuote.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgwQuote_DoWork);
            // 
            // bgwLog
            // 
            this.bgwLog.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgwLog_DoWork);
            // 
            // comboBox_Broker
            // 
            this.comboBox_Broker.FormattingEnabled = true;
            this.comboBox_Broker.Items.AddRange(new object[] {
            "YSG",
            "WFB"});
            this.comboBox_Broker.Location = new System.Drawing.Point(12, 12);
            this.comboBox_Broker.Name = "comboBox_Broker";
            this.comboBox_Broker.Size = new System.Drawing.Size(63, 20);
            this.comboBox_Broker.TabIndex = 11;
            this.comboBox_Broker.Text = "YSG";
            this.comboBox_Broker.SelectedIndexChanged += new System.EventHandler(this.comboBox_Broker_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 242);
            this.Controls.Add(this.comboBox_Broker);
            this.Controls.Add(this.comboBoxAccountType);
            this.Controls.Add(this._LstLog);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBoxIp);
            this.Controls.Add(this.buttonBind);
            this.Controls.Add(this.buttonLogout);
            this.Controls.Add(this.buttonLogin);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxUserCode);
            this.Controls.Add(this.label0);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label0;
        private System.Windows.Forms.TextBox textBoxUserCode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Button buttonLogin;
        private System.Windows.Forms.Button buttonLogout;
        private System.Windows.Forms.Button buttonBind;
        private System.Windows.Forms.TextBox textBoxIp;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox _LstLog;
        private System.Windows.Forms.ComboBox comboBoxAccountType;
        private System.ComponentModel.BackgroundWorker bgwQuote;
        private System.ComponentModel.BackgroundWorker bgwLog;
        private System.Windows.Forms.ComboBox comboBox_Broker;
    }
}

