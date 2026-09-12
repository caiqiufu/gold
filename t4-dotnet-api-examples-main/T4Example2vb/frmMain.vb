' Import the T4 definitions namespace.
Imports T4

' Import the API namespace.
Imports T4.API

' Import XML for saving and retriving markets.
Imports System.Xml

' Generic collections.
Imports System.Collections.Generic
Imports System.Linq

' API specific initialization occurs in the moHost_LoginSuccess routin.
' No data can be pulled from the API without a successfull login.
Public Class frmMain
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        SetupMiscExamples()

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents cboMarkets As System.Windows.Forms.ComboBox
    Friend WithEvents cboExchanges As System.Windows.Forms.ComboBox
    Friend WithEvents cboContracts As System.Windows.Forms.ComboBox
    Friend WithEvents lblExchange As System.Windows.Forms.Label
    Friend WithEvents lblContract As System.Windows.Forms.Label
    Friend WithEvents lblMarket As System.Windows.Forms.Label
    Friend WithEvents txtMarketDescription1 As System.Windows.Forms.TextBox
    Friend WithEvents txtBid1 As System.Windows.Forms.TextBox
    Friend WithEvents txtOffer1 As System.Windows.Forms.TextBox
    Friend WithEvents txtBidVol1 As System.Windows.Forms.TextBox
    Friend WithEvents txtOfferVol1 As System.Windows.Forms.TextBox
    Friend WithEvents txtLastVol1 As System.Windows.Forms.TextBox
    Friend WithEvents txtLast1 As System.Windows.Forms.TextBox
    Friend WithEvents txtLastVolTotal1 As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lblBidPrice As System.Windows.Forms.Label
    Friend WithEvents lblOfferPrice As System.Windows.Forms.Label
    Friend WithEvents lblLastPrice As System.Windows.Forms.Label
    Friend WithEvents lblBidVol As System.Windows.Forms.Label
    Friend WithEvents lblOfferVol As System.Windows.Forms.Label
    Friend WithEvents lblLastVol As System.Windows.Forms.Label
    Friend WithEvents lblTotalVol As System.Windows.Forms.Label
    Friend WithEvents txtLastVolTotal2 As System.Windows.Forms.TextBox
    Friend WithEvents txtLastVol2 As System.Windows.Forms.TextBox
    Friend WithEvents txtLast2 As System.Windows.Forms.TextBox
    Friend WithEvents txtOfferVol2 As System.Windows.Forms.TextBox
    Friend WithEvents txtBidVol2 As System.Windows.Forms.TextBox
    Friend WithEvents txtOffer2 As System.Windows.Forms.TextBox
    Friend WithEvents txtBid2 As System.Windows.Forms.TextBox
    Friend WithEvents txtMarketDescription2 As System.Windows.Forms.TextBox
    Friend WithEvents cmdGet1 As System.Windows.Forms.Button
    Friend WithEvents cmdGet2 As System.Windows.Forms.Button
    Friend WithEvents cmdSave As System.Windows.Forms.Button
    Friend WithEvents lblAccount As System.Windows.Forms.Label
    Friend WithEvents cboAccounts As System.Windows.Forms.ComboBox
    Friend WithEvents txtNet1 As System.Windows.Forms.TextBox
    Friend WithEvents txtNet2 As System.Windows.Forms.TextBox
    Friend WithEvents lblNet As System.Windows.Forms.Label
    Friend WithEvents lblBuys As System.Windows.Forms.Label
    Friend WithEvents lblSells As System.Windows.Forms.Label
    Friend WithEvents txtBuys1 As System.Windows.Forms.TextBox
    Friend WithEvents txtBuys2 As System.Windows.Forms.TextBox
    Friend WithEvents txtSells1 As System.Windows.Forms.TextBox
    Friend WithEvents txtSells2 As System.Windows.Forms.TextBox
    Friend WithEvents cmdBuy1 As System.Windows.Forms.Button
    Friend WithEvents cmdBuy2 As System.Windows.Forms.Button
    Friend WithEvents cmdSell1 As System.Windows.Forms.Button
    Friend WithEvents lstOrders As System.Windows.Forms.ListBox
    Friend WithEvents cmdSell2 As System.Windows.Forms.Button
    Friend WithEvents txtCash As System.Windows.Forms.TextBox
    Friend WithEvents lblCash As System.Windows.Forms.Label
    Friend WithEvents grpAccount As System.Windows.Forms.GroupBox
    Friend WithEvents grpOrders As System.Windows.Forms.GroupBox
    Friend WithEvents lblSaveInfo As System.Windows.Forms.Label
    Friend WithEvents lblOrderInfo As System.Windows.Forms.Label
    Friend WithEvents lblAccountInfo As System.Windows.Forms.Label
    Friend WithEvents cboMisc2 As System.Windows.Forms.ComboBox
    Friend WithEvents lblMisc2 As System.Windows.Forms.Label
    Friend WithEvents cmdRunMisc2 As System.Windows.Forms.Button
    Friend WithEvents lblMisc1 As System.Windows.Forms.Label
    Friend WithEvents cmdRunMisc1 As System.Windows.Forms.Button
    Friend WithEvents grpMarket1 As System.Windows.Forms.GroupBox
    Friend WithEvents grpMarket2 As System.Windows.Forms.GroupBox
    Friend WithEvents txtMode As TextBox
    Friend WithEvents lblMode As Label
    Friend WithEvents cboMisc1 As System.Windows.Forms.ComboBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.cboMarkets = New System.Windows.Forms.ComboBox()
        Me.cboExchanges = New System.Windows.Forms.ComboBox()
        Me.cboContracts = New System.Windows.Forms.ComboBox()
        Me.lblMarket = New System.Windows.Forms.Label()
        Me.lblContract = New System.Windows.Forms.Label()
        Me.lblExchange = New System.Windows.Forms.Label()
        Me.cmdGet1 = New System.Windows.Forms.Button()
        Me.txtMarketDescription1 = New System.Windows.Forms.TextBox()
        Me.txtBid1 = New System.Windows.Forms.TextBox()
        Me.txtOffer1 = New System.Windows.Forms.TextBox()
        Me.txtBidVol1 = New System.Windows.Forms.TextBox()
        Me.txtOfferVol1 = New System.Windows.Forms.TextBox()
        Me.txtLastVol1 = New System.Windows.Forms.TextBox()
        Me.txtLast1 = New System.Windows.Forms.TextBox()
        Me.txtLastVolTotal1 = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblBidPrice = New System.Windows.Forms.Label()
        Me.lblOfferPrice = New System.Windows.Forms.Label()
        Me.lblLastPrice = New System.Windows.Forms.Label()
        Me.lblBidVol = New System.Windows.Forms.Label()
        Me.lblOfferVol = New System.Windows.Forms.Label()
        Me.lblLastVol = New System.Windows.Forms.Label()
        Me.lblTotalVol = New System.Windows.Forms.Label()
        Me.txtLastVolTotal2 = New System.Windows.Forms.TextBox()
        Me.txtLastVol2 = New System.Windows.Forms.TextBox()
        Me.txtLast2 = New System.Windows.Forms.TextBox()
        Me.txtOfferVol2 = New System.Windows.Forms.TextBox()
        Me.txtBidVol2 = New System.Windows.Forms.TextBox()
        Me.txtOffer2 = New System.Windows.Forms.TextBox()
        Me.txtBid2 = New System.Windows.Forms.TextBox()
        Me.txtMarketDescription2 = New System.Windows.Forms.TextBox()
        Me.cmdGet2 = New System.Windows.Forms.Button()
        Me.cmdSave = New System.Windows.Forms.Button()
        Me.lblAccount = New System.Windows.Forms.Label()
        Me.cboAccounts = New System.Windows.Forms.ComboBox()
        Me.txtCash = New System.Windows.Forms.TextBox()
        Me.lblCash = New System.Windows.Forms.Label()
        Me.txtNet1 = New System.Windows.Forms.TextBox()
        Me.txtNet2 = New System.Windows.Forms.TextBox()
        Me.lblNet = New System.Windows.Forms.Label()
        Me.lblBuys = New System.Windows.Forms.Label()
        Me.lblSells = New System.Windows.Forms.Label()
        Me.txtBuys1 = New System.Windows.Forms.TextBox()
        Me.txtBuys2 = New System.Windows.Forms.TextBox()
        Me.txtSells1 = New System.Windows.Forms.TextBox()
        Me.txtSells2 = New System.Windows.Forms.TextBox()
        Me.cmdBuy1 = New System.Windows.Forms.Button()
        Me.cmdBuy2 = New System.Windows.Forms.Button()
        Me.cmdSell1 = New System.Windows.Forms.Button()
        Me.cmdSell2 = New System.Windows.Forms.Button()
        Me.lstOrders = New System.Windows.Forms.ListBox()
        Me.grpAccount = New System.Windows.Forms.GroupBox()
        Me.lblAccountInfo = New System.Windows.Forms.Label()
        Me.grpOrders = New System.Windows.Forms.GroupBox()
        Me.lblOrderInfo = New System.Windows.Forms.Label()
        Me.lblMisc1 = New System.Windows.Forms.Label()
        Me.cmdRunMisc1 = New System.Windows.Forms.Button()
        Me.cboMisc1 = New System.Windows.Forms.ComboBox()
        Me.lblMisc2 = New System.Windows.Forms.Label()
        Me.cmdRunMisc2 = New System.Windows.Forms.Button()
        Me.cboMisc2 = New System.Windows.Forms.ComboBox()
        Me.lblSaveInfo = New System.Windows.Forms.Label()
        Me.grpMarket1 = New System.Windows.Forms.GroupBox()
        Me.txtMode = New System.Windows.Forms.TextBox()
        Me.lblMode = New System.Windows.Forms.Label()
        Me.grpMarket2 = New System.Windows.Forms.GroupBox()
        Me.grpAccount.SuspendLayout()
        Me.grpOrders.SuspendLayout()
        Me.grpMarket1.SuspendLayout()
        Me.grpMarket2.SuspendLayout()
        Me.SuspendLayout()
        '
        'cboMarkets
        '
        Me.cboMarkets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboMarkets.Location = New System.Drawing.Point(90, 67)
        Me.cboMarkets.Name = "cboMarkets"
        Me.cboMarkets.Size = New System.Drawing.Size(257, 21)
        Me.cboMarkets.TabIndex = 6
        Me.cboMarkets.TabStop = False
        '
        'cboExchanges
        '
        Me.cboExchanges.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboExchanges.Location = New System.Drawing.Point(90, 19)
        Me.cboExchanges.Name = "cboExchanges"
        Me.cboExchanges.Size = New System.Drawing.Size(257, 21)
        Me.cboExchanges.Sorted = True
        Me.cboExchanges.TabIndex = 7
        Me.cboExchanges.TabStop = False
        '
        'cboContracts
        '
        Me.cboContracts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboContracts.Location = New System.Drawing.Point(90, 43)
        Me.cboContracts.Name = "cboContracts"
        Me.cboContracts.Size = New System.Drawing.Size(257, 21)
        Me.cboContracts.Sorted = True
        Me.cboContracts.TabIndex = 8
        Me.cboContracts.TabStop = False
        '
        'lblMarket
        '
        Me.lblMarket.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMarket.Location = New System.Drawing.Point(10, 66)
        Me.lblMarket.Name = "lblMarket"
        Me.lblMarket.Size = New System.Drawing.Size(78, 21)
        Me.lblMarket.TabIndex = 11
        Me.lblMarket.Text = "Market:"
        Me.lblMarket.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblContract
        '
        Me.lblContract.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblContract.Location = New System.Drawing.Point(8, 42)
        Me.lblContract.Name = "lblContract"
        Me.lblContract.Size = New System.Drawing.Size(80, 21)
        Me.lblContract.TabIndex = 10
        Me.lblContract.Text = "Contract:"
        Me.lblContract.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblExchange
        '
        Me.lblExchange.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblExchange.Location = New System.Drawing.Point(14, 18)
        Me.lblExchange.Name = "lblExchange"
        Me.lblExchange.Size = New System.Drawing.Size(74, 21)
        Me.lblExchange.TabIndex = 9
        Me.lblExchange.Text = "Exchange:"
        Me.lblExchange.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'cmdGet1
        '
        Me.cmdGet1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdGet1.Location = New System.Drawing.Point(311, 94)
        Me.cmdGet1.Name = "cmdGet1"
        Me.cmdGet1.Size = New System.Drawing.Size(36, 20)
        Me.cmdGet1.TabIndex = 10
        Me.cmdGet1.TabStop = False
        Me.cmdGet1.Text = "Get"
        '
        'txtMarketDescription1
        '
        Me.txtMarketDescription1.BackColor = System.Drawing.Color.White
        Me.txtMarketDescription1.Location = New System.Drawing.Point(11, 140)
        Me.txtMarketDescription1.Name = "txtMarketDescription1"
        Me.txtMarketDescription1.ReadOnly = True
        Me.txtMarketDescription1.Size = New System.Drawing.Size(208, 20)
        Me.txtMarketDescription1.TabIndex = 11
        Me.txtMarketDescription1.TabStop = False
        '
        'txtBid1
        '
        Me.txtBid1.BackColor = System.Drawing.Color.LightCyan
        Me.txtBid1.Location = New System.Drawing.Point(221, 140)
        Me.txtBid1.Name = "txtBid1"
        Me.txtBid1.ReadOnly = True
        Me.txtBid1.Size = New System.Drawing.Size(60, 20)
        Me.txtBid1.TabIndex = 12
        Me.txtBid1.TabStop = False
        Me.txtBid1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtOffer1
        '
        Me.txtOffer1.BackColor = System.Drawing.Color.MistyRose
        Me.txtOffer1.Location = New System.Drawing.Point(313, 140)
        Me.txtOffer1.Name = "txtOffer1"
        Me.txtOffer1.ReadOnly = True
        Me.txtOffer1.Size = New System.Drawing.Size(60, 20)
        Me.txtOffer1.TabIndex = 14
        Me.txtOffer1.TabStop = False
        Me.txtOffer1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtBidVol1
        '
        Me.txtBidVol1.BackColor = System.Drawing.Color.LightCyan
        Me.txtBidVol1.Location = New System.Drawing.Point(283, 140)
        Me.txtBidVol1.Name = "txtBidVol1"
        Me.txtBidVol1.ReadOnly = True
        Me.txtBidVol1.Size = New System.Drawing.Size(28, 20)
        Me.txtBidVol1.TabIndex = 16
        Me.txtBidVol1.TabStop = False
        Me.txtBidVol1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtOfferVol1
        '
        Me.txtOfferVol1.BackColor = System.Drawing.Color.MistyRose
        Me.txtOfferVol1.Location = New System.Drawing.Point(375, 140)
        Me.txtOfferVol1.Name = "txtOfferVol1"
        Me.txtOfferVol1.ReadOnly = True
        Me.txtOfferVol1.Size = New System.Drawing.Size(28, 20)
        Me.txtOfferVol1.TabIndex = 17
        Me.txtOfferVol1.TabStop = False
        Me.txtOfferVol1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtLastVol1
        '
        Me.txtLastVol1.BackColor = System.Drawing.Color.Honeydew
        Me.txtLastVol1.Location = New System.Drawing.Point(467, 140)
        Me.txtLastVol1.Name = "txtLastVol1"
        Me.txtLastVol1.ReadOnly = True
        Me.txtLastVol1.Size = New System.Drawing.Size(28, 20)
        Me.txtLastVol1.TabIndex = 20
        Me.txtLastVol1.TabStop = False
        Me.txtLastVol1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtLast1
        '
        Me.txtLast1.BackColor = System.Drawing.Color.Honeydew
        Me.txtLast1.Location = New System.Drawing.Point(405, 140)
        Me.txtLast1.Name = "txtLast1"
        Me.txtLast1.ReadOnly = True
        Me.txtLast1.Size = New System.Drawing.Size(60, 20)
        Me.txtLast1.TabIndex = 18
        Me.txtLast1.TabStop = False
        Me.txtLast1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtLastVolTotal1
        '
        Me.txtLastVolTotal1.BackColor = System.Drawing.Color.Honeydew
        Me.txtLastVolTotal1.Location = New System.Drawing.Point(497, 140)
        Me.txtLastVolTotal1.Name = "txtLastVolTotal1"
        Me.txtLastVolTotal1.ReadOnly = True
        Me.txtLastVolTotal1.Size = New System.Drawing.Size(60, 20)
        Me.txtLastVolTotal1.TabIndex = 21
        Me.txtLastVolTotal1.TabStop = False
        Me.txtLastVolTotal1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(10, 119)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(129, 20)
        Me.Label1.TabIndex = 22
        Me.Label1.Text = "Market Description:"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblBidPrice
        '
        Me.lblBidPrice.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBidPrice.ForeColor = System.Drawing.Color.RoyalBlue
        Me.lblBidPrice.Location = New System.Drawing.Point(219, 120)
        Me.lblBidPrice.Name = "lblBidPrice"
        Me.lblBidPrice.Size = New System.Drawing.Size(60, 20)
        Me.lblBidPrice.TabIndex = 23
        Me.lblBidPrice.Text = "Price:"
        Me.lblBidPrice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblOfferPrice
        '
        Me.lblOfferPrice.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblOfferPrice.ForeColor = System.Drawing.Color.Crimson
        Me.lblOfferPrice.Location = New System.Drawing.Point(311, 120)
        Me.lblOfferPrice.Name = "lblOfferPrice"
        Me.lblOfferPrice.Size = New System.Drawing.Size(60, 20)
        Me.lblOfferPrice.TabIndex = 24
        Me.lblOfferPrice.Text = "Price:"
        Me.lblOfferPrice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblLastPrice
        '
        Me.lblLastPrice.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLastPrice.ForeColor = System.Drawing.Color.DarkGreen
        Me.lblLastPrice.Location = New System.Drawing.Point(403, 120)
        Me.lblLastPrice.Name = "lblLastPrice"
        Me.lblLastPrice.Size = New System.Drawing.Size(60, 20)
        Me.lblLastPrice.TabIndex = 25
        Me.lblLastPrice.Text = "Price:"
        Me.lblLastPrice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblBidVol
        '
        Me.lblBidVol.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBidVol.ForeColor = System.Drawing.Color.RoyalBlue
        Me.lblBidVol.Location = New System.Drawing.Point(281, 120)
        Me.lblBidVol.Name = "lblBidVol"
        Me.lblBidVol.Size = New System.Drawing.Size(30, 20)
        Me.lblBidVol.TabIndex = 26
        Me.lblBidVol.Text = "Vol:"
        Me.lblBidVol.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblOfferVol
        '
        Me.lblOfferVol.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblOfferVol.ForeColor = System.Drawing.Color.Crimson
        Me.lblOfferVol.Location = New System.Drawing.Point(373, 120)
        Me.lblOfferVol.Name = "lblOfferVol"
        Me.lblOfferVol.Size = New System.Drawing.Size(30, 20)
        Me.lblOfferVol.TabIndex = 27
        Me.lblOfferVol.Text = "Vol:"
        Me.lblOfferVol.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblLastVol
        '
        Me.lblLastVol.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLastVol.ForeColor = System.Drawing.Color.DarkGreen
        Me.lblLastVol.Location = New System.Drawing.Point(465, 120)
        Me.lblLastVol.Name = "lblLastVol"
        Me.lblLastVol.Size = New System.Drawing.Size(30, 20)
        Me.lblLastVol.TabIndex = 28
        Me.lblLastVol.Text = "Vol:"
        Me.lblLastVol.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblTotalVol
        '
        Me.lblTotalVol.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTotalVol.ForeColor = System.Drawing.Color.DarkGreen
        Me.lblTotalVol.Location = New System.Drawing.Point(495, 120)
        Me.lblTotalVol.Name = "lblTotalVol"
        Me.lblTotalVol.Size = New System.Drawing.Size(62, 20)
        Me.lblTotalVol.TabIndex = 29
        Me.lblTotalVol.Text = "Total Vol:"
        Me.lblTotalVol.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'txtLastVolTotal2
        '
        Me.txtLastVolTotal2.BackColor = System.Drawing.Color.Honeydew
        Me.txtLastVolTotal2.Location = New System.Drawing.Point(497, 44)
        Me.txtLastVolTotal2.Name = "txtLastVolTotal2"
        Me.txtLastVolTotal2.ReadOnly = True
        Me.txtLastVolTotal2.Size = New System.Drawing.Size(60, 20)
        Me.txtLastVolTotal2.TabIndex = 38
        Me.txtLastVolTotal2.TabStop = False
        Me.txtLastVolTotal2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtLastVol2
        '
        Me.txtLastVol2.BackColor = System.Drawing.Color.Honeydew
        Me.txtLastVol2.Location = New System.Drawing.Point(467, 44)
        Me.txtLastVol2.Name = "txtLastVol2"
        Me.txtLastVol2.ReadOnly = True
        Me.txtLastVol2.Size = New System.Drawing.Size(28, 20)
        Me.txtLastVol2.TabIndex = 37
        Me.txtLastVol2.TabStop = False
        Me.txtLastVol2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtLast2
        '
        Me.txtLast2.BackColor = System.Drawing.Color.Honeydew
        Me.txtLast2.Location = New System.Drawing.Point(405, 44)
        Me.txtLast2.Name = "txtLast2"
        Me.txtLast2.ReadOnly = True
        Me.txtLast2.Size = New System.Drawing.Size(60, 20)
        Me.txtLast2.TabIndex = 36
        Me.txtLast2.TabStop = False
        Me.txtLast2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtOfferVol2
        '
        Me.txtOfferVol2.BackColor = System.Drawing.Color.MistyRose
        Me.txtOfferVol2.Location = New System.Drawing.Point(375, 44)
        Me.txtOfferVol2.Name = "txtOfferVol2"
        Me.txtOfferVol2.ReadOnly = True
        Me.txtOfferVol2.Size = New System.Drawing.Size(28, 20)
        Me.txtOfferVol2.TabIndex = 35
        Me.txtOfferVol2.TabStop = False
        Me.txtOfferVol2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtBidVol2
        '
        Me.txtBidVol2.BackColor = System.Drawing.Color.LightCyan
        Me.txtBidVol2.Location = New System.Drawing.Point(283, 44)
        Me.txtBidVol2.Name = "txtBidVol2"
        Me.txtBidVol2.ReadOnly = True
        Me.txtBidVol2.Size = New System.Drawing.Size(28, 20)
        Me.txtBidVol2.TabIndex = 34
        Me.txtBidVol2.TabStop = False
        Me.txtBidVol2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtOffer2
        '
        Me.txtOffer2.BackColor = System.Drawing.Color.MistyRose
        Me.txtOffer2.Location = New System.Drawing.Point(313, 44)
        Me.txtOffer2.Name = "txtOffer2"
        Me.txtOffer2.ReadOnly = True
        Me.txtOffer2.Size = New System.Drawing.Size(60, 20)
        Me.txtOffer2.TabIndex = 33
        Me.txtOffer2.TabStop = False
        Me.txtOffer2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtBid2
        '
        Me.txtBid2.BackColor = System.Drawing.Color.LightCyan
        Me.txtBid2.Location = New System.Drawing.Point(221, 44)
        Me.txtBid2.Name = "txtBid2"
        Me.txtBid2.ReadOnly = True
        Me.txtBid2.Size = New System.Drawing.Size(60, 20)
        Me.txtBid2.TabIndex = 32
        Me.txtBid2.TabStop = False
        Me.txtBid2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtMarketDescription2
        '
        Me.txtMarketDescription2.BackColor = System.Drawing.Color.White
        Me.txtMarketDescription2.Location = New System.Drawing.Point(13, 44)
        Me.txtMarketDescription2.Name = "txtMarketDescription2"
        Me.txtMarketDescription2.ReadOnly = True
        Me.txtMarketDescription2.Size = New System.Drawing.Size(206, 20)
        Me.txtMarketDescription2.TabIndex = 31
        Me.txtMarketDescription2.TabStop = False
        '
        'cmdGet2
        '
        Me.cmdGet2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdGet2.Location = New System.Drawing.Point(13, 19)
        Me.cmdGet2.Name = "cmdGet2"
        Me.cmdGet2.Size = New System.Drawing.Size(51, 20)
        Me.cmdGet2.TabIndex = 30
        Me.cmdGet2.TabStop = False
        Me.cmdGet2.Text = "Picker"
        '
        'cmdSave
        '
        Me.cmdSave.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdSave.Location = New System.Drawing.Point(12, 398)
        Me.cmdSave.Name = "cmdSave"
        Me.cmdSave.Size = New System.Drawing.Size(140, 26)
        Me.cmdSave.TabIndex = 40
        Me.cmdSave.TabStop = False
        Me.cmdSave.Text = "Save Selected Markets"
        '
        'lblAccount
        '
        Me.lblAccount.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAccount.Location = New System.Drawing.Point(6, 22)
        Me.lblAccount.Name = "lblAccount"
        Me.lblAccount.Size = New System.Drawing.Size(107, 21)
        Me.lblAccount.TabIndex = 41
        Me.lblAccount.Text = "Current Account:"
        Me.lblAccount.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'cboAccounts
        '
        Me.cboAccounts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboAccounts.Location = New System.Drawing.Point(117, 22)
        Me.cboAccounts.Name = "cboAccounts"
        Me.cboAccounts.Size = New System.Drawing.Size(208, 21)
        Me.cboAccounts.Sorted = True
        Me.cboAccounts.TabIndex = 42
        Me.cboAccounts.TabStop = False
        '
        'txtCash
        '
        Me.txtCash.BackColor = System.Drawing.Color.White
        Me.txtCash.Location = New System.Drawing.Point(400, 23)
        Me.txtCash.Name = "txtCash"
        Me.txtCash.ReadOnly = True
        Me.txtCash.Size = New System.Drawing.Size(108, 20)
        Me.txtCash.TabIndex = 43
        Me.txtCash.TabStop = False
        Me.txtCash.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'lblCash
        '
        Me.lblCash.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCash.Location = New System.Drawing.Point(337, 22)
        Me.lblCash.Name = "lblCash"
        Me.lblCash.Size = New System.Drawing.Size(57, 21)
        Me.lblCash.TabIndex = 44
        Me.lblCash.Text = "Cash:"
        Me.lblCash.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'txtNet1
        '
        Me.txtNet1.BackColor = System.Drawing.Color.White
        Me.txtNet1.Location = New System.Drawing.Point(559, 140)
        Me.txtNet1.Name = "txtNet1"
        Me.txtNet1.ReadOnly = True
        Me.txtNet1.Size = New System.Drawing.Size(38, 20)
        Me.txtNet1.TabIndex = 46
        Me.txtNet1.TabStop = False
        Me.txtNet1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtNet2
        '
        Me.txtNet2.BackColor = System.Drawing.Color.White
        Me.txtNet2.Location = New System.Drawing.Point(559, 44)
        Me.txtNet2.Name = "txtNet2"
        Me.txtNet2.ReadOnly = True
        Me.txtNet2.Size = New System.Drawing.Size(38, 20)
        Me.txtNet2.TabIndex = 47
        Me.txtNet2.TabStop = False
        Me.txtNet2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'lblNet
        '
        Me.lblNet.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblNet.Location = New System.Drawing.Point(557, 121)
        Me.lblNet.Name = "lblNet"
        Me.lblNet.Size = New System.Drawing.Size(38, 18)
        Me.lblNet.TabIndex = 48
        Me.lblNet.Text = "Net:"
        Me.lblNet.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblBuys
        '
        Me.lblBuys.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBuys.Location = New System.Drawing.Point(597, 121)
        Me.lblBuys.Name = "lblBuys"
        Me.lblBuys.Size = New System.Drawing.Size(38, 18)
        Me.lblBuys.TabIndex = 49
        Me.lblBuys.Text = "Buys:"
        Me.lblBuys.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblSells
        '
        Me.lblSells.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSells.Location = New System.Drawing.Point(637, 121)
        Me.lblSells.Name = "lblSells"
        Me.lblSells.Size = New System.Drawing.Size(38, 18)
        Me.lblSells.TabIndex = 50
        Me.lblSells.Text = "Sells:"
        Me.lblSells.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'txtBuys1
        '
        Me.txtBuys1.BackColor = System.Drawing.Color.White
        Me.txtBuys1.ForeColor = System.Drawing.Color.RoyalBlue
        Me.txtBuys1.Location = New System.Drawing.Point(599, 140)
        Me.txtBuys1.Name = "txtBuys1"
        Me.txtBuys1.ReadOnly = True
        Me.txtBuys1.Size = New System.Drawing.Size(38, 20)
        Me.txtBuys1.TabIndex = 51
        Me.txtBuys1.TabStop = False
        Me.txtBuys1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtBuys2
        '
        Me.txtBuys2.BackColor = System.Drawing.Color.White
        Me.txtBuys2.ForeColor = System.Drawing.Color.RoyalBlue
        Me.txtBuys2.Location = New System.Drawing.Point(599, 44)
        Me.txtBuys2.Name = "txtBuys2"
        Me.txtBuys2.ReadOnly = True
        Me.txtBuys2.Size = New System.Drawing.Size(38, 20)
        Me.txtBuys2.TabIndex = 52
        Me.txtBuys2.TabStop = False
        Me.txtBuys2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtSells1
        '
        Me.txtSells1.BackColor = System.Drawing.Color.White
        Me.txtSells1.ForeColor = System.Drawing.Color.Crimson
        Me.txtSells1.Location = New System.Drawing.Point(639, 140)
        Me.txtSells1.Name = "txtSells1"
        Me.txtSells1.ReadOnly = True
        Me.txtSells1.Size = New System.Drawing.Size(38, 20)
        Me.txtSells1.TabIndex = 53
        Me.txtSells1.TabStop = False
        Me.txtSells1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtSells2
        '
        Me.txtSells2.BackColor = System.Drawing.Color.White
        Me.txtSells2.ForeColor = System.Drawing.Color.Crimson
        Me.txtSells2.Location = New System.Drawing.Point(639, 44)
        Me.txtSells2.Name = "txtSells2"
        Me.txtSells2.ReadOnly = True
        Me.txtSells2.Size = New System.Drawing.Size(38, 20)
        Me.txtSells2.TabIndex = 54
        Me.txtSells2.TabStop = False
        Me.txtSells2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'cmdBuy1
        '
        Me.cmdBuy1.BackColor = System.Drawing.Color.RoyalBlue
        Me.cmdBuy1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdBuy1.ForeColor = System.Drawing.Color.White
        Me.cmdBuy1.Location = New System.Drawing.Point(219, 162)
        Me.cmdBuy1.Name = "cmdBuy1"
        Me.cmdBuy1.Size = New System.Drawing.Size(60, 20)
        Me.cmdBuy1.TabIndex = 55
        Me.cmdBuy1.TabStop = False
        Me.cmdBuy1.Text = "Buy"
        Me.cmdBuy1.UseVisualStyleBackColor = False
        '
        'cmdBuy2
        '
        Me.cmdBuy2.BackColor = System.Drawing.Color.RoyalBlue
        Me.cmdBuy2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdBuy2.ForeColor = System.Drawing.Color.White
        Me.cmdBuy2.Location = New System.Drawing.Point(219, 66)
        Me.cmdBuy2.Name = "cmdBuy2"
        Me.cmdBuy2.Size = New System.Drawing.Size(60, 20)
        Me.cmdBuy2.TabIndex = 56
        Me.cmdBuy2.TabStop = False
        Me.cmdBuy2.Text = "Buy"
        Me.cmdBuy2.UseVisualStyleBackColor = False
        '
        'cmdSell1
        '
        Me.cmdSell1.BackColor = System.Drawing.Color.Crimson
        Me.cmdSell1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdSell1.ForeColor = System.Drawing.Color.White
        Me.cmdSell1.Location = New System.Drawing.Point(311, 162)
        Me.cmdSell1.Name = "cmdSell1"
        Me.cmdSell1.Size = New System.Drawing.Size(60, 20)
        Me.cmdSell1.TabIndex = 58
        Me.cmdSell1.TabStop = False
        Me.cmdSell1.Text = "Sell"
        Me.cmdSell1.UseVisualStyleBackColor = False
        '
        'cmdSell2
        '
        Me.cmdSell2.BackColor = System.Drawing.Color.Crimson
        Me.cmdSell2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdSell2.ForeColor = System.Drawing.Color.White
        Me.cmdSell2.Location = New System.Drawing.Point(311, 66)
        Me.cmdSell2.Name = "cmdSell2"
        Me.cmdSell2.Size = New System.Drawing.Size(60, 20)
        Me.cmdSell2.TabIndex = 59
        Me.cmdSell2.TabStop = False
        Me.cmdSell2.Text = "Sell"
        Me.cmdSell2.UseVisualStyleBackColor = False
        '
        'lstOrders
        '
        Me.lstOrders.Location = New System.Drawing.Point(8, 21)
        Me.lstOrders.Name = "lstOrders"
        Me.lstOrders.Size = New System.Drawing.Size(751, 134)
        Me.lstOrders.TabIndex = 60
        Me.lstOrders.TabStop = False
        '
        'grpAccount
        '
        Me.grpAccount.Controls.Add(Me.lblAccountInfo)
        Me.grpAccount.Controls.Add(Me.cboAccounts)
        Me.grpAccount.Controls.Add(Me.lblCash)
        Me.grpAccount.Controls.Add(Me.lblAccount)
        Me.grpAccount.Controls.Add(Me.txtCash)
        Me.grpAccount.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpAccount.Location = New System.Drawing.Point(12, 12)
        Me.grpAccount.Name = "grpAccount"
        Me.grpAccount.Size = New System.Drawing.Size(768, 51)
        Me.grpAccount.TabIndex = 63
        Me.grpAccount.TabStop = False
        Me.grpAccount.Text = "Account"
        '
        'lblAccountInfo
        '
        Me.lblAccountInfo.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAccountInfo.Location = New System.Drawing.Point(70, 95)
        Me.lblAccountInfo.Name = "lblAccountInfo"
        Me.lblAccountInfo.Size = New System.Drawing.Size(188, 30)
        Me.lblAccountInfo.TabIndex = 68
        Me.lblAccountInfo.Text = "An Account must be selected to view positions and submit trades."
        Me.lblAccountInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'grpOrders
        '
        Me.grpOrders.Controls.Add(Me.lstOrders)
        Me.grpOrders.Controls.Add(Me.lblOrderInfo)
        Me.grpOrders.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpOrders.Location = New System.Drawing.Point(12, 430)
        Me.grpOrders.Name = "grpOrders"
        Me.grpOrders.Size = New System.Drawing.Size(768, 190)
        Me.grpOrders.TabIndex = 64
        Me.grpOrders.TabStop = False
        Me.grpOrders.Text = "Orders"
        '
        'lblOrderInfo
        '
        Me.lblOrderInfo.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblOrderInfo.Location = New System.Drawing.Point(40, 166)
        Me.lblOrderInfo.Name = "lblOrderInfo"
        Me.lblOrderInfo.Size = New System.Drawing.Size(634, 18)
        Me.lblOrderInfo.TabIndex = 67
        Me.lblOrderInfo.Text = "Double Click orders to Pull them.  Volume is displayed Filled/Working to clarify " &
    "which orders have been Pulled."
        Me.lblOrderInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblMisc1
        '
        Me.lblMisc1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMisc1.Location = New System.Drawing.Point(382, 162)
        Me.lblMisc1.Name = "lblMisc1"
        Me.lblMisc1.Size = New System.Drawing.Size(70, 20)
        Me.lblMisc1.TabIndex = 67
        Me.lblMisc1.Text = "Misc Code:"
        Me.lblMisc1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'cmdRunMisc1
        '
        Me.cmdRunMisc1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdRunMisc1.Location = New System.Drawing.Point(640, 162)
        Me.cmdRunMisc1.Name = "cmdRunMisc1"
        Me.cmdRunMisc1.Size = New System.Drawing.Size(38, 20)
        Me.cmdRunMisc1.TabIndex = 66
        Me.cmdRunMisc1.TabStop = False
        Me.cmdRunMisc1.Text = "Run"
        '
        'cboMisc1
        '
        Me.cboMisc1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboMisc1.Location = New System.Drawing.Point(456, 162)
        Me.cboMisc1.Name = "cboMisc1"
        Me.cboMisc1.Size = New System.Drawing.Size(180, 21)
        Me.cboMisc1.Sorted = True
        Me.cboMisc1.TabIndex = 65
        Me.cboMisc1.TabStop = False
        '
        'lblMisc2
        '
        Me.lblMisc2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMisc2.Location = New System.Drawing.Point(382, 66)
        Me.lblMisc2.Name = "lblMisc2"
        Me.lblMisc2.Size = New System.Drawing.Size(70, 20)
        Me.lblMisc2.TabIndex = 64
        Me.lblMisc2.Text = "Misc Code:"
        Me.lblMisc2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'cmdRunMisc2
        '
        Me.cmdRunMisc2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdRunMisc2.Location = New System.Drawing.Point(640, 66)
        Me.cmdRunMisc2.Name = "cmdRunMisc2"
        Me.cmdRunMisc2.Size = New System.Drawing.Size(38, 20)
        Me.cmdRunMisc2.TabIndex = 63
        Me.cmdRunMisc2.TabStop = False
        Me.cmdRunMisc2.Text = "Run"
        '
        'cboMisc2
        '
        Me.cboMisc2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboMisc2.Location = New System.Drawing.Point(456, 66)
        Me.cboMisc2.Name = "cboMisc2"
        Me.cboMisc2.Size = New System.Drawing.Size(180, 21)
        Me.cboMisc2.Sorted = True
        Me.cboMisc2.TabIndex = 62
        Me.cboMisc2.TabStop = False
        '
        'lblSaveInfo
        '
        Me.lblSaveInfo.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSaveInfo.Location = New System.Drawing.Point(153, 396)
        Me.lblSaveInfo.Name = "lblSaveInfo"
        Me.lblSaveInfo.Size = New System.Drawing.Size(370, 26)
        Me.lblSaveInfo.TabIndex = 66
        Me.lblSaveInfo.Text = "Click Save to store the current markets in an XML file on the server.  The market" &
    "s will be loaded automatically on the next login."
        Me.lblSaveInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'grpMarket1
        '
        Me.grpMarket1.Controls.Add(Me.txtMode)
        Me.grpMarket1.Controls.Add(Me.lblMode)
        Me.grpMarket1.Controls.Add(Me.cboExchanges)
        Me.grpMarket1.Controls.Add(Me.lblMisc1)
        Me.grpMarket1.Controls.Add(Me.lblMarket)
        Me.grpMarket1.Controls.Add(Me.cmdRunMisc1)
        Me.grpMarket1.Controls.Add(Me.cboContracts)
        Me.grpMarket1.Controls.Add(Me.cboMisc1)
        Me.grpMarket1.Controls.Add(Me.cboMarkets)
        Me.grpMarket1.Controls.Add(Me.lblExchange)
        Me.grpMarket1.Controls.Add(Me.lblContract)
        Me.grpMarket1.Controls.Add(Me.Label1)
        Me.grpMarket1.Controls.Add(Me.lblLastPrice)
        Me.grpMarket1.Controls.Add(Me.lblBidVol)
        Me.grpMarket1.Controls.Add(Me.lblOfferPrice)
        Me.grpMarket1.Controls.Add(Me.lblOfferVol)
        Me.grpMarket1.Controls.Add(Me.lblBidPrice)
        Me.grpMarket1.Controls.Add(Me.lblLastVol)
        Me.grpMarket1.Controls.Add(Me.lblTotalVol)
        Me.grpMarket1.Controls.Add(Me.txtOfferVol1)
        Me.grpMarket1.Controls.Add(Me.txtBidVol1)
        Me.grpMarket1.Controls.Add(Me.txtLastVolTotal1)
        Me.grpMarket1.Controls.Add(Me.cmdBuy1)
        Me.grpMarket1.Controls.Add(Me.txtOffer1)
        Me.grpMarket1.Controls.Add(Me.txtLast1)
        Me.grpMarket1.Controls.Add(Me.lblBuys)
        Me.grpMarket1.Controls.Add(Me.cmdSell1)
        Me.grpMarket1.Controls.Add(Me.txtBid1)
        Me.grpMarket1.Controls.Add(Me.txtMarketDescription1)
        Me.grpMarket1.Controls.Add(Me.cmdGet1)
        Me.grpMarket1.Controls.Add(Me.txtSells1)
        Me.grpMarket1.Controls.Add(Me.txtBuys1)
        Me.grpMarket1.Controls.Add(Me.lblSells)
        Me.grpMarket1.Controls.Add(Me.txtNet1)
        Me.grpMarket1.Controls.Add(Me.txtLastVol1)
        Me.grpMarket1.Controls.Add(Me.lblNet)
        Me.grpMarket1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpMarket1.Location = New System.Drawing.Point(12, 84)
        Me.grpMarket1.Name = "grpMarket1"
        Me.grpMarket1.Size = New System.Drawing.Size(768, 195)
        Me.grpMarket1.TabIndex = 66
        Me.grpMarket1.TabStop = False
        Me.grpMarket1.Text = "Market 1"
        '
        'txtMode
        '
        Me.txtMode.BackColor = System.Drawing.Color.White
        Me.txtMode.ForeColor = System.Drawing.Color.Crimson
        Me.txtMode.Location = New System.Drawing.Point(679, 140)
        Me.txtMode.Name = "txtMode"
        Me.txtMode.ReadOnly = True
        Me.txtMode.Size = New System.Drawing.Size(80, 20)
        Me.txtMode.TabIndex = 69
        Me.txtMode.TabStop = False
        Me.txtMode.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'lblMode
        '
        Me.lblMode.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMode.Location = New System.Drawing.Point(679, 121)
        Me.lblMode.Name = "lblMode"
        Me.lblMode.Size = New System.Drawing.Size(49, 18)
        Me.lblMode.TabIndex = 68
        Me.lblMode.Text = "Mode:"
        Me.lblMode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'grpMarket2
        '
        Me.grpMarket2.Controls.Add(Me.cmdGet2)
        Me.grpMarket2.Controls.Add(Me.txtMarketDescription2)
        Me.grpMarket2.Controls.Add(Me.lblMisc2)
        Me.grpMarket2.Controls.Add(Me.txtLastVolTotal2)
        Me.grpMarket2.Controls.Add(Me.cmdRunMisc2)
        Me.grpMarket2.Controls.Add(Me.txtLastVol2)
        Me.grpMarket2.Controls.Add(Me.cboMisc2)
        Me.grpMarket2.Controls.Add(Me.txtSells2)
        Me.grpMarket2.Controls.Add(Me.txtLast2)
        Me.grpMarket2.Controls.Add(Me.cmdSell2)
        Me.grpMarket2.Controls.Add(Me.cmdBuy2)
        Me.grpMarket2.Controls.Add(Me.txtOfferVol2)
        Me.grpMarket2.Controls.Add(Me.txtBuys2)
        Me.grpMarket2.Controls.Add(Me.txtBidVol2)
        Me.grpMarket2.Controls.Add(Me.txtNet2)
        Me.grpMarket2.Controls.Add(Me.txtOffer2)
        Me.grpMarket2.Controls.Add(Me.txtBid2)
        Me.grpMarket2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpMarket2.Location = New System.Drawing.Point(12, 285)
        Me.grpMarket2.Name = "grpMarket2"
        Me.grpMarket2.Size = New System.Drawing.Size(768, 98)
        Me.grpMarket2.TabIndex = 67
        Me.grpMarket2.TabStop = False
        Me.grpMarket2.Text = "Market 2"
        '
        'frmMain
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(789, 631)
        Me.Controls.Add(Me.grpMarket2)
        Me.Controls.Add(Me.grpMarket1)
        Me.Controls.Add(Me.grpAccount)
        Me.Controls.Add(Me.cmdSave)
        Me.Controls.Add(Me.grpOrders)
        Me.Controls.Add(Me.lblSaveInfo)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmMain"
        Me.Text = "T4 Example 2"
        Me.grpAccount.ResumeLayout(False)
        Me.grpAccount.PerformLayout()
        Me.grpOrders.ResumeLayout(False)
        Me.grpMarket1.ResumeLayout(False)
        Me.grpMarket1.PerformLayout()
        Me.grpMarket2.ResumeLayout(False)
        Me.grpMarket2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Member Variables "

    ' Reference to the main api host object.
    Private WithEvents moHost As Host

    ' Reference to the current account.
    Private WithEvents moAccount As Account

    ' Reference to the current market.
    Private moPickerMarket As Market

    ' References to selected markets.
    Private moMarket1 As Market
    Private moMarket2 As Market

    ' References to marketid's retrieved from saved settings..
    Dim mstrMarketID1 As String
    Dim mstrMarketID2 As String

    ' Reference to the accounts list.
    Private WithEvents moAccounts As AccountList

