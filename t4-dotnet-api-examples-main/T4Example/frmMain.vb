' Import the T4 definitions namespace.
Imports T4

' Import the API namespace.
Imports T4.API

Public Class frmMain
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

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
    Friend WithEvents cmdBuy As System.Windows.Forms.Button
    Friend WithEvents cmdReviseOrder As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cmdSelectAccount As System.Windows.Forms.Button
    Friend WithEvents cmdSelectMarket As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents cmdSell As Button
    Friend WithEvents cmdCancelOrder As System.Windows.Forms.Button
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.cmdBuy = New System.Windows.Forms.Button()
        Me.cmdReviseOrder = New System.Windows.Forms.Button()
        Me.cmdCancelOrder = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.cmdSelectAccount = New System.Windows.Forms.Button()
        Me.cmdSelectMarket = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.cmdSell = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'cmdBuy
        '
        Me.cmdBuy.Location = New System.Drawing.Point(15, 214)
        Me.cmdBuy.Name = "cmdBuy"
        Me.cmdBuy.Size = New System.Drawing.Size(96, 23)
        Me.cmdBuy.TabIndex = 0
        Me.cmdBuy.Text = "Buy Order"
        '
        'cmdReviseOrder
        '
        Me.cmdReviseOrder.Location = New System.Drawing.Point(246, 214)
        Me.cmdReviseOrder.Name = "cmdReviseOrder"
        Me.cmdReviseOrder.Size = New System.Drawing.Size(96, 23)
        Me.cmdReviseOrder.TabIndex = 1
        Me.cmdReviseOrder.Text = "Revise Order"
        '
        'cmdCancelOrder
        '
        Me.cmdCancelOrder.Location = New System.Drawing.Point(348, 214)
        Me.cmdCancelOrder.Name = "cmdCancelOrder"
        Me.cmdCancelOrder.Size = New System.Drawing.Size(96, 23)
        Me.cmdCancelOrder.TabIndex = 2
        Me.cmdCancelOrder.Text = "Cancel Order"
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(447, 43)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "This example project shows the basics of logging into the API, getting market and" &
    " account data and submitting, revising and cancelling an order."
        '
        'cmdSelectAccount
        '
        Me.cmdSelectAccount.Location = New System.Drawing.Point(170, 82)
        Me.cmdSelectAccount.Name = "cmdSelectAccount"
        Me.cmdSelectAccount.Size = New System.Drawing.Size(96, 23)
        Me.cmdSelectAccount.TabIndex = 4
        Me.cmdSelectAccount.Text = "Select Account"
        '
        'cmdSelectMarket
        '
        Me.cmdSelectMarket.Location = New System.Drawing.Point(170, 118)
        Me.cmdSelectMarket.Name = "cmdSelectMarket"
        Me.cmdSelectMarket.Size = New System.Drawing.Size(96, 23)
        Me.cmdSelectMarket.TabIndex = 5
        Me.cmdSelectMarket.Text = "Select Market"
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(12, 52)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(447, 30)
        Me.Label2.TabIndex = 6
        Me.Label2.Text = "Output from your actions appears in Trace and the Studio debugger Output window."
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(12, 82)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(152, 23)
        Me.Label3.TabIndex = 7
        Me.Label3.Text = "First, select an account:"
        '
        'Label4
        '
        Me.Label4.Location = New System.Drawing.Point(12, 123)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(152, 23)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Now, select a market:"
        '
        'Label5
        '
        Me.Label5.Location = New System.Drawing.Point(12, 169)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(447, 42)
        Me.Label5.TabIndex = 9
        Me.Label5.Text = "Submit an order (details are hardcoded), then revise or cancel it if it doesn't f" &
    "ill. See the code behind the Submit Order button to change the order details"
        '
        'cmdSell
        '
        Me.cmdSell.Location = New System.Drawing.Point(117, 214)
        Me.cmdSell.Name = "cmdSell"
        Me.cmdSell.Size = New System.Drawing.Size(96, 23)
        Me.cmdSell.TabIndex = 10
        Me.cmdSell.Text = "Sell Order"
        '
        'frmMain
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(471, 264)
        Me.Controls.Add(Me.cmdSell)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.cmdSelectMarket)
        Me.Controls.Add(Me.cmdSelectAccount)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.cmdCancelOrder)
        Me.Controls.Add(Me.cmdReviseOrder)
        Me.Controls.Add(Me.cmdBuy)
        Me.Name = "frmMain"
        Me.Text = "T4Example"
        Me.ResumeLayout(False)

    End Sub

