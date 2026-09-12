Imports T4
Imports T4.API

''' <summary>
''' Form that handles a single user instance.
''' </summary>
''' <remarks></remarks>
Public Class frmUser

    ''' <summary>
    ''' Reference to the parent window.
    ''' </summary>
    ''' <remarks></remarks>
    Private moParent As frmMain

    ''' <summary>
    ''' Reference to the specific user for this form.
    ''' </summary>
    ''' <remarks></remarks>
    Private moUser As User

    ''' <summary>
    ''' Display the form for the master user.
    ''' </summary>
    ''' <param name="poParent"></param>
    ''' <remarks></remarks>
    Public Sub DisplayMasterUser(poParent As frmMain)

        moParent = poParent

        ' Get the master user reference.
        moUser = moParent.Host.MasterUser

        ' Setup the form.
        Init(Nothing)

        ' Show the form.
        Me.Show(poParent)

    End Sub

    ''' <summary>
    ''' Login the additional user.
    ''' </summary>
    ''' <param name="poParent"></param>
    ''' <param name="psUsername"></param>
    ''' <param name="psPassword"></param>
    ''' <remarks></remarks>
    Public Sub LoginUser(poParent As frmMain, psUsername As String, psPassword As String)

        moParent = poParent

        ' Login the user.
        moParent.Host.Users.LoginUser(psUsername, psPassword, Sub(e)
                                                                  ' Update the form.
                                                                  Me.BeginInvoke(New OnLoginResponse(AddressOf Init), e)
                                                              End Sub)

        ' Show the form.
        Me.Show(poParent)

    End Sub

    ''' <summary>
    ''' Initialise the display of the form.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Init(e As LoginResponseEventArgs)

        If e IsNot Nothing Then
            moUser = e.User
        End If

        ' Display the username and login state.
        If moUser.IsMaster Then
            Me.Text = String.Format("{0} MASTER User", moUser.Username)
        Else
            Me.Text = String.Format("{0} User", moUser.Username)
        End If
        lblLoginResult.Text = moUser.LoginResult.ToString

        ' Display the list of roles this user has set.
        lstRoles.Items.Clear()
        For Each sRole As String In moUser.Roles
            If Not String.IsNullOrWhiteSpace(sRole) Then
                lstRoles.Items.Add(sRole)
            End If
        Next

        ' Display the list of accounts this user has permission for.
        DisplayAccounts()

        ' Display the list of exchanges this user can see.
        DisplayExchanges()

        ' Subscribe to all the accounts.
        For Each oAccount As Account In moUser.Accounts
            AddHandler oAccount.AccountDetails, AddressOf OnAccountDetails
            AddHandler oAccount.AccountUpdate, AddressOf OnAccountUpdate
            oAccount.Subscribe(Sub()
                                   Me.BeginInvoke(New MethodInvoker(AddressOf DisplayAccounts))
                               End Sub)
        Next

    End Sub

    ''' <summary>
    ''' Display the account list.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub DisplayAccounts()

        lstAccounts.Items.Clear()
        For Each oAccount As Account In moUser.Accounts.GetSortedList
            lstAccounts.Items.Add(String.Format("{0}, {1}, PL: {2}", oAccount.AccountNumber, oAccount.Status.ToString, oAccount.PL))
        Next

    End Sub

    ''' <summary>
    ''' Display the list of exchanges.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub DisplayExchanges()

        lstExchanges.Items.Clear()
        For Each oUE As UserExchange In moUser.Exchanges.GetSortedList
            lstExchanges.Items.Add(String.Format("{0}, MarketDataType: {1}", oUE.ExchangeID, oUE.MarketDataType))
        Next

    End Sub

    ''' <summary>
    ''' Submit a single order into the previously selected market.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub cmdSubmitOrder_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSubmitOrder.Click

        ' Check that we have a market selected on the main form.
        If Not moParent.Market Is Nothing Then

            ' Get the first account that this user can see, if any.
            If moUser.Accounts.Count > 0 Then

                ' Get the account.
                Dim oAccount As Account = moUser.Accounts.First

                ' Use the settlement price of the market for the order limit price.
                Dim decPrice As Decimal? = moParent.Market.GetSettlement.Price

                ' Create the order submission object.
                Dim oBatch As OrderSubmissionBatch = moUser.Host.GetOrderSubmission()

                ' Add a buy limit order for 1 lot at the last settlement price, ensuring that we set the User so that the order comes from us and not the master user.
                oBatch.Add(oAccount, moParent.Market, BuySell.Buy, PriceType.Limit, TimeType.Normal, 1, decPrice, Nothing, "", Nothing, ActivationType.Immediate, Nothing, 0, 0, moUser, True)

                ' Submit the order.
                oBatch.Send()

            End If

        End If

    End Sub

    ''' <summary>
    ''' Update our display of accounts.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub OnAccountDetails(e As AccountDetailsEventArgs)

        ' Redisplaying entire list is not efficient but simple for this example.
        Me.BeginInvoke(New MethodInvoker(AddressOf DisplayAccounts))

    End Sub

    ''' <summary>
    ''' Update our display of accounts.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub OnAccountUpdate(e As AccountUpdateEventArgs)

        ' Redisplaying entire list is not efficient but simple for this example.
        Me.BeginInvoke(New MethodInvoker(AddressOf DisplayAccounts))

    End Sub

    ''' <summary>
    ''' Logoff this user.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub cmdLogoff_Click(sender As System.Object, e As System.EventArgs) Handles cmdLogoff.Click

        moUser.Logoff()

    End Sub

    ''' <summary>
    ''' Close the form.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub frmUser_FormClosed(sender As Object, e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed

        If moUser IsNot Nothing Then

            ' Release the event handlers.
            For Each oAccount As Account In moUser.Accounts
                RemoveHandler oAccount.AccountDetails, AddressOf OnAccountDetails
                RemoveHandler oAccount.AccountUpdate, AddressOf OnAccountUpdate
            Next
            moUser = Nothing

        End If

    End Sub

End Class