#End Region

#Region " Initialization "

    ' Initialize the application.
    Private Sub Init()

        Trace.WriteLine("Init")

        ' Populate the available exchanges.
        ExchangeListComplete()

        ' Set the account list reference so that we can get 
        ' Account and order events.
        moAccounts = moHost.Accounts
        OnAccountListComplete()

        Try

            ' Read saved markets.
            ' XML Doc.
            Dim oDoc As New XmlDocument

            ' XML Nodes for viewing the doc.
            Dim oMarket As XmlNode
            Dim oMarkets As XmlNode

            ' Pull the xml doc from the server.
            oDoc = moHost.MasterUser.UserSettings

            ' Reference the saved markets via xml node.
            oMarkets = oDoc.ChildNodes(0)

            If Not oMarkets Is Nothing Then

                ' Load the saved markets.
                For Each oMarket In oMarkets
                    ' Check each child node for existance of saved markets.
                    Select Case oMarket.Name
                        Case "market1"
                            mstrMarketID1 = oMarket.Attributes("MarketID").Value

                            ' Get the specified market.
                            moHost.MarketData.GetMarket(mstrMarketID1, Sub(e)
                                                                           ' Subscribe to market1.
                                                                           If e.Markets.Count > 0 Then
                                                                               NewMarketSubscription(moMarket1, e.Markets.First)
                                                                           End If
                                                                       End Sub,
                                                                        Nothing)

                        Case "market2"
                            mstrMarketID2 = oMarket.Attributes("MarketID").Value

                            ' Get the specified market.
                            moHost.MarketData.GetMarket(mstrMarketID2, Sub(e)
                                                                           ' Subscribe to market1.
                                                                           If e.Markets.Count > 0 Then
                                                                               NewMarketSubscription(moMarket2, e.Markets.First)
                                                                           End If
                                                                       End Sub,
                                                                        Nothing)


                    End Select
                Next

            End If

        Catch ex As Exception

            ' Trace the exception.
            Trace.WriteLine("Error: " & ex.ToString)

        End Try

    End Sub

