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
    Friend WithEvents cmdAdd As System.Windows.Forms.Button
    Friend WithEvents lblAccount As System.Windows.Forms.Label
    Friend WithEvents cboAccounts As System.Windows.Forms.ComboBox
    Friend WithEvents grpAccount As System.Windows.Forms.GroupBox
    Friend WithEvents lblAccountInfo As System.Windows.Forms.Label
    Friend WithEvents cmdCreateUDS As Button
    Friend WithEvents lstLegs As ListBox
    Friend WithEvents lstStatus As ListBox
    Friend WithEvents lblStatus As Label
    Friend WithEvents lblLegs As Label
    Friend WithEvents lblQuantity As Label
    Friend WithEvents lblBuySell As Label
    Friend WithEvents cboQuantity As ComboBox
    Friend WithEvents cboBuySell As ComboBox
    Friend WithEvents grpMarket1 As System.Windows.Forms.GroupBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.cboMarkets = New System.Windows.Forms.ComboBox()
        Me.cboExchanges = New System.Windows.Forms.ComboBox()
        Me.cboContracts = New System.Windows.Forms.ComboBox()
        Me.lblMarket = New System.Windows.Forms.Label()
        Me.lblContract = New System.Windows.Forms.Label()
        Me.lblExchange = New System.Windows.Forms.Label()
        Me.cmdAdd = New System.Windows.Forms.Button()
        Me.lblAccount = New System.Windows.Forms.Label()
        Me.cboAccounts = New System.Windows.Forms.ComboBox()
        Me.grpAccount = New System.Windows.Forms.GroupBox()
        Me.lblAccountInfo = New System.Windows.Forms.Label()
        Me.grpMarket1 = New System.Windows.Forms.GroupBox()
        Me.lblQuantity = New System.Windows.Forms.Label()
        Me.lblBuySell = New System.Windows.Forms.Label()
        Me.cboQuantity = New System.Windows.Forms.ComboBox()
        Me.cboBuySell = New System.Windows.Forms.ComboBox()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lblLegs = New System.Windows.Forms.Label()
        Me.lstStatus = New System.Windows.Forms.ListBox()
        Me.cmdCreateUDS = New System.Windows.Forms.Button()
        Me.lstLegs = New System.Windows.Forms.ListBox()
        Me.grpAccount.SuspendLayout()
        Me.grpMarket1.SuspendLayout()
        Me.SuspendLayout()
        '
        'cboMarkets
        '
        Me.cboMarkets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboMarkets.Location = New System.Drawing.Point(144, 98)
        Me.cboMarkets.Name = "cboMarkets"
        Me.cboMarkets.Size = New System.Drawing.Size(579, 28)
        Me.cboMarkets.TabIndex = 6
        Me.cboMarkets.TabStop = False
        '
        'cboExchanges
        '
        Me.cboExchanges.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboExchanges.Location = New System.Drawing.Point(144, 28)
        Me.cboExchanges.Name = "cboExchanges"
        Me.cboExchanges.Size = New System.Drawing.Size(579, 28)
        Me.cboExchanges.Sorted = True
        Me.cboExchanges.TabIndex = 7
        Me.cboExchanges.TabStop = False
        '
        'cboContracts
        '
        Me.cboContracts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboContracts.Location = New System.Drawing.Point(144, 63)
        Me.cboContracts.Name = "cboContracts"
        Me.cboContracts.Size = New System.Drawing.Size(579, 28)
        Me.cboContracts.Sorted = True
        Me.cboContracts.TabIndex = 8
        Me.cboContracts.TabStop = False
        '
        'lblMarket
        '
        Me.lblMarket.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMarket.Location = New System.Drawing.Point(16, 96)
        Me.lblMarket.Name = "lblMarket"
        Me.lblMarket.Size = New System.Drawing.Size(125, 31)
        Me.lblMarket.TabIndex = 11
        Me.lblMarket.Text = "Market:"
        Me.lblMarket.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblContract
        '
        Me.lblContract.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblContract.Location = New System.Drawing.Point(13, 61)
        Me.lblContract.Name = "lblContract"
        Me.lblContract.Size = New System.Drawing.Size(128, 31)
        Me.lblContract.TabIndex = 10
        Me.lblContract.Text = "Contract:"
        Me.lblContract.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblExchange
        '
        Me.lblExchange.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblExchange.Location = New System.Drawing.Point(22, 26)
        Me.lblExchange.Name = "lblExchange"
        Me.lblExchange.Size = New System.Drawing.Size(119, 31)
        Me.lblExchange.TabIndex = 9
        Me.lblExchange.Text = "Exchange:"
        Me.lblExchange.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'cmdAdd
        '
        Me.cmdAdd.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdAdd.Location = New System.Drawing.Point(552, 235)
        Me.cmdAdd.Name = "cmdAdd"
        Me.cmdAdd.Size = New System.Drawing.Size(171, 30)
        Me.cmdAdd.TabIndex = 10
        Me.cmdAdd.TabStop = False
        Me.cmdAdd.Text = "Add"
        '
        'lblAccount
        '
        Me.lblAccount.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAccount.Location = New System.Drawing.Point(10, 32)
        Me.lblAccount.Name = "lblAccount"
        Me.lblAccount.Size = New System.Drawing.Size(171, 31)
        Me.lblAccount.TabIndex = 41
        Me.lblAccount.Text = "Current Account:"
        Me.lblAccount.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'cboAccounts
        '
        Me.cboAccounts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboAccounts.Location = New System.Drawing.Point(187, 32)
        Me.cboAccounts.Name = "cboAccounts"
        Me.cboAccounts.Size = New System.Drawing.Size(536, 28)
        Me.cboAccounts.Sorted = True
        Me.cboAccounts.TabIndex = 42
        Me.cboAccounts.TabStop = False
        '
        'grpAccount
        '
        Me.grpAccount.Controls.Add(Me.lblAccountInfo)
        Me.grpAccount.Controls.Add(Me.cboAccounts)
        Me.grpAccount.Controls.Add(Me.lblAccount)
        Me.grpAccount.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpAccount.Location = New System.Drawing.Point(19, 18)
        Me.grpAccount.Name = "grpAccount"
        Me.grpAccount.Size = New System.Drawing.Size(751, 74)
        Me.grpAccount.TabIndex = 63
        Me.grpAccount.TabStop = False
        Me.grpAccount.Text = "Account"
        '
        'lblAccountInfo
        '
        Me.lblAccountInfo.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAccountInfo.Location = New System.Drawing.Point(112, 139)
        Me.lblAccountInfo.Name = "lblAccountInfo"
        Me.lblAccountInfo.Size = New System.Drawing.Size(301, 44)
        Me.lblAccountInfo.TabIndex = 68
        Me.lblAccountInfo.Text = "An Account must be selected to view positions and submit trades."
        Me.lblAccountInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'grpMarket1
        '
        Me.grpMarket1.Controls.Add(Me.lblQuantity)
        Me.grpMarket1.Controls.Add(Me.lblBuySell)
        Me.grpMarket1.Controls.Add(Me.cboQuantity)
        Me.grpMarket1.Controls.Add(Me.cboBuySell)
        Me.grpMarket1.Controls.Add(Me.lblStatus)
        Me.grpMarket1.Controls.Add(Me.lblLegs)
        Me.grpMarket1.Controls.Add(Me.lstStatus)
        Me.grpMarket1.Controls.Add(Me.cmdCreateUDS)
        Me.grpMarket1.Controls.Add(Me.lstLegs)
        Me.grpMarket1.Controls.Add(Me.cboExchanges)
        Me.grpMarket1.Controls.Add(Me.lblMarket)
        Me.grpMarket1.Controls.Add(Me.cboContracts)
        Me.grpMarket1.Controls.Add(Me.cboMarkets)
        Me.grpMarket1.Controls.Add(Me.lblExchange)
        Me.grpMarket1.Controls.Add(Me.lblContract)
        Me.grpMarket1.Controls.Add(Me.cmdAdd)
        Me.grpMarket1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpMarket1.Location = New System.Drawing.Point(19, 123)
        Me.grpMarket1.Name = "grpMarket1"
        Me.grpMarket1.Size = New System.Drawing.Size(751, 814)
        Me.grpMarket1.TabIndex = 66
        Me.grpMarket1.TabStop = False
        Me.grpMarket1.Text = "Legs"
        '
        'lblQuantity
        '
        Me.lblQuantity.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblQuantity.Location = New System.Drawing.Point(16, 177)
        Me.lblQuantity.Name = "lblQuantity"
        Me.lblQuantity.Size = New System.Drawing.Size(125, 31)
        Me.lblQuantity.TabIndex = 20
        Me.lblQuantity.Text = "Quantity:"
        Me.lblQuantity.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblBuySell
        '
        Me.lblBuySell.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBuySell.Location = New System.Drawing.Point(16, 137)
        Me.lblBuySell.Name = "lblBuySell"
        Me.lblBuySell.Size = New System.Drawing.Size(125, 31)
        Me.lblBuySell.TabIndex = 19
        Me.lblBuySell.Text = "Buy/Sell:"
        Me.lblBuySell.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'cboQuantity
        '
        Me.cboQuantity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboQuantity.Location = New System.Drawing.Point(144, 177)
        Me.cboQuantity.Name = "cboQuantity"
        Me.cboQuantity.Size = New System.Drawing.Size(579, 28)
        Me.cboQuantity.TabIndex = 18
        Me.cboQuantity.TabStop = False
        '
        'cboBuySell
        '
        Me.cboBuySell.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboBuySell.Location = New System.Drawing.Point(144, 137)
        Me.cboBuySell.Name = "cboBuySell"
        Me.cboBuySell.Size = New System.Drawing.Size(579, 28)
        Me.cboBuySell.TabIndex = 17
        Me.cboBuySell.TabStop = False
        '
        'lblStatus
        '
        Me.lblStatus.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStatus.Location = New System.Drawing.Point(10, 558)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(124, 31)
        Me.lblStatus.TabIndex = 16
        Me.lblStatus.Text = "Status:"
        Me.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblLegs
        '
        Me.lblLegs.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLegs.Location = New System.Drawing.Point(16, 273)
        Me.lblLegs.Name = "lblLegs"
        Me.lblLegs.Size = New System.Drawing.Size(125, 31)
        Me.lblLegs.TabIndex = 15
        Me.lblLegs.Text = "Legs:"
        Me.lblLegs.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lstStatus
        '
        Me.lstStatus.FormattingEnabled = True
        Me.lstStatus.ItemHeight = 20
        Me.lstStatus.Location = New System.Drawing.Point(144, 558)
        Me.lstStatus.Name = "lstStatus"
        Me.lstStatus.Size = New System.Drawing.Size(579, 204)
        Me.lstStatus.TabIndex = 14
        '
        'cmdCreateUDS
        '
        Me.cmdCreateUDS.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdCreateUDS.Location = New System.Drawing.Point(552, 516)
        Me.cmdCreateUDS.Name = "cmdCreateUDS"
        Me.cmdCreateUDS.Size = New System.Drawing.Size(171, 29)
        Me.cmdCreateUDS.TabIndex = 13
        Me.cmdCreateUDS.TabStop = False
        Me.cmdCreateUDS.Text = "Create UDS"
        '
        'lstLegs
        '
        Me.lstLegs.FormattingEnabled = True
        Me.lstLegs.ItemHeight = 20
        Me.lstLegs.Location = New System.Drawing.Point(144, 273)
        Me.lstLegs.Name = "lstLegs"
        Me.lstLegs.Size = New System.Drawing.Size(579, 204)
        Me.lstLegs.TabIndex = 12
        '
        'frmMain
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(8, 19)
        Me.ClientSize = New System.Drawing.Size(846, 1031)
        Me.Controls.Add(Me.grpMarket1)
        Me.Controls.Add(Me.grpAccount)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmMain"
        Me.Text = "T4 Example UDS vb"
        Me.grpAccount.ResumeLayout(False)
        Me.grpMarket1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Member Variables "

    ' Reference to the main api host object.
    Private moHost As Host

    ' Reference to the current account.
    Private moAccount As Account

    ' Reference the desired market list in order to pickup events.
    Private moMarkets As MarketList

    ' Reference to the current market.
    Private moPickerMarket As Market

    ' A dictionary containing all pending requests.
    Private moPendingUDSRequests As New Concurrent.ConcurrentDictionary(Of String, CreateUDS)

    ' To enable uds related responses to be handled properly we need to have subscribed to at least one market of the contract.
    Private moAtLeastOneMarket As Market

