Imports T4
Imports T4.api 

''' <summary>
''' Describes a UDS leg.
''' </summary>
Public Class UDSLegItem
    
    Sub New(market As market,buysell As buysell,quantity As integer)

        Me.Market = market
        Me.BuySell = buysell
        Me.Quantity  = quantity 

    End Sub

    Public readonly Market As Market
    Public readonly BuySell As BuySell
    Public readonly Quantity As Integer

    Public Overrides Function ToString() As String

        If Not Market Is Nothing Then
            Return Market.Description & " Buy/Sell: " & BuySell.ToString & " Quantity: " & Quantity.ToString 
            Else 
            Return MyBase.ToString 
        End If

    End Function

End Class