#End Region

#Region " Market Picker "

    Private Sub ExchangeListComplete()

        ' First clear all the combo's.
        cboExchanges.Items.Clear()
        cboContracts.Items.Clear()
        cboMarkets.Items.Clear()

        ' Eliminate any previous references.
        moPickerMarket = Nothing

        ' Populate the list of exchanges.
        Try

            ' Add the exchanges to the dropdown list.
            For Each oExchange As Exchange In moHost.MarketData.Exchanges

                If oExchange.Enabled AndAlso oExchange.MarketDataType <> MarketDataType.None Then

                    'Trace.WriteLine(String.Format("Exchange: {0}, Description: {1}, MarketDataType: {2}", oExchange.ExchangeID, oExchange.Description, oExchange.MarketDataType))
                    cboExchanges.Items.Add(oExchange)

                End If

            Next

        Catch ex As Exception

            ' Trace the error.
            Trace.WriteLine("Error " & ex.ToString)

        End Try

    End Sub

    Private Sub cboExchanges_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cboExchanges.SelectedIndexChanged

        ' Populate the current exchange's available contracts.
        If Not cboExchanges.SelectedItem Is Nothing Then

            ' Reference the current exchange.
            Dim oExchange As Exchange = DirectCast(cboExchanges.SelectedItem, Exchange)

            ' Populate the list of contracts available for the current exchange.

            ' First clear all the combo's.
            cboContracts.Items.Clear()
            cboMarkets.Items.Clear()

            ' Eliminate any previous references.
            moPickerMarket = Nothing

            Try

                ' Add the exchanges to the dropdown list.
                For Each oContract As Contract In oExchange.Contracts

                    If oContract.Enabled Then

                        'Trace.Writeline(String.Format("Contract: {0}, Description: {1}, ContractType: {2}", oContract.ContractID, oContract.Description, oContract.ContractType))
                        cboContracts.Items.Add(oContract)

                    End If

                Next

            Catch ex As Exception

                ' Trace the error.
                Trace.WriteLine("Error " & ex.ToString)

            End Try

        End If

    End Sub

    Private Sub cboContracts_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cboContracts.SelectedIndexChanged

        ' Populate the current contract's available markets.
        If Not cboContracts.SelectedItem Is Nothing Then

            ' Reference the current contract.
            Dim oContract As Contract = DirectCast(cboContracts.SelectedItem, Contract)

            ' Reference the contract's available markets.

            ' This would return all markets for the contract.
            ' moPickerMarkets = moContract.Markets

            ' This will return outright futures only.
            oContract.GetMarkets(0, StrategyType.None, Sub(e2)
                                                           'For Each oMarket As Market In e2.Markets
                                                           '    Trace.WriteLine(String.Format("Market: {0}, Description: {1}", oMarket.MarketID, oMarket.Description))
                                                           'Next
                                                           ' Invoke the update.
                                                           ' This places process on GUI thread.
                                                           If Me.IsHandleCreated Then
                                                               Me.BeginInvoke(New OnMarketListComplete(AddressOf MarketListComplete), e2)
                                                           Else
                                                               MarketListComplete(e2)
                                                           End If
                                                       End Sub)

        End If

    End Sub

    Private Sub MarketListComplete(e As MarketListEventArgs)

        ' Populate the list of markets available for the current contract.

        ' First clear the combo.
        cboMarkets.Items.Clear()

        ' Eliminate any previous references.
        moPickerMarket = Nothing

        If Not e.Markets Is Nothing Then

            Try

                ' Create a sorted list of the markets.
                ' Remember to turn sorting off on the combo or it will do a text sort.
                For Each oMarket As Market In e.Markets.GetSortedList

                    If oMarket.Enabled Then

                        cboMarkets.Items.Add(oMarket)

                    End If

                Next

            Catch ex As Exception

                ' Trace the error.
                Trace.WriteLine("Error " & ex.ToString)

            End Try

        End If

    End Sub

    Private Sub cboMarkets_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cboMarkets.SelectedIndexChanged

        ' Store a reference to the current market.
        If Not cboMarkets.SelectedItem Is Nothing Then
            moPickerMarket = DirectCast(cboMarkets.SelectedItem, Market)
        End If

    End Sub

