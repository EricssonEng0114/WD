<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="UBPCWeb.CRViewerReportForm.ReportViewer" %>
<%@ Register assembly="CrystalDecisions.Web, Version=13.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" namespace="CrystalDecisions.Web" tagprefix="CR"  %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="../admincss/assets/plugins/jquery/jquery-3.6.2.min.js" type="text/javascript"></script>
</head>

<body>
    <form id="form1" runat="server">
    <div>
        <CR:CrystalReportViewer ID="CrystalReportViewer1" runat="server" AutoDataBind="true" />
    </div>
    </form>
</body>
</html>

<script type="text/javascript" language="javascript" class="init">
    $(document).ready(function () {
        $(function () {
            $(this).bind("contextmenu", function (e) {
                e.preventDefault();
            });
        });

        $(document).keydown(function (event) {
            if (event.keyCode == 123) {
                return false;
            }
            else if (event.ctrlKey && event.shiftKey && event.keyCode == 73) {
                return false;  //Prevent from ctrl+shift+i
            }
            else if (event.ctrlKey && event.keyCode == 85) {
                return false;  //Prevent from ctrl+u
            }
        });
                
    });

</script>

