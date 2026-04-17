<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb.Modules.RejectedItemDecision.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">
    
    <form runat="server">

        <div class="content">
            <ul class="breadcrumb">
                <li>
                    <p>Operator Tasks</p>
                </li>
                <li><a href="/WebDecision" class="active">Rejected Item Decision</a></li>
            </ul>
            <div class="page-title">
                <i class="icon-custom-right"></i>
                <h3><span class="semi-bold">Rejected Item Decision - List</span></h3>
            </div>
            <div class="row-fluid">
                <div class="span12">
                    <div class="grid simple">
                        <div class="grid-title">
                            <div class="row">
                                <div class="col-md-2">
                                    <div class="form-group">
                                        <label class="form-label">*Business Date:</label>
                                        <asp:TextBox ID="txtDateTime" runat="server" CssClass="form-control"></asp:TextBox>
                                        <span id="Date_Required" class="help" style="display: none; color: red;">*Required</span>
                                    </div>
                                </div>

                                <div class="col-md-2">
                                    <div class="form-group">
                                        <label class="form-label">Client Code:</label>
                                        <asp:DropDownList ID="ddlClient" runat="server" CssClass="form-control" OnSelectedIndexChanged="ddlClient_SelectedIndexChanged">
                                        </asp:DropDownList>
                                    </div>
                                </div>

                                <div class="col-md-2">
                                    <div class="form-group">
                                        <label class="form-label">Presenting Branch:</label>
                                        <asp:TextBox ID="txtPresentingBSB" runat="server" MaxLength="7"  onkeypress="return onlyNumbers(event);"></asp:TextBox>
                                    </div>
                                </div>
                                
                                 <div class="col-md-2">
                                    <div class="form-group">
                                        <label class="form-label">Batch No.:</label>
                                        <asp:TextBox ID="txtBatchNo" runat="server" MaxLength="8"  onkeypress="return onlyNumbers(event);"></asp:TextBox>
                                    </div>
                                </div>


                                <div class="col-md-2">
                                    <div class="form-group">
                                        <label class="form-label">Reject Category:</label>
                                        <asp:DropDownList ID="ddlRejCategory" runat="server" CssClass="form-control">
                                            <asp:ListItem>RL</asp:ListItem>
                                            <asp:ListItem>DI</asp:ListItem>
                                            <asp:ListItem>PV</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>


                                <div class="col-md-2">
                                    <%--                            <button type="button" style="margin-top:25px;" class="btn btn-success" onclick="search();">Search</button>--%>
                                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-success" Style="margin-top: 25px;" OnClick="btnSearch_Click" />
                                </div>
                            </div>


                        </div>
                        <div class="grid-body ">
                            <div>
                                <p>
                                    <asp:Label ID="lblCutOffTimeMsg" runat="server" Text="*Sorry, you cannot perform decision after Cut Off time." Visible="False" Font-Bold="True" Font-Size="Medium" ForeColor="Red"></asp:Label></p>
                            </div>
                            <table id="dataTable" class="table table-hover table-condensed">
                                <thead>
                                    <tr class="dataTable_HeaderRow">
                                        <th>Presenting Branch</th>
                                        <th>Batch No.</th>
                                        <th>Trans No.</th>
                                        <th>Reject Category</th>
                                        <th>Reject Reason</th>
                                        <th>Reject Time (By Unisys)</th>
                                        <% if (clientCode == "CIMB")
                                        { %>
                                            <th>TRANS AMT</th>
                                        <%} %>
                                        <th></th>
                                    </tr>
                                </thead>
                                <tbody></tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <asp:PlaceHolder runat="server">
            <%: Scripts.Render("~/DataTableGrid/js") %>
        </asp:PlaceHolder>

        <script type="text/javascript" class="init">

            $(document).ready(function () {

                if (navigator.appVersion.indexOf("MSIE") != -1)
                    $('select').ieExpandSelectWidth();

                //Dialog
                var redirectURL;
       <%--         $("#<%: txtDateTime.ClientID %>").datepicker(
           {
               autoclose: true,
               changeMonth: true,
               changeYear: true
           });//.datepicker("setDate", new Date());--%>


         $("#<%: txtDateTime.ClientID %>").attr("readonly", true);

     });

    
        </script>

         <script>
             function displaymsg(title, msg) {
                 BootstrapDialog.show({
                     title: title,//'Message',
                     message: msg,
                     buttons: [{
                         label: 'Close',
                         action: function (dialog) {
                             dialog.close();
                         }
                     }]
                 });
             }
        </script>
    </form>
</asp:Content>