#End Region

#Region " Account Data "

    ' Event that is raised when details for an account have 
    ' changed, or a new account is recieved.
    Private Sub moAccounts_AccountDetails(ByVal e As AccountDetailsEventArgs) Handles moAccounts.AccountDetails

        ' Invoke the update.
        ' This places process on GUI thread.
        ' Must use a delegate to pass arguments.
        If Me.IsHandleCreated Then
            Me.BeginInvoke(New Account.AccountDetailsEventHandler(AddressOf OnAccountDetails), New Object() {e})
        Else
            OnAccountDetails(e)
        End If

    End Sub

    Private Sub OnAccountDetails(ByVal e As AccountDetailsEventArgs)

        ' Check to see if the account exists prior to adding/subscribing to it.
        If Not e.Account.Subscribed Then

            ' Add the account to the list.
            If Not cboAccounts.Items.Contains(e.Account) Then
                cboAccounts.Items.Add(e.Account)

                If cboAccounts.Items.Count = 1 Then

                    cboAccounts.SelectedIndex = 0

                End If

            End If

        End If

    End Sub


    ' Event that is raised when the accounts overall balance,
    ' P&L or margin details have changed.
    Private Sub moAccount_AccountUpdate(ByVal e As AccountUpdateEventArgs) Handles moAccount.AccountUpdate

        ' Invoke the update.
        ' This places process on GUI thread.
        ' Must use a delegate to pass arguments.
        If Me.IsHandleCreated Then
            Me.BeginInvoke(New Account.AccountUpdateEventHandler(AddressOf OnAccountUpdate), New Object() {e})
        Else
            OnAccountUpdate(e)
        End If

    End Sub

    Private Sub OnAccountUpdate(ByVal e As AccountUpdateEventArgs)

        ' Just refresh the current account.
        DisplayAccount()

    End Sub

    ' Event that is raised when positions for accounts have
    ' changed.
    Private Sub moAccount_PositionUpdate(ByVal e As PositionUpdateEventArgs) Handles moAccount.PositionUpdate

        If Me.IsHandleCreated Then
            Me.BeginInvoke(New Account.PositionUpdateEventHandler(AddressOf OnPositionUpdate), New Object() {e})
        Else
            OnPositionUpdate(e)
        End If

    End Sub

    Private Sub OnPositionUpdate(ByVal e As PositionUpdateEventArgs)

        If e.Position.Market Is moMarket1 Then

            ' Display the position details.
            DisplayPosition(e.Position.Market, 1)

        ElseIf e.Position.Market Is moMarket2 Then

            ' Display the position details.
            DisplayPosition(e.Position.Market, 2)

        End If

    End Sub

    Private Sub OnAccountListComplete()

        Try

            ' Display the account list.
            For Each oAccount As Account In moHost.Accounts

                ' Add the account to the combo.
                cboAccounts.Items.Add(oAccount)
                oAccount.Subscribe()

            Next

            If cboAccounts.Items.Count > 0 Then

                cboAccounts.SelectedIndex = 0

            End If

        Catch ex As Exception
            ' Trace Errors.
            Trace.WriteLine(ex.ToString)
        End Try

    End Sub

    Private Sub DisplayAccount()

        If Not moAccount Is Nothing Then

            Try

                ' Display the current account balance.
                txtCash.Text = String.Format("{0:#,###,##0.00}", moAccount.AvailableCash)

            Catch ex As Exception
                ' Trace the error.
                Trace.WriteLine("Error: " & ex.ToString)

            End Try

        End If

    End Sub

    Private Sub DisplayPosition(ByVal poMarket As Market, piID As Integer)

        Dim strNet As String = ""
        Dim strBuys As String = ""
        Dim strSells As String = ""

        Try

            If Not poMarket Is Nothing AndAlso Not moAccount Is Nothing Then

                ' Display positions for current account and market1.

                ' Reference the market's positions.
                Dim oPosition As Position = moAccount.Positions(poMarket)

                If Not oPosition Is Nothing Then
                    ' Reference the net position.
                    strNet = oPosition.Net.ToString
                    strBuys = oPosition.Buys.ToString
                    strSells = oPosition.Sells.ToString
                End If

                Select Case piID
                    Case 1

                        ' Display the net position.
                        txtNet1.Text = strNet
                        ' Display the total Buys.
                        txtBuys1.Text = strBuys
                        ' Display the total Sells.
                        txtSells1.Text = strSells

                    Case 2

                        ' Display the net position.
                        txtNet2.Text = strNet
                        ' Display the total Buys.
                        txtBuys2.Text = strBuys
                        ' Display the total Sells.
                        txtSells2.Text = strSells

                End Select

            End If

        Catch ex As Exception
            ' Trace the error.
            Trace.WriteLine("Error " & ex.ToString)

        End Try


    End Sub

    Private Sub cboAccounts_SelectedValueChanged(sender As Object, e As EventArgs) Handles cboAccounts.SelectedValueChanged

        '    Private Sub cboAccounts_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cboAccounts.SelectedIndexChanged

        If Not cboAccounts.SelectedItem Is Nothing Then

            ' Reference the current account.
            moAccount = DirectCast(cboAccounts.SelectedItem, Account)
            moAccount.Subscribe(Sub()
                                    If Me.IsHandleCreated Then
                                        Me.BeginInvoke(New MethodInvoker(AddressOf DisplayAccountDetails))
                                    Else
                                        DisplayAccountDetails()
                                    End If
                                End Sub)

        End If

    End Sub

    Private Sub DisplayAccountDetails()

        ' Display the current account balance.
        DisplayAccount()

        ' Refresh positions.
        DisplayPosition(moMarket1, 1)
        DisplayPosition(moMarket2, 2)

        ' Display the orders.
        DisplayOrders()

    End Sub

