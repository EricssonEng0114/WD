<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Detail.aspx.cs" Inherits="UBPCWeb.Modules.OutlookRecipientMaintenance.Detail" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">
    <form runat="server" id="submitForm">
        <div class="content">
            <ul class="breadcrumb">
                <li>
                    <p>Maintenance Tasks</p>
                </li>
                <li><a href="/OutlookRecipient" class="active">Outlook Recipient Maintenance</a> </li>
            </ul>
            <div class="page-title">
                <i class="icon-custom-right"></i>
                <h3><span class="semi-bold">
                    <asp:Label ID="lblTitle" runat="server" Text="Create/Update Outlook Recipient"></asp:Label>
                </span></h3>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <div class="grid simple">
                        <div class="grid-title no-border">
                            <h4>Please fill in all mandatory field(s) below and click Save.</h4>
                        </div>
                        <div class="grid-body no-border">
                            <div class="row">
                                <div class="form-group">
                                    <label class="form-label">*Client Code</label>
                                    <span id="ClientCode_Required" class="help" style="display: none; color: red;">*Required</span>
                                    <div class="controls">
                                        <asp:DropDownList ID="ddlClient" runat="server" CssClass="form-control"></asp:DropDownList>
                                    </div>
                                </div>
                                  
                                <div id="divSite" class="form-group" style="display: none">
                                    <label class="form-label">Site</label>
                                    <span id="Site_Required" class="help" style="display: none; color: red;">*Required</span>
                                    <div class="controls">
                                        <asp:DropDownList ID="ddlSite" runat="server" CssClass="form-control">
                                            <asp:ListItem Value="">-- SELECT ---</asp:ListItem>
                                            <asp:ListItem Value="KL2">KL2</asp:ListItem>
                                            <asp:ListItem Value="KL1">KL1</asp:ListItem>
                                            <asp:ListItem Value="JB">JB</asp:ListItem>
                                            <asp:ListItem Value="PP">PP</asp:ListItem>
                                            <asp:ListItem Value="IP">IP</asp:ListItem>
                                            <asp:ListItem Value="KN">KN</asp:ListItem>
                                            <asp:ListItem Value="ML">ML</asp:ListItem>
                                            <asp:ListItem Value="AS">AS</asp:ListItem>
                                            <asp:ListItem Value="OTH">Others</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="form-label">*Email Address</label>
                                    <span id="EmailAddress_Required" class="help" style="display: none; color: red;">*Required</span>
                                    <span id="EmailAddress_Invalid" class="help" style="display: none; color: red;">*Invalid Email Address</span>
                                    <span id="EmailAddress_InvalidUsys" class="help" style="display: none; color: red;">*Unisys Email Required</span>

                                    <span id="EmailAddress_Exist" class="help" style="display: none; color: red;">*Email Address is exist</span>
                                    <div class="controls">
                                        <asp:TextBox ID="txtEmailAddress" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                                    </div>
                                </div>

                                <asp:HiddenField ID="hfActionType" runat="server" />
                                <asp:HiddenField ID="hfRecpID" runat="server" />
                                <asp:HiddenField ID="hfPrevEmailAddr" runat="server" />
                                <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-info" OnClick="btnSave_Click" OnClientClick="return checkSaveData()" />

                                <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" CssClass="btn btn-danger" OnClientClick="oncancel()" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>
    <script>
        document.addEventListener("DOMContentLoaded", () => {
            const ddl = document.getElementById('<%= ddlClient.ClientID %>');
             const ddlSite = document.getElementById('<%= ddlSite.ClientID %>');
             const divSite = document.getElementById('divSite');

             const toggleSiteDiv = (client) => {
                 divSite.style.display = client === 'USYS' ? 'block' : 'none';
             };

             const handleChange = ({ target }) => {
                 ddlSite.selectedIndex = 0;   // reset site dropdown
                 toggleSiteDiv(target.value);
             };

             // initial load
             toggleSiteDiv(ddl?.value);

             // event binding
             ddl?.addEventListener("change", handleChange);
         });

        function checkSaveData() {
            loadSpinner();
            var emailAddr = $("#<%:txtEmailAddress.ClientID %>").val();
            var clientCode = $("#<%:ddlClient.ClientID %>").val();
            var site = $("#<%:ddlSite.ClientID %>").val();
            var valid = true;

            $('#ClientCode_Required').hide();
            $('#EmailAddress_Invalid').hide();
            $('#EmailAddress_InvalidUsys').hide();

            $('#EmailAddress_Required').hide();
            $('#EmailAddress_Exist').hide();

            $('#Site_Required').hide();

            if (clientCode.length == 0) {
                $('#ClientCode_Required').show();
                valid = false;
            }

            if (clientCode === 'USYS' && site === '') {
                valid = false;

                $('#Site_Required').show();
            }

            if (emailAddr.length == 0) {
                valid = false;
                $('#EmailAddress_Required').show();
            }
            else {
                var ret = checkEmailAddress(emailAddr);
                if (!ret) {
                    valid = false;
                    $('#EmailAddress_Invalid').show();
                }
                else {
                    if (clientCode == 'USYS') {
                        //Check if valid unisys email
                        var retU = validateUnisysEmail(emailAddr);
                        if (!retU) {
                            valid = false;
                            $('#EmailAddress_InvalidUsys').show();
                        }
                    }
                }
            }

            if (valid) {
                var ret = validateEmailAddrExist();
                if (ret != "0") {
                    valid = false;
                    $('#EmailAddress_Exist').show();
                }
            }

            if (!valid) {
                isLoadSpinner = false;
                unloadSpinner();
            }

            return valid;
        }

        function oncancel() {
            loadSpinner();
            isLoadSpinner = true;
        }

        function validateEmailAddrExist() {
            var emailAddr = $("#<%:txtEmailAddress.ClientID %>").val();
            var clientCode = $("#<%:ddlClient.ClientID %>").val();
            var id = $("#<%:hfRecpID.ClientID %>").val();
            var obj = { id: id, emailAddr: emailAddr, clientCode: clientCode };
            var param = JSON.stringify(obj);

            return checkEmailAddrExist(param);
        }

        function validateUnisysEmail(email) {
            const lowerCaseEmail = email.toLowerCase();

            // Check if the email address ends with "unisys.com"
            return lowerCaseEmail.endsWith("unisys.com");
        }

    </script>
</asp:Content>
