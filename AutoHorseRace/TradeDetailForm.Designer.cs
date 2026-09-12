namespace AutoHorseRace
{
    partial class TradeDetailForm
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
            dataGridViewBettingDetail = new DataGridView();
            CheckBox = new DataGridViewCheckBoxColumn();
            type = new DataGridViewTextBoxColumn();
            combo = new DataGridViewTextBoxColumn();
            action = new DataGridViewTextBoxColumn();
            odds = new DataGridViewTextBoxColumn();
            stakeAmount = new DataGridViewTextBoxColumn();
            limit = new DataGridViewTextBoxColumn();
            timestamp = new DataGridViewTextBoxColumn();
            status = new DataGridViewTextBoxColumn();
            remark = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridViewBettingDetail).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewBettingDetail
            // 
            dataGridViewBettingDetail.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewBettingDetail.Columns.AddRange(new DataGridViewColumn[] { CheckBox, type, combo, action, odds, stakeAmount, limit, timestamp, status, remark });
            dataGridViewBettingDetail.Location = new Point(2, 0);
            dataGridViewBettingDetail.Name = "dataGridViewBettingDetail";
            dataGridViewBettingDetail.RowHeadersVisible = false;
            dataGridViewBettingDetail.Size = new Size(922, 435);
            dataGridViewBettingDetail.TabIndex = 1;
            // 
            // CheckBox
            // 
            CheckBox.HeaderText = "";
            CheckBox.Name = "CheckBox";
            CheckBox.Width = 30;
            // 
            // type
            // 
            type.DataPropertyName = "type";
            type.HeaderText = "类型";
            type.Name = "type";
            type.Width = 60;
            // 
            // combo
            // 
            combo.DataPropertyName = "combo";
            combo.HeaderText = "马号";
            combo.Name = "combo";
            // 
            // action
            // 
            action.DataPropertyName = "action";
            action.HeaderText = "赌/吃";
            action.Name = "action";
            action.Width = 60;
            // 
            // odds
            // 
            odds.DataPropertyName = "odds";
            odds.HeaderText = "水折";
            odds.Name = "odds";
            odds.Width = 60;
            // 
            // stakeAmount
            // 
            stakeAmount.DataPropertyName = "stakeAmount";
            stakeAmount.HeaderText = "票数";
            stakeAmount.Name = "stakeAmount";
            stakeAmount.Width = 60;
            // 
            // limit
            // 
            limit.DataPropertyName = "limit";
            limit.HeaderText = "限额";
            limit.Name = "limit";
            limit.Width = 60;
            // 
            // timestamp
            // 
            timestamp.DataPropertyName = "timestamp";
            timestamp.HeaderText = "时间";
            timestamp.Name = "timestamp";
            timestamp.Width = 150;
            // 
            // status
            // 
            status.DataPropertyName = "status";
            status.HeaderText = "结果";
            status.Name = "status";
            status.Width = 120;
            // 
            // remark
            // 
            remark.DataPropertyName = "remark";
            remark.HeaderText = "备注";
            remark.Name = "remark";
            remark.Width = 200;
            // 
            // TradeDetailForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(926, 437);
            Controls.Add(dataGridViewBettingDetail);
            MaximizeBox = false;
            Name = "TradeDetailForm";
            Text = "TradeDetailForm";
            FormClosing += TradeDetailForm_FormClosing;
            Load += TradeDetailForm_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewBettingDetail).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridViewBettingDetail;
        private DataGridViewCheckBoxColumn CheckBox;
        private DataGridViewTextBoxColumn type;
        private DataGridViewTextBoxColumn combo;
        private DataGridViewTextBoxColumn action;
        private DataGridViewTextBoxColumn odds;
        private DataGridViewTextBoxColumn stakeAmount;
        private DataGridViewTextBoxColumn limit;
        private DataGridViewTextBoxColumn timestamp;
        private DataGridViewTextBoxColumn status;
        private DataGridViewTextBoxColumn remark;
    }
}