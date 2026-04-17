<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb.Modules.OutlookRecipientMaintenance.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">
    <form runat="server">
        <div class="content">
            <ul class="breadcrumb">
                <li>
                    <p>Maintenance Tasks</p>
                </li>
                <li><a href="/OutlookRecipient" class="active">Outlook Recipient Maintenance</a></li>
            </ul>
            <div class="page-title">
                <i class="icon-custom-right"></i>
                <h3><span class="semi-bold">Outlook Recipient Maintenance - List</span></h3>
            </div>
            <div class="row-fluid">
                <div class="span12">
                    <div class="grid simple">
                        <div class="grid-title">
                            <asp:Button ID="New" runat="server" Text="Add New" CssClass="btn btn-primary" OnClick="New_Click" />
                            <asp:Button ID="btnClearFilter" runat="server" Text="Clear Filters" CssClass="btn btn-success" OnClick="btnClearFilter_Click" />
                        </div>
                        <div class="grid-body ">
                            <div class="row">
                                <div class="col-md-2">
                                    <div class="form-group">
                                        <label class="form-label">Client Code:</label>
                                        <asp:DropDownList ID="ddlClient" runat="server" CssClass="form-control">
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div id="divSite" class="col-md-2" style="display: none;">
                                    <div class="form-group">
                                        <label class="form-label">Site:</label>
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
                                <div class="col-md-2">
                                    <div class="form-group">
                                        <label class="form-label">Email Address:</label>
                                        <asp:TextBox ID="txtEmailAddr" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                </div>

                                <div class="col-md-2">
                                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-success" Style="margin-top: 25px;" OnClick="btnSearch_Click" />
                                </div>
                            </div>

                            <table id="dataTable" class="table table-hover table-condensed">
                                <thead>
                                    <tr class="dataTable_HeaderRow">
                                        <th>Client Code</th>
                                        <th>Email Address</th>
                                        <th>Site</th>
                                        <th></th>
                                    </tr>
                                </thead>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <asp:PlaceHolder runat="server">
            <%: Scripts.Render("~/DataTableGrid/js") %>
        </asp:PlaceHolder>

        <div id="dialog-delete" title="Delete Outlook Recipient" style="display: none">
            <p>Are you confirm to delete selected Outlook Recipient?</p>
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
    </script>
</asp:Content>
