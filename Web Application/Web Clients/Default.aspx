<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    
<div id="wrapper">
    <form runat="server" class="login-form">

        <div class="header">
            <h1>Login</h1>
            <span>Fill out the form below to login.</span>
        </div>
        <div class="contenta">
            <table>
                <tr>
                    <td width="100">User ID</td>
                    <td>                        
                        <asp:TextBox ID="txtUserID" runat="server" CssClass="input username" Text=""  placeholder="User Name" MaxLength="8"></asp:TextBox>
                        <div class="user-icon"></div>
                    </td>
                    <td> 
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtUserID"  
                        ErrorMessage="Required." runat="server" Font-Bold="True" Font-Names="Calibri" ForeColor="#CC0000" Font-Size="Medium" />  

                    </td>
                </tr>
                <tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp</td></tr>
                <tr>
                    <td>Password</td>
                    <td>
                         <asp:TextBox ID="txtpassword" runat="server" CssClass="input username" Text="" placeholder="Password" TextMode="Password" MaxLength="100"></asp:TextBox>
                        <div class="pass-icon"></div>
                    </td>
                    <td>
                         <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ControlToValidate="txtpassword"  
                        ErrorMessage="Required." runat="server" Font-Bold="True" Font-Names="Calibri" ForeColor="#CC0000" Font-Size="Medium" /> 
                    </td>
                </tr>
            </table>
        </div>
        <div class="footer">
            <table width="100%">
                <tr>
                    <td>
                        <asp:Label ID="lblErrMessage" runat="server" Text="Label" Visible="false" Font-Bold="True" Font-Names="Calibri" ForeColor="#CC0000" Font-Size="Small"></asp:Label>
                        <asp:Label ID="lblErrMessageEx" runat="server" Text="Label" style="display:none" Font-Bold="True" Font-Names="Calibri" ForeColor="#CC0000" Font-Size="Small"></asp:Label>
                        <asp:Label ID="lblChangePasswordMsg" runat="server" Text="ChangePasswordLabel" Visible="False" Font-Bold="True" Font-Names="Calibri" ForeColor="#000099" Font-Size="Small"></asp:Label>
                                               
                    </td>
                    <td>
                        <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="button" OnClick="btnLogin_Click" OnClientClick="return OnLogin();" />
                    </td>

                </tr>
            </table>
        </div>

    </form>
</div>

<div class="gradient"></div>
<script type="text/javascript">
    function OnLogin() {
        var txtUserID = document.getElementById("<%=txtUserID.ClientID %>");
        var valid = true;

        if ($('span[id*=lblErrMessage]').length) {
            $('span[id*=lblErrMessage]').css('display', 'none');
        }

        if (txtUserID.value.length > 8) {
            $('span[id*=lblErrMessageEx]').css('display', '');
            $('span[id*=lblErrMessageEx]').html('User ID must be not more than 8 characters.');
            valid = false;
        }
        else {
            $('span[id*=lblErrMessageEx]').css('display', 'none');
            valid = true;
        }

        return valid;
    }
</script>
</asp:Content>