#End Region

    ''' <summary>
    ''' Reference to the main api host object.
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents moHost As Host

    ''' <summary>
    ''' Reference to a market.
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents moMarket As Market

    ''' <summary>
    ''' Reference to an account.
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents moAccount As Account

    ''' <summary>
    ''' Reference to a submitted order.
    ''' </summary>
    ''' <remarks></remarks>
    Private moOrder As Order

#Region " Startup and shutdown code "

    ''' <summary>
    ''' Initialise the api when the application starts.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ' Create the api host object using the built in Login dialog.
        moHost = Host.Login(APIServerType.Simulator, "T4Example", "112A04B0-5AAF-42F4-994E-FA7CB959C60B")

        ' Check for success.
        If moHost Is Nothing Then

            ' Host object not returned which means the user cancelled the login dialog.
            Me.Close()

        Else

            ' Login was successfull.
            Trace.WriteLine("Login Success")

        End If

    End Sub

    ''' <summary>
    ''' Shutdown the api when the application exits.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub frmMain_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Closed

        ' Check to see that we have an api object.
        If Not moHost Is Nothing Then

            ' Dispose of the api.
            moHost.Dispose()
            moHost = Nothing

        End If

    End Sub

#End Region

#Region " Login Result"

    ''' <summary>
    ''' Event raised if login fails or reconnects.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks>
    ''' When using the Host.Login method to login this will only be raised in the event of a disconnection from the server and subsequent reconnection.
    ''' </remarks>
    Private Sub moHost_LoginResponse(ByVal e As LoginResponseEventArgs) Handles moHost.LoginResponse

        Trace.WriteLine(String.Format("Login Response: {0} {1}", e.Result.ToString, e.Text))

        ' Nothing else needs to be done here when Host.Login is used to login - any market and account subscriptions active when disconnection occurred
        ' will automatically be restored on reconnection.

    End Sub

#End Region

