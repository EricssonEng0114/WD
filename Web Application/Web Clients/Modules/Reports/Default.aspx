<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb.Modules.Reports.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">    

<form runat="server">

 <div class="content">
    <ul class="breadcrumb">
        <li>
            <p>Operator Tasks</p>
        </li>
        <li><a href="/Reports" class="active">Reports</a></li>
    </ul>
    <div class="page-title">
        <i class="icon-custom-right"></i>
        <h3><span class="semi-bold"> Reports</span></h3>
    </div>
    <div class="row-fluid">
        <div class="span12">
            <div class="grid simple">
                <div class="grid-title">
                    <div class="row">
                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">*Busdate:</label>
                                <asp:TextBox ID="txtDateTime" runat="server" CssClass="form-control"></asp:TextBox>
                                 <span id="Date_Required" class="help" style="display: none; color: red;">*Required</span>
                            </div>
                        </div>

                                            
                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">Client Code:</label>
                                <asp:DropDownList ID="ddlClient" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlClient_SelectedIndexChanged1"></asp:DropDownList>
                            </div>
                        </div>

                         <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">Worksource:</label>
                                <asp:DropDownList ID="ddlWorksource" runat="server" CssClass="form-control"></asp:DropDownList>
                            </div>
                        </div>

                        <div class="col-md-2">
                            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-success" style="margin-top:25px;"  OnClick="btnSearch_Click" OnClientClick="return checkSeachData()" />
                        </div>
                    </div>
                
                </div>
                <div class="grid-body">
                    <table id="dataTable" class="table table-hover table-condensed">
                        <thead>
                              <tr class="dataTable_HeaderRow">
                                <th>Report Code</th>
                                <th>Report Name</th>
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

         validNavigation = true;


        if (navigator.appVersion.indexOf("MSIE") != -1)
            $('select').ieExpandSelectWidth();

        //Dialog
        var redirectURL;

        $("#<%: txtDateTime.ClientID %>").datepicker(
             {
                 autoclose: true,
                 changeMonth: true,
                 changeYear: true
             });//.datepicker("setDate", new Date());;


     $("#<%: txtDateTime.ClientID %>").attr("readonly", true);

    });

     function setValidNavigation() {
         validNavigation = true;
     }


    function checkSeachData()
    {
        var dateTime = $("#<%: txtDateTime.ClientID %>").val();

        if (dateTime.length > 0)
        {
            $('#Date_Required').hide();
        }
        else
        {
            $('#Date_Required').show();
            unloadSpinner();
            return false;
        }
        return true;
    }
</script>

 </form>
</asp:Content>



