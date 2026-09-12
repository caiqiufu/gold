' Import the service process namespace.
Imports System.ServiceProcess

' Import IO.
Imports System.IO

' Import the T4 definitions namespace.
Imports T4

' Import the API namespace.
Imports T4.API

' Import the text library for the string builder.
Imports System.Text

' The easiest way to see this service in action is to:
' 1) Install and run the service.  See the included command files for help on this.
' 2) Login to the T4 Sim frontend as the T4Example user to place some trades that the service will pick up.
' Our Sim frontend can be downloaded and installed upon self registration.  Just visit www.ctsfutures.com and 
' click on the Register link for a specific firm or click the Register tab to register within the generic CTS Sim firm.

Public Class T4APIService
    Inherits System.ServiceProcess.ServiceBase

#Region " Component Designer generated code "

    ''' <summary>Constructor</summary>
    Public Sub New()
        MyBase.New()

        ' This call is required by the Component Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call

    End Sub

    ''' <summary>UserService overrides dispose to clean up the component list.</summary>
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then

            ' Check to see that we have an api object.
            If Not moHost Is Nothing Then

                ' Dispose of the api.
                moHost.Dispose()
                moHost = Nothing

            End If

            ' Check to see that we have a streamwriter class to close.
            If Not moStreamWriter Is Nothing Then

                ' Close the stream writer.
                moStreamWriter.Close()

            End If

            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    ' The main entry point for the process
    <MTAThread()>
    Shared Sub Main()
        Dim ServicesToRun() As System.ServiceProcess.ServiceBase

        ' More than one NT Service may run within the same process. To add
        ' another service to this process, change the following line to
        ' create a second service object. For example,
        '
        '   ServicesToRun = New System.ServiceProcess.ServiceBase () {New Service1, New MySecondUserService}


        If Debugger.IsAttached Then

            Dim o As New T4APIService
            o.OnStart(Nothing)

            ' Display a dialog form to keep the application alive.
            Dim ofrmDebug As New frmDebug
            ofrmDebug.ShowDialog()

            o.OnStop()

        Else
            ServicesToRun = New System.ServiceProcess.ServiceBase() {New T4APIService}
            System.ServiceProcess.ServiceBase.Run(ServicesToRun)
        End If

    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    ' NOTE: The following procedure is required by the Component Designer
    ' It can be modified using the Component Designer.  
    ' Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        components = New System.ComponentModel.Container
        Me.ServiceName = "T4APIService"
    End Sub

#End Region

#Region " Member Variables "

    ' Reference to the main api host object.
    Private WithEvents moHost As Host

    ' Reference to the accounts list.
    Private WithEvents moAccounts As AccountList

    ' Reference to Order arraylist.
    ' Stores the collection of orders.
    Private moOrderArrayList As New ArrayList

    ' Create an instance of StreamWriter to write text to a file.
    Dim moStreamWriter As StreamWriter = New StreamWriter(APP_Path() & "\Output.csv")

#End Region

