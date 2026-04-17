<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb.Modules.AuditLogViewer.Default" %>
<%@ Register assembly="CrystalDecisions.Web, Version=13.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" namespace="CrystalDecisions.Web" tagprefix="CR" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">    

<form runat="server">

 <div class="content">
    <ul class="breadcrumb">
        <li>
            <p>Monitoring Tasks</p>
        </li>
        <li><a href="/Log" class="active">Audit Log Viewer</a></li>
    </ul>
    <div class="page-title">
        <i class="icon-custom-right"></i>
        <h3><span class="semi-bold"> Audit Log Viewer</span></h3>
    </div>
    <div class="row-fluid">
        <div class="span12">
            <div class="grid simple">
                <div class="grid-title">
<%--                    <button id="ClearFilter" class="btn btn-success">Clear Filters</button>--%>
                    <div class="row">
                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">*Date:</label>
                                <asp:TextBox ID="txtDateTime" runat="server" CssClass="form-control"></asp:TextBox>
                                 <span id="Date_Required" class="help" style="display: none; color: red;">*Required</span>
                            </div>
                        </div>

                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">Severity:</label>
                                <asp:DropDownList ID="ddlSeverity" runat="server" CssClass="form-control">
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">Caller:</label>
                                <asp:DropDownList ID="ddlCaller" runat="server" CssClass="form-control"></asp:DropDownList>
                            </div>
                        </div>
                        
                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">Client Code</label>
                                <asp:DropDownList ID="ddlClient" runat="server" CssClass="form-control"></asp:DropDownList>
                            </div>
                        </div>

                        <div class="col-md-4">
<%--                            <button type="button" style="margin-top:25px;" class="btn btn-success" onclick="search();">Search</button>--%>
                            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-success" style="margin-top:25px;"  OnClick="btnSearch_Click" OnClientClick="return checkSeachData()" />
                            <asp:LinkButton runat="server" ID="lnkPrint" Text="<i class='fa fa-print'></i> Print " CssClass="btn btn-primary" style="margin-top:25px;float: right;margin-left:5px" OnClientClick="return checkAllSelected(true);" OnClick="lnkPrint_Click" />
                           
                            <asp:LinkButton runat="server" ID="lnkSave" Text="<i class='fa fa-save'></i> Save" CssClass="btn btn-primary" style="margin-top:25px;float: right;" OnClick="lnkSave_Click" OnClientClick="return checkAllSelected(false);" />                                                        

                        </div>
                    </div>
                
                </div>
                <div class="grid-body">
                    <table id="dataTable" class="table table-hover table-condensed">
                        <thead>
                            <tr>
                                <td></td> <td></td> <td></td> <td></td> <td></td> <td></td> <td></td> <td></td>
                            </tr>
                              <tr class="dataTable_HeaderRow">
                                <th>Log ID</th>
                                <th>Date Time</th>
                                <th>Severity</th>
                                <th>Caller</th>
                                <th>Client Code</th>
                                <th>User ID</th>
                                <th>Message</th>
                                <th>Data</th>
                                <th>Exception</th>
                            </tr>
                        </thead>                      
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>

<div id="myExceptionLog" class="modal fade" role="dialog">
    <div class="modal-dialog">
        <!-- Modal content-->
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal">&times;</button>
                <h4 class="modal-title">Exception</h4>
            </div>
            <div class="modal-body">
               
                <p>
                    <span style="width: 50px;" id="messageExp"></span>   
                </p>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
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

        $("#<%: txtDateTime.ClientID %>").datepicker(
             {
                 autoclose: true,
                 changeMonth: true,
                 changeYear: true
             });//.datepicker("setDate", new Date());;
 

     $("#<%: txtDateTime.ClientID %>").attr("readonly", true);

    });

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

 <script>
        
         function checkAllSelected(isPrint) {
             var selectedSev = $("#<%: ddlSeverity.ClientID %>").find(":selected").val();
             var selectCaller = $("#<%: ddlCaller.ClientID %>").find(":selected").val();
             var selectClient = $("#<%: ddlClient.ClientID %>").find(":selected").val();

             if (selectedSev == "ALL" && selectCaller == "ALL" && selectClient == "ALL") {
                 jQuery("#dialog-confirmbox").html("Generate <b>ALL</b> data will take longer time. <br /> It's <b>not advisable</b> to do it during <b>peak</b> hour.<br />  Are you sure to proceed?");

                 jQuery("#dialog-confirmbox").dialog(
                 {
                     modal: true,
                     buttons: {
                         "Ok": function () {
                             validNavigation = true;
                             $(this).dialog("close");
                             loadSpinner();
                             if (isPrint == true) {
                                 <%=Page.ClientScript.GetPostBackEventReference(lnkPrint, "") %>
                             }
                             else {
                                 <%=Page.ClientScript.GetPostBackEventReference(lnkSave, "") %>
                             }
                         },
                         "Cancel": function () {
                             validNavigation = true;
                             $(this).dialog("close");
                             unloadSpinner();
                             return false;
                         }
                     }
                 });
             }
             else
             {
                 loadSpinner();
                 return true;
             }

             return false;

        }

    </script>

     <script>
         function openCR() {
             var selectedDT = $("#<%: txtDateTime.ClientID %>").val();
             var selectedClient = $("#<%: ddlClient.ClientID %>").val();
             var selectedSev = $("#<%: ddlSeverity.ClientID %>").val();
             var selectedCaller = $("#<%: ddlCaller.ClientID %>").val();

             var urlNet = setAuditPrintRptURL(selectedDT, selectedClient, selectedSev, selectedCaller);

            var option = 'height=' + screen.availHeight + ', width=' + screen.availWidth + ',scrollbars=1,resizable=1,top=0,left=0';
            myWindow = window.open(urlNet, '_blank', option);
            myWindow.resizeTo(window.screen.availWidth, window.screen.availHeight);
            myWindow.focus();
        }

    </script>

 </form>
</asp:Content>



