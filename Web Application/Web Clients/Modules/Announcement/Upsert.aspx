<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Upsert.aspx.cs" Inherits="UBPCWeb.Modules.Announcement.Upsert" %>
<asp:Content ID="ContentUpsertAnnouncement" ContentPlaceHolderID="MainAdminContent" runat="server">
    <form runat="server" id="submitForm">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <div class="content">
    <ul class="breadcrumb">
        <li>
            <p>Maintenance Tasks</p>
        </li>
        <li><a href="/Announcement" class="active">Announcement Maintenance</a> </li>
    </ul>
    <div class="page-title">
        <i class="icon-custom-right"></i>
            <h3><span class="semi-bold">
                <asp:Label ID="lblTitle" runat="server" Text="Create/Update Announcement"></asp:Label>
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
                                <label class="form-label">*Title</label>
                                    <span id="Title_Required" class="help" style="display: none; color: red;">*Required</span>
                                <div class="controls">
                                    <asp:TextBox ID="txtID" runat="server" CssClass="form-control" Visible="false"></asp:TextBox>

                                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                            </div>
                                <div class="form-group">
                                <label class="form-label">*Client</label>
                                    <span id="Client_Required" class="help" style="display: none; color: red;">*Required</span>
                                <div class="controls">
                                        <asp:DropDownList ID="ddlClient" runat="server" CssClass="form-control" Width="300px"></asp:DropDownList>
                                </div>
                            </div>

                            <div class="form-group">
                                <label class="form-label">*Description</label>
                                    <span id="Desc_Required" class="help" style="display: none; color: red;">*Required</span>
                                <div class="controls">
                                    <asp:TextBox ID="txtDesc" MaxLength="4000" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>
                            </div>
                            
                            <div class="form-group">
                                    <label class="form-label">*Type</label>

                                    <div class="controls">
                                        <asp:DropDownList ID="ddlType" runat="server" CssClass="form-control" Width="300px"></asp:DropDownList>
                                    </div>
                                </div>
                            
                            <div class="form-group">
                                <label class="form-label">*Date From</label>
                                    <span id="FromDate_Required" class="help" style="display: none; color: red;">*Required</span>   
                                    <span id="FromDate_NotGreater" class="help" style="display: none; color: red;">*Date From must not greater than Date To</span>                                  
                                                            
                                <div class="controls">
                                    <asp:TextBox ID="txtDateFrom" runat="server" CssClass="form-control" Width="300px"></asp:TextBox>
                                </div>
                            </div>

                            <div class="form-group">
                                <label class="form-label">*Date To</label>
                                    <span id="ToDate_Required" class="help" style="display: none; color: red;">*Required</span>
                                <div class="controls">
                                    <asp:TextBox ID="txtDateTo" runat="server" CssClass="form-control" Width="300px"></asp:TextBox>
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
    $(document).ready(function () {
        $("#<%: txtDateFrom.ClientID %>").datepicker(
             {
                 autoclose: true,
                 changeMonth: true,
                 changeYear: true
             });

        $("#<%: txtDateTo.ClientID %>").datepicker(
       {
           autoclose: true,
           changeMonth: true,
           changeYear: true
       });

        $("#<%: txtDateFrom.ClientID %>").attr("readonly", true);
        $("#<%: txtDateTo.ClientID %>").attr("readonly", true);

    });
</script>

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


            var title = $("#<%: txtTitle.ClientID %>").val();
            var desc = $("#<%: txtDesc.ClientID %>").val();
            var fromDate = $("#<%: txtDateFrom.ClientID %>").val();
            var toDate = $("#<%: txtDateTo.ClientID %>").val();


            var countEmpty = 0;

            if (title.length > 0) {
                $('#Title_Required').hide();
            }
            else {
                countEmpty = countEmpty + 1;
                $('#Title_Required').show();
            }

            if (desc.length > 0) {
                $('#Desc_Required').hide();
            }
            else {
                countEmpty = countEmpty + 1;
                $('#Desc_Required').show();
            }

            if (fromDate.length > 0) {
                $('#FromDate_Required').hide();
            }
            else {
                countEmpty = countEmpty + 1;
                $('#FromDate_Required').show();
            }

            if (toDate.length > 0) {
                $('#ToDate_Required').hide();
            }
            else {
                countEmpty = countEmpty + 1;
                $('#ToDate_Required').show();
            }

            if (fromDate.length > 0 && toDate.length > 0) {
                var FromDate = new Date(fromDate);
                var ToDate = new Date(toDate);

                if (FromDate > ToDate) {
                    countEmpty = countEmpty + 1;
                    $('#FromDate_NotGreater').show();
                }
                else
                {
                    $('#FromDate_NotGreater').hide();
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