#End Region

#Region " Initialization "

    ' Initialize the application.
    Private Sub Init()

        Trace.WriteLine("Init")

        ' Populate the available exchanges.
        ExchangeListComplete()

        ' Set the account list reference so that we can get 
        ' Account and order events.
        OnAccountListComplete()

        cboBuySell.Items.Add(BuySell.Buy)
        cboBuySell.Items.Add(BuySell.Sell)

        cboQuantity.Items.Add(1)
        cboQuantity.Items.Add(2)
        cboQuantity.Items.Add(3)
        cboQuantity.Items.Add(4)

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

                If oExchange.ContractType = ContractType.Option Then

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
                    'Trace.Writeline(String.Format("Contract: {0}, Description: {1}, ContractType: {2}", oContract.ContractID, oContract.Description, oContract.ContractType))
                    cboContracts.Items.Add(oContract)
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

            ' This will return outright futures only.
            oContract.GetMarkets(0, StrategyType.Any, Sub(e2)

                                                          ' Invoke the update.
                                                          ' This places process on GUI thread.
                                                          If Me.InvokeRequired Then
                                                              Me.BeginInvoke(New OnMarketListComplete(AddressOf OnMarketListComplete), e2)
                                                          Else
                                                              OnMarketListComplete(e2)
                                                          End If

                                                      End Sub)

        End If

    End Sub

    Private Sub OnMarketListComplete(e As MarketListEventArgs)

        ' Populate the list of markets available for the current contract.

        ' First clear the combo.
        cboMarkets.Items.Clear()

        ' Eliminate any previous references.
        moPickerMarket = Nothing

        If Not e.Markets Is Nothing Then

            Try

                If Not moAtLeastOneMarket Is Nothing Then
                    RemoveHandler moAtLeastOneMarket.MarketCheckSubscription, AddressOf OnAtLeastOneMarketCheckSubscription
                End If

                For Each oMarket As Market In e.Markets

                    ' To get the UDS related messaging enabled we need to have subscribed to at least one market within a contract.
                    ' This may seem a bit odd but that is how it is.

                    moAtLeastOneMarket = oMarket

                    AddHandler moAtLeastOneMarket.MarketCheckSubscription, AddressOf OnAtLeastOneMarketCheckSubscription
                    moAtLeastOneMarket.DepthSubscribe(DepthBuffer.Smart, DepthLevels.BestOnly)

                    Exit For

                Next

                ' Market list event unregistration.
                If Not moMarkets Is Nothing Then
                    RemoveHandler moMarkets.MarketDetails, AddressOf OnMarketDetails
                End If
                ' Update market list reference.
                moMarkets = e.Markets
                ' Market list event registration.
                AddHandler e.Markets.MarketDetails, AddressOf OnMarketDetails

                ' Create a sorted list of the markets.
                ' Remember to turn sorting off on the combo or it will do a text sort.
                For Each oMarket As Market In e.Markets.GetSortedList

                    ' Don't allow strategies for the legs.
                    If oMarket.StrategyType = StrategyType.None Then

                        cboMarkets.Items.Add(oMarket)

                    End If

                Next

            Catch ex As Exception

                ' Trace the error.
                Trace.WriteLine("Error " & ex.ToString)

            End Try

        End If

    End Sub

    Private Sub OnAtLeastOneMarketCheckSubscription(e As MarketCheckSubscriptionEventArgs)

        e.DepthBuffer = DepthBuffer.Smart
        e.DepthLevels = DepthLevels.BestOnly

    End Sub

    Private Sub cboMarkets_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cboMarkets.SelectedIndexChanged

        ' Store a reference to the current market.
        If Not cboMarkets.SelectedItem Is Nothing Then

            moPickerMarket = DirectCast(cboMarkets.SelectedItem, Market)

        End If

    End Sub


    ''' <summary>
    ''' Raised any time a change to a market's definition occurs or when a new market is added after a contract's market list has been requested.
    ''' </summary>
    ''' <param name="e"></param>
    Private Sub OnMarketDetails(e As MarketDetailsEventArgs)

        If Me.InvokeRequired Then
            ' Invoke the update.
            ' This places process on GUI thread.
            Me.BeginInvoke(New MarketList.MarketDetailsEventHandler(AddressOf OnMarketDetails), e)
        Else

            ' Attempt to locate markets from our pending requests.

            For Each oPendingUDS As CreateUDS In moPendingUDSRequests.Values

                Dim oMarket As Market = moHost.MarketData.GetMarketByRef(oPendingUDS.ExchangeID, oPendingUDS.MarketRef)


                If Not oMarket Is Nothing Then

                    ' We have found the newly created UDS market.
                    ' Remove from the pending list and notify.
                    Dim oRemoveUDS As CreateUDS = Nothing
                    moPendingUDSRequests.TryRemove(oPendingUDS.RequestID, oRemoveUDS)

                    ' Remove the event handler.
                    RemoveHandler oRemoveUDS.RequestComplete, AddressOf OnRequestComplete

                    lstStatus.Items.Add("OnMarketDetails: Located UDS market: " & oMarket.Description)
                    Trace.WriteLine("OnMarketDetails: Located UDS market: " & oMarket.Description)

                    'Else

                    '    lstStatus.Items.Add("OnMarketDetails: Could not locate UDS market: " & oPendingUDS.MarketRef)
                    '    Trace.WriteLine("OnMarketDetails: Could not locate UDS market: " & oPendingUDS.MarketRef)

                End If

            Next

        End If


    End Sub


