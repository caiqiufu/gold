using DS;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
// Import XML for saving and retriving markets.
using System.Xml;
// Import the T4 definitions namespace.
using T4;
// Import the API namespace.
using T4.API;


// Generic collections.
/*
Dear Cai Qiufu,
You have been successfully registered for the CTS T4 Trading Simulator:
Firm: CTS
UserName: cqiufu
Password: RMEEjtuz
Expires: 15 Nov 2020
Latest Promotions!
Trial the entire T4 product suite with the same simulator account: T4 Desktop, T4 Mobile and T4 WebTrader.
T4 Desktop
Download T4 Desktop to your PC.
    */

namespace T4Example2CSharp
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class Form1 : System.Windows.Forms.Form
    {


        #region Windows Form Designer generated code
        internal System.Windows.Forms.GroupBox grpAccountPicker;
        internal System.Windows.Forms.ComboBox cboAccounts;
        internal System.Windows.Forms.Label lblCash;
        internal System.Windows.Forms.Label lblAccount;
        internal System.Windows.Forms.TextBox txtCash;
        internal System.Windows.Forms.Label lblMisc2;
        internal System.Windows.Forms.ComboBox cboMisc2;
        internal System.Windows.Forms.TextBox txtLast2;
        internal System.Windows.Forms.TextBox txtOfferVol2;
        internal System.Windows.Forms.TextBox txtBidVol2;
        internal System.Windows.Forms.TextBox txtOffer2;
        internal System.Windows.Forms.TextBox txtBid2;
        internal System.Windows.Forms.TextBox txtMarketDescription2;
        internal System.Windows.Forms.Button cmdGet2;
        internal System.Windows.Forms.TextBox txtNet2;
        internal System.Windows.Forms.TextBox txtBuys2;
        internal System.Windows.Forms.TextBox txtSells2;
        internal System.Windows.Forms.TextBox txtLastVol2;
        internal System.Windows.Forms.TextBox txtLastVolTotal2;
        internal System.Windows.Forms.Button cmdSave;
        internal System.Windows.Forms.Label lblSaveInfo;
        private GroupBox grpMarket2;
        private Label label8;
        private TextBox txtPublishIP;
        private Button btnPublish;
        internal Label label2;
        internal Label label3;
        internal Label label4;
        internal Label label5;
        internal Label label6;
        internal Label label7;
        internal Label label9;
        internal Label label10;
        internal Label label11;
        internal Label label12;
        private Button button1;
        private Label label1;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public Form1()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            // Finnally register Form events.
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Closed += new System.EventHandler(this.frmMain_Closed);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpAccountPicker = new System.Windows.Forms.GroupBox();
            this.cboAccounts = new System.Windows.Forms.ComboBox();
            this.lblCash = new System.Windows.Forms.Label();
            this.lblAccount = new System.Windows.Forms.Label();
            this.txtCash = new System.Windows.Forms.TextBox();
            this.lblMisc2 = new System.Windows.Forms.Label();
            this.cboMisc2 = new System.Windows.Forms.ComboBox();
            this.txtLast2 = new System.Windows.Forms.TextBox();
            this.txtOfferVol2 = new System.Windows.Forms.TextBox();
            this.txtBidVol2 = new System.Windows.Forms.TextBox();
            this.txtOffer2 = new System.Windows.Forms.TextBox();
            this.txtBid2 = new System.Windows.Forms.TextBox();
            this.txtMarketDescription2 = new System.Windows.Forms.TextBox();
            this.cmdGet2 = new System.Windows.Forms.Button();
            this.txtNet2 = new System.Windows.Forms.TextBox();
            this.txtBuys2 = new System.Windows.Forms.TextBox();
            this.txtSells2 = new System.Windows.Forms.TextBox();
            this.txtLastVol2 = new System.Windows.Forms.TextBox();
            this.txtLastVolTotal2 = new System.Windows.Forms.TextBox();
            this.cmdSave = new System.Windows.Forms.Button();
            this.lblSaveInfo = new System.Windows.Forms.Label();
            this.grpMarket2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtPublishIP = new System.Windows.Forms.TextBox();
            this.btnPublish = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.grpAccountPicker.SuspendLayout();
            this.grpMarket2.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpAccountPicker
            // 
            this.grpAccountPicker.Controls.Add(this.cboAccounts);
            this.grpAccountPicker.Controls.Add(this.lblCash);
            this.grpAccountPicker.Controls.Add(this.lblAccount);
            this.grpAccountPicker.Controls.Add(this.txtCash);
            this.grpAccountPicker.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpAccountPicker.Location = new System.Drawing.Point(10, 9);
            this.grpAccountPicker.Name = "grpAccountPicker";
            this.grpAccountPicker.Size = new System.Drawing.Size(447, 56);
            this.grpAccountPicker.TabIndex = 64;
            this.grpAccountPicker.TabStop = false;
            this.grpAccountPicker.Text = "Account";
            // 
            // cboAccounts
            // 
            this.cboAccounts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAccounts.Location = new System.Drawing.Point(112, 25);
            this.cboAccounts.Name = "cboAccounts";
            this.cboAccounts.Size = new System.Drawing.Size(151, 21);
            this.cboAccounts.Sorted = true;
            this.cboAccounts.TabIndex = 42;
            this.cboAccounts.TabStop = false;
            this.cboAccounts.SelectedIndexChanged += new System.EventHandler(this.cboAccounts_SelectedIndexChanged);
            // 
            // lblCash
            // 
            this.lblCash.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCash.Location = new System.Drawing.Point(269, 25);
            this.lblCash.Name = "lblCash";
            this.lblCash.Size = new System.Drawing.Size(43, 22);
            this.lblCash.TabIndex = 44;
            this.lblCash.Text = "Cash:";
            this.lblCash.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAccount
            // 
            this.lblAccount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAccount.Location = new System.Drawing.Point(7, 24);
            this.lblAccount.Name = "lblAccount";
            this.lblAccount.Size = new System.Drawing.Size(142, 22);
            this.lblAccount.TabIndex = 41;
            this.lblAccount.Text = "Current Account:";
            this.lblAccount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtCash
            // 
            this.txtCash.BackColor = System.Drawing.Color.White;
            this.txtCash.Location = new System.Drawing.Point(312, 27);
            this.txtCash.Name = "txtCash";
            this.txtCash.ReadOnly = true;
            this.txtCash.Size = new System.Drawing.Size(130, 20);
            this.txtCash.TabIndex = 43;
            this.txtCash.TabStop = false;
            this.txtCash.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblMisc2
            // 
            this.lblMisc2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMisc2.Location = new System.Drawing.Point(463, 124);
            this.lblMisc2.Name = "lblMisc2";
            this.lblMisc2.Size = new System.Drawing.Size(84, 21);
            this.lblMisc2.TabIndex = 64;
            this.lblMisc2.Text = "Misc Code:";
            this.lblMisc2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cboMisc2
            // 
            this.cboMisc2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMisc2.Location = new System.Drawing.Point(552, 124);
            this.cboMisc2.Name = "cboMisc2";
            this.cboMisc2.Size = new System.Drawing.Size(216, 21);
            this.cboMisc2.Sorted = true;
            this.cboMisc2.TabIndex = 62;
            this.cboMisc2.TabStop = false;
            // 
            // txtLast2
            // 
            this.txtLast2.BackColor = System.Drawing.Color.Honeydew;
            this.txtLast2.Location = new System.Drawing.Point(491, 54);
            this.txtLast2.Name = "txtLast2";
            this.txtLast2.ReadOnly = true;
            this.txtLast2.Size = new System.Drawing.Size(72, 20);
            this.txtLast2.TabIndex = 36;
            this.txtLast2.TabStop = false;
            this.txtLast2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtOfferVol2
            // 
            this.txtOfferVol2.BackColor = System.Drawing.Color.MistyRose;
            this.txtOfferVol2.Location = new System.Drawing.Point(455, 54);
            this.txtOfferVol2.Name = "txtOfferVol2";
            this.txtOfferVol2.ReadOnly = true;
            this.txtOfferVol2.Size = new System.Drawing.Size(33, 20);
            this.txtOfferVol2.TabIndex = 35;
            this.txtOfferVol2.TabStop = false;
            this.txtOfferVol2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtBidVol2
            // 
            this.txtBidVol2.BackColor = System.Drawing.Color.LightCyan;
            this.txtBidVol2.Location = new System.Drawing.Point(344, 54);
            this.txtBidVol2.Name = "txtBidVol2";
            this.txtBidVol2.ReadOnly = true;
            this.txtBidVol2.Size = new System.Drawing.Size(34, 20);
            this.txtBidVol2.TabIndex = 34;
            this.txtBidVol2.TabStop = false;
            this.txtBidVol2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtOffer2
            // 
            this.txtOffer2.BackColor = System.Drawing.Color.MistyRose;
            this.txtOffer2.Location = new System.Drawing.Point(380, 54);
            this.txtOffer2.Name = "txtOffer2";
            this.txtOffer2.ReadOnly = true;
            this.txtOffer2.Size = new System.Drawing.Size(72, 20);
            this.txtOffer2.TabIndex = 33;
            this.txtOffer2.TabStop = false;
            this.txtOffer2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtBid2
            // 
            this.txtBid2.BackColor = System.Drawing.Color.LightCyan;
            this.txtBid2.Location = new System.Drawing.Point(270, 54);
            this.txtBid2.Name = "txtBid2";
            this.txtBid2.ReadOnly = true;
            this.txtBid2.Size = new System.Drawing.Size(72, 20);
            this.txtBid2.TabIndex = 32;
            this.txtBid2.TabStop = false;
            this.txtBid2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMarketDescription2
            // 
            this.txtMarketDescription2.BackColor = System.Drawing.Color.White;
            this.txtMarketDescription2.Location = new System.Drawing.Point(18, 54);
            this.txtMarketDescription2.Name = "txtMarketDescription2";
            this.txtMarketDescription2.ReadOnly = true;
            this.txtMarketDescription2.Size = new System.Drawing.Size(250, 20);
            this.txtMarketDescription2.TabIndex = 31;
            this.txtMarketDescription2.TabStop = false;
            // 
            // cmdGet2
            // 
            this.cmdGet2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdGet2.Location = new System.Drawing.Point(17, 26);
            this.cmdGet2.Name = "cmdGet2";
            this.cmdGet2.Size = new System.Drawing.Size(69, 21);
            this.cmdGet2.TabIndex = 30;
            this.cmdGet2.TabStop = false;
            this.cmdGet2.Text = "Picker";
            this.cmdGet2.Click += new System.EventHandler(this.cmdGet2_Click);
            // 
            // txtNet2
            // 
            this.txtNet2.BackColor = System.Drawing.Color.White;
            this.txtNet2.Location = new System.Drawing.Point(676, 54);
            this.txtNet2.Name = "txtNet2";
            this.txtNet2.ReadOnly = true;
            this.txtNet2.Size = new System.Drawing.Size(45, 20);
            this.txtNet2.TabIndex = 47;
            this.txtNet2.TabStop = false;
            this.txtNet2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtBuys2
            // 
            this.txtBuys2.BackColor = System.Drawing.Color.White;
            this.txtBuys2.ForeColor = System.Drawing.Color.RoyalBlue;
            this.txtBuys2.Location = new System.Drawing.Point(724, 54);
            this.txtBuys2.Name = "txtBuys2";
            this.txtBuys2.ReadOnly = true;
            this.txtBuys2.Size = new System.Drawing.Size(45, 20);
            this.txtBuys2.TabIndex = 52;
            this.txtBuys2.TabStop = false;
            this.txtBuys2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtSells2
            // 
            this.txtSells2.BackColor = System.Drawing.Color.White;
            this.txtSells2.ForeColor = System.Drawing.Color.Crimson;
            this.txtSells2.Location = new System.Drawing.Point(772, 54);
            this.txtSells2.Name = "txtSells2";
            this.txtSells2.ReadOnly = true;
            this.txtSells2.Size = new System.Drawing.Size(45, 20);
            this.txtSells2.TabIndex = 54;
            this.txtSells2.TabStop = false;
            this.txtSells2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtLastVol2
            // 
            this.txtLastVol2.BackColor = System.Drawing.Color.Honeydew;
            this.txtLastVol2.Location = new System.Drawing.Point(565, 54);
            this.txtLastVol2.Name = "txtLastVol2";
            this.txtLastVol2.ReadOnly = true;
            this.txtLastVol2.Size = new System.Drawing.Size(34, 20);
            this.txtLastVol2.TabIndex = 37;
            this.txtLastVol2.TabStop = false;
            this.txtLastVol2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtLastVolTotal2
            // 
            this.txtLastVolTotal2.BackColor = System.Drawing.Color.Honeydew;
            this.txtLastVolTotal2.Location = new System.Drawing.Point(601, 54);
            this.txtLastVolTotal2.Name = "txtLastVolTotal2";
            this.txtLastVolTotal2.ReadOnly = true;
            this.txtLastVolTotal2.Size = new System.Drawing.Size(72, 20);
            this.txtLastVolTotal2.TabIndex = 38;
            this.txtLastVolTotal2.TabStop = false;
            this.txtLastVolTotal2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // cmdSave
            // 
            this.cmdSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdSave.Location = new System.Drawing.Point(19, 77);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(168, 28);
            this.cmdSave.TabIndex = 40;
            this.cmdSave.TabStop = false;
            this.cmdSave.Text = "Save Selected Markets";
            this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
            // 
            // lblSaveInfo
            // 
            this.lblSaveInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSaveInfo.Location = new System.Drawing.Point(194, 77);
            this.lblSaveInfo.Name = "lblSaveInfo";
            this.lblSaveInfo.Size = new System.Drawing.Size(444, 28);
            this.lblSaveInfo.TabIndex = 66;
            this.lblSaveInfo.Text = "Click Save to store the current markets in an XML file on the server.  The market" +
    "s will be loaded automatically on the next login.";
            this.lblSaveInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // grpMarket2
            // 
            this.grpMarket2.Controls.Add(this.label2);
            this.grpMarket2.Controls.Add(this.label3);
            this.grpMarket2.Controls.Add(this.label4);
            this.grpMarket2.Controls.Add(this.label5);
            this.grpMarket2.Controls.Add(this.label6);
            this.grpMarket2.Controls.Add(this.label7);
            this.grpMarket2.Controls.Add(this.label9);
            this.grpMarket2.Controls.Add(this.label10);
            this.grpMarket2.Controls.Add(this.label11);
            this.grpMarket2.Controls.Add(this.label12);
            this.grpMarket2.Controls.Add(this.txtMarketDescription2);
            this.grpMarket2.Controls.Add(this.txtLastVolTotal2);
            this.grpMarket2.Controls.Add(this.txtLastVol2);
            this.grpMarket2.Controls.Add(this.lblMisc2);
            this.grpMarket2.Controls.Add(this.cmdSave);
            this.grpMarket2.Controls.Add(this.txtSells2);
            this.grpMarket2.Controls.Add(this.lblSaveInfo);
            this.grpMarket2.Controls.Add(this.cboMisc2);
            this.grpMarket2.Controls.Add(this.txtBuys2);
            this.grpMarket2.Controls.Add(this.txtNet2);
            this.grpMarket2.Controls.Add(this.cmdGet2);
            this.grpMarket2.Controls.Add(this.txtBid2);
            this.grpMarket2.Controls.Add(this.txtOffer2);
            this.grpMarket2.Controls.Add(this.txtBidVol2);
            this.grpMarket2.Controls.Add(this.txtLast2);
            this.grpMarket2.Controls.Add(this.txtOfferVol2);
            this.grpMarket2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpMarket2.Location = new System.Drawing.Point(10, 71);
            this.grpMarket2.Name = "grpMarket2";
            this.grpMarket2.Size = new System.Drawing.Size(864, 115);
            this.grpMarket2.TabIndex = 68;
            this.grpMarket2.TabStop = false;
            this.grpMarket2.Text = "Market 2";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.DarkGreen;
            this.label2.Location = new System.Drawing.Point(490, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 22);
            this.label2.TabIndex = 69;
            this.label2.Text = "Price:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Crimson;
            this.label3.Location = new System.Drawing.Point(380, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 22);
            this.label3.TabIndex = 68;
            this.label3.Text = "Price:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label4.Location = new System.Drawing.Point(270, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 22);
            this.label4.TabIndex = 67;
            this.label4.Text = "Price:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(675, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 19);
            this.label5.TabIndex = 74;
            this.label5.Text = "Net:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(771, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 19);
            this.label6.TabIndex = 76;
            this.label6.Text = "Sells:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(723, 29);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 19);
            this.label7.TabIndex = 75;
            this.label7.Text = "Buys:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.DarkGreen;
            this.label9.Location = new System.Drawing.Point(601, 29);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(77, 22);
            this.label9.TabIndex = 73;
            this.label9.Text = "Total Vol:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.DarkGreen;
            this.label10.Location = new System.Drawing.Point(565, 29);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(38, 22);
            this.label10.TabIndex = 72;
            this.label10.Text = "Vol:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label11
            // 
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.Crimson;
            this.label11.Location = new System.Drawing.Point(454, 29);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(39, 22);
            this.label11.TabIndex = 71;
            this.label11.Text = "Vol:";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label12
            // 
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label12.Location = new System.Drawing.Point(344, 29);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(38, 22);
            this.label12.TabIndex = 70;
            this.label12.Text = "Vol:";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(470, 36);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 12);
            this.label8.TabIndex = 69;
            this.label8.Text = "Publish Address";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtPublishIP
            // 
            this.txtPublishIP.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPublishIP.Location = new System.Drawing.Point(570, 33);
            this.txtPublishIP.Name = "txtPublishIP";
            this.txtPublishIP.Size = new System.Drawing.Size(168, 21);
            this.txtPublishIP.TabIndex = 70;
            this.txtPublishIP.TextChanged += new System.EventHandler(this.txtPublishIP_TextChanged);
            // 
            // btnPublish
            // 
            this.btnPublish.Location = new System.Drawing.Point(743, 31);
            this.btnPublish.Name = "btnPublish";
            this.btnPublish.Size = new System.Drawing.Size(70, 25);
            this.btnPublish.TabIndex = 71;
            this.btnPublish.Text = "Bind";
            this.btnPublish.UseVisualStyleBackColor = true;
            this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(819, 31);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(50, 25);
            this.button1.TabIndex = 72;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(570, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 12);
            this.label1.TabIndex = 73;
            this.label1.Text = "样例:tcp://{IP}:{Port}";
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(879, 280);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtPublishIP);
            this.Controls.Add(this.btnPublish);
            this.Controls.Add(this.grpMarket2);
            this.Controls.Add(this.grpAccountPicker);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "T4.Publish.Server  V20201124";
            this.grpAccountPicker.ResumeLayout(false);
            this.grpAccountPicker.PerformLayout();
            this.grpMarket2.ResumeLayout(false);
            this.grpMarket2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.Run(new Form1());
        }
        #endregion
        #region Member Variables

        // Reference to the main api host object.
        internal Host moHost;

        //  Reference to the current account.
        internal Account moAccount;

        // Reference to the current exchange.
        internal Exchange moExchange;

        // Reference to the current contract.
        internal Contract moContract;

        // Reference to the current market.
        internal Market moPickerMarket;

        // References to selected markets.
        internal Market moMarket1;
        internal Market moMarket2;

        // References to marketid's retrieved from saved settings.
        internal string mstrMarketID1;
        internal string mstrMarketID2;

        #endregion

        private PublisherSocket publisher;

        #region " Initialization "

        // Initialize the application.
        private void Init()
        {

            //Trace.WriteLine("Init");
            //SetupMiscExamples();
            // Populate the available exchanges.
            //DisplayExchanges();
            // Populate the accounts.
            DisplayAccounts();
            // Register the accountlist events.
            moHost.Accounts.AccountDetails += new T4.API.AccountList.AccountDetailsEventHandler(moAccounts_AccountDetails);
            try
            {
                // Read saved markets.
                // XML Doc.
                XmlDocument oDoc;
                // XML Nodes for viewing the doc.
                XmlNode oMarkets;
                // Pull the xml doc from the server.
                oDoc = moHost.MasterUser.UserSettings;
                // Reference the saved markets via xml node.
                oMarkets = oDoc.ChildNodes[0];
                if (oMarkets != null)
                {
                    // Load the saved markets.
                    foreach (XmlNode oMarket in oMarkets)
                    {
                        // Check each child node for existance of saved markets.
                        switch (oMarket.Name)
                        {
                            case "market1":
                                mstrMarketID1 = oMarket.Attributes["MarketID"].Value;
                                // Get the market.
                                moHost.MarketData.GetMarket(mstrMarketID1, e =>
                                {
                                    // Subscribe to market1.
                                    if (e.Markets.Count > 0)
                                        NewMarketSubscription(ref moMarket1, e.Markets.First());
                                });
                                break;
                            case "market2":
                                mstrMarketID2 = oMarket.Attributes["MarketID"].Value;
                                // Get the market.
                                moHost.MarketData.GetMarket(mstrMarketID2, e =>
                                {
                                    // Subscribe to market1.
                                    if (e.Markets.Count > 0)
                                        NewMarketSubscription(ref moMarket2, e.Markets.First());
                                });
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Trace the exception.
                Trace.WriteLine("Error: " + ex.ToString());
            }
        }

        #endregion
        #region Account Data

        // Event that is raised when details for an account have 
        // changed, or a new account is recieved.
        private void moAccounts_AccountDetails(AccountDetailsEventArgs e)
        {
            //  Invoke the update.
            //  This places process on GUI thread.
            if (this.InvokeRequired)
            {
                Invoke(new AccountList.AccountDetailsEventHandler(OnAccountDetails), new Object[] { e });
            }
            else
            {
                OnAccountDetails(e);
            }
        }

        private void OnAccountDetails(AccountDetailsEventArgs e)
        {
            // Check to see if the account exists prior to adding/subscribing to it.
            if (e.Account.Subscribed != true)
            {
                // Add the account to the list.
                cboAccounts.Items.Add(e.Account);
                // Subscribe to the account.
                e.Account.Subscribe(e2 =>
                {
                    if (this.InvokeRequired)
                        Invoke(new OnAccountComplete(moAccounts_AccountComplete), new object[] { e2 });
                    else
                        moAccounts_AccountComplete(e2);
                });
            }
        }

        private void moAccounts_AccountComplete(AccountCompleteEventArgs e)
        {
            DisplayAccount();
            // Refresh positions.
            //DisplayPosition(moMarket1, 1);
            //DisplayPosition(moMarket2, 2);
            //DisplayOrders();
        }

        // Event that is raised when the accounts overall balance,
        // P&L or margin details have changed.
        private void moAccounts_AccountUpdate(AccountUpdateEventArgs e)
        {
            // Invoke the update.
            // This places process on GUI thread.
            if (this.InvokeRequired)
            {
                Invoke(new Account.AccountUpdateEventHandler(OnAccountUpdate), new Object[] { e });
            }
            else
            {
                OnAccountUpdate(e);
            }

        }

        private void OnAccountUpdate(AccountUpdateEventArgs e)
        {
            // Just refresh the current account.
            DisplayAccount();
        }

        private void DisplayAccounts()
        {

            try
            {
                // Lock the API.
                moHost.EnterLock();
                // Display the account list.
                foreach (Account oAccount in moHost.Accounts)
                {
                    OnAccountDetails(new AccountDetailsEventArgs(oAccount));
                }
                if (cboAccounts.Items.Count > 0)
                {
                    cboAccounts.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                // Trace Errors.
                Trace.WriteLine(ex.ToString());
            }
            finally
            {
                // Unlock the api.
                moHost.ExitLock();
            }
        }

        //' Event that is raised when positions for accounts have changed.
        private void moAccounts_PositionUpdate(PositionUpdateEventArgs e)
        {

            // Display the position details.

            // If the position is for the current account
            // then update the value.

            if (e.Account == moAccount)
            {
                // Invoke the update.
                // This places process on GUI thread.
                // Must use a delegate to pass arguments.
                if (this.InvokeRequired)
                {
                    this.Invoke(new Account.PositionUpdateEventHandler(OnPositionUpdate), new object[] { e });
                }
                else
                {
                    OnPositionUpdate(e);
                }
            }
        }

        private void OnPositionUpdate(PositionUpdateEventArgs e)
        {
            // Display the position details.
            if (e.Position.Market == moMarket1)
                DisplayPosition(e.Position.Market, 1);
            else if (e.Position.Market == moMarket2)
                DisplayPosition(e.Position.Market, 2);
        }

        private void DisplayAccount()
        {

            if ((moAccount != null))
            {

                try
                {
                    // Display the current account balance.
                    txtCash.Text = String.Format("{0:#,###,##0.00}", moAccount.AvailableCash);

                }
                catch (Exception ex)
                {
                    // Trace the error.
                    Trace.WriteLine("Error: " + ex.ToString());

                }
            }
        }

        private void DisplayPosition(Market poMarket, int piID)
        {
            string strNet = "";
            string strBuys = "";
            string strSells = "";

            try
            {

                if ((poMarket != null) && (moAccount != null))
                {

                    // Display positions for current account and market1.

                    // Reference the market's positions.
                    Position oPosition = moAccount.Positions[poMarket.MarketID];

                    if ((oPosition != null))
                    {
                        // Reference the net position.
                        strNet = oPosition.Net.ToString();
                        strBuys = oPosition.Buys.ToString();
                        strSells = oPosition.Sells.ToString();
                    }

                    switch (piID)
                    {
                        case 1:

                            // Display the net position.
                            //txtNet1.Text = strNet;
                            // Display the total Buys.
                            //txtBuys1.Text = strBuys;
                            // Display the total Sells.
                            //txtSells1.Text = strSells;

                            break;
                        case 2:

                            // Display the net position.
                            txtNet2.Text = strNet;
                            // Display the total Buys.
                            txtBuys2.Text = strBuys;
                            // Display the total Sells.
                            txtSells2.Text = strSells;

                            break;
                    }

                }

            }
            catch (Exception ex)
            {
                // Trace the error.
                Trace.WriteLine("Error " + ex.ToString());

            }
        }

        private void cboAccounts_SelectedIndexChanged(Object sender, System.EventArgs e)
        {

            if ((cboAccounts.SelectedItem != null))
            {
                // Reference the current account.
                moAccount = (Account)cboAccounts.SelectedItem;

                // Register the account's events.
                if (moAccount != null)
                {
                    //moAccount.OrderUpdate += new T4.API.Account.OrderUpdateEventHandler(moAccount_OrderUpdate);
                    //moAccount.PositionUpdate += new T4.API.Account.PositionUpdateEventHandler(moAccounts_PositionUpdate);
                    moAccount.AccountUpdate += new T4.API.Account.AccountUpdateEventHandler(moAccounts_AccountUpdate);

                    // Display the current account balance.
                    DisplayAccount();

                    // Refresh positions.
                    //DisplayPosition(moMarket1, 1);
                    //DisplayPosition(moMarket2, 2);

                }

            }

        }


        #endregion

        #region Startup and shutdown code

        // Initialise the api when the application starts.
        private void frmMain_Load(object sender, System.EventArgs e)
        {
            //moHost = new Host(APIServerType.Live, "unieap_longjiao", "7D7E1899-1A23-4D22-B8FC-BDC1922FF1CD", "Phillip", "ZuoweiC", "caiBO12345!");
            moHost = Host.Login(APIServerType.Simulator, "T4Example", "112A04B0-5AAF-42F4-994E-FA7CB959C60B");
            //moHost = Host.Login(APIServerType.Live, "unieap_longjiao", "7D7E1899-1A23-4D22-B8FC-BDC1922FF1CD");
            //moHost = Host.Login(APIServerType.Simulator, "unieap_longjiao", "43043E7A-CBB1-410A-9101-D58B3D4FAC77");
            // Check for success.
            if (moHost == null)
            {
                // Host object not returned which means the user cancelled the login dialog.
                this.Close();
            }
            else
            {
                // Login was successfull.
                //Trace.WriteLine("Login Success");
                // Initialize.
                Init();
                LoadConfig();

            }

        }
        public string rootPath = "C:\\iAutoTrade";
        /// <summary>
        /// 系统配置文件
        /// </summary>
        public string _ConfigFile = "\\Config.json";
        public Config _Config;
        string _DSType = "T4";
        string _BindAddress = "";
        //string _AccountName = "";
        //string _Password = "";
        private void LoadConfig()
        {
            string newConfigFile = rootPath + _ConfigFile;
            if (System.IO.File.Exists(newConfigFile))
            {
                _Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(newConfigFile));
                if (_Config != null)
                {
                    if (string.IsNullOrEmpty(_BindAddress))
                    {
                        string address = string.Format("tcp://{0}:{1}", GetLocalIP(), "5558");
                        _BindAddress = address;
                    }
                }
                InitialPara();
            }
        }
        private void InitialPara()
        {
            txtPublishIP.Text = _BindAddress;
            //tbLogin.Text = _AccountName;
            //tbPassword.Text = _Password;

        }
        // Shutdown the api when the application exits.
        private void frmMain_Closed(object sender, System.EventArgs e)
        {

            // Check to see that we have an api object.
            if (moHost != null)
            {

                // Unregister events.

                // Markets.
                if (moMarket1 != null)
                {
                    moMarket1.MarketCheckSubscription -= new T4.API.Market.MarketCheckSubscriptionEventHandler(Markets_MarketCheckSubscription);
                    moMarket1.MarketDepthUpdate -= new T4.API.Market.MarketDepthUpdateEventHandler(Markets_MarketDepthUpdate);
                }
                if (moMarket2 != null)
                {
                    moMarket2.MarketCheckSubscription -= new T4.API.Market.MarketCheckSubscriptionEventHandler(Markets_MarketCheckSubscription);
                    moMarket2.MarketDepthUpdate -= new T4.API.Market.MarketDepthUpdateEventHandler(Markets_MarketDepthUpdate);
                }

                // Account events.
                moHost.Accounts.AccountDetails -= new T4.API.AccountList.AccountDetailsEventHandler(moAccounts_AccountDetails);

                if (moAccount != null)
                {
                    //moAccount.OrderUpdate -= new T4.API.Account.OrderUpdateEventHandler(moAccount_OrderUpdate);
                    //moAccount.PositionUpdate -= new T4.API.Account.PositionUpdateEventHandler(moAccounts_PositionUpdate);
                    moAccount.AccountUpdate -= new T4.API.Account.AccountUpdateEventHandler(moAccounts_AccountUpdate);
                }

                // Dispose of the api.
                moHost.Dispose();
                moHost = null;
            }
        }

        #endregion

        #region Market Subscription 

        private void cmdGet1_Click(System.Object sender, System.EventArgs e)
        {

            // Clear the values.
            DisplayMarketDetails(null, 1);

            // Subscribe to market1.
            NewMarketSubscription(ref moMarket1, moPickerMarket);

            // Start the morket mode countdown.
            //StartModeCountdown();

            // Refresh the positions.
            //DisplayPosition(moMarket1, 1);

        }


        private void cmdGet2_Click(System.Object sender, System.EventArgs e)
        {
            //Market oMarket = moHost.MarketData.MarketPicker(ref moMarket2);

            Market oMarket = moHost.MarketData.MarketPicker(new List<ContractType>() { ContractType.Future }, new List<StrategyType>() { StrategyType.Any }, ref moMarket2, "Gold");

            // Clear the values.
            DisplayMarketDetails(null, 2);

            // Subscribe to market2.
            NewMarketSubscription(ref moMarket2, oMarket);

            // Refresh the positions.
            DisplayPosition(moMarket2, 2);

        }

        private void NewMarketSubscription(ref Market poMarket, Market poNewMarket)
        {
            // Update an existing market reference to subscribe to a new/different market.

            // If they are the same then don't do anything.
            // We don't need to resubscribe to the same market.

            // Explicitly register events as opposed to declaring withevents.
            // This gives us more control.  
            // It is important to unregister the marketchecksubscription prior to unsubscribing or the event will override and maintain the subscription.


            if ((!object.ReferenceEquals(poMarket, poNewMarket)))
            {
                // Unsubscribe from the currently selected market.
                if ((poMarket != null))

                {
                    // Unregister the events for this market.
                    poMarket.MarketCheckSubscription -= new T4.API.Market.MarketCheckSubscriptionEventHandler(Markets_MarketCheckSubscription);
                    poMarket.MarketDepthUpdate -= new T4.API.Market.MarketDepthUpdateEventHandler(Markets_MarketDepthUpdate);

                    poMarket.DepthUnsubscribe();

                }

                // Update the market reference.
                poMarket = poNewMarket;

                if ((poMarket != null))
                {

                    // Register the events.
                    poMarket.MarketCheckSubscription += new T4.API.Market.MarketCheckSubscriptionEventHandler(Markets_MarketCheckSubscription);
                    poMarket.MarketDepthUpdate += new T4.API.Market.MarketDepthUpdateEventHandler(Markets_MarketDepthUpdate);

                    // Subscribe to the market.
                    // Use smart buffering.
                    poMarket.DepthSubscribe(DepthBuffer.Smart, DepthLevels.BestOnly);

                }

            }

        }

        private void Markets_MarketCheckSubscription(MarketCheckSubscriptionEventArgs e)
        {
            // No need to invoke on the gui thread.
            e.DepthSubscribeAtLeast(DepthBuffer.Smart, DepthLevels.BestOnly);

        }

        private void Markets_MarketDepthUpdate(MarketDepthUpdateEventArgs e)
        {
            // Invoke the update.
            // This places process on GUI thread.
            // Must use a delegate to pass arguments.
            if (this.InvokeRequired)
            {
                this.Invoke(new Market.MarketDepthUpdateEventHandler(OnMarketDepthUpdate), new object[] { e });
            }
            else
            {
                OnMarketDepthUpdate(e);
            }

        }

        private void OnMarketDepthUpdate(MarketDepthUpdateEventArgs e)
        {

            try
            {

                if (e.Market == moMarket1)
                {
                    DisplayMarketDetails(e.Market, 1);
                }
                else if (e.Market == moMarket2)
                {
                    DisplayMarketDetails(e.Market, 2);

                    sendPrice(e.Market);
                }




            }
            catch (Exception ex)
            {
                // Trace the error.
                Trace.WriteLine("Error " + ex.ToString());

            }
        }

        private void sendPrice(Market poMarket)
        {

            if (publisher != null)//&& e.Market.ContractID == "@GGC"
            {
                string strDescription = "";
                string strBid = "";
                string strBidVol = "";
                string strOffer = "";
                string strOfferVol = "";
                string strLast = "";
                string strLastVol = "";
                string strLastVolTotal = "";

                if ((poMarket != null))
                {

                    try
                    {
                        // Display the market description.
                        strDescription = poMarket.Description;

                        MarketDepth d = poMarket.GetDepth();
                        MarketTrade t = poMarket.GetTrade();

                        decimal Bid = d.Bids[0].Price / 10;
                        decimal Ask = d.Offers[0].Price / 10;
                        strLastVolTotal = t.TotalVolume.ToString();
                        string symbol = "Gold";

                        publisher.SendFrame($"{symbol} {Bid} {Ask} {strLastVolTotal}");

                        // Best bid.
                        if (d.Bids.Count > 0)
                        {
                            strBid = poMarket.PriceToDisplay(d.Bids[0].Price);
                            strBidVol = d.Bids[0].Volume.ToString();
                        }

                        // Best offer.
                        if (d.Offers.Count > 0)
                        {
                            strOffer = poMarket.PriceToDisplay(d.Offers[0].Price);
                            strOfferVol = d.Offers[0].Volume.ToString();
                        }

                        // Last trade.
                        strLast = poMarket.PriceToDisplay(t.Price);
                        strLastVol = t.Volume.ToString();
                        //strLastVolTotal = t.TotalVolume.ToString();

                    }
                    catch (Exception ex)
                    {
                        // Trace the error.
                        Trace.WriteLine("Error " + ex.ToString());

                    }

                }
            }
        }



        /// <summary>
        /// Update the market display values.
        /// </summary>

        private void DisplayMarketDetails(Market poMarket, int piID)
        {
            string strDescription = "";
            string strBid = "";
            string strBidVol = "";
            string strOffer = "";
            string strOfferVol = "";
            string strLast = "";
            string strLastVol = "";
            string strLastVolTotal = "";

            if ((poMarket != null))
            {

                try
                {
                    // Display the market description.
                    strDescription = poMarket.Description;

                    MarketDepth d = poMarket.GetDepth();

                    // Best bid.
                    if (d.Bids.Count > 0)
                    {
                        strBid = poMarket.PriceToDisplay(d.Bids[0].Price);
                        strBidVol = d.Bids[0].Volume.ToString();
                    }

                    // Best offer.
                    if (d.Offers.Count > 0)
                    {
                        strOffer = poMarket.PriceToDisplay(d.Offers[0].Price);
                        strOfferVol = d.Offers[0].Volume.ToString();
                    }

                    MarketTrade t = poMarket.GetTrade();

                    // Last trade.
                    strLast = poMarket.PriceToDisplay(t.Price);
                    strLastVol = t.Volume.ToString();
                    strLastVolTotal = t.TotalVolume.ToString();


                }
                catch (Exception ex)
                {
                    // Trace the error.
                    Trace.WriteLine("Error " + ex.ToString());

                }

            }

            switch (piID)
            {
                case 1:

                    // Update the market1 display values.
                    //txtMarketDescription1.Text = strDescription;
                    //txtBid1.Text = strBid;
                    //txtBidVol1.Text = strBidVol;
                    //txtOffer1.Text = strOffer;
                    //txtOfferVol1.Text = strOfferVol;
                    //txtLast1.Text = strLast;
                    //txtLastVol1.Text = strLastVol;
                    //txtLastVolTotal1.Text = strLastVolTotal;

                    break;
                case 2:

                    // Update the market2 display values.
                    txtMarketDescription2.Text = strDescription;
                    txtBid2.Text = strBid;
                    txtBidVol2.Text = strBidVol;
                    txtOffer2.Text = strOffer;
                    txtOfferVol2.Text = strOfferVol;
                    txtLast2.Text = strLast;
                    txtLastVol2.Text = strLastVol;
                    txtLastVolTotal2.Text = strLastVolTotal;

                    break;
            }

        }

        #endregion

        #region Save Settings

        private void cmdSave_Click(System.Object sender, System.EventArgs e)
        {
            try
            {

                // XML Doc.
                XmlDocument oDoc = new XmlDocument();

                // XML Node.
                XmlNode oMarket;
                XmlNode oMarkets;
                XmlAttribute oAttribute;

                // Create the main node.
                oMarkets = oDoc.CreateNode(XmlNodeType.Element, "markets", "");
                oDoc.AppendChild(oMarkets);

                if (moMarket1 != null)
                {

                    // Create a node.
                    oMarket = oDoc.CreateNode(XmlNodeType.Element, "market1", "");

                    // Market ID.
                    oAttribute = oDoc.CreateAttribute("MarketID");
                    oAttribute.Value = moMarket1.MarketID;
                    oMarket.Attributes.Append(oAttribute);

                    // Add the node to the xml document.
                    oMarkets.AppendChild(oMarket);
                }

                if (moMarket2 != null)
                {

                    // Create a node.
                    oMarket = oDoc.CreateNode(XmlNodeType.Element, "market2", "");

                    // Market ID.
                    oAttribute = oDoc.CreateAttribute("MarketID");
                    oAttribute.Value = moMarket2.MarketID;
                    oMarket.Attributes.Append(oAttribute);

                    // Add the node to the xml document.
                    oMarkets.AppendChild(oMarket);

                }

                // Save the xml to the server.
                moHost.MasterUser.UserSettings = oDoc;
                moHost.MasterUser.SaveUserSettings();
            }
            catch (Exception ex)
            {
                // Trace.
                Trace.WriteLine(ex.ToString());
            }

        }

        public string App_Path()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }


        #endregion

        #region Single Order

        // Method that submits a single order.
        private void SubmitSingleOrder(Market poMarket, BuySell peBuySell, decimal pdecLimitPrice)
        {
            if (moAccount != null && poMarket != null)
            {

                // Submit an order.
                Order oOrder = moHost.SubmitOrder(
                    moAccount,
                    poMarket,
                    peBuySell,
                    PriceType.Limit,
                    1,
                    pdecLimitPrice);

                // Display the orders.
                //DisplayOrders();

            }
        }

        // Pull the single order that was submitted.
        private void PullSingleOrder(Order poOrder)
        {
            // Check to see that we have an order.
            if (poOrder != null)
            {
                // Check to see if the order is working.
                if (poOrder.IsWorking)
                {
                    // Pull the order.
                    poOrder.Pull();
                }
            }
        }


        #endregion

        #region Submission/Cancelation

        private void cmdSell2_Click(System.Object sender, System.EventArgs e)
        {
            // Submit a single order.
            decimal decPrice = 0;
            if (decimal.TryParse(txtOffer2.Text, out decPrice))
            {
                SubmitSingleOrder(moMarket2, BuySell.Sell, decPrice);
            }
        }

        private void cmdBuy2_Click(System.Object sender, System.EventArgs e)
        {
            // Submit a single order.
            decimal decPrice = 0;
            if (decimal.TryParse(txtBid2.Text, out decPrice))
            {
                SubmitSingleOrder(moMarket2, BuySell.Buy, decPrice);
            }
        }

        #endregion

        #region  Order Data

        private void DisplayOrders()
        {
            try
            {

                // Lock the api.
                moHost.EnterLock();

                // Suspend the layout of the listbox.
                //lstOrders.SuspendLayout();

                // Clear and repopulate the list.
                //lstOrders.Items.Clear();

                // Itterate through the orders, newest is first.
                foreach (Order oOrder in moAccount.Orders.GetSortedList())
                {
                    // Display some order details.
                    //lstOrders.Items.Add(new OrderInfo(oOrder));

                }
            }
            catch (Exception ex)
            {
                // Trace the error.
                Trace.WriteLine("Error: " + ex.ToString());
            }
            finally
            {
                // Unlock the api.
                moHost.ExitLock();

                // Resume layout of the listbox.
                //lstOrders.ResumeLayout();
            }
        }

        #endregion

        #region Misc Examples

        const string AUTOOCO = "Submit Auto OCO";
        const string FIVETICKSOFF = "Work 5 Ticks Off Market";


        private void cmdRunMisc2_Click(Object sender, System.EventArgs e)
        {
            if (moMarket2 != null)
            {

                switch (cboMisc2.Text)
                {
                    case AUTOOCO:
                        {
                            // Run autooco sample code.
                            SubmitAOCO(moMarket2, BuySell.Sell, txtOffer2.Text);
                            break;
                        }
                    case FIVETICKSOFF:
                        {
                            // Run the five ticks off code.
                            SubmitFiveTicksOff(moMarket2, BuySell.Sell, txtOffer2.Text);
                            break;
                        }
                }
            }
        }

        #region Auto OCO

        // Simple example of how to submit and cancel an Auto OCO.
        private void SubmitAOCO(Market poMarket, BuySell peBuySell, string pstrLimitDisplayPrice)
        {
            if (moAccount != null && poMarket != null)
            {

                // Limit price reference.
                decimal decLimitPrice = 0;
                if (decimal.TryParse(pstrLimitDisplayPrice, out decLimitPrice))
                {
                    // Create the batch submission object for AutoOCO
                    OrderSubmissionBatch oBatch = moHost.GetOrderSubmission(OrderLink.AutoOCO);

                    // Add an order to the batch.
                    // This is the trigger order.
                    oBatch.Add(
                        moAccount,
                        poMarket,
                        peBuySell,
                        PriceType.Limit,
                        1,
                        decLimitPrice);

                    // Add an order to the batch.
                    // This is the sell limit of the oco above the market.
                    // Note the flip of Buy/Sell.
                    // Note the ticks is a distance not a price representation.
                    oBatch.Add(
                        moAccount,
                        poMarket,
                        (BuySell)(-(int)peBuySell),
                        PriceType.Limit,
                        0,
                        poMarket.AddPriceIncrements(5 * (int)peBuySell, 0));

                    // Add an order to the batch.
                    // This is the stop of the oco below the market.
                    // Note the flip of Buy/Sell.
                    // Note the ticks is a distance not a price representation.
                    oBatch.Add(
                        moAccount,
                        poMarket,
                        (BuySell)(-(int)peBuySell),
                        PriceType.StopMarket,
                        0,
                        null,
                        poMarket.AddPriceIncrements(-5 * (int)peBuySell, 0),
                        "");


                    // Submit the batch.
                    List<Order> oSent = oBatch.Send();

                    // Display the orders.
                    DisplayOrders();

                }
            }
        }

        #endregion

        #region  Work Order Five Ticks From Market

        // Place an order five ticks off the market.
        private void SubmitFiveTicksOff(Market poMarket, BuySell peBuySell, string pstrLimitDisplayPrice)
        {
            // Limit price reference.
            decimal decLimitPrice = 0;
            if (decimal.TryParse(pstrLimitDisplayPrice, out decLimitPrice))
            {
                // Add or subtract five ticks from the current price depending on what side of the market we are.
                if (peBuySell == BuySell.Buy)
                    decLimitPrice = poMarket.AddPriceIncrements(-5, decLimitPrice);
                else
                    decLimitPrice = poMarket.AddPriceIncrements(5, decLimitPrice);

                // Submit a single order five ticks off the market.
                SubmitSingleOrder(poMarket, peBuySell, decLimitPrice);
            }

        }

        #endregion

        #endregion

        #region Market Mode Countdown 

        /// <summary>
        ///     ''' Timer for providing a countdown to the next market mode change.
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        private System.Threading.Timer moModeCountdown;

        /// <summary>
        ///     ''' Timer interval when no countdown is currently in process.
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        private const int mciModeCountdownSlow = 1000;

        /// <summary>
        ///     ''' Timer interval while we are counting down.
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        private const int mciModeCountdownFast = 200;

        /// <summary>
        ///     ''' The number of seconds to countdown to the next market mode change.
        ///     ''' For test purposes set to 24 hours so we can see it function.
        ///     ''' </summary>
        private const int mciCountDownSeconds = 86400;

        /// <summary>
        ///     ''' The timer interval we are currently using.
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        private int miModeCountdownInterval = mciModeCountdownSlow;

        /// <summary>
        ///     ''' Start the market mode countdown support.
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        private void StartModeCountdown()
        {
            //moModeCountdown = new System.Threading.Timer(OnModeCountdown, null, 1000, miModeCountdownInterval);
        }

        #endregion

        private void btnPublish_Click(object sender, EventArgs e)
        {
            if (btnPublish.Text.ToLower() == "bind")
            {
                if (string.IsNullOrEmpty(_BindAddress))
                {
                    string address = string.Format("tcp://{0}:{1}", GetLocalIP(), "5558");
                    _BindAddress = address;
                }

                if (_BindAddress != null)
                {
                    try
                    {

                        Trace.WriteLine("bind to :" + _BindAddress);
                        publisher = new PublisherSocket();
                        publisher.Bind(_BindAddress);

                        btnPublish.Text = "Unbind";//172.31.47.203
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Bind Error:" + ex.Message);
                        Trace.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                try
                {
                    if (publisher != null)
                    {
                        Trace.WriteLine("Unbind to :" + _BindAddress);
                        publisher.Unbind(_BindAddress);
                        btnPublish.Text = "Bind";

                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("UnBind Error:" + ex.Message);
                    Trace.WriteLine(ex.Message);
                }

            }
        }

        public  string GetLocalIP()
        {
            string name = System.Net.Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            foreach (IPAddress ipa in ipadrlist)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipa.ToString();
                }
            }
            return "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveConfig();
            MessageBox.Show("保存成功");
        }
        private void SaveConfig()
        {
            if ("OEC".Equals(_DSType))
            {
                _Config.BindAddressOEC = _BindAddress;
            }
            if ("T4".Equals(_DSType))
            {
                _Config.BindAddressT4 = _BindAddress;
            }
            _Config.DSType = _DSType;
            //_config.AccountName = _AccountName;
            //_config.Password = _Password; 
            string newConfigFile = rootPath + _ConfigFile;
            System.IO.File.WriteAllText(newConfigFile, JsonConvert.SerializeObject(_Config));
        }

        private void txtPublishIP_TextChanged(object sender, EventArgs e)
        {
            _BindAddress = txtPublishIP.Text;
        }
    }
}
