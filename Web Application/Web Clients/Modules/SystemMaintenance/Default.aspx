<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb.Modules.SystemMaintenance.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">    

<form runat="server">

 <div class="content">
    <ul class="breadcrumb">
        <li>
            <p>Maintenance Tasks</p>
        </li>
        <li><a href="/System" class="active">System Maintenance</a></li>
    </ul>
    <div class="page-title">
        <i class="icon-custom-right"></i>
        <h3><span class="semi-bold"> System Maintenance - List</span></h3>
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
                                <label class="form-label">Name:</label>
                                 <asp:TextBox ID="txtName" MaxLength="50" runat="server" CssClass="form-control"></asp:TextBox>

                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">Value 1:</label>
                                <asp:TextBox ID="txtValue1" MaxLength="100" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">Value 2:</label>
                                <asp:TextBox ID="txtValue2" MaxLength="50" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>              
                        <div class="col-md-2">
                            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-success" Style="margin-top: 25px;" OnClick="btnSearch_Click" />

                        </div>
                    </div>

                    <table id="dataTable" class="table table-hover table-condensed">
                        <thead>
                            
                            <tr class="dataTable_HeaderRow">
<%--                                <th></th>--%>
                                <th>Name</th>
                                <th>Value 1</th>
                                <th>Value 2</th>
                                <th>Comment</th>
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

    });

</script>


<div id="dialog-delete" title="Delete Parameters" style="display: none">
    <p>Are you confirm to delete selected Record?</p>
</div>
<div id="dialog-info" title="Delete Parameters" style="display: none">
    <p>You are not allowed to delete Administrator Account</p>
</div>

 </form>
</asp:Content>


