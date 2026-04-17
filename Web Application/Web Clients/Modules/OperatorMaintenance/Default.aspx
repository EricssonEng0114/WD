<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb.Modules.OperatorMaintenance.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">
    

    <form runat="server">

 <div class="content">
    <ul class="breadcrumb">
        <li>
            <p>Maintenance Tasks</p>
        </li>
        <li><a href="/Operator" class="active">Operator Maintenance</a></li>
    </ul>
    <div class="page-title">
        <i class="icon-custom-right"></i>
        <h3><span class="semi-bold"> Operator Maintenance - List</span></h3>
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
                                <label class="form-label">User ID:</label>
                                <asp:TextBox ID="txtUserID" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">User Name:</label>
                                <asp:TextBox ID="txtUsrName" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">User Group:</label>
                                <asp:DropDownList ID="ddlUsrGroup" runat="server" CssClass="form-control" Height="16px">
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">Client Code:</label>
                                <asp:DropDownList ID="ddlClient" runat="server" CssClass="form-control">
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="col-md-2">
                            <%--                            <button type="button" style="margin-top:25px;" class="btn btn-success" onclick="search();">Search</button>--%>
                            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-success" Style="margin-top: 25px;" OnClick="btnSearch_Click" />

                        </div>
                    </div>
                    <table id="dataTable" class="table table-hover table-condensed">
                        <thead>
                            <%--<tr>
                                <td><input class="textbox" type="text" name="search_UserID" value="" placeholder="Search by User ID" /></td>
                                <td><input class="textbox" type="text" name="search_UserName" value="" placeholder="Search by User Name" /></td>
                                <td><input class="textbox" type="text" name="search_UserGroup" value="" placeholder="Search by User Group" /></td>
                                <td><input class="textbox" type="text" name="search_UserClient" value="" placeholder="Search by Client" /></td>
                                <td><input class="textbox" type="text" name="search_UserComment" value="" placeholder="Search by Comment" /></td>
                                <td></td>
                            </tr>--%>
                            <tr class="dataTable_HeaderRow">
                                <th>User ID</th>
                                <th>User Name</th>
                                <th>User Group</th>
                                <th>Clients</th>
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

        //intiOperatorDataTable();
        //configureTableByTemp();

        var tempMsg = '<%: Session["s_TempMsg"] %>';

        if (tempMsg != "") {
            alert(tempMsg);
            $("#dialog-general-message").dialog('open');
        }

    });
     


    function validateUsrLogin(userID) {
        loadSpinner();

        var obj = { id: userID };
        var param = JSON.stringify(obj);
        return checkIsUserLogin(param);
    }

    </script>


<div id="dialog-delete" title="Delete User" style="display: none">
    <p>Are you confirm to delete selected User Record?</p>
</div>
<div id="dialog-info" title="Delete User" style="display: none">
    <p>You are not allowed to delete Administrator Account</p>
</div>
<div id="dialog-usrLogin" title="Delete User" style="display: none">
    <p>You are not allowed to delete as this operator is currently login</p>
</div>

<div id="dialog-general-message" title="Operator Maintenance" style="display: none">
    <p><%: Session["s_TempMsg"] %>    </p>
 </div>


 </form>
</asp:Content>