#Region " Getting Market Data "

    ''' <summary>
    ''' Allow the user to select a market, then subscribe to it.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub cmdSelectMarket_Click(sender As System.Object, e As System.EventArgs) Handles cmdSelectMarket.Click

        ' Display the market picker to the user for them to select a market.
        moMarket = moHost.MarketData.MarketPicker(moMarket)

        ' Check to see if a market was selected.
        If Not moMarket Is Nothing Then

            ' Subscribe to the market.
            moMarket.DepthSubscribe(DepthBuffer.SmartTrade, DepthLevels.BestOnly)

        End If

    End Sub

    ''' <summary>
    ''' Make sure we stay subscribed to the market if we want to.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub moMarket_MarketCheckSubscription(e As MarketCheckSubscriptionEventArgs) Handles moMarket.MarketCheckSubscription

        e.DepthSubscribeAtLeast(DepthBuffer.SmartTrade, DepthLevels.BestOnly)

    End Sub

    ''' <summary>
    ''' Event raised when there is a new depth update for the market.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub moMarket_MarketDepthUpdate(e As MarketDepthUpdateEventArgs) Handles moMarket.MarketDepthUpdate

        Dim sBid As String = ""
        If e.Depth.Bids.Count > 0 Then
            sBid = String.Format("{0}@{1}", e.Depth.Bids(0).Volume, e.Depth.Bids(0).Price)
        End If
        Dim sOffer As String = ""
        If e.Depth.Offers.Count > 0 Then
            sOffer = String.Format("{0}@{1}", e.Depth.Offers(0).Volume, e.Depth.Offers(0).Price)
        End If
        Trace.WriteLine(String.Format("DepthUpdate: {0}, Time: {1}, Mode: {2}, Bid: {3}, Offer: {4}", e.Market.Description, e.Depth.Time, e.Depth.Mode, sBid, sOffer))

    End Sub

    ''' <summary>
    ''' Event raised when a trade occurs.
    ''' </summary>
    ''' <param name="e"></param>
    Private Sub moMarket_MarketTrade(e As MarketTradeEventArgs) Handles moMarket.MarketTrade

        Trace.WriteLine(String.Format("MarketTrade: {0}, Trade: {1}@{2}", e.Market.Description, e.Trade.Volume, e.Trade.Price))

    End Sub

    ''' <summary>
    ''' Event raised when the high/low changes.
    ''' </summary>
    ''' <param name="e"></param>
    Private Sub moMarket_MarketHighLow(e As MarketHighLowEventArgs) Handles moMarket.MarketHighLow

        Trace.WriteLine(String.Format("MarketHighLow: {0}, High: {1}, Low: {2}", e.Market.Description, e.HighLow.HighPrice, e.HighLow.LowPrice))

    End Sub

    ''' <summary>
    ''' Event raised when the settlement is updated.
    ''' </summary>
    ''' <param name="e"></param>
    Private Sub moMarket_MarketSettlement(e As MarketSettlementEventArgs) Handles moMarket.MarketSettlement

        Trace.WriteLine(String.Format("MarketSettlement: {0}, Price: {1}", e.Market.Description, e.Settlement.Price))

    End Sub

    ''' <summary>
    ''' Event raised when the price limits are updated. 
    ''' </summary>
    ''' <param name="e"></param>
    Private Sub moMarket_MarketPriceLimits(e As MarketPriceLimitsEventArgs) Handles moMarket.MarketPriceLimits

        Trace.WriteLine(String.Format("MarketPriceLimits: {0}, High: {1}, Low: {2}", e.Market.Description, e.PriceLimits.HighPrice, e.PriceLimits.LowPrice))

    End Sub

#End Region

#Region " Account Data "

    ''' <summary>
    ''' Allow the user to select an account.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub cmdSelectAccount_Click(sender As System.Object, e As System.EventArgs) Handles cmdSelectAccount.Click

        ' Display the account picker to the user and allow them to select an account.
        moAccount = moHost.Accounts.AccountPicker(moAccount)

        ' Check to see if the user selected anything.
        If Not moAccount Is Nothing Then

            ' Subscribe to the account.
            moAccount.Subscribe()

        End If

    End Sub

    ''' <summary>
    ''' Event that is raised when the accounts overall balance, P&amp;L or margin details have changed.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub moAccount_AccountUpdate(ByVal e As AccountUpdateEventArgs) Handles moAccount.AccountUpdate

        ' Display the account balance.
        Trace.WriteLine(String.Format("Account: {0}, Balance: {1}, PL: {2}", e.Account.AccountNumber, e.Account.Balance, e.Account.PL))

    End Sub

    ''' <summary>
    ''' Event that is raised when positions for the account have changed.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub moAccount_PositionUpdate(e As PositionUpdateEventArgs) Handles moAccount.PositionUpdate

        ' Display the account, market and net position.
        Trace.WriteLine(String.Format("Account: {0}, Market: {1}, Net: {2}, PL: {3}", e.Account.AccountNumber, e.Position.Market.Description, e.Position.Net, e.Position.PL))

    End Sub

#End Region

