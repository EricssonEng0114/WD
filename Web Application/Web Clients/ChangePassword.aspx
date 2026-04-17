<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="UBPCWeb.ChangePassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    
<div id="wrapper">
    <form runat="server" class="login-form">

        <div class="header">
            <h1>Change Password</h1>
            <span>Please enter new password following.</span>
        </div>
        <div class="contenta">
            <table>
                <tr>
                    <td width="45%">
                        <asp:Label ID="Label2" runat="server" Text="New Password" Font-Size="Small"></asp:Label>

                    </td>
                    <td width="60%">                        
                        <asp:TextBox ID="txtNewPassword" runat="server" CssClass="input username" Text=""  placeholder="New Password" TextMode="Password" MaxLength="100" Width="210px"></asp:TextBox>
                        <div class="pass-icon"></div>
                    </td>
                    <td> 
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtNewPassword"  
                        ErrorMessage="Required." runat="server" Font-Bold="True" Font-Names="Calibri" ForeColor="#CC0000" Font-Size="Medium" />  

                    </td>
                </tr>
                <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp</td></tr>
                 <tr>
                    <td width="45%">
                        <asp:Label ID="Label1" runat="server" Text="Confirm New Password" Font-Size="Small"></asp:Label>
<%--                        Confirm New Password&nbsp--%>

                    </td>
                    <td width="60%">                        
                        <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="input username" Text=""  placeholder="New Password" TextMode="Password" MaxLength="100" Width="210px"></asp:TextBox>
                        <div class="pass-icon"></div>
                    </td>
                    <td> 
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ControlToValidate="txtNewPassword"  
                        ErrorMessage="Required." runat="server" Font-Bold="True" Font-Names="Calibri" ForeColor="#CC0000" Font-Size="Medium" />  

                    </td>
                </tr>
            </table>
        </div>
        <div class="footer">
            <table width="100%">
                <tr>
                    <td>
                        <asp:Label ID="lblErrMessage" runat="server" Text="Label" Visible="False" Font-Bold="True" Font-Names="Calibri" ForeColor="#CC0000" Font-Size="Small"></asp:Label>
                         <a href="/" style="color: blue;font-size:small;font-family:calibri"><u>Click to Back to Login Page</u></a>

                    </td>
                    <td>
                        <asp:Button ID="btnChange" runat="server" Text="Change" CssClass="button" OnClick="btnChange_Click" />
                    </td>

                </tr>
            </table>
        </div>

    </form>
</div>

<div class="gradient"></div>
</asp:Content>
