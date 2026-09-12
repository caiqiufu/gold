<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmUser
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lstExchanges = New System.Windows.Forms.ListBox()
        Me.lstAccounts = New System.Windows.Forms.ListBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lblLoginResult = New System.Windows.Forms.Label()
        Me.lstRoles = New System.Windows.Forms.ListBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.cmdSubmitOrder = New System.Windows.Forms.Button()
        Me.cmdLogoff = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(20, 199)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(52, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Accounts"
        '
        'lstExchanges
        '
        Me.lstExchanges.FormattingEnabled = True
        Me.lstExchanges.Location = New System.Drawing.Point(23, 362)
        Me.lstExchanges.Name = "lstExchanges"
        Me.lstExchanges.Size = New System.Drawing.Size(358, 95)
        Me.lstExchanges.TabIndex = 1
        '
        'lstAccounts
        '
        Me.lstAccounts.FormattingEnabled = True
        Me.lstAccounts.Location = New System.Drawing.Point(23, 227)
        Me.lstAccounts.Name = "lstAccounts"
        Me.lstAccounts.Size = New System.Drawing.Size(358, 95)
        Me.lstAccounts.TabIndex = 0
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(20, 337)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(60, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Exchanges"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(20, 19)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(69, 13)
        Me.Label3.TabIndex = 4
        Me.Label3.Text = "Login Result:"
        '
        'lblLoginResult
        '
        Me.lblLoginResult.AutoSize = True
        Me.lblLoginResult.Location = New System.Drawing.Point(106, 19)
        Me.lblLoginResult.Name = "lblLoginResult"
        Me.lblLoginResult.Size = New System.Drawing.Size(0, 13)
        Me.lblLoginResult.TabIndex = 5
        '
        'lstRoles
        '
        Me.lstRoles.FormattingEnabled = True
        Me.lstRoles.Location = New System.Drawing.Point(23, 86)
        Me.lstRoles.Name = "lstRoles"
        Me.lstRoles.Size = New System.Drawing.Size(358, 95)
        Me.lstRoles.TabIndex = 7
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(20, 58)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(34, 13)
        Me.Label4.TabIndex = 6
        Me.Label4.Text = "Roles"
        '
        'Label5
        '
        Me.Label5.Location = New System.Drawing.Point(20, 477)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(361, 42)
        Me.Label5.TabIndex = 11
        Me.Label5.Text = "Submit an order for this user (details are hardcoded). See the code behind the Su" & _
    "bmit Order button to change the order details"
        '
        'cmdSubmitOrder
        '
        Me.cmdSubmitOrder.Location = New System.Drawing.Point(23, 522)
        Me.cmdSubmitOrder.Name = "cmdSubmitOrder"
        Me.cmdSubmitOrder.Size = New System.Drawing.Size(96, 23)
        Me.cmdSubmitOrder.TabIndex = 10
        Me.cmdSubmitOrder.Text = "Submit Order"
        '
        'cmdLogoff
        '
        Me.cmdLogoff.Location = New System.Drawing.Point(285, 14)
        Me.cmdLogoff.Name = "cmdLogoff"
        Me.cmdLogoff.Size = New System.Drawing.Size(96, 23)
        Me.cmdLogoff.TabIndex = 12
        Me.cmdLogoff.Text = "Logoff"
        '
        'frmUser
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(403, 613)
        Me.Controls.Add(Me.cmdLogoff)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.cmdSubmitOrder)
        Me.Controls.Add(Me.lstRoles)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.lblLoginResult)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.lstAccounts)
        Me.Controls.Add(Me.lstExchanges)
        Me.Controls.Add(Me.Label1)
        Me.Name = "frmUser"
        Me.Text = "User"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lstExchanges As System.Windows.Forms.ListBox
    Friend WithEvents lstAccounts As System.Windows.Forms.ListBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents lblLoginResult As System.Windows.Forms.Label
    Friend WithEvents lstRoles As System.Windows.Forms.ListBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents cmdSubmitOrder As System.Windows.Forms.Button
    Friend WithEvents cmdLogoff As System.Windows.Forms.Button
End Class