#Region " Methods "

    ''' <summary>Return the current run directory.</summary>
    Private Function APP_Path() As String

        Return System.AppDomain.CurrentDomain.BaseDirectory

    End Function

#End Region

#Region " Startup and shutdown code "

    ''' <summary>Add code here to start your service. This method should set things in motion so your service can do its work.</summary>
    Protected Overrides Sub OnStart(ByVal args() As String)

        Try

            ' Create the api host object.
            ' TODO: Set you firm, username and password in the code in T4APIService.VB OnStart method
            Trace.WriteLine("Set you firm, username and password in the code in T4APIService.VB OnStart method")
            moHost = New Host(APIServerType.Simulator,
                             "T4Example",
                            "112A04B0-5AAF-42F4-994E-FA7CB959C60B",
                            "<Your firm>",
                            "<Your user>",
                            "<Your password>")

        Catch ex As Exception

            ' Trace Errors.
            Trace.WriteLine("T4APIService::OnStart Error: " & ex.ToString)

        End Try


    End Sub

    ''' <summary>Add code here to perform any tear-down necessary to stop your service.</summary>
    Protected Overrides Sub OnStop()

        Try

            ' Check to see that we have an api object.
            If Not moHost Is Nothing Then

                ' Dispose of the api.
                moHost.Dispose()
                moHost = Nothing

            End If

            ' Close the stream writer.
            moStreamWriter.Close()

            ' Delete the output file.
            File.Delete(APP_Path() & "\Output.csv")

        Catch ex As Exception

            ' Trace Errors.
            Trace.WriteLine("T4APIService::OnStop Error: " & ex.ToString)

        End Try

    End Sub

#End Region

#Region " Login Result"

    ''' <summary> Event raised on change in login status </summary>
    Private Sub moHost_LoginResponse(e As LoginResponseEventArgs) Handles moHost.LoginResponse

        If e.Result = LoginResult.Success Then

            Trace.WriteLine("Login Success")

            ' Turn on autoflush to clear the buffer.
            moStreamWriter.AutoFlush = True

            Dim oSB As New StringBuilder

            oSB.Append("Fill,")
            oSB.Append("AccountNumber,")
            oSB.Append("ExchangeID,")
            oSB.Append("ContractID,")
            oSB.Append("MarketDescription,")
            oSB.Append("MarketID,")
            oSB.Append("ExpiryDate,")
            oSB.Append("MarketDetails,")
            oSB.Append("StrategyType,")
            oSB.Append("ContractType,")
            oSB.Append("BuySell.,")
            oSB.Append("ExchangeTime,")
            oSB.Append("Time,")
            oSB.Append("TradeDate,")
            oSB.Append("ExchangeTradeID,")
            oSB.Append("Volume,")
            oSB.Append("ResidualVolume,")
            oSB.Append("FillPrice,")
            oSB.Append("TradeID")

            Trace.WriteLine(oSB.ToString)

            ' Write the fill to the file.
            ' Obviously there are many more fields available...
            moStreamWriter.WriteLine(oSB.ToString)

            ' Set the account list reference so that we can get 
            ' Account and order events.
            moAccounts = moHost.Accounts
            SubscribeToAccounts()

        ElseIf e.Result = LoginResult.TwoFactorAuthenticationRequired Then

            Trace.WriteLine("Service accounts should not enable two-factor authentication. Contact support for other options.")

        Else

            Trace.WriteLine("Login Failed due to " & e.Result.ToString)

        End If

    End Sub

#End Region