#End Region

#Region " Account Data "

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

    Private Sub cboAccounts_SelectedValueChanged(sender As Object, e As EventArgs) Handles cboAccounts.SelectedValueChanged

        If Not cboAccounts.SelectedItem Is Nothing Then

            ' Reference the current account.
            moAccount = DirectCast(cboAccounts.SelectedItem, Account)

        End If

    End Sub

#End Region

#Region " Startup and shutdown code "

    ' Initialise the api when the application starts.
    Private Sub frmMain_Load(ByVal sender As System.Object,
        ByVal e As System.EventArgs) Handles MyBase.Load

        ' Create the api host object using the built in Login dialog.

        ' This needs to run in test or live.
        ' UDS is not supported in Sim.
        ' As a 3rd party developer please contact CTS if access to exchange test systems are required.
        moHost = Host.Login(APIServerType.Test, "T4Example", "3BBF425E-538F-47E2-A3CA-937C80482146")
        ' moHost = Host.Login(APIServerType.Simulator, "T4Example", "112A04B0-5AAF-42F4-994E-FA7CB959C60B")

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

        If Not moPendingUDSRequests Is Nothing Then
            For Each oCreateUDS As CreateUDS In moPendingUDSRequests.Values
                RemoveHandler oCreateUDS.RequestComplete, AddressOf OnRequestComplete
            Next
        End If

        ' Check to see that we have an api object.
        If Not moHost Is Nothing Then

            ' Dispose of the api.
            moHost.Dispose()
            moHost = Nothing

        End If

    End Sub

