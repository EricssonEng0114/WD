<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PrintReportViewer.aspx.cs" Inherits="UBPCWeb.Modules.AuditLogViewer.PrintReportViewer" %>
<%@ Register assembly="CrystalDecisions.Web, Version=13.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" namespace="CrystalDecisions.Web" tagprefix="CR"  %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>

<body>
    <form id="form1" runat="server">
    <div>
        <CR:CrystalReportViewer ID="PrintCrystalReportViewer1" runat="server" AutoDataBind="true" HasCrystalLogo="False" HasExportButton="False" ToolPanelView="None" />
    </div>
    </form>
</body>
</html>