#Region " Account Subscription "

    ''' <summary>Method called following login success in order to subscribe to account data.</summary>
    Private Sub SubscribeToAccounts()

        Try

            Trace.WriteLine("Subscribing to " & moAccounts.Count & " accounts")

            ' Display the account list.
            For Each oAccount As Account In moHost.Accounts
                'Trace.WriteLine(String.Format("Account: {0}, Description: {1}", oAccount.AccountNumber, oAccount.Description)
                Trace.WriteLine("Subscribing to account " & oAccount.Description)

                ' Subscribe to the account.
                ' No need to subscribe to P&L.
                oAccount.Subscribe(False, Sub(e)
                                              OnAccountComplete(e)
                                          End Sub)

            Next
        Catch ex As Exception

            ' Trace Errors.
            Trace.WriteLine("T4APIService::SubscribeToAccounts Error: " & ex.ToString)

        End Try

    End Sub

#End Region

#Region " Trade Data "

    ''' <summary>Event raised when a fill occurs.</summary>
    Private Sub OnOrderTrade(e As OrderTradeEventArgs)

        ' No invoking necessary as there is no gui thread.

        ' Process the fill.
        For Each oTrade As Trade In e.Trades
            ProcessFill(e.Order, oTrade)
        Next

    End Sub

    ''' <summary>Event raised when a fill leg has been reported.</summary>
    Private Sub OnOrderTradeLeg(e As OrderTradeLegEventArgs)

        ' No invoking necessary as there is no gui thread.

        ' Fill legs should be treated as suplimental data.
        ' Not all spread markets are guarenteed to support leg fills.
        ' This is a limitation of some markets on some exchanges.

        ' Process the fill leg.
        For Each oTradeLeg As TradeLeg In e.TradeLegs
            ProcessFillLeg(e.Order, oTradeLeg)
        Next

    End Sub

    ''' <summary>Event raised when a given account has finished loading all of it's data.</summary>
    Private Sub OnAccountComplete(ByVal e As AccountCompleteEventArgs)

        Trace.WriteLine("Subscribed to " & e.Account.AccountNumber)

        Try

            ' Itterate through each order in the account.
            For Each oOrder As Order In e.Account.Orders

                'Trace.WriteLine(String.Format("Order: {0}, Market: {1}, SubmitTime: {2}, Status: {3}, {4}, CurrentLimitPrice: {5}, CurrentVolume: {6}, TotalFillVolume: {7}", oOrder.UniqueID, oOrder.Market.Description, oOrder.SubmitTime, oOrder.Status, oOrder.StatusDetail, oOrder.CurrentLimitPrice, oOrder.CurrentVolume, oOrder.TotalFillVolume))

                ' Don't do anything for an order that dosn't have at least a partial fill.
                If oOrder.TotalFillVolume > 0 Then

                    ' Process all the fill legs that exist for this order.
                    For Each oTrade As Trade In oOrder.Trades
                        ProcessFill(oOrder, oTrade)
                    Next

                    ' Process all the fill legs that exist for this order.
                    For Each oTradeLeg As TradeLeg In oOrder.TradeLegs
                        ProcessFillLeg(oOrder, oTradeLeg)
                    Next

                End If

            Next

        Catch ex As Exception

            Trace.WriteLine("T4APIService::OnAccountComplete Error: " & ex.Message)

        End Try

    End Sub

    ''' <summary>Process the fill message.</summary>
    Private Sub ProcessFill(ByVal poOrder As T4.API.Order, ByVal poTrade As T4.API.Trade)

        ' Do something with the fill.

        ' At this time T4 dosn't pass a unique id for fills.  
        ' This is being looked into.
        ' Please contact CTS Support regarding progress with this functionality only if required.

        ' Reference the current exchange.
        Dim oExchange As Exchange = moHost.MarketData.Exchanges(poOrder.ExchangeID)

        If Not oExchange Is Nothing Then

            ' Now we want to filter some data out.
            ' 1) Pit trades - manually entered fills for book keeping purposes.
            ' 2) GTC partial fills from previous days as we will have already processed them.
            ' 3) Overnight positions that have been rolled over.

            If Not poOrder.PriceType = PriceType.Pit _
                AndAlso Not poOrder.PriceType = PriceType.OvernightPosition _
                AndAlso Not (poOrder.TimeType = TimeType.GoodTillCancelled AndAlso Not poTrade.TradeDate.Equals(poOrder.Market.GetTradeDate(Now))) Then

                Dim oSB As New StringBuilder

                oSB.Append("Fill,")
                oSB.Append(poOrder.Account.AccountNumber)
                oSB.Append(",")
                oSB.Append(oExchange.ExchangeID)
                oSB.Append(",")
                oSB.Append(poOrder.Market.ContractID)
                oSB.Append(",")
                oSB.Append(poOrder.Market.Description)
                oSB.Append(",")
                oSB.Append(poOrder.Market.MarketID)
                oSB.Append(",")
                oSB.Append(poOrder.Market.GetExpiryDate.ToString("MMM yy"))
                oSB.Append(",")
                oSB.Append(poOrder.Market.Details)
                oSB.Append(",")
                oSB.Append(poOrder.Market.StrategyType.ToString)
                oSB.Append(",")
                oSB.Append(poOrder.Market.ContractType.ToString)
                oSB.Append(",")
                oSB.Append(poOrder.BuySell.ToString)
                oSB.Append(",")
                oSB.Append(poTrade.ExchangeTime.ToString("HH:mm:ss"))
                oSB.Append(",")
                oSB.Append(poTrade.Time.ToString("HH:mm:ss"))
                oSB.Append(",")
                oSB.Append(poTrade.TradeDate.ToString("dd MMM yyyy"))
                oSB.Append(",")
                oSB.Append(poTrade.ExchangeTradeID)
                oSB.Append(",")
                oSB.Append(poTrade.Volume)
                oSB.Append(",")
                oSB.Append(poTrade.ResidualVolume)
                oSB.Append(",")
                oSB.Append(poOrder.Market.PriceToDisplay(poTrade.Price))
                oSB.Append(",")
                oSB.Append(poTrade.TradeID)

                Trace.WriteLine(oSB.ToString)

                ' Write the fill to the file.
                ' Obviously there are many more fields available...
                moStreamWriter.WriteLine(oSB.ToString)


            End If

        End If



    End Sub

    ''' <summary>Process the fill leg message.</summary>
    Private Sub ProcessFillLeg(ByVal poOrder As T4.API.Order, ByVal poTradeLeg As T4.API.TradeLeg)

        ' Do something with the fill leg.

        ' Remember, not all spread/strategy markets support trade legs.
        ' This is supplimental data only.

        ' Now we want to filter some data out.
        ' 1) Pit trades - manually entered fills for book keeping purposes.
        ' 2) GTC partial fills from previous days as we will have already processed them.
        ' 3) Overnight positions that have been rolled over.
        If Not poOrder.PriceType = PriceType.Pit _
        AndAlso Not poOrder.PriceType = PriceType.OvernightPosition _
        AndAlso Not (poOrder.TimeType = TimeType.GoodTillCancelled AndAlso Not poTradeLeg.TradeDate.Equals(poOrder.Market.GetTradeDate(Now))) Then

            '' Write the fill leg to the file.
            '' Obviously there are many more fields available...

            Dim oSB As New StringBuilder

            oSB.Append("FillLeg,")
            oSB.Append(poOrder.Account.AccountNumber)
            oSB.Append(",")
            oSB.Append(poTradeLeg.Leg.Market.ExchangeID)
            oSB.Append(",")
            oSB.Append(poOrder.Market.ContractID)
            oSB.Append(",")
            oSB.Append(poOrder.Market.Description)
            oSB.Append(",")
            oSB.Append(poTradeLeg.Leg.Market.MarketID)
            oSB.Append(",")
            oSB.Append(poOrder.Market.GetExpiryDate.ToString("MMM yy"))
            oSB.Append(",")
            oSB.Append(poOrder.Market.Details)
            oSB.Append(",")
            oSB.Append(poOrder.Market.StrategyType.ToString)
            oSB.Append(",")
            oSB.Append(poOrder.Market.ContractType.ToString)
            oSB.Append(",")
            oSB.Append(poTradeLeg.BuySell.ToString)
            oSB.Append(",")
            oSB.Append(poTradeLeg.Time.ToString("HH:mm:ss"))
            oSB.Append(",")
            oSB.Append(poTradeLeg.ExchangeTime.ToString("HH:mm:ss"))
            oSB.Append(",")
            oSB.Append(poTradeLeg.TradeDate.ToString("dd MMM yyyy"))
            oSB.Append(",")
            oSB.Append(poTradeLeg.ExchangeTradeID)
            oSB.Append(",")
            oSB.Append(poTradeLeg.Volume)
            oSB.Append(",")
            oSB.Append(poTradeLeg.ResidualVolume)
            oSB.Append(",")
            oSB.Append(poTradeLeg.Leg.Market.PriceToDisplay(poTradeLeg.Price))
            oSB.Append(",")
            oSB.Append(poTradeLeg.TradeID)

            Trace.WriteLine(oSB.ToString)

            ' Write the fill to the file.
            ' Obviously there are many more fields available...
            moStreamWriter.WriteLine(oSB.ToString)

        End If

    End Sub

#End Region


End Class
