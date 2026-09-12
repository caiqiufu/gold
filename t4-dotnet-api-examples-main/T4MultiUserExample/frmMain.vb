Imports T4
Imports T4.API

''' <summary>
''' Main form for logging in additional users.
''' </summary>
''' <remarks></remarks>
Public Class frmMain

#Region " Properties "

    ''' <summary>
    ''' Reference to the api host.
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents moHost As Host

    ''' <summary>
    ''' Allow access to the host from other forms.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Host As Host
        Get
            Return moHost
        End Get
    End Property

    ''' <summary>
    ''' Reference to a market.
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents moMarket As Market

    ''' <summary>
    ''' Allow access to the market from other forms.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Market As Market
        Get
            Return moMarket
        End Get
    End Property

#End Region

#Region " Load / Close "

    ''' <summary>
    ''' Login the master api user.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub frmMain_Load(sender As Object, e As System.EventArgs) Handles Me.Load

        ' Create the host and login the master user.
        moHost = Host.Login(APIServerType.Simulator, "T4Example", "112A04B0-5AAF-42F4-994E-FA7CB959C60B")
        ' For live:
        'moHost = Host.Login( APIServerType.Live, "T4Example", "112A04B0-5AAF-42F4-994E-FA7CB959C60B")

        ' Check for success.
        If moHost Is Nothing Then

            ' Host object not returned which means the user cancelled the login dialog.
            Me.Close()

        Else

            ' Login was successfull, create the master user form.
            Dim oFrm As New frmUser
            oFrm.DisplayMasterUser(Me)

            ' Enable the login button for additional users.
            cmdLoginUser.Enabled = True

        End If

    End Sub

    ''' <summary>
    ''' Shut the api down.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub frmMain_FormClosed(sender As Object, e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed

        ' Release the api.
        If Not moHost Is Nothing Then
            moHost.Dispose()
            moHost = Nothing
        End If

    End Sub

#End Region

    ''' <summary>
    ''' Login the additional user and display the user specific window.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub cmdLoginUser_Click(sender As System.Object, e As System.EventArgs) Handles cmdLoginUser.Click

        Dim oFrm As New frmUser
        oFrm.LoginUser(Me, txtUsername.Text, txtPassword.Text)

    End Sub

#Region " Select Market "

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
            moMarket.DepthSubscribe()

        End If

    End Sub

    ''' <summary>
    ''' Make sure we stay subscribed to the market if we want to.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub moMarket_MarketCheckSubscription(e As MarketCheckSubscriptionEventArgs) Handles moMarket.MarketCheckSubscription

        e.DepthSubscribeAtLeast(DepthBuffer.Smart, DepthLevels.BestOnly)

    End Sub

#End Region

End Class