#End Region

#Region " Startup and shutdown code "

    ' Initialise the api when the application starts.
    Private Sub frmMain_Load(ByVal sender As System.Object,
        ByVal e As System.EventArgs) Handles MyBase.Load

        ' Create the api host object using the built in Login dialog.
        moHost = Host.Login(APIServerType.Simulator, "T4Example", "112A04B0-5AAF-42F4-994E-FA7CB959C60B")

        ' Check for success.
        If moHost Is Nothing Then

            ' Host object not returned which means the user cancelled the login dialog.
            Me.Close()

        Else

            ' Login was successfull.
            Trace.WriteLine("Login Success")

            Init()

        End If

    End Sub

    ' Shutdown the api when the application exits.
    Private Sub frmMain_Closed(ByVal sender As Object,
        ByVal e As System.EventArgs) Handles MyBase.Closed

        ' Check to see that we have an api object.
        If Not moHost Is Nothing Then

            If Not moMarket1 Is Nothing Then
                RemoveHandler moMarket1.MarketCheckSubscription, AddressOf Markets_MarketCheckSubscription
                RemoveHandler moMarket1.MarketDepthUpdate, AddressOf Markets_MarketDepthUpdate
            End If

            If Not moMarket2 Is Nothing Then
                RemoveHandler moMarket2.MarketCheckSubscription, AddressOf Markets_MarketCheckSubscription
                RemoveHandler moMarket2.MarketDepthUpdate, AddressOf Markets_MarketDepthUpdate
            End If

            ' Dispose of the api.
            moHost.Dispose()
            moHost = Nothing

        End If

    End Sub

