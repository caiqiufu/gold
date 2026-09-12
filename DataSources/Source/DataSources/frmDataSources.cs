using System;
using System.Windows.Forms;
using CQG;

namespace DataSources
{
    public class frmDataSources : System.Windows.Forms.Form
    {

        [STAThread]
        static void Main()
        {
            Application.Run(new frmDataSources());
        }

        // The CQGCEL object, which encapsulates the main functionality of CQG API
        [field: System.CLSCompliant(false)]
        public CQGCEL CEL;

        // Current data sources
        private CQGDataSources m_DataSources;

        // Current data source symbols
        private CQGDataSourceSymbols m_DataSourceSymbols;


        #region " Windows Form Designer generated code "

        public frmDataSources()
        {

            //This call is required by the Windows Form Designer.
            InitializeComponent();

            //Add any initialization after the InitializeComponent() call

        }

        //Form overrides dispose to clean up the component list.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!(components == null))
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        //Required by the Windows Form Designer
        private System.ComponentModel.Container components = null;

        //NOTE: The following procedure is required by the Windows Form Designer
        //It can be modified using the Windows Form Designer.
        //Do not modify it using the code editor.
        internal System.Windows.Forms.Label lblDataConnection;
        internal System.Windows.Forms.PictureBox PictureBox1;
        internal System.Windows.Forms.LinkLabel llWeb;
        internal System.Windows.Forms.GroupBox GroupBox1;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.Label Label6;
        internal System.Windows.Forms.Label Label8;
        internal System.Windows.Forms.Label Label12;
        internal System.Windows.Forms.GroupBox GroupBox3;
        internal System.Windows.Forms.ComboBox cmbDSAbbreviation;
        internal System.Windows.Forms.Button btnQueryDataSources;
        internal System.Windows.Forms.Label lblDSType;
        internal System.Windows.Forms.Label lblDSName;
        internal System.Windows.Forms.Label lblDSStatus;
        internal System.Windows.Forms.TextBox txtDSSDataSourceAbbreviation;
        internal System.Windows.Forms.Button btnQueryDataSourceSymbols;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Label lblDSSDescription;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.Label lblDSSType;
        internal System.Windows.Forms.Label Label7;
        internal System.Windows.Forms.Label lblDSSDataSourceAbbreviation;
        internal System.Windows.Forms.Label Label10;
        internal System.Windows.Forms.ComboBox cmbDSSAbbreviation;
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.lblDataConnection = new System.Windows.Forms.Label();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.llWeb = new System.Windows.Forms.LinkLabel();
            this.cmbDSAbbreviation = new System.Windows.Forms.ComboBox();
            this.btnQueryDataSources = new System.Windows.Forms.Button();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.Label12 = new System.Windows.Forms.Label();
            this.lblDSType = new System.Windows.Forms.Label();
            this.Label8 = new System.Windows.Forms.Label();
            this.lblDSName = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.lblDSStatus = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.txtDSSDataSourceAbbreviation = new System.Windows.Forms.TextBox();
            this.GroupBox3 = new System.Windows.Forms.GroupBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.lblDSSDescription = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.lblDSSType = new System.Windows.Forms.Label();
            this.Label7 = new System.Windows.Forms.Label();
            this.lblDSSDataSourceAbbreviation = new System.Windows.Forms.Label();
            this.Label10 = new System.Windows.Forms.Label();
            this.cmbDSSAbbreviation = new System.Windows.Forms.ComboBox();
            this.btnQueryDataSourceSymbols = new System.Windows.Forms.Button();
            this.GroupBox1.SuspendLayout();
            this.GroupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDataConnection
            // 
            this.lblDataConnection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDataConnection.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.lblDataConnection.Location = new System.Drawing.Point(8, 8);
            this.lblDataConnection.Name = "lblDataConnection";
            this.lblDataConnection.Size = new System.Drawing.Size(216, 16);
            this.lblDataConnection.TabIndex = 0;
            this.lblDataConnection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PictureBox1
            // 
            this.PictureBox1.BackColor = System.Drawing.Color.Black;
            this.PictureBox1.Location = new System.Drawing.Point(0, 40);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(1272, 3);
            this.PictureBox1.TabIndex = 23;
            this.PictureBox1.TabStop = false;
            // 
            // llWeb
            // 
            this.llWeb.BackColor = System.Drawing.SystemColors.Control;
            this.llWeb.LinkArea = new System.Windows.Forms.LinkArea(18, 58);
            this.llWeb.Location = new System.Drawing.Point(280, 8);
            this.llWeb.Name = "llWeb";
            this.llWeb.Size = new System.Drawing.Size(408, 16);
            this.llWeb.TabIndex = 1;
            this.llWeb.TabStop = true;
            this.llWeb.Text = "CQG API web page: http://www.cqg.com/Products/CQG-API.aspx";
            this.llWeb.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.llWeb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llWeb_LinkClicked);
            // 
            // cmbDSAbbreviation
            // 
            this.cmbDSAbbreviation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDSAbbreviation.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.cmbDSAbbreviation.Location = new System.Drawing.Point(200, 88);
            this.cmbDSAbbreviation.Name = "cmbDSAbbreviation";
            this.cmbDSAbbreviation.Size = new System.Drawing.Size(248, 25);
            this.cmbDSAbbreviation.Sorted = true;
            this.cmbDSAbbreviation.TabIndex = 2;
            this.cmbDSAbbreviation.SelectedIndexChanged += new System.EventHandler(this.cmbDSAbbreviation_SelectedIndexChanged);
            // 
            // btnQueryDataSources
            // 
            this.btnQueryDataSources.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.btnQueryDataSources.Location = new System.Drawing.Point(16, 32);
            this.btnQueryDataSources.Name = "btnQueryDataSources";
            this.btnQueryDataSources.Size = new System.Drawing.Size(200, 32);
            this.btnQueryDataSources.TabIndex = 0;
            this.btnQueryDataSources.Text = "Query Data Sources";
            this.btnQueryDataSources.Click += new System.EventHandler(this.btnQueryDataSources_Click);
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.Label12);
            this.GroupBox1.Controls.Add(this.lblDSType);
            this.GroupBox1.Controls.Add(this.Label8);
            this.GroupBox1.Controls.Add(this.lblDSName);
            this.GroupBox1.Controls.Add(this.Label6);
            this.GroupBox1.Controls.Add(this.lblDSStatus);
            this.GroupBox1.Controls.Add(this.Label4);
            this.GroupBox1.Controls.Add(this.btnQueryDataSources);
            this.GroupBox1.Controls.Add(this.cmbDSAbbreviation);
            this.GroupBox1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.GroupBox1.Location = new System.Drawing.Point(8, 48);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(680, 224);
            this.GroupBox1.TabIndex = 2;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Data Sources";
            // 
            // Label12
            // 
            this.Label12.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.Label12.Location = new System.Drawing.Point(96, 88);
            this.Label12.Name = "Label12";
            this.Label12.Size = new System.Drawing.Size(96, 24);
            this.Label12.TabIndex = 1;
            this.Label12.Text = "Abbreviation :";
            this.Label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDSType
            // 
            this.lblDSType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDSType.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.lblDSType.Location = new System.Drawing.Point(200, 184);
            this.lblDSType.Name = "lblDSType";
            this.lblDSType.Size = new System.Drawing.Size(360, 24);
            this.lblDSType.TabIndex = 8;
            this.lblDSType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label8
            // 
            this.Label8.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.Label8.Location = new System.Drawing.Point(88, 184);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(104, 24);
            this.Label8.TabIndex = 7;
            this.Label8.Text = "Symbol Types :";
            this.Label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDSName
            // 
            this.lblDSName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDSName.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.lblDSName.Location = new System.Drawing.Point(200, 152);
            this.lblDSName.Name = "lblDSName";
            this.lblDSName.Size = new System.Drawing.Size(360, 24);
            this.lblDSName.TabIndex = 6;
            this.lblDSName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label6
            // 
            this.Label6.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.Label6.Location = new System.Drawing.Point(136, 152);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(56, 24);
            this.Label6.TabIndex = 5;
            this.Label6.Text = "Name :";
            this.Label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDSStatus
            // 
            this.lblDSStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDSStatus.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.lblDSStatus.Location = new System.Drawing.Point(200, 120);
            this.lblDSStatus.Name = "lblDSStatus";
            this.lblDSStatus.Size = new System.Drawing.Size(248, 24);
            this.lblDSStatus.TabIndex = 4;
            this.lblDSStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label4
            // 
            this.Label4.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.Label4.Location = new System.Drawing.Point(112, 120);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(80, 24);
            this.Label4.TabIndex = 3;
            this.Label4.Text = "Status :";
            this.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDSSDataSourceAbbreviation
            // 
            this.txtDSSDataSourceAbbreviation.Location = new System.Drawing.Point(16, 32);
            this.txtDSSDataSourceAbbreviation.Name = "txtDSSDataSourceAbbreviation";
            this.txtDSSDataSourceAbbreviation.Size = new System.Drawing.Size(200, 25);
            this.txtDSSDataSourceAbbreviation.TabIndex = 0;
            this.txtDSSDataSourceAbbreviation.Text = "";
            this.txtDSSDataSourceAbbreviation.TextChanged += new System.EventHandler(this.txtDSSDataSourceAbbreviation_TextChanged);
            // 
            // GroupBox3
            // 
            this.GroupBox3.Controls.Add(this.Label1);
            this.GroupBox3.Controls.Add(this.lblDSSDescription);
            this.GroupBox3.Controls.Add(this.Label3);
            this.GroupBox3.Controls.Add(this.lblDSSType);
            this.GroupBox3.Controls.Add(this.Label7);
            this.GroupBox3.Controls.Add(this.lblDSSDataSourceAbbreviation);
            this.GroupBox3.Controls.Add(this.Label10);
            this.GroupBox3.Controls.Add(this.cmbDSSAbbreviation);
            this.GroupBox3.Controls.Add(this.btnQueryDataSourceSymbols);
            this.GroupBox3.Controls.Add(this.txtDSSDataSourceAbbreviation);
            this.GroupBox3.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.GroupBox3.Location = new System.Drawing.Point(8, 280);
            this.GroupBox3.Name = "GroupBox3";
            this.GroupBox3.Size = new System.Drawing.Size(680, 256);
            this.GroupBox3.TabIndex = 3;
            this.GroupBox3.TabStop = false;
            this.GroupBox3.Text = "Data Source Symbols";
            // 
            // Label1
            // 
            this.Label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.Label1.Location = new System.Drawing.Point(96, 152);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(104, 24);
            this.Label1.TabIndex = 4;
            this.Label1.Text = "Abbreviation :";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDSSDescription
            // 
            this.lblDSSDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDSSDescription.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.lblDSSDescription.Location = new System.Drawing.Point(208, 216);
            this.lblDSSDescription.Name = "lblDSSDescription";
            this.lblDSSDescription.Size = new System.Drawing.Size(360, 24);
            this.lblDSSDescription.TabIndex = 9;
            this.lblDSSDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label3
            // 
            this.Label3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.Label3.Location = new System.Drawing.Point(96, 216);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(104, 24);
            this.Label3.TabIndex = 8;
            this.Label3.Text = "Description :";
            this.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDSSType
            // 
            this.lblDSSType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDSSType.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.lblDSSType.Location = new System.Drawing.Point(208, 184);
            this.lblDSSType.Name = "lblDSSType";
            this.lblDSSType.Size = new System.Drawing.Size(248, 24);
            this.lblDSSType.TabIndex = 7;
            this.lblDSSType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label7
            // 
            this.Label7.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.Label7.Location = new System.Drawing.Point(144, 184);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(56, 24);
            this.Label7.TabIndex = 6;
            this.Label7.Text = "Type :";
            this.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDSSDataSourceAbbreviation
            // 
            this.lblDSSDataSourceAbbreviation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDSSDataSourceAbbreviation.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.lblDSSDataSourceAbbreviation.Location = new System.Drawing.Point(208, 120);
            this.lblDSSDataSourceAbbreviation.Name = "lblDSSDataSourceAbbreviation";
            this.lblDSSDataSourceAbbreviation.Size = new System.Drawing.Size(248, 24);
            this.lblDSSDataSourceAbbreviation.TabIndex = 3;
            this.lblDSSDataSourceAbbreviation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label10
            // 
            this.Label10.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.Label10.Location = new System.Drawing.Point(16, 120);
            this.Label10.Name = "Label10";
            this.Label10.Size = new System.Drawing.Size(184, 24);
            this.Label10.TabIndex = 2;
            this.Label10.Text = "Data Source Abbreviation :";
            this.Label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbDSSAbbreviation
            // 
            this.cmbDSSAbbreviation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDSSAbbreviation.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.cmbDSSAbbreviation.Location = new System.Drawing.Point(208, 152);
            this.cmbDSSAbbreviation.Name = "cmbDSSAbbreviation";
            this.cmbDSSAbbreviation.Size = new System.Drawing.Size(248, 25);
            this.cmbDSSAbbreviation.TabIndex = 5;
            this.cmbDSSAbbreviation.SelectedIndexChanged += new System.EventHandler(this.cmbDSSAbbreviation_SelectedIndexChanged);
            // 
            // btnQueryDataSourceSymbols
            // 
            this.btnQueryDataSourceSymbols.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.btnQueryDataSourceSymbols.Location = new System.Drawing.Point(16, 64);
            this.btnQueryDataSourceSymbols.Name = "btnQueryDataSourceSymbols";
            this.btnQueryDataSourceSymbols.Size = new System.Drawing.Size(200, 32);
            this.btnQueryDataSourceSymbols.TabIndex = 1;
            this.btnQueryDataSourceSymbols.Text = "Query Data Source Symbols";
            this.btnQueryDataSourceSymbols.Click += new System.EventHandler(this.btnQueryDataSourceSymbols_Click);
            // 
            // frmDataSources
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(694, 540);
            this.Controls.Add(this.GroupBox1);
            this.Controls.Add(this.lblDataConnection);
            this.Controls.Add(this.PictureBox1);
            this.Controls.Add(this.llWeb);
            this.Controls.Add(this.GroupBox3);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "frmDataSources";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DataSources";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.frmDataSources_Closing);
            this.Load += new System.EventHandler(this.frmDataSources_Load);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>
        /// Creates a CEL object, changes its configurations, and starts up the created CEL object.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// An EventArgs that contains the event data.
        /// </param>
        private void frmDataSources_Load(Object sender,
                                         EventArgs e)
        {
            try
            {
                // Creates the CQGCEL object
                CEL = new CQGCEL();
                CEL.DataError += new CQG._ICQGCELEvents_DataErrorEventHandler(CEL_DataError);
                CEL.DataConnectionStatusChanged += new CQG._ICQGCELEvents_DataConnectionStatusChangedEventHandler(CEL_DataConnectionStatusChanged);
                CEL.DataSourcesResolved += new CQG._ICQGCELEvents_DataSourcesResolvedEventHandler(CEL_DataSourcesResolved);
                CEL.DataSourceSymbolsResolved += new CQG._ICQGCELEvents_DataSourceSymbolsResolvedEventHandler(CEL_DataSourceSymbolsResolved);
                CEL.APIConfiguration.ReadyStatusCheck = eReadyStatusCheck.rscOff;
                CEL.APIConfiguration.CollectionsThrowException = false;
                CEL.APIConfiguration.TimeZoneCode = eTimeZone.tzCentral;
                // Disables the controls
                CEL_DataConnectionStatusChanged(eConnectionStatus.csConnectionDown);
                // Starts up the CQGCEL
                CEL.Startup();

                ClearDataSources();
                ClearDataSourceSymbols();

                m_DataSources = null;

                // Changing buttons enablements
                EnableControls();
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "frmDataSources_Load", ex);
                this.Close();
            }
        }

        /// <summary>
        /// Shuts down CEL.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// A CancelEventArgs that contains the event data.
        /// </param>
        private void frmDataSources_Closing(object sender,
                                            System.ComponentModel.CancelEventArgs e)
        {
            if (CEL != null)
            {
                CEL.Shutdown();
            }
        }

        /// <summary>
        /// This event is fired, when CQGCEL detects some abnormal discrepancy between data expected and data received.
        /// </summary>
        /// <param name="cqg_error">
        /// The object, in which the error has occurred.
        /// </param>
        /// <param name="error_description">
        /// The string, describing the error.
        /// </param>
        private void CEL_DataError(object cqg_error,
                                   string error_description)
        {
            try
            {
                CQGError cqgErr = cqg_error as CQGError;
                if (CEL.IsValid(cqgErr))
                {
                    if (cqgErr.Code == 102)
                    {
                        error_description += " Restart the application.";
                    }
                    else if (cqgErr.Code == 125)
                    {
                        error_description += " Turn on CQG Client and restart the application.";
                    }
                }

                MessageBox.Show(error_description, "DataSources", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "CEL_DataError", ex);
            }
        }

        /// <summary>
        /// This event is fired, when some changes occur in the connection with CQG data server.
        /// Depending from connection status changes enablements of buttons.
        /// </summary>
        /// <param name="newStatus">
        /// The current status of the connection with the data server.
        /// </param>
        private void CEL_DataConnectionStatusChanged(eConnectionStatus new_status)
        {
            try
            {
                string sInfo;
                System.Drawing.Color BackCol;

                if (new_status == eConnectionStatus.csConnectionUp)
                {
                    BackCol = System.Drawing.Color.FromArgb(192, 209, 205);
                    sInfo = "DATA Connection is UP";
                }
                else if (new_status == eConnectionStatus.csConnectionDelayed)
                {
                    BackCol = System.Drawing.Color.FromArgb(255, 114, 0);
                    sInfo = "DATA Connection is Delayed";
                }
                else
                {
                    BackCol = System.Drawing.Color.FromArgb(255, 114, 0);
                    sInfo = "DATA Connection is Down";
                }

                lblDataConnection.BackColor = BackCol;
                lblDataConnection.Text = sInfo;

                EnableControls();
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "CEL_DataConnectionStatusChanged", ex);
            }
        }

        /// <summary>
        /// Adds resolved data sources to the combo box.
        /// </summary>
        /// <param name="cqg_data_sources">
        /// Collection of resolved data sources
        /// </param>
        /// <param name="cqg_error">
        /// CQGError object describing last error occurred during
        /// processing data sources request or Nothing/Invalid_Error
        /// in case of no error
        /// </param>
        private void CEL_DataSourcesResolved(CQGDataSources cqg_data_sources,
                                             CQGError cqg_error)
        {
            try
            {
                // Clearing data sources and data source symbols
                ClearDataSources();

                // Notifying when any error occurred during request
                if (CEL.IsValid(cqg_error))
                {
                    MessageBox.Show(cqg_error.Description, "DataSources", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Set current data sources
                m_DataSources = cqg_data_sources;

                if (m_DataSources.Count == 0)
                {
                    return;
                }

                // Adding data sources to the combo box
                for (int i = 0; i <= m_DataSources.Count - 1; i++)
                {
                    cmbDSAbbreviation.Items.Add(m_DataSources[i].Abbreviation);
                }

                cmbDSAbbreviation.SelectedIndex = 0;

                // Changing buttons enablements
                EnableControls();
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "CEL_DataSourcesResolved", ex);
            }
        }

        /// <summary>
        /// Dumps resolved data sources symbols to the combo box.
        /// </summary>
        /// <param name="data_source_abbreviation">
        /// Abbreviation of the data source symbols of which are resolved
        /// </param>
        /// <param name="cqg_data_source_symbols">
        /// Collection of resolved data source symbols
        /// </param>
        /// <param name="cqg_error">
        /// CQGError object describing last error occurred during processing data source symbols request
        /// </param>
        private void CEL_DataSourceSymbolsResolved(string data_source_abbreviation,
                                                   CQGDataSourceSymbols cqg_data_source_symbols,
                                                   CQGError cqg_error)
        {
            try
            {
                // Clearing data source symbols
                ClearDataSourceSymbols();

                // Notifying when any error occurred during request
                if (CEL.IsValid(cqg_error))
                {
                    MessageBox.Show(cqg_error.Description, "DataSources", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                m_DataSourceSymbols = cqg_data_source_symbols;

                if (m_DataSourceSymbols.Count == 0)
                {
                    return;
                }

                // Adding data source symbols to the combo box
                for (int i = 0; i <= m_DataSourceSymbols.Count - 1; i++)
                {
                    cmbDSSAbbreviation.Items.Add(m_DataSourceSymbols[i].Abbreviation);
                }

                cmbDSSAbbreviation.SelectedIndex = 0;

                lblDSSDataSourceAbbreviation.Text = m_DataSourceSymbols.DataSourceAbbreviation;
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "CEL_DataSourceSymbolsResolved", ex);
            }
        }

        /// <summary>
        /// Open CQG API web page.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// A LinkLabelLinkClickedEventArgs that contains the event data.
        /// </param>
        private void llWeb_LinkClicked(System.Object sender,
                                       LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("iexplore.exe", "http://www.cqg.com/Products/CQG-API.aspx");
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "llWeb_LinkClicked", ex);
            }
        }

        /// <summary>
        /// Clears data sources and data source symbols, and requests new data sources.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// An EventArgs that contains the event data.
        /// </param>
        private void btnQueryDataSources_Click(Object sender,
                                               EventArgs e)
        {
            try
            {
                // Clearing data sources and data source symbols
                ClearDataSources();

                // Requesting new data sources
                CEL.RequestDataSources();

                // Changing buttons enablements
                EnableControls();
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "btnQueryDataSources_Click", ex);
            }
        }

        /// <summary>
        /// Clears data source symbols and requests the new data source symbols.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// An EventArgs that contains the event data.
        /// </param>
        private void btnQueryDataSourceSymbols_Click(Object sender,
                                                     EventArgs e)
        {
            try
            {
                // Clearing data source symbols
                ClearDataSourceSymbols();

                // Requesting new data source symbols
                CEL.RequestDataSourceSymbols(txtDSSDataSourceAbbreviation.Text);
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "btnQueryDataSourceSymbols_Click", ex);
            }
        }

        /// <summary>
        /// Dumps all necessary data sources information.
        /// </summary>
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// An EventArgs that contains the event data
        /// </param>
        private void cmbDSAbbreviation_SelectedIndexChanged(Object sender,
                                                            EventArgs e)
        {
            try
            {
                ClearDataSource();

                if (cmbDSAbbreviation.SelectedIndex == -1)
                {
                    return;
                }

                CQGDataSource dataSource = m_DataSources.get_ItemByAbbreviation(cmbDSAbbreviation.Text);

                lblDSName.Text = dataSource.Name;
                lblDSStatus.Text = dataSource.Status.ToString();
                lblDSType.Text = SymbolTypeToString(dataSource.SymbolTypes);

                // Changing text in the text box
                txtDSSDataSourceAbbreviation.Text = cmbDSAbbreviation.Text;

                // Changing buttons enablements
                EnableControls();
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "cmbDSAbbreviation_SelectedIndexChanged", ex);
            }
        }

        /// <summary>
        /// Changes data source symbols query button enablement.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// A EventArgs that contains the event data.
        /// </param>
        private void txtDSSDataSourceAbbreviation_TextChanged(Object sender,
                                                              EventArgs e)
        {
            try
            {
                ClearDataSourceSymbols();

                EnableControls();
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "txtDSSDataSourceAbbreviation_TextChanged", ex);
            }
        }

        /// <summary>
        /// Dumps all necessary data source symbols information.
        /// </summary>
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// An EventArgs that contains the event data
        /// </param>
        private void cmbDSSAbbreviation_SelectedIndexChanged(Object sender,
                                                             EventArgs e)
        {
            try
            {
                ClearDataSourceSymbol();

                if (cmbDSSAbbreviation.SelectedIndex == -1)
                {
                    return;
                }

                CQGDataSourceSymbol dataSourceSymbol = m_DataSourceSymbols[cmbDSSAbbreviation.SelectedIndex];

                lblDSSDescription.Text = dataSourceSymbol.Description;
                lblDSSType.Text = SymbolTypeToString(dataSourceSymbol.Type);
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "cmbDSSAbbreviation_SelectedIndexChanged", ex);
            }
        }

        /// <summary>
        /// Clears all data sources.
        /// </summary>
        private void ClearDataSources()
        {
            try
            {
                m_DataSources = null;

                cmbDSAbbreviation.Items.Clear();

                ClearDataSource();
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "ClearDataSources", ex);
            }
        }

        /// <summary>
        /// Clears data source information.
        /// </summary>
        private void ClearDataSource()
        {
            try
            {
                lblDSName.Text = string.Empty;
                lblDSStatus.Text = string.Empty;
                lblDSType.Text = string.Empty;

                txtDSSDataSourceAbbreviation.Text = string.Empty;
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "ClearDataSource", ex);
            }
        }

        /// <summary>
        /// Clears data source all symbols.
        /// </summary>
        private void ClearDataSourceSymbols()
        {
            try
            {
                m_DataSourceSymbols = null;

                cmbDSSAbbreviation.Items.Clear();

                lblDSSDataSourceAbbreviation.Text = string.Empty;

                ClearDataSourceSymbol();
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "ClearDataSourceSymbols", ex);
            }
        }

        /// <summary>
        /// Clears data source symbol information.
        /// </summary>
        private void ClearDataSourceSymbol()
        {
            try
            {
                lblDSSDescription.Text = string.Empty;
                lblDSSType.Text = string.Empty;
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "ClearDataSourceSymbol", ex);
            }
        }

        /// <summary>
        /// Changes controls enablements.
        /// </summary>
        private void EnableControls()
        {
            try
            {
                bool isDataConnectionUp = false;

                if (CEL.IsStarted)
                {
                    isDataConnectionUp = CEL.Environment.DataConnectionStatus == eConnectionStatus.csConnectionUp;
                }

                btnQueryDataSources.Enabled = isDataConnectionUp;

                bool isDataSourcesSelected = txtDSSDataSourceAbbreviation.Text.Trim().Length > 0;

                btnQueryDataSourceSymbols.Enabled = isDataSourcesSelected && isDataConnectionUp;
            }
            catch (Exception ex)
            {
                modErrorHandler.ShowError("frmDataSources", "EnableControls", ex);
            }
        }

        /// <summary>
        /// Converts enum of symbol type to appropriate string.
        /// </summary>
        /// <param name="symbolType">
        /// Enum which will be converted.
        /// </param>
        /// <returns>
        /// String which describes symbol type.
        /// </returns>
        private string SymbolTypeToString(eSymbolType symbolType)
        {

            string symbolTypeStr = string.Empty;

            if ((symbolType & eSymbolType.stFuture) == eSymbolType.stFuture)
            {
                symbolTypeStr = symbolTypeStr + "Future, ";
            }
            if ((symbolType & eSymbolType.stOption) == eSymbolType.stOption)
            {
                symbolTypeStr = symbolTypeStr + "Option, ";
            }
            if ((symbolType & eSymbolType.stStock) == eSymbolType.stStock)
            {
                symbolTypeStr = symbolTypeStr + "Stock, ";
            }
            if ((symbolType & eSymbolType.stTreasury) == eSymbolType.stTreasury)
            {
                symbolTypeStr = symbolTypeStr + "Treasury, ";
            }
            if ((symbolType & eSymbolType.stCash) == eSymbolType.stCash)
            {
                symbolTypeStr = symbolTypeStr + "Cash, ";
            }
            if ((symbolType & eSymbolType.stCurrency) == eSymbolType.stCurrency)
            {
                symbolTypeStr = symbolTypeStr + "Currency, ";
            }
            if ((symbolType & eSymbolType.stIndex) == eSymbolType.stIndex)
            {
                symbolTypeStr = symbolTypeStr + "Index, ";
            }
            if ((symbolType & eSymbolType.stReport) == eSymbolType.stReport)
            {
                symbolTypeStr = symbolTypeStr + "Report, ";
            }

            if (symbolTypeStr.Length == 0)
            {
                symbolTypeStr = "Undefined";
            }
            else
            {
                symbolTypeStr = symbolTypeStr.Substring(0, symbolTypeStr.Length - 2);
            }

            return symbolTypeStr;

        }

    }

}
