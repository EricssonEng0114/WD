<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Upsert.aspx.cs" Inherits="UBPCWeb.Modules.SystemMaintenance.Upsert" %>
<asp:Content ID="ContentUpsertAnnouncement" ContentPlaceHolderID="MainAdminContent" runat="server">
    <form runat="server" id="submitForm">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <div class="content">
    <ul class="breadcrumb">
        <li>
            <p>Maintenance Tasks</p>
        </li>
        <li><a href="/System" class="active">System Maintenance</a> </li>
    </ul>
    <div class="page-title">
        <i class="icon-custom-right"></i>
            <h3><span class="semi-bold">
                <asp:Label ID="lblTitle" runat="server" Text="Create/Update Parameters"></asp:Label>
                </span></h3>
    </div>
    <div class="row">
        <div class="col-md-12">
            <div class="grid simple">
                <div class="grid-title no-border">
                    <h4> Please fill in all mandatory field(s) below and click Save.</h4>
                </div>
                <div class="grid-body no-border">
                    <div class="row">
                        <div class="col-md-8 col-sm-8 col-xs-8">
                            <div class="form-group">
                                <label class="form-label">*Name</label>
                                    <span id="Name_Required" class="help" style="display: none; color: red;">*Required</span>
                                    <span id="Alphanumeric_Required" class="help" style="display: none; color: red;">*Please enter only alphabets and/or numbers.</span>

                                
                                 <div class="controls">
                                    <asp:TextBox ID="txtParamName" runat="server" CssClass="form-control" MaxLength="50" OnTextChanged="txtParamName_TextChanged"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label class="form-label">Value 1</label>
                                <div class="controls">
                                    <asp:TextBox ID="txtValue1" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                                </div>
                            </div>
                             <div class="form-group">
                                <label class="form-label">Value 2</label>
                                <div class="controls">
                                    <asp:TextBox ID="txtValue2" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="form-label">Comment</label>
                                <div class="controls">
                                    <textarea id="txtComment" rows="3" runat="server" class="form-control" maxlength="70"></textarea>
                                </div>
                            </div>
                           
                           
<%--            <button type="button" id="btnSave" name="command" value="Save" class="btn btn-info" onclick="onSave()">Save</button>--%>
             <asp:Button ID="btnSave" runat="server" Text="Save"  CssClass="btn btn-info" OnClick="btnSave_Click" OnClientClick="return checkSaveData()" />

             <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" CssClass="btn btn-danger" OnClientClick="oncancel()" />


    </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
        </div>
</form>


    <script>
        function oncancel()
        {
            isLoadSpinner = true;
        }

        function pad(n, width, z) {
            z = z || '0';
            n = n + '';
            return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
        }

        function checkSaveData() {

            var check_value = document.createElement("INPUT");
            check_value.type = "hidden";
            check_value.name = "check_value";

            var paramName = $("#<%: txtParamName.ClientID %>").val();

            var countEmpty = 0;

            if ($("#<%: txtParamName.ClientID %>").attr('readonly') == 'readonly') {
            }
            else
            {
                if (paramName.length > 0) {
                    $('#Name_Required').hide();

                    if (isAlphaNumeric(paramName)) {
                        $('#Alphanumeric_Required').hide();
                    }
                    else {
                        countEmpty = countEmpty + 1;
                        $('#Alphanumeric_Required').show();
                    }
                }
                else {
                    countEmpty = countEmpty + 1;
                    $('#Name_Required').show();
                }
            }

            if (countEmpty > 0) {
                isLoadSpinner = false;

                check_value.value = "No";
                unloadSpinner();
                return false;
            }
            else {
                isLoadSpinner = true;
                check_value.value = "Yes";
                return true;
            }
        }
    </script>
</asp:Content>