#End Region

#Region " Market Subscription "

    Private Sub cmdGet1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdGet1.Click

        ' Clear the values.
        DisplayMarketDetails(Nothing, 1)

        ' Subscribe to market1.
        NewMarketSubscription(moMarket1, moPickerMarket)

        ' Start the morket mode countdown.
        StartModeCountdown()

        ' Refresh the positions.
        DisplayPosition(moMarket1, 1)

    End Sub

    Private Sub cmdGet2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdGet2.Click

        Dim oMarket As Market = moHost.MarketData.MarketPickerHistorical(moMarket2)

        ' Clear the values.
        DisplayMarketDetails(Nothing, 2)

        ' Refresh the positions.
        DisplayPosition(moMarket2, 2)

        ' Subscribe to market2.
        NewMarketSubscription(moMarket2, oMarket)

    End Sub

    Private Sub NewMarketSubscription(ByRef poMarket As Market, ByRef poNewMarket As Market)
        ' Update an existing market reference to subscribe to a new/different market.

        ' If they are the same then don't do anything.
        ' We don't need to resubscribe to the same market.

        ' Explicitly register events as opposed to declaring withevents.
        ' This gives us more control.  
        ' It is important to unregister the marketchecksubscription prior to unsubscribing or the event will override and maintain the subscription.

        If (Not Object.ReferenceEquals(poMarket, poNewMarket)) Then
            ' Unsubscribe from the currently selected market.
            If (poMarket IsNot Nothing) Then

                ' Unregister the events for this market.
                RemoveHandler poMarket.MarketCheckSubscription, AddressOf Markets_MarketCheckSubscription
                RemoveHandler poMarket.MarketDepthUpdate, AddressOf Markets_MarketDepthUpdate

                poMarket.DepthUnsubscribe()

            End If

            ' Update the market reference.
            poMarket = poNewMarket

            If (poMarket IsNot Nothing) Then

                ' Register the events.
                AddHandler poMarket.MarketCheckSubscription, AddressOf Markets_MarketCheckSubscription
                AddHandler poMarket.MarketDepthUpdate, AddressOf Markets_MarketDepthUpdate

                ' Subscribe to the market.
                ' Use smart buffering.

                poMarket.DepthSubscribe(DepthBuffer.Smart, DepthLevels.BestOnly)

            End If
        End If

    End Sub


    Private Sub Markets_MarketCheckSubscription(ByVal e As MarketCheckSubscriptionEventArgs)

        ' No need to invoke on the gui thread.
        e.DepthSubscribeAtLeast(DepthBuffer.Smart, DepthLevels.BestOnly)

    End Sub

    Private Sub Markets_MarketDepthUpdate(ByVal e As MarketDepthUpdateEventArgs)

        ' Invoke the update.
        ' This places process on GUI thread.
        ' Must use a delegate to pass arguments.
        If Me.IsHandleCreated Then
            Me.BeginInvoke(New Market.MarketDepthUpdateEventHandler(AddressOf OnMarketDepthUpdate), New Object() {e})
        Else
            OnMarketDepthUpdate(e)
        End If

    End Sub

    Private Sub OnMarketDepthUpdate(ByVal e As MarketDepthUpdateEventArgs)

        Try

            If e.Market Is moMarket1 Then

                DisplayMarketDetails(e.Market, 1)

            ElseIf e.Market Is moMarket2 Then

                DisplayMarketDetails(e.Market, 2)

            End If

        Catch ex As Exception

            ' Trace the error.
            Trace.WriteLine("Error " & ex.ToString)

        End Try

    End Sub

    ''' <summary>
    ''' Update the market display values.
    ''' </summary>
    Private Sub DisplayMarketDetails(poMarket As Market, piID As Integer)

        Dim strDescription As String = ""
        Dim strBid As String = ""
        Dim strBidVol As String = ""
        Dim strOffer As String = ""
        Dim strOfferVol As String = ""
        Dim strLast As String = ""
        Dim strLastVol As String = ""
        Dim strLastVolTotal As String = ""

        If Not poMarket Is Nothing Then

            Try
                ' Display the market description.
                strDescription = poMarket.Description

                ' Get a reference to the MarketDepth object returned by GetDepth and then use the 
                ' properties from that reference. They won't change so can be accessed without locking
                ' the api host.
                With poMarket.GetDepth

                    ' Best bid.
                    If .Bids.Count > 0 Then
                        strBid = poMarket.PriceToDisplay(.Bids(0).Price)
                        strBidVol = .Bids(0).Volume.ToString
                    End If

                    ' Best offer.
                    If .Offers.Count > 0 Then
                        strOffer = poMarket.PriceToDisplay(.Offers(0).Price)
                        strOfferVol = .Offers(0).Volume.ToString
                    End If

                End With

                ' Get a reference to the MarketTrade object returned by GetTrade and then use the 
                ' properties from that reference. They won't change so can be accessed without locking
                ' the api host.
                With poMarket.GetTrade

                    ' Last trade.
                    strLast = poMarket.PriceToDisplay(.Price)
                    strLastVol = .Volume.ToString
                    strLastVolTotal = .TotalVolume.ToString

                End With


            Catch ex As Exception
                ' Trace the error.
                Trace.WriteLine("Error " & ex.ToString)
            End Try

        End If

        Select Case piID
            Case 1

                ' Update the market1 display values.
                txtMarketDescription1.Text = strDescription
                txtBid1.Text = strBid
                txtBidVol1.Text = strBidVol
                txtOffer1.Text = strOffer
                txtOfferVol1.Text = strOfferVol
                txtLast1.Text = strLast
                txtLastVol1.Text = strLastVol
                txtLastVolTotal1.Text = strLastVolTotal

            Case 2

                ' Update the market2 display values.
                txtMarketDescription2.Text = strDescription
                txtBid2.Text = strBid
                txtBidVol2.Text = strBidVol
                txtOffer2.Text = strOffer
                txtOfferVol2.Text = strOfferVol
                txtLast2.Text = strLast
                txtLastVol2.Text = strLastVol
                txtLastVolTotal2.Text = strLastVolTotal

        End Select

    End Sub

#End Region

#Region " Save Settings "

    Private Sub cmdSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSave.Click

        Try

            ' XML Doc.
            Dim oDoc As New XmlDocument

            ' XML Node.
            Dim oMarket As XmlNode
            Dim oMarkets As XmlNode
            Dim oAttribute As XmlAttribute

            ' Create the main node.
            oMarkets = oDoc.CreateNode(XmlNodeType.Element, "markets", "")
            oDoc.AppendChild(oMarkets)

            If Not moMarket1 Is Nothing Then

                ' Create a node.
                oMarket = oDoc.CreateNode(XmlNodeType.Element, "market1", "")

                ' Market ID.
                oAttribute = oDoc.CreateAttribute("MarketID")
                oAttribute.Value = moMarket1.MarketID
                oMarket.Attributes.Append(oAttribute)

                ' Add the node to the xml document.
                oMarkets.AppendChild(oMarket)

            End If

            If Not moMarket2 Is Nothing Then

                ' Create a node.
                oMarket = oDoc.CreateNode(XmlNodeType.Element, "market2", "")


                ' Market ID.
                oAttribute = oDoc.CreateAttribute("MarketID")
                oAttribute.Value = moMarket2.MarketID
                oMarket.Attributes.Append(oAttribute)

                ' Add the node to the xml document.
                oMarkets.AppendChild(oMarket)

            End If

            ' Save the xml to the server.
            moHost.MasterUser.UserSettings = oDoc
            moHost.MasterUser.SaveUserSettings()

        Catch ex As Exception

            ' Trace.
            Trace.WriteLine(ex.ToString)

        End Try

    End Sub

    Public Function App_Path() As String
        Return System.AppDomain.CurrentDomain.BaseDirectory()
    End Function

#End Region

#Region " Single Order "

    ' Method that submits a single order.
    Private Sub SubmitSingleOrder(ByVal poMarket As Market, ByVal peBuySell As BuySell, ByVal pdecLimitPrice As Decimal?)

        If Not moAccount Is Nothing AndAlso Not poMarket Is Nothing Then
            ' Submit an order.
            Dim oOrder As Order = moHost.SubmitOrder(
                moAccount,
                poMarket,
                peBuySell,
                PriceType.Limit,
                1,
                pdecLimitPrice)

            ' Display the orders.
            DisplayOrders()

        End If

    End Sub

    ' Pull the single order that was submitted.
    Private Sub PullSingleOrder(ByVal poOrder As Order)

        ' Check to see that we have an order.
        If Not poOrder Is Nothing Then

            ' Check to see if the order is working.
            If poOrder.IsWorking Then

                ' Pull the order.
                poOrder.Pull()

            End If

        End If

    End Sub

#End Region

#Region " Submission/Cancelation "

    Private Sub cmdBuy1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdBuy1.Click
        ' Submit a single order.
        SubmitSingleOrder(moMarket1, BuySell.Buy, moMarket1.DisplayToPrice(txtBid1.Text))
    End Sub
    Private Sub cmdSell1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSell1.Click
        ' Submit a single order.
        SubmitSingleOrder(moMarket1, BuySell.Sell, moMarket1.DisplayToPrice(txtOffer1.Text))
    End Sub

    Private Sub cmdSell2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSell2.Click
        ' Submit a single order.
        SubmitSingleOrder(moMarket2, BuySell.Sell, moMarket2.DisplayToPrice(txtOffer2.Text))
    End Sub

    Private Sub cmdBuy2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdBuy2.Click
        ' Submit a single order.
        SubmitSingleOrder(moMarket2, BuySell.Buy, moMarket2.DisplayToPrice(txtBid2.Text))
    End Sub

    Private Sub lstOrders_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstOrders.DoubleClick

        ' Pull the order that has been double clicked on.
        Dim oItem As OrderItem = DirectCast(lstOrders.SelectedItem, OrderItem)
        If oItem IsNot Nothing Then

            ' Attempt to pull the order.
            PullSingleOrder(oItem.Order)

        End If

    End Sub

#End Region

#Region " Order Data "

    Private Sub moAccount_OrderUpdate(ByVal e As OrderUpdateEventArgs) Handles moAccount.OrderUpdate

        ' Trace.WriteLine("moAccount_OrderUpdate Start")

        ' Invoke the update.
        ' This places process on GUI thread.
        ' Must use a delegate to pass arguments.
        If Me.IsHandleCreated Then
            Me.BeginInvoke(New Account.OrderUpdateEventHandler(AddressOf OnAccountOrderUpdate), New Object() {e})
        Else
            OnAccountOrderUpdate(e)
        End If

        ' Trace.WriteLine("moAccount_OrderUpdate End")

    End Sub

    Private Sub OnAccountOrderUpdate(ByVal e As OrderUpdateEventArgs)

        ' Redraw the order list.
        DisplayOrders()

    End Sub

    ''' <summary>
    ''' Class to hold an order and its description in the list box.
    ''' </summary>
    Public Class OrderItem

        ''' <summary>
        ''' The Order.
        ''' </summary>
        Public ReadOnly Order As Order

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="poOrder"></param>
        Public Sub New(poOrder As Order)
            Order = poOrder
        End Sub

        ''' <summary>
        ''' Display a description of the order.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String

            With Order
                Return .Market.Description & "   " &
                    .BuySell.ToString & "   " &
                    .TotalFillVolume & "/" & .CurrentVolume & " @ " &
                    .Market.PriceToDisplay(.CurrentLimitPrice) & "   " &
                    .Status.ToString & "   " &
                    .StatusDetail & "  " &
                    .SubmitTime & " " &
                    .ActivationType.ToString & " " &
                    If(.ActivationData Is Nothing, "", .ActivationData.ToString)
            End With

        End Function

    End Class

    Private Sub DisplayOrders()

        ' Trace.WriteLine("DisplayOrders Start")

        Try

            ' Suspend the layout of the listbox.
            lstOrders.SuspendLayout()

            ' Clear and repopulate the list.
            lstOrders.Items.Clear()

            For Each oOrder As Order In moAccount.Orders.GetSortedList

                lstOrders.Items.Add(New OrderItem(oOrder))

            Next

        Catch ex As Exception

            ' Trace the error.
            Trace.WriteLine("Error: " & ex.ToString)

        Finally

            ' Resume layout of the listbox.
            lstOrders.ResumeLayout()

        End Try

        ' Trace.WriteLine("DisplayOrders End")

    End Sub

#End Region

#Region " Market Mode Countdown "

    ''' <summary>
    ''' Timer for providing a countdown to the next market mode change.
    ''' </summary>
    ''' <remarks></remarks>
    Private moModeCountdown As System.Threading.Timer

    ''' <summary>
    ''' Timer interval when no countdown is currently in process.
    ''' </summary>
    ''' <remarks></remarks>
    Private Const mciModeCountdownSlow As Integer = 1000

    ''' <summary>
    ''' Timer interval while we are counting down.
    ''' </summary>
    ''' <remarks></remarks>
    Private Const mciModeCountdownFast As Integer = 200

    ''' <summary>
    ''' The number of seconds to countdown to the next market mode change.
    ''' For test purposes set to 24 hours so we can see it function.
    ''' </summary>
    Private Const mciCountDownSeconds As Integer = 86400

    ''' <summary>
    ''' The timer interval we are currently using.
    ''' </summary>
    ''' <remarks></remarks>
    Private miModeCountdownInterval As Integer = mciModeCountdownSlow

    ''' <summary>
    ''' Start the market mode countdown support.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub StartModeCountdown()

        moModeCountdown = New Threading.Timer(AddressOf OnModeCountdown, Nothing, 1000, miModeCountdownInterval)

    End Sub

    ''' <summary>
    ''' Stop the market mode countdown support.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub StopModeCountdown()

        If Not moModeCountdown Is Nothing Then
            moModeCountdown.Dispose()
            moModeCountdown = Nothing
        End If

    End Sub


    ''' <summary>
    ''' Timer event to provide a countdown to the next market mode change.
    ''' </summary>
    ''' <param name="state"></param>
    ''' <remarks></remarks>
    Private Sub OnModeCountdown(state As Object)

        Try

            If Not moMarket1 Is Nothing AndAlso Not moMarket1.Contract.Exchange.MarketDataType = MarketDataType.Delayed Then

                ' Get the next trading event.
                Dim dTime As DateTime = moHost.RemoteTime
                Dim oSE As T4.API.TradingSchedule.SessionEvent = moMarket1.Contract.TradingSchedule.GetNextEvent(dTime, moMarket1.LastTradingDate)

                ' Check to see if we are within the countdown time.
                If Not oSE Is Nothing AndAlso dTime.AddSeconds(mciCountDownSeconds) >= oSE.Time Then

                    ' Check to see we are updating frequently enough.
                    If miModeCountdownInterval <> mciModeCountdownFast Then
                        miModeCountdownInterval = mciModeCountdownFast
                        moModeCountdown.Change(mciModeCountdownFast, mciModeCountdownFast)
                    End If

                    ' Get the current market mode.
                    Dim enCurrentMode As MarketMode = moMarket1.GetDepth().Mode

                    ' Check to see if the mode is actually changing.
                    If enCurrentMode <> oSE.Mode Then

                        ' Mode is changing, update the display to a countdown to the new mode.
                        Dim dTimeToGo As TimeSpan = oSE.Time.Subtract(dTime)
                        Dim sText As String = ""
                        If dTimeToGo.Hours > 0 Then
                            sText = dTimeToGo.ToString("h\:mm\:ss")
                        Else
                            sText = dTimeToGo.ToString("m\:ss")
                        End If

                        ' Invoke the update.
                        Me.BeginInvoke(New MethodInvoker(Sub()

                                                             txtMode.Text = sText

                                                         End Sub))

                    End If

                Else

                    ' Check to see we are updating frequently enough.
                    If miModeCountdownInterval <> mciModeCountdownSlow Then
                        miModeCountdownInterval = mciModeCountdownSlow
                        moModeCountdown.Change(mciModeCountdownSlow, mciModeCountdownSlow)
                    End If

                    ' Get the current market mode.
                    Dim enCurrentMode As MarketMode = moMarket1.GetDepth().Mode

                    ' Check to see if the mode is actually changing.
                    If enCurrentMode <> oSE.Mode Then

                        Dim sText As String = enCurrentMode.ToString

                        ' Invoke the update.
                        Me.BeginInvoke(New MethodInvoker(Sub()

                                                             txtMode.Text = sText

                                                         End Sub))

                    End If

                End If

            End If

        Catch ex As Exception
            Trace.WriteLine("Error " & ex.ToString)
        End Try

    End Sub

