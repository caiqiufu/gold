namespace AutoHorseRace
{
    partial class AutoBettingStatusForm
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
            dataGridViewAutoBettingStatus = new DataGridView();
            CheckBox = new DataGridViewCheckBoxColumn();
            ColumnRaceNo = new DataGridViewTextBoxColumn();
            type = new DataGridViewTextBoxColumn();
            combo = new DataGridViewTextBoxColumn();
            action = new DataGridViewTextBoxColumn();
            odds = new DataGridViewTextBoxColumn();
            stakeAmount = new DataGridViewTextBoxColumn();
            limit = new DataGridViewTextBoxColumn();
            timestamp = new DataGridViewTextBoxColumn();
            status = new DataGridViewTextBoxColumn();
            remark = new DataGridViewTextBoxColumn();
            label1 = new Label();
            pictureBoxAutoBettingStatus = new PictureBox();
            buttonRefreshAutoBettingStatus = new Button();
            comboBoxRaceNo = new ComboBox();
            label10 = new Label();
            textBoxCombo = new TextBox();
            label2 = new Label();
            buttonCleanQuery = new Button();
            buttonOpenQSelector = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewAutoBettingStatus).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAutoBettingStatus).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewAutoBettingStatus
            // 
            dataGridViewAutoBettingStatus.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewAutoBettingStatus.Columns.AddRange(new DataGridViewColumn[] { CheckBox, ColumnRaceNo, type, combo, action, odds, stakeAmount, limit, timestamp, status, remark });
            dataGridViewAutoBettingStatus.Location = new Point(2, 50);
            dataGridViewAutoBettingStatus.Name = "dataGridViewAutoBettingStatus";
            dataGridViewAutoBettingStatus.RowHeadersVisible = false;
            dataGridViewAutoBettingStatus.Size = new Size(978, 540);
            dataGridViewAutoBettingStatus.TabIndex = 2;
            // 
            // CheckBox
            // 
            CheckBox.HeaderText = "";
            CheckBox.Name = "CheckBox";
            CheckBox.Width = 30;
            // 
            // ColumnRaceNo
            // 
            ColumnRaceNo.DataPropertyName = "raceNo";
            ColumnRaceNo.HeaderText = "场次";
            ColumnRaceNo.Name = "ColumnRaceNo";
            ColumnRaceNo.Width = 60;
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
            action.Width = 70;
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
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 19);
            label1.Name = "label1";
            label1.Size = new Size(80, 17);
            label1.TabIndex = 3;
            label1.Text = "自动下注进展";
            // 
            // pictureBoxAutoBettingStatus
            // 
            pictureBoxAutoBettingStatus.Location = new Point(90, 13);
            pictureBoxAutoBettingStatus.Name = "pictureBoxAutoBettingStatus";
            pictureBoxAutoBettingStatus.Size = new Size(28, 28);
            pictureBoxAutoBettingStatus.TabIndex = 61;
            pictureBoxAutoBettingStatus.TabStop = false;
            pictureBoxAutoBettingStatus.Paint += pictureBoxAutoBettingStatus_Paint;
            // 
            // buttonRefreshAutoBettingStatus
            // 
            buttonRefreshAutoBettingStatus.Location = new Point(889, 14);
            buttonRefreshAutoBettingStatus.Name = "buttonRefreshAutoBettingStatus";
            buttonRefreshAutoBettingStatus.Size = new Size(80, 28);
            buttonRefreshAutoBettingStatus.TabIndex = 63;
            buttonRefreshAutoBettingStatus.Text = "刷新数据";
            buttonRefreshAutoBettingStatus.UseVisualStyleBackColor = true;
            buttonRefreshAutoBettingStatus.Click += buttonRefreshAutoBettingStatus_Click;
            // 
            // comboBoxRaceNo
            // 
            comboBoxRaceNo.AllowDrop = true;
            comboBoxRaceNo.AutoCompleteCustomSource.AddRange(new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" });
            comboBoxRaceNo.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxRaceNo.FormattingEnabled = true;
            comboBoxRaceNo.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" });
            comboBoxRaceNo.Location = new Point(167, 16);
            comboBoxRaceNo.Name = "comboBoxRaceNo";
            comboBoxRaceNo.Size = new Size(43, 25);
            comboBoxRaceNo.TabIndex = 65;
            comboBoxRaceNo.SelectedIndexChanged += comboBoxRaceNo_SelectedIndexChanged;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Microsoft YaHei", 10.5F);
            label10.Location = new Point(124, 19);
            label10.Name = "label10";
            label10.Size = new Size(37, 20);
            label10.TabIndex = 64;
            label10.Text = "场次";
            // 
            // textBoxCombo
            // 
            textBoxCombo.BorderStyle = BorderStyle.FixedSingle;
            textBoxCombo.Location = new Point(272, 18);
            textBoxCombo.Name = "textBoxCombo";
            textBoxCombo.Size = new Size(407, 23);
            textBoxCombo.TabIndex = 66;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft YaHei", 10.5F);
            label2.Location = new Point(229, 19);
            label2.Name = "label2";
            label2.Size = new Size(37, 20);
            label2.TabIndex = 67;
            label2.Text = "组合";
            // 
            // buttonCleanQuery
            // 
            buttonCleanQuery.Location = new Point(775, 14);
            buttonCleanQuery.Name = "buttonCleanQuery";
            buttonCleanQuery.Size = new Size(80, 28);
            buttonCleanQuery.TabIndex = 68;
            buttonCleanQuery.Text = "清空查询";
            buttonCleanQuery.UseVisualStyleBackColor = true;
            buttonCleanQuery.Click += buttonCleanQuery_Click;
            // 
            // buttonOpenQSelector
            // 
            buttonOpenQSelector.Location = new Point(689, 14);
            buttonOpenQSelector.Name = "buttonOpenQSelector";
            buttonOpenQSelector.Size = new Size(80, 28);
            buttonOpenQSelector.TabIndex = 81;
            buttonOpenQSelector.Text = "选择组合";
            buttonOpenQSelector.UseVisualStyleBackColor = true;
            buttonOpenQSelector.Click += buttonOpenQSelector_Click;
            // 
            // AutoBettingStatusForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 597);
            Controls.Add(buttonOpenQSelector);
            Controls.Add(buttonCleanQuery);
            Controls.Add(label2);
            Controls.Add(textBoxCombo);
            Controls.Add(comboBoxRaceNo);
            Controls.Add(label10);
            Controls.Add(buttonRefreshAutoBettingStatus);
            Controls.Add(pictureBoxAutoBettingStatus);
            Controls.Add(label1);
            Controls.Add(dataGridViewAutoBettingStatus);
            MaximizeBox = false;
            Name = "AutoBettingStatusForm";
            Text = "AutoBettingStatusForm";
            FormClosing += AutoBettingStatusForm_FormClosing;
            Load += AutoBettingStatusForm_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewAutoBettingStatus).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAutoBettingStatus).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridViewAutoBettingStatus;
        private Label label1;
        private PictureBox pictureBoxAutoBettingStatus;
        private DataGridViewCheckBoxColumn CheckBox;
        private DataGridViewTextBoxColumn ColumnRaceNo;
        private DataGridViewTextBoxColumn type;
        private DataGridViewTextBoxColumn combo;
        private DataGridViewTextBoxColumn action;
        private DataGridViewTextBoxColumn odds;
        private DataGridViewTextBoxColumn stakeAmount;
        private DataGridViewTextBoxColumn limit;
        private DataGridViewTextBoxColumn timestamp;
        private DataGridViewTextBoxColumn status;
        private DataGridViewTextBoxColumn remark;
        private Button buttonRefreshAutoBettingStatus;
        private ComboBox comboBoxRaceNo;
        private Label label10;
        private TextBox textBoxCombo;
        private Label label2;
        private Button buttonCleanQuery;
        private Button buttonOpenQSelector;
    }
}