#Region " Order Handling"

    ''' <summary>
    ''' Submit a single order into the previously selected market and account.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub cmdBuy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdBuy.Click

        ' Check that we have an account and market to submit with.
        If Not moMarket Is Nothing AndAlso Not moAccount Is Nothing Then

            ' Use the settlement price of the market for the order limit price.
            Dim decPrice As Decimal? = moMarket.GetSettlement.Price

            ' Submit a buy limit order for 1 lot at the last settlement price.
            moOrder = moHost.SubmitOrder(moAccount, moMarket, BuySell.Buy, PriceType.Limit, 1, decPrice)

        End If

    End Sub

    ''' <summary>
    ''' Submit a single order into the previously selected market and account.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub cmdSell_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSell.Click

        ' Check that we have an account and market to submit with.
        If Not moMarket Is Nothing AndAlso Not moAccount Is Nothing Then

            ' Use the settlement price of the market for the order limit price.
            Dim decPrice As Decimal? = moMarket.GetSettlement.Price

            ' Submit a buy limit order for 1 lot at the last settlement price.
            moOrder = moHost.SubmitOrder(moAccount, moMarket, BuySell.Sell, PriceType.Limit, 1, decPrice)

        End If

    End Sub

    ''' <summary>
    ''' Revise the previously submitted order.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub cmdReviseOrder_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdReviseOrder.Click

        ' Check to see that we have an order.
        If Not moOrder Is Nothing Then

            ' Check to see if the order is working.
            If moOrder.IsWorking Then

                ' Revise the order to be a 2 lot with the same price.
                moOrder.Revise(2, moOrder.CurrentLimitPrice)

            End If

        End If

    End Sub

    ''' <summary>
    ''' Pull the single order that was submitted.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub cmdCancelOrder_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancelOrder.Click

        ' Check to see that we have an order.
        If Not moOrder Is Nothing Then

            ' Check to see if the order is working.
            If moOrder.IsWorking Then

                ' Pull the order.
                moOrder.Pull()

            End If

        End If

    End Sub

    ''' <summary>
    ''' Event raised when the order has been updated.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub moAccount_OrderUpdate(ByVal e As OrderUpdateEventArgs) Handles moAccount.OrderUpdate

        For Each oOrder As Order In e.Orders

            ' Only trace out details for our 1 current order.
            If oOrder Is moOrder Then

                ' Display some of the order details.
                Trace.WriteLine(String.Format("OrderUpdate: {0}, Status: {1}, {2}, Change: {3}", oOrder.UniqueID, oOrder.Status.ToString, oOrder.StatusDetail, oOrder.Change.ToString))

            End If

        Next

    End Sub

    ''' <summary>
    ''' Event raised when orders are removed, i.e. on day change.
    ''' </summary>
    ''' <param name="e"></param>
    Private Sub moAccount_OrderRemoved(e As OrderRemovedEventArgs) Handles moAccount.OrderRemoved

        For Each oOrder As Order In e.Orders

            ' Only trace out details for our 1 current order.
            If oOrder Is moOrder Then

                ' Display some of the order details.
                Trace.WriteLine(String.Format("OrderRemoved: {0}, Status: {1}, {2}, Change: {3}", oOrder.UniqueID, oOrder.Status.ToString, oOrder.StatusDetail, oOrder.Change.ToString))

            End If

        Next

    End Sub

    ''' <summary>
    ''' Event raised when the order has received a fill.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub moAccount_OrderTrade(ByVal e As OrderTradeEventArgs) Handles moAccount.OrderTrade

        ' Only trace out details for our 1 current order.
        If e.Order Is moOrder Then

            For Each oTrade As Trade In e.Trades

                ' Display some of the fill details.
                Trace.WriteLine(String.Format("OrderTrade: {0}, Fill: {1}@{2}, PossibleResend: {3}", e.Order.UniqueID, oTrade.Volume, e.Order.Market.PriceToDisplay(oTrade.Price), e.PossibleResend))

            Next

        End If

    End Sub

    ''' <summary>
    ''' Event raised when the order has received a leg fill.
    ''' </summary>
    ''' <param name="e"></param>
    Private Sub moAccount_OrderTradeLeg(e As OrderTradeLegEventArgs) Handles moAccount.OrderTradeLeg

        ' Only trace out details for our 1 current order.
        If e.Order Is moOrder Then

            For Each oTradeLeg As TradeLeg In e.TradeLegs

                ' Display some of the fill details.
                Trace.WriteLine(String.Format("OrderTradeLeg: {0}, Fill: {1}@{2}, PossibleResend: {3}", e.Order.UniqueID, oTradeLeg.Volume, e.Order.Market.PriceToDisplay(oTradeLeg.Price), e.PossibleResend))

            Next

        End If

    End Sub

#End Region

End Class