#End Region

#Region " Misc Examples "

    Const AUTOOCO As String = "Submit Auto OCO"
    Const FIVETICKSOFF As String = "Work 5 Ticks Off Market"
    Const TIMEACTIVATION As String = "Time Activation"
    Const PRICEACTIVATION As String = "Price Activation"
    Const SPARK As String = "Spark Order"
    Const REVISESPARKORDER As String = "Revise Spark"
    Const AUTOOCOM As String = "Submit Auto OCO M"

    ' Setup misc example combos.
    Private Sub SetupMiscExamples()

        ' Add examples to combos.
        cboMisc1.Items.Add(AUTOOCO)
        cboMisc1.Items.Add(AUTOOCOM)
        cboMisc1.Items.Add(SPARK)
        cboMisc1.Items.Add(REVISESPARKORDER)
        cboMisc1.Items.Add(FIVETICKSOFF)
        cboMisc1.Items.Add(TIMEACTIVATION)
        cboMisc1.Items.Add(PRICEACTIVATION)

        cboMisc2.Items.Add(AUTOOCO)
        cboMisc2.Items.Add(AUTOOCOM)
        cboMisc2.Items.Add(SPARK)
        cboMisc2.Items.Add(REVISESPARKORDER)
        cboMisc2.Items.Add(FIVETICKSOFF)
        cboMisc2.Items.Add(TIMEACTIVATION)
        cboMisc2.Items.Add(PRICEACTIVATION)


        ' Be sure the first items are selected.
        cboMisc1.SelectedIndex = 0
        cboMisc2.SelectedIndex = 0

    End Sub

    Private Sub cmdRunMisc1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdRunMisc1.Click

        If Not moMarket1 Is Nothing Then

            Select Case cboMisc1.Text
                Case AUTOOCO

                    ' Run autooco sample code.
                    SubmitAOCO(moMarket1, BuySell.Buy, txtBid1.Text)

                Case AUTOOCOM

                    ' Run autooco sample code.
                    SubmitAOCOM(moMarket1, BuySell.Buy, txtBid1.Text)


                Case FIVETICKSOFF

                    ' Run the five ticks off code.
                    SubmitFiveTicksOff(moMarket1, BuySell.Buy, txtBid1.Text)

                Case TIMEACTIVATION

                    ' Time activation order.
                    SubmitTimeActivation(moMarket1, BuySell.Buy, txtBid1.Text)

                Case PRICEACTIVATION

                    ' Time activation order.
                    SubmitPriceActivation(moMarket1, BuySell.Buy, txtBid1.Text)

                Case SPARK

                    ' Spark order.
                    SubmitSpark(moMarket1, BuySell.Sell, txtOffer1.Text)

                Case REVISESPARKORDER

                    ' Revise the spark.
                    ReviseSpark()

            End Select

        End If

    End Sub

    Private Sub cmdRunMisc2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdRunMisc2.Click

        If Not moMarket2 Is Nothing Then

            Select Case cboMisc2.Text
                Case AUTOOCO

                    ' Run autooco sample code.
                    SubmitAOCO(moMarket2, BuySell.Sell, txtOffer2.Text)

                Case AUTOOCOM

                    ' Run autooco sample code.
                    SubmitAOCOM(moMarket2, BuySell.Sell, txtOffer2.Text)

                Case FIVETICKSOFF

                    ' Run the five ticks off code.
                    SubmitFiveTicksOff(moMarket2, BuySell.Sell, txtOffer2.Text)


                Case TIMEACTIVATION

                    ' Time activation order.
                    SubmitTimeActivation(moMarket2, BuySell.Buy, txtBid2.Text)

                Case PRICEACTIVATION

                    ' Time activation order.
                    SubmitPriceActivation(moMarket2, BuySell.Buy, txtBid2.Text)

                Case SPARK

                    ' Spark order.
                    SubmitSpark(moMarket2, BuySell.Sell, txtOffer2.Text)

            End Select

        End If

    End Sub

