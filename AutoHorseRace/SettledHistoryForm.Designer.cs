namespace AutoHorseRace
{
    partial class SettledHistoryForm
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
            dataGridViewSettledHistoryInfo = new DataGridView();
            Column8 = new DataGridViewCheckBoxColumn();
            Date = new DataGridViewTextBoxColumn();
            Location = new DataGridViewTextBoxColumn();
            OddsType = new DataGridViewTextBoxColumn();
            StakeAmount = new DataGridViewTextBoxColumn();
            Tax = new DataGridViewTextBoxColumn();
            WinLoss = new DataGridViewTextBoxColumn();
            dataGridViewSettledSummaryInfo = new DataGridView();
            Column1 = new DataGridViewCheckBoxColumn();
            RaceNo = new DataGridViewTextBoxColumn();
            BetStakeAmount = new DataGridViewTextBoxColumn();
            BetAmount = new DataGridViewTextBoxColumn();
            BetTax = new DataGridViewTextBoxColumn();
            BetRefund = new DataGridViewTextBoxColumn();
            EatStakeAmount = new DataGridViewTextBoxColumn();
            EatAmount = new DataGridViewTextBoxColumn();
            EatTax = new DataGridViewTextBoxColumn();
            EatRefund = new DataGridViewTextBoxColumn();
            SettledSummaryWinLoss = new DataGridViewTextBoxColumn();
            label1 = new Label();
            label2 = new Label();
            dataGridViewRaceBettingRecordInfo = new DataGridView();
            label3 = new Label();
            pictureBoxSettledHistoryProcessing = new PictureBox();
            pictureBoxSettledSummaryProcessing = new PictureBox();
            dataGridViewCheckBoxColumn1 = new DataGridViewCheckBoxColumn();
            ColumnRaceNo = new DataGridViewTextBoxColumn();
            type = new DataGridViewTextBoxColumn();
            horses = new DataGridViewTextBoxColumn();
            stake = new DataGridViewTextBoxColumn();
            pct = new DataGridViewTextBoxColumn();
            limit = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxTax = new DataGridViewTextBoxColumn();
            bet_payout = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxOdds = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn9 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxWinLoss = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxAction = new DataGridViewTextBoxColumn();
            actDate = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridViewSettledHistoryInfo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewSettledSummaryInfo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewRaceBettingRecordInfo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSettledHistoryProcessing).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSettledSummaryProcessing).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewSettledHistoryInfo
            // 
            dataGridViewSettledHistoryInfo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewSettledHistoryInfo.Columns.AddRange(new DataGridViewColumn[] { Column8, Date, Location, OddsType, StakeAmount, Tax, WinLoss });
            dataGridViewSettledHistoryInfo.Location = new Point(2, 33);
            dataGridViewSettledHistoryInfo.Name = "dataGridViewSettledHistoryInfo";
            dataGridViewSettledHistoryInfo.RowHeadersVisible = false;
            dataGridViewSettledHistoryInfo.Size = new Size(957, 171);
            dataGridViewSettledHistoryInfo.TabIndex = 0;
            dataGridViewSettledHistoryInfo.CellFormatting += dataGridViewSettledHistoryInfo_CellFormatting;
            dataGridViewSettledHistoryInfo.SelectionChanged += dataGridViewSettledHistoryInfo_SelectionChanged;
            // 
            // Column8
            // 
            Column8.HeaderText = "";
            Column8.Name = "Column8";
            Column8.Width = 30;
            // 
            // Date
            // 
            Date.DataPropertyName = "Date";
            Date.HeaderText = "日期";
            Date.Name = "Date";
            Date.Width = 150;
            // 
            // Location
            // 
            Location.DataPropertyName = "Location";
            Location.HeaderText = "地点";
            Location.Name = "Location";
            Location.Width = 150;
            // 
            // OddsType
            // 
            OddsType.DataPropertyName = "OddsType";
            OddsType.HeaderText = "赔率";
            OddsType.Name = "OddsType";
            // 
            // StakeAmount
            // 
            StakeAmount.DataPropertyName = "stake";
            StakeAmount.HeaderText = "总票";
            StakeAmount.Name = "StakeAmount";
            // 
            // Tax
            // 
            Tax.DataPropertyName = "Tax";
            Tax.HeaderText = "总税";
            Tax.Name = "Tax";
            // 
            // WinLoss
            // 
            WinLoss.DataPropertyName = "WinLoss";
            WinLoss.HeaderText = "输赢";
            WinLoss.Name = "WinLoss";
            // 
            // dataGridViewSettledSummaryInfo
            // 
            dataGridViewSettledSummaryInfo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewSettledSummaryInfo.Columns.AddRange(new DataGridViewColumn[] { Column1, RaceNo, BetStakeAmount, BetAmount, BetTax, BetRefund, EatStakeAmount, EatAmount, EatTax, EatRefund, SettledSummaryWinLoss });
            dataGridViewSettledSummaryInfo.Location = new Point(2, 235);
            dataGridViewSettledSummaryInfo.Name = "dataGridViewSettledSummaryInfo";
            dataGridViewSettledSummaryInfo.RowHeadersVisible = false;
            dataGridViewSettledSummaryInfo.Size = new Size(957, 275);
            dataGridViewSettledSummaryInfo.TabIndex = 1;
            dataGridViewSettledSummaryInfo.CellFormatting += dataGridViewSettledSummaryInfo_CellFormatting;
            // 
            // Column1
            // 
            Column1.HeaderText = "";
            Column1.Name = "Column1";
            Column1.Resizable = DataGridViewTriState.True;
            Column1.SortMode = DataGridViewColumnSortMode.Automatic;
            Column1.Width = 30;
            // 
            // RaceNo
            // 
            RaceNo.DataPropertyName = "RaceNo";
            RaceNo.HeaderText = "场次";
            RaceNo.Name = "RaceNo";
            RaceNo.Width = 60;
            // 
            // BetStakeAmount
            // 
            BetStakeAmount.DataPropertyName = "betStake";
            BetStakeAmount.HeaderText = "赌票";
            BetStakeAmount.Name = "BetStakeAmount";
            // 
            // BetAmount
            // 
            BetAmount.DataPropertyName = "BetAmount";
            BetAmount.HeaderText = "$(赌)";
            BetAmount.Name = "BetAmount";
            BetAmount.Width = 80;
            // 
            // BetTax
            // 
            BetTax.DataPropertyName = "BetTax";
            BetTax.HeaderText = "赌税";
            BetTax.Name = "BetTax";
            BetTax.Width = 80;
            // 
            // BetRefund
            // 
            BetRefund.DataPropertyName = "BetRefund";
            BetRefund.HeaderText = "赌赔出数目";
            BetRefund.Name = "BetRefund";
            // 
            // EatStakeAmount
            // 
            EatStakeAmount.DataPropertyName = "eatStake";
            EatStakeAmount.HeaderText = "吃票";
            EatStakeAmount.Name = "EatStakeAmount";
            // 
            // EatAmount
            // 
            EatAmount.DataPropertyName = "EatAmount";
            EatAmount.HeaderText = "$(吃)";
            EatAmount.Name = "EatAmount";
            // 
            // EatTax
            // 
            EatTax.DataPropertyName = "EatTax";
            EatTax.HeaderText = "吃税";
            EatTax.Name = "EatTax";
            EatTax.Width = 80;
            // 
            // EatRefund
            // 
            EatRefund.DataPropertyName = "EatRefund";
            EatRefund.HeaderText = "吃赔出数目";
            EatRefund.Name = "EatRefund";
            // 
            // SettledSummaryWinLoss
            // 
            SettledSummaryWinLoss.DataPropertyName = "WinLoss";
            SettledSummaryWinLoss.HeaderText = "输赢";
            SettledSummaryWinLoss.Name = "SettledSummaryWinLoss";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(5, 9);
            label1.Name = "label1";
            label1.Size = new Size(68, 17);
            label1.TabIndex = 2;
            label1.Text = "已结算交易";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(6, 210);
            label2.Name = "label2";
            label2.Size = new Size(56, 17);
            label2.TabIndex = 3;
            label2.Text = "交易总表";
            // 
            // dataGridViewRaceBettingRecordInfo
            // 
            dataGridViewRaceBettingRecordInfo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewRaceBettingRecordInfo.Columns.AddRange(new DataGridViewColumn[] { dataGridViewCheckBoxColumn1, ColumnRaceNo, type, horses, stake, pct, limit, dataGridViewTextBoxTax, bet_payout, dataGridViewTextBoxOdds, dataGridViewTextBoxColumn9, dataGridViewTextBoxWinLoss, dataGridViewTextBoxAction, actDate });
            dataGridViewRaceBettingRecordInfo.Location = new Point(2, 536);
            dataGridViewRaceBettingRecordInfo.Name = "dataGridViewRaceBettingRecordInfo";
            dataGridViewRaceBettingRecordInfo.RowHeadersVisible = false;
            dataGridViewRaceBettingRecordInfo.Size = new Size(957, 433);
            dataGridViewRaceBettingRecordInfo.TabIndex = 4;
            dataGridViewRaceBettingRecordInfo.CellFormatting += dataGridViewRaceBettingRecordInfo_CellFormatting;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(5, 515);
            label3.Name = "label3";
            label3.Size = new Size(56, 17);
            label3.TabIndex = 5;
            label3.Text = "交易记录";
            // 
            // pictureBoxSettledHistoryProcessing
            // 
            pictureBoxSettledHistoryProcessing.Location = new Point(76, 3);
            pictureBoxSettledHistoryProcessing.Name = "pictureBoxSettledHistoryProcessing";
            pictureBoxSettledHistoryProcessing.Size = new Size(28, 28);
            pictureBoxSettledHistoryProcessing.TabIndex = 64;
            pictureBoxSettledHistoryProcessing.TabStop = false;
            pictureBoxSettledHistoryProcessing.Paint += pictureBoxSettledHistoryProcessing_Paint;
            // 
            // pictureBoxSettledSummaryProcessing
            // 
            pictureBoxSettledSummaryProcessing.Location = new Point(64, 205);
            pictureBoxSettledSummaryProcessing.Name = "pictureBoxSettledSummaryProcessing";
            pictureBoxSettledSummaryProcessing.Size = new Size(28, 28);
            pictureBoxSettledSummaryProcessing.TabIndex = 65;
            pictureBoxSettledSummaryProcessing.TabStop = false;
            pictureBoxSettledSummaryProcessing.Paint += pictureBoxSettledSummaryProcessing_Paint;
            // 
            // dataGridViewCheckBoxColumn1
            // 
            dataGridViewCheckBoxColumn1.HeaderText = "";
            dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            dataGridViewCheckBoxColumn1.Resizable = DataGridViewTriState.True;
            dataGridViewCheckBoxColumn1.SortMode = DataGridViewColumnSortMode.Automatic;
            dataGridViewCheckBoxColumn1.Width = 30;
            // 
            // ColumnRaceNo
            // 
            ColumnRaceNo.DataPropertyName = "RaceNo";
            ColumnRaceNo.HeaderText = "场次";
            ColumnRaceNo.Name = "ColumnRaceNo";
            ColumnRaceNo.Width = 60;
            // 
            // type
            // 
            type.DataPropertyName = "type";
            type.HeaderText = "类式";
            type.Name = "type";
            type.Width = 60;
            // 
            // horses
            // 
            horses.DataPropertyName = "horses";
            horses.HeaderText = "马";
            horses.Name = "horses";
            horses.Width = 80;
            // 
            // stake
            // 
            stake.DataPropertyName = "stake";
            stake.HeaderText = "票数$";
            stake.Name = "stake";
            stake.Width = 70;
            // 
            // pct
            // 
            pct.DataPropertyName = "pct";
            pct.HeaderText = "%";
            pct.Name = "pct";
            pct.Width = 35;
            // 
            // limit
            // 
            limit.DataPropertyName = "limit";
            limit.HeaderText = "限额";
            limit.Name = "limit";
            limit.Width = 60;
            // 
            // dataGridViewTextBoxTax
            // 
            dataGridViewTextBoxTax.DataPropertyName = "tax";
            dataGridViewTextBoxTax.HeaderText = "税";
            dataGridViewTextBoxTax.Name = "dataGridViewTextBoxTax";
            dataGridViewTextBoxTax.Width = 50;
            // 
            // bet_payout
            // 
            bet_payout.DataPropertyName = "betPayout";
            bet_payout.HeaderText = "总数";
            bet_payout.Name = "bet_payout";
            bet_payout.Width = 80;
            // 
            // dataGridViewTextBoxOdds
            // 
            dataGridViewTextBoxOdds.DataPropertyName = "odds";
            dataGridViewTextBoxOdds.HeaderText = "赔率";
            dataGridViewTextBoxOdds.Name = "dataGridViewTextBoxOdds";
            dataGridViewTextBoxOdds.Width = 60;
            // 
            // dataGridViewTextBoxColumn9
            // 
            dataGridViewTextBoxColumn9.DataPropertyName = "(none)";
            dataGridViewTextBoxColumn9.HeaderText = "总数";
            dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            dataGridViewTextBoxColumn9.Width = 60;
            // 
            // dataGridViewTextBoxWinLoss
            // 
            dataGridViewTextBoxWinLoss.DataPropertyName = "winLoss";
            dataGridViewTextBoxWinLoss.HeaderText = "输赢";
            dataGridViewTextBoxWinLoss.Name = "dataGridViewTextBoxWinLoss";
            dataGridViewTextBoxWinLoss.Width = 80;
            // 
            // dataGridViewTextBoxAction
            // 
            dataGridViewTextBoxAction.DataPropertyName = "action";
            dataGridViewTextBoxAction.HeaderText = "赌/吃";
            dataGridViewTextBoxAction.Name = "dataGridViewTextBoxAction";
            dataGridViewTextBoxAction.Width = 68;
            // 
            // actDate
            // 
            actDate.DataPropertyName = "actDate";
            actDate.HeaderText = "时间";
            actDate.Name = "actDate";
            actDate.Width = 150;
            // 
            // SettledHistoryForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(963, 972);
            Controls.Add(pictureBoxSettledSummaryProcessing);
            Controls.Add(pictureBoxSettledHistoryProcessing);
            Controls.Add(label3);
            Controls.Add(dataGridViewRaceBettingRecordInfo);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(dataGridViewSettledSummaryInfo);
            Controls.Add(dataGridViewSettledHistoryInfo);
            MaximizeBox = false;
            Name = "SettledHistoryForm";
            Text = "SettledHistory";
            FormClosing += SettledHistory_FormClosing;
            Load += SettledHistory_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewSettledHistoryInfo).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewSettledSummaryInfo).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewRaceBettingRecordInfo).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSettledHistoryProcessing).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSettledSummaryProcessing).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridViewSettledHistoryInfo;
        private DataGridView dataGridViewSettledSummaryInfo;
        private Label label1;
        private Label label2;
        private DataGridView dataGridViewRaceBettingRecordInfo;
        private Label label3;
        private DataGridViewCheckBoxColumn Column8;
        private DataGridViewTextBoxColumn Date;
        private DataGridViewTextBoxColumn Location;
        private DataGridViewTextBoxColumn OddsType;
        private DataGridViewTextBoxColumn StakeAmount;
        private DataGridViewTextBoxColumn Tax;
        private DataGridViewTextBoxColumn WinLoss;
        private DataGridViewCheckBoxColumn Column1;
        private DataGridViewTextBoxColumn RaceNo;
        private DataGridViewTextBoxColumn BetStakeAmount;
        private DataGridViewTextBoxColumn BetAmount;
        private DataGridViewTextBoxColumn BetTax;
        private DataGridViewTextBoxColumn BetRefund;
        private DataGridViewTextBoxColumn EatStakeAmount;
        private DataGridViewTextBoxColumn EatAmount;
        private DataGridViewTextBoxColumn EatTax;
        private DataGridViewTextBoxColumn EatRefund;
        private DataGridViewTextBoxColumn SettledSummaryWinLoss;
        private PictureBox pictureBoxSettledHistoryProcessing;
        private PictureBox pictureBoxSettledSummaryProcessing;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private DataGridViewTextBoxColumn ColumnRaceNo;
        private DataGridViewTextBoxColumn type;
        private DataGridViewTextBoxColumn horses;
        private DataGridViewTextBoxColumn stake;
        private DataGridViewTextBoxColumn pct;
        private DataGridViewTextBoxColumn limit;
        private DataGridViewTextBoxColumn dataGridViewTextBoxTax;
        private DataGridViewTextBoxColumn bet_payout;
        private DataGridViewTextBoxColumn dataGridViewTextBoxOdds;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private DataGridViewTextBoxColumn dataGridViewTextBoxWinLoss;
        private DataGridViewTextBoxColumn dataGridViewTextBoxAction;
        private DataGridViewTextBoxColumn actDate;
    }
}