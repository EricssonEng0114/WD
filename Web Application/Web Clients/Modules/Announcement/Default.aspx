<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb.Modules.Announcement.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">    

<form runat="server">

 <div class="content">
    <ul class="breadcrumb">
        <li>
            <p>Maintenance Tasks</p>
        </li>
        <li><a href="/Announcement" class="active">Announcement Maintenance</a></li>
    </ul>
    <div class="page-title">
        <i class="icon-custom-right"></i>
        <h3><span class="semi-bold"> Announcement - List</span></h3>
    </div>
    <div class="row-fluid">
        <div class="span12">
            <div class="grid simple">
                <div class="grid-title">
                    <asp:Button ID="New" runat="server" Text="Add New" CssClass="btn btn-primary" OnClick="New_Click" />
                    <asp:Button ID="btnClearFilter" runat="server" Text="Clear Filters" CssClass="btn btn-success" OnClick="btnClearFilter_Click"/>
                </div>
                <div class="grid-body">
                    <div class="row">
                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">Client Code:</label>
                                <asp:DropDownList ID="ddlClient" runat="server" CssClass="form-control">
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">Title:</label>
                                <asp:TextBox ID="txtTitle" MaxLength="100" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">Description:</label>
                                <asp:TextBox ID="txtDesc" MaxLength="4000" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>              
                        <div class="col-md-2">
                            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-success" Style="margin-top: 25px;" OnClick="btnSearch_Click" />

                        </div>
                    </div>
                    <table id="dataTable" class="table table-hover table-condensed">
                        <thead>                            
                            <tr class="dataTable_HeaderRow">
                                <th>ID</th>
                                <th>Client</th>
                                <th>Title</th>
                                <th>Description</th>
                                <th>From</th>
                                <th>To</th>
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

        //initAnnouncementDataTable();
        //configureTable();
    });

</script>


<div id="dialog-delete" title="Delete Annoucement" style="display: none">
    <p>Are you confirm to delete selected Announcement Record?</p>
</div>
<div id="dialog-info" title="Delete Announcement" style="display: none">
    <p>You are not allowed to delete Administrator Account</p>
</div>

 </form>
</asp:Content>