#Region " Auto OCO "

    ' Simple example of how to submit and cancel an Auto OCO.
    Private Sub SubmitAOCO(ByVal poMarket As Market, ByVal peBuySell As BuySell, ByVal pstrLimitDisplayPrice As String)


        If Not moAccount Is Nothing AndAlso Not poMarket Is Nothing Then

            ' Limit price reference.
            Dim decLimitPrice As Decimal? = poMarket.DisplayToPrice(pstrLimitDisplayPrice)

            ' Create the batch submission object for AutoOCO
            Dim oBatch As OrderSubmissionBatch = moHost.GetOrderSubmission(OrderLink.AutoOCO)

            ' Add an order to the batch.
            ' This is the trigger order.
            oBatch.Add(moAccount,
                       poMarket,
                       peBuySell,
                        PriceType.Limit,
                        1,
                        decLimitPrice)

            If peBuySell = BuySell.Buy Then

                ' Add an order to the batch.
                ' This is the sell limit of the oco above the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price.
                oBatch.Add(moAccount,
                        poMarket,
                        BuySell.Sell,
                        PriceType.Limit,
                        0,
                        poMarket.AddPriceIncrements(5, 0))

                ' Add an order to the batch.
                ' This is the stop of the oco below the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price.
                oBatch.Add(moAccount,
                           poMarket,
                           BuySell.Sell,
                           PriceType.StopLimit,
                           0,
                           poMarket.AddPriceIncrements(-7, 0),
                           poMarket.AddPriceIncrements(-5, 0),
                           "")

            Else

                ' Add an order to the batch.
                ' This is the buy limit of the oco below the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price.
                oBatch.Add(moAccount,
                           poMarket,
                           BuySell.Buy,
                           PriceType.Limit,
                           0,
                           poMarket.AddPriceIncrements(-5, 0))

                ' Add an order to the batch.
                ' This is the buy stop of the oco above the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price.
                oBatch.Add(moAccount,
                           poMarket,
                           BuySell.Buy,
                           PriceType.StopMarket,
                           0,
                           Nothing,
                           poMarket.AddPriceIncrements(5, 0),
                           "")

            End If


            ' Submit the batch.
            Dim oOrders As List(Of Order) = oBatch.Send()

            ' Display the orders.
            DisplayOrders()


            ' Pull may fail if attempted too soon.
            ' Like 1 millisecond later.

            '' This is how you would cancel the batch.
            'Dim oBatchPull As OrderPullBatch = moHost.GetOrderPull

            '' Add the orders to the pull.
            'oBatchPull.Add(oOrder1)
            'oBatchPull.Add(oOrder2)
            'oBatchPull.Add(oOrder3)

            '' Pull the batch.
            'oBatchPull.Send()

        End If

    End Sub


    ' Simple example of how to submit and cancel an Auto OCO.
    Private Sub SubmitAOCOM(ByVal poMarket As Market, ByVal peBuySell As BuySell, ByVal pstrLimitDisplayPrice As String)


        If Not moAccount Is Nothing AndAlso Not poMarket Is Nothing Then

            ' Limit price reference.
            Dim decLimitPrice As Decimal? = poMarket.DisplayToPrice(pstrLimitDisplayPrice)

            ' Create the batch submission object for AutoOCO
            Dim oBatch As OrderSubmissionBatch = moHost.GetOrderSubmission(OrderLink.AutoOCOm)



            If peBuySell = BuySell.Buy Then

                ' Add an order to the batch.
                ' This is the trigger order.
                ' Place it a bit off the market so that we can see it work at the market.
                ' Using the frontend revise the order to fill.  This way we can see the OCO logic in action.
                oBatch.Add(moAccount,
                           poMarket,
                           peBuySell,
                            PriceType.Limit,
                            10,
                            poMarket.AddPriceIncrements(-5, decLimitPrice.GetValueOrDefault))

                ' Add an order to the batch.
                ' This is the sell limit of the oco above the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price.
                oBatch.Add(moAccount,
                        poMarket,
                        BuySell.Sell,
                        PriceType.Limit,
                        TimeType.Normal,
                        0,
                        poMarket.AddPriceIncrements(5, 0), Nothing, "", Nothing, ActivationType.Hold, Nothing, 0, 4)

                ' Add an order to the batch.
                ' This is the sell limit of the oco above the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price.
                ' Slightly different price.
                oBatch.Add(moAccount,
                        poMarket,
                        BuySell.Sell,
                        PriceType.Limit,
                        TimeType.Normal,
                        0,
                        poMarket.AddPriceIncrements(6, 0), Nothing, "", Nothing, ActivationType.Hold, Nothing, 0, 3)

                ' Add an order to the batch.
                ' This is the sell limit of the oco above the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price.
                ' Slightly different price.
                oBatch.Add(moAccount,
                        poMarket,
                        BuySell.Sell,
                        PriceType.Limit,
                        TimeType.Normal,
                        0,
                        poMarket.AddPriceIncrements(7, 0), Nothing, "", Nothing, ActivationType.Hold, Nothing, 0, 0)


                ' Add an order to the batch.
                ' This is the stop of the oco below the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price.
                oBatch.Add(moAccount,
                           poMarket,
                           BuySell.Sell,
                           PriceType.StopLimit,
                           0,
                           poMarket.AddPriceIncrements(-7, 0),
                           poMarket.AddPriceIncrements(-5, 0),
                           "")

            Else


                ' Add an order to the batch.
                ' This is the trigger order.
                ' Place it a bit off the market so that we can see it work at the market.
                ' Using the frontend revise the order to fill.  This way we can see the OCO logic in action.
                oBatch.Add(moAccount,
                           poMarket,
                           peBuySell,
                            PriceType.Limit,
                            10,
                            poMarket.AddPriceIncrements(5, decLimitPrice.GetValueOrDefault))

                ' Add an order to the batch.
                ' This is the buy limit of the oco below the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price.
                oBatch.Add(moAccount,
                           poMarket,
                           BuySell.Buy,
                           PriceType.Limit,
                              TimeType.Normal,
                           0,
                           poMarket.AddPriceIncrements(-5, 0), Nothing, "", Nothing, ActivationType.Hold, Nothing, 0, 4)

                ' Add an order to the batch.
                ' This is the buy limit of the oco below the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price.
                ' Slightly different price.
                oBatch.Add(moAccount,
                           poMarket,
                           BuySell.Buy,
                           PriceType.Limit,
                              TimeType.Normal,
                           0,
                           poMarket.AddPriceIncrements(-6, 0), Nothing, "", Nothing, ActivationType.Hold, Nothing, 0, 3)

                ' Add an order to the batch.
                ' This is the buy limit of the oco below the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price.
                ' Slightly different price.
                oBatch.Add(moAccount,
                           poMarket,
                           BuySell.Buy,
                           PriceType.Limit,
                              TimeType.Normal,
                           0,
                           poMarket.AddPriceIncrements(-7, 0), Nothing, "", Nothing, ActivationType.Hold, Nothing, 0, 0)


                ' Add an order to the batch.
                ' This is the buy stop of the oco above the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price.
                oBatch.Add(moAccount,
                           poMarket,
                           BuySell.Buy,
                           PriceType.StopMarket,
                           0,
                           Nothing,
                           poMarket.AddPriceIncrements(5, 0),
                           "")

            End If


            ' Submit the batch.
            Dim oOrders As List(Of Order) = oBatch.Send()

            ' Display the orders.
            DisplayOrders()


            ' Pull may fail if attempted too soon.
            ' Like 1 millisecond later.

            '' This is how you would cancel the batch.
            'Dim oBatchPull As OrderPullBatch = moHost.GetOrderPull

            '' Add the orders to the pull.
            'oBatchPull.Add(oOrder1)
            'oBatchPull.Add(oOrder2)
            'oBatchPull.Add(oOrder3)

            '' Pull the batch.
            'oBatchPull.Send()

        End If

    End Sub

#End Region

#Region " Spark order "

    Private moOrder1 As Order
    Private moOrder2 As Order

    ' Simple example of how to submit and cancel a Spark order.
    Private Sub SubmitSpark(ByVal poMarket As Market, ByVal peBuySell As BuySell, ByVal pstrLimitDisplayPrice As String)


        If Not moAccount Is Nothing AndAlso Not poMarket Is Nothing Then

            ' Limit price reference.
            ' Convert the limit price to a double.
            Dim decLimitPrice As Decimal? = poMarket.DisplayToPrice(pstrLimitDisplayPrice)

            ' Create the batch submission object.
            Dim oBatch As OrderSubmissionBatch = moHost.GetOrderSubmission(OrderLink.Spark2)

            ' Add an order to the batch.
            ' This is the trigger order.
            oBatch.Add(moAccount,
                       poMarket,
                       peBuySell,
                       PriceType.Limit,
                       1,
                       decLimitPrice)


            If peBuySell = BuySell.Buy Then

                ' Add an order to the batch.
                ' This is the sell limit of the oco above the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price
                oBatch.Add(moAccount,
                           poMarket,
                           BuySell.Sell,
                           PriceType.Limit,
                           0,
                            poMarket.AddPriceIncrements(5, 0))

            Else

                ' Add an order to the batch.
                ' This is the buy limit of the oco below the market.
                ' Note the flip of Buy/Sell.
                ' Note the price is a distance not an absolute price
                oBatch.Add(moAccount,
                           poMarket,
                           BuySell.Buy,
                           PriceType.Limit,
                           0,
                           poMarket.AddPriceIncrements(-5, 0))


            End If

            ' Submit the batch.
            Dim oOrders As List(Of Order) = oBatch.Send()

            moOrder1 = oOrders(0)
            moOrder2 = oOrders(1)

            ' Display the orders.
            DisplayOrders()

        End If

    End Sub

    Private Sub ReviseSpark()

        If Not moOrder1 Is Nothing AndAlso Not moOrder2 Is Nothing Then

            ' Create the batch revision object.
            Dim oBatch As OrderRevisionBatch = moHost.GetOrderRevision
            oBatch.Add(moOrder1, 2, moOrder1.CurrentLimitPrice)
            oBatch.Add(moOrder2, 2, moOrder2.CurrentLimitPrice)
            oBatch.Send()

        End If

    End Sub

#End Region

#Region " Work Order Five Ticks From Market "

    ' Place an order five ticks off the market.
    Private Sub SubmitFiveTicksOff(ByVal poMarket As Market, ByVal peBuySell As BuySell, ByVal pstrLimitDisplayPrice As String)

        ' Limit price reference.
        ' Convert the limit price to a double.
        Dim decLimitPrice As Decimal? = poMarket.DisplayToPrice(pstrLimitDisplayPrice)

        ' Add or subtract five increments from the current price depending on what side of the market we are.
        If peBuySell = BuySell.Buy Then
            decLimitPrice = poMarket.AddPriceIncrements(-5, decLimitPrice.GetValueOrDefault)
        Else
            decLimitPrice = poMarket.AddPriceIncrements(5, decLimitPrice.GetValueOrDefault)
        End If

        ' Submit a single order five ticks off the market.
        SubmitSingleOrder(poMarket, peBuySell, decLimitPrice)

    End Sub

#End Region

#Region " Submit Activation "

    ' Place an order five ticks off the market.
    Private Sub SubmitTimeActivation(ByVal poMarket As Market, ByVal peBuySell As BuySell, ByVal pstrLimitDisplayPrice As String)

        ' Limit price reference.
        Dim decLimitPrice As Decimal? = poMarket.DisplayToPrice(pstrLimitDisplayPrice)

        If Not moAccount Is Nothing AndAlso Not poMarket Is Nothing Then

            ' Activation times must be in US Central Time (Chicago time).
            Dim oAct As New ActivationData
            oAct.SubmitTime = moHost.RemoteTime.AddSeconds(10)
            oAct.CancelTime = moHost.RemoteTime.AddSeconds(30)

            ' The following also works.  The delay values represent seconds of time laps from now.
            'oAct.SubmitDelay = New TimeSpan(0, 0, 10)
            'oAct.CancelDelay = New TimeSpan(0, 0, 30)

            ' Submit an order.
            moHost.SubmitOrder(
                moAccount,
                poMarket,
                peBuySell,
                PriceType.Limit,
                TimeType.Normal,
                1,
                decLimitPrice,
                Nothing,
                "",
                Nothing,
                ActivationType.AtOrAfterTime,
                oAct)

            ' Display the orders.
            DisplayOrders()

        End If

    End Sub

    ' Place an order five ticks off the market.
    Private Sub SubmitPriceActivation(ByVal poMarket As Market, ByVal peBuySell As BuySell, ByVal pstrLimitDisplayPrice As String)

        ' Limit price reference.
        ' Convert the limit price to a double.
        Dim decLimitPrice As Decimal? = poMarket.DisplayToPrice(pstrLimitDisplayPrice)

        If Not moAccount Is Nothing AndAlso Not poMarket Is Nothing Then

            ' Activation times must be in US Central Time (Chicago time).
            Dim oAct As New ActivationData
            oAct.CancelDelay = New TimeSpan(0, 0, 30)
            oAct.Price = poMarket.AddPriceIncrements(5, poMarket.GetDepth.Bids(0).Price)

            ' Submit an order.
            ' This order will activate if a price trades that greater than or equal to five ticks above the current best bid.
            ' The order will be canceled after 30 seconds.
            moHost.SubmitOrder(
                moAccount,
                poMarket,
                peBuySell,
                PriceType.Limit,
                TimeType.Normal,
                1,
                decLimitPrice,
                Nothing,
                "",
                Nothing,
                ActivationType.AtOrAboveTradeTicks,
                oAct)

            ' Display the orders.
            DisplayOrders()

        End If

    End Sub



#End Region

#End Region



End Class