#End Region

#Region " Add Legs & Create UDS "

    Private Sub cmdAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdAdd.Click

        Dim oMarket As Market = TryCast(cboMarkets.SelectedItem, Market)
        Dim iQuantity As Integer = 0
        Dim eBuySell As BuySell = BuySell.Undefined

        If IsNumeric(cboBuySell.SelectedItem) Then
            eBuySell = CType(CInt(cboBuySell.SelectedItem), BuySell)
        End If

        If IsNumeric(cboQuantity.SelectedItem) Then
            iQuantity = CInt(cboQuantity.SelectedItem)
        End If

        If Not oMarket Is Nothing AndAlso Not eBuySell = BuySell.Undefined AndAlso Not iQuantity = 0 Then

            ' Add the desired leg to the list.
            lstLegs.Items.Add(New UDSLegItem(oMarket, eBuySell, iQuantity))

        End If

    End Sub

    Private Sub cmdCreateUDS_Click(sender As Object, e As EventArgs) Handles cmdCreateUDS.Click

          If Not (moHost.MasterUser.IsInRole("Charting") OrElse moHost.MasterUser.IsInRole("ANTS1") OrElse moHost.MasterUser.IsInRole("ANTS2") OrElse moHost.MasterUser.IsInRole("ANTS3")) Then

            Trace.WriteLine("Failed")
          End If
        Dim oCreateUDS As CreateUDS = moHost.MarketData.CreateUDS(StrategyType.Generic, moAccount)

        If Not oCreateUDS Is Nothing Then

            For Each oItem As UDSLegItem In lstLegs.Items

                ' Add the leg.
                oCreateUDS.AddLeg(New CreateUDS.Leg(oItem.Market, oItem.BuySell, oItem.Quantity))

            Next

            ' Add the createuds object to the pending list.
            moPendingUDSRequests(oCreateUDS.RequestID) = oCreateUDS

            ' Register it's events.
            AddHandler oCreateUDS.RequestComplete, AddressOf OnRequestComplete

            ' Send the request.
            oCreateUDS.Send()

        Else

            lstStatus.Items.Add("Error: Not able to create UDS.  Contact support and confirm user has the correct permissions/roles")
            Trace.WriteLine("frmMain::cmdCreateUDS_Click Error: Not able to create UDS.  Contact support and confirm user has the correct permissions/roles")

        End If


    End Sub


    Private Sub OnRequestComplete(poStrategy As CreateUDS)

        If Me.InvokeRequired Then

            ' We must be on the API thread and we can't interact with the GUI when on the API thread.
            Me.BeginInvoke(New CreateUDS.RequestCompleteEventHandler(AddressOf OnRequestComplete), New Object() {poStrategy})

        Else

            ' Remove/reference the request from the pending list.
            Dim oPendingUDS As CreateUDS = Nothing

            moPendingUDSRequests.TryGetValue(poStrategy.RequestID, oPendingUDS)

            If Not oPendingUDS Is Nothing Then

                ' Remove the event handler.
                RemoveHandler poStrategy.RequestComplete, AddressOf OnRequestComplete

                Select Case poStrategy.Status

                    Case UDSStatus.Success

                        ' Success and rfq would have been submitted so nothing else to do.
                        ' Market may or may not have been broadcast to us yet.

                        lstStatus.Items.Add("OnRequestComplete: Successfully Created UDS")
                        Trace.WriteLine("OnRequestComplete: Successfully Created UDS")

                        ' Attempt to reference the market.
                        ' This may be redundant but the exchange doesn't guarentee the order of messages.
                        ' The exchange's UDS system is separate from the exchange's quote servers that host the newly created UDS market.

                        If Not oPendingUDS Is Nothing Then

                            Dim oMarket As Market = moHost.MarketData.GetMarketByRef(poStrategy.ExchangeID, oPendingUDS.MarketRef)

                            If Not oMarket Is Nothing Then

                                moPendingUDSRequests.TryRemove(poStrategy.RequestID, oPendingUDS)

                                lstStatus.Items.Add("OnRequestComplete: Located UDS market: " & oMarket.Description)
                                Trace.WriteLine("OnRequestComplete: Located UDS market: " & oMarket.Description)

                            Else

                                lstStatus.Items.Add("OnRequestComplete: Could not locate UDS market: " & oPendingUDS.MarketRef)
                                Trace.WriteLine("OnRequestComplete: Could not locate UDS market: " & oPendingUDS.MarketRef)

                            End If

                        End If


                    Case UDSStatus.Failed

                        ' Request failed.
                        lstStatus.Items.Add("OnRequestComplete: UDS Creation Failed")
                        Trace.WriteLine("OnRequestComplete: UDS Creation Failed")

                    Case UDSStatus.AlreadyExists

                        lstStatus.Items.Add("OnRequestComplete: UDS Already Exists")
                        Trace.WriteLine("OnRequestComplete: UDS Already Exists")

                        ' Find the market - if we don't have it loaded this won't work.
                        Dim oMarket As Market = moHost.MarketData.GetMarketByRef(oPendingUDS.ExchangeID, oPendingUDS.MarketRef)

                        If Not oMarket Is Nothing Then

                            moPendingUDSRequests.TryRemove(poStrategy.RequestID, oPendingUDS)

                            lstStatus.Items.Add("OnRequestComplete: UDS market already exists: " & oMarket.Description)
                            Trace.WriteLine("OnRequestComplete: UDS market already exists: " & oMarket.Description)

                        Else

                            lstStatus.Items.Add("OnRequestComplete: UDS market should already exist but it could not be located: " & oPendingUDS.MarketRef)
                            Trace.WriteLine("OnRequestComplete: UDS market should already exist but it could not be located: " & oPendingUDS.MarketRef)

                        End If

                End Select

            End If

        End If

    End Sub




#End Region



End Class
