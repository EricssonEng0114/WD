<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb.Modules.Dashboard.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">
    <form runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

        <style>
            #MonitorLabel {
                width: 100%;
                border-collapse: collapse;
            }

            #MonitorLabel th, td {
                padding: 5px;
                text-align: center;
                font-size: 16px; /* Increased font size */
                color: black; /* Text color changed to black */
            }

            #MonitorLabel th {
                font-weight: bold;
            }

            #MonitorLabel .label-text {
                font-size: 20px; /* Applies to all values */
                color: black; /* Ensures all values are black */
            }

            .dataTables_scrollBody {
                width: 100% !important;
            }

            .dataTables_scrollHeadInner {
                width: 100% !important;
            }

            .table-responsive {
                width: 100% !important;
            }

            #firstCol {
                width: 20% !important;
            }
        </style>

        <div class="content">
            <div style="width: 100%;">
                <div style="width: 67%; float: left; padding-right: 0.5cm">
                    <div style="width: 100%;">
                        <div class="tabA">
                            <button type="button" class="tablinksS"
                                onclick="loadAnnouncement(event)" id="headerAnnouncement">
                                Announcement 
                            </button>
                        </div>
                        <div id="announce_0" class="tabcontentA">
                            <% if (annList.Count() > 0)
                                { %>

                            <table style="width: 100%">
                                <% foreach (UBPCWeb.Modules.Announcement.AnnouncementModel obj in annList)
                                    {
                                        string selectdType = obj.AnnType.Trim();%>

                                <tr style="padding: 5px">
                                    <td style="padding: 5px">
                                        <div class="message_box <%: selectdType %>">
                                            <%: obj.Client + ":" + obj.Desc %>
                                        </div>
                                    </td>
                                </tr>

                                <% } %>
                            </table>
                            <% }
                                else
                                {  %>
                            <div class="message_box info">
                                There is No announcement today.
                            </div>
                            <% }%>
                        </div>
                    </div>

                    <div style="width: 100%; padding-top: 15px">
                        <div class="panel-group" id="accordion">
                            <div class="panel panel-primary">
                                <div class="panel-heading">
                                    <div class="panel-title">
                                        <a data-toggle="collapse"
                                            data-parent="#accordion"
                                            href="#BranchStatus">Rejected Status</a>
                                    </div>
                                </div>
                                <div id="BranchStatus" class="panel-collapse collapse in">
                                    <div class="panel-body">
                                        <div class="table-responsive">
                                            <asp:UpdatePanel ID="ajaxPanel1" runat="server">
                                                <ContentTemplate>

                                                    <table style="width: 100%">
                                                        <tr>
                                                            <td style="width: 20%;">Select Business Date:</td>
                                                            <td style="width: 30%;">
                                                                <asp:TextBox ID="txtDateTime" runat="server" CssClass="form-control" AutoPostBack="true" OnTextChanged="txtDateTime_TextChanged"></asp:TextBox>
                                                            </td>
                                                            <td style="width: 20%;" align="right">Client:</td>
                                                            <td style="width: 35%;"> 
                                                                <asp:DropDownList ID="ddlClient" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlClient_SelectedIndexChanged"></asp:DropDownList></td>
                                                        </tr>
                                                        <tr>
                                                            <td colspan="4">
                                                                <div id="StatusTab">
                                                                    <div class="tab">
                                                                        <button type="button" class="tablinks"
                                                                            onclick="loadRejStatus(event, 'Status_All')" id="allType">
                                                                            All Reject
                                                                        </button>

                                                                        <% if (!isUnisys)
                                                                            {
                                                                        %>

                                                                        <button type="button" class="tablinks"
                                                                            onclick="loadRejStatus(event, 'Status_RL')" id="RLType">
                                                                            RL Reject
                                                                        </button>
                                                                        <button type="button" class="tablinks"
                                                                            onclick="loadRejStatus(event, 'Status_DI')" id="DIType">
                                                                            DI Reject
                                                                        </button>
                                                                        <button type="button" class="tablinks"
                                                                            onclick="loadRejStatus(event, 'Status_PV')" id="PVType">
                                                                            PV Reject
                                                                        </button>
                                                                        <%
                                                                            } %>
                                                                    </div>

                                                                    <div id="Status_All" class="tabcontent">
                                                                        <table align="center" style="width: 80%">
                                                                            <tr>
                                                                                <td>Waiting</td>
                                                                                <!--CR015-20:edit by AK for multiple level approval -->
                                                                                <td id="tdtxtAllWaiting" runat="server">
                                                                                    <asp:TextBox ID="txtAllWaiting" CssClass="form-control" runat="server" ReadOnly="true">10</asp:TextBox></td>
                                                                                <td id="tdlblAllPending" runat="server">Pending Approval</td>
                                                                                <td id="tdtxtAllPending" runat="server">
                                                                                    <asp:TextBox ID="txtAllPending" CssClass="form-control" runat="server" ReadOnly="true">10</asp:TextBox></td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td>In Use</td>
                                                                                <td colspan="3">
                                                                                    <asp:TextBox ID="txtAllInUse" CssClass="form-control" runat="server" ReadOnly="true">10</asp:TextBox></td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td>Completed</td>
                                                                                <td colspan="3">
                                                                                    <asp:TextBox ID="txtAllCompleted" CssClass="form-control" runat="server" ReadOnly="true">10</asp:TextBox></td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td><b>Total</b></td>
                                                                                <td colspan="3">
                                                                                    <asp:TextBox ID="txtAllTotal" CssClass="form-control" Font-Bold="true" runat="server" ReadOnly="true">10</asp:TextBox></td>
                                                                            </tr>
                                                                        </table>
                                                                    </div>

                                                                    <div id="Status_RL" class="tabcontent">
                                                                        <table align="center" style="width: 80%">
                                                                            <tr>
                                                                                <td>Waiting</td>
                                                                                <!--CR015-20:edit by AK for multiple level approval -->
                                                                                <td id="tdtxtRLWaiting" runat="server">
                                                                                    <asp:TextBox ID="txtRLWaiting" CssClass="form-control" runat="server" ReadOnly="true">2</asp:TextBox></td>
                                                                                <td id="tdlblRLPending" runat="server">Pending Approval</td>
                                                                                <td id="tdtxtRLPending" runat="server">
                                                                                    <asp:TextBox ID="txtRLPending" CssClass="form-control" runat="server" ReadOnly="true">10</asp:TextBox></td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td>In Use</td>
                                                                                <td colspan="3">
                                                                                    <asp:TextBox ID="txtRLInUse" CssClass="form-control" runat="server" ReadOnly="true">2</asp:TextBox></td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td>Completed</td>
                                                                                <td colspan="3">
                                                                                    <asp:TextBox ID="txtRLCompleted" CssClass="form-control" runat="server" ReadOnly="true">2</asp:TextBox></td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td><b>Total</b></td>
                                                                                <td colspan="3">
                                                                                    <asp:TextBox ID="txtRLTotal" CssClass="form-control" runat="server" Font-Bold="true" ReadOnly="true">2</asp:TextBox></td>
                                                                            </tr>
                                                                        </table>
                                                                    </div>

                                                                    <div id="Status_DI" class="tabcontent">
                                                                        <table align="center" style="width: 80%">
                                                                            <tr>
                                                                                <td>Waiting</td>
                                                                                <!--CR015-20:edit by AK for multiple level approval -->
                                                                                <td id="tdtxtDIWaiting" runat="server">
                                                                                    <asp:TextBox ID="txtDIWaiting" CssClass="form-control" runat="server" ReadOnly="true">2</asp:TextBox></td>
                                                                                <td id="tdlblDIPending" runat="server">Pending Approval</td>
                                                                                <td id="tdtxtDIPending" runat="server">
                                                                                    <asp:TextBox ID="txtDIPending" CssClass="form-control" runat="server" ReadOnly="true">10</asp:TextBox></td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td>In Use</td>
                                                                                <td colspan="3">
                                                                                    <asp:TextBox ID="txtDIInUse" CssClass="form-control" runat="server" ReadOnly="true">2</asp:TextBox></td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td>Completed</td>
                                                                                <td colspan="3">
                                                                                    <asp:TextBox ID="txtDICompleted" CssClass="form-control" runat="server" ReadOnly="true">2</asp:TextBox></td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td><b>Total</b></td>
                                                                                <td colspan="3">
                                                                                    <asp:TextBox ID="txtDITotal" CssClass="form-control" runat="server" ReadOnly="true" Font-Bold="true">2</asp:TextBox></td>
                                                                            </tr>
                                                                        </table>
                                                                    </div>

                                                                    <div id="Status_PV" class="tabcontent">
                                                                        <table align="center" style="width: 80%">
                                                                            <tr>
                                                                                <td>Waiting</td>
                                                                                <!--CR015-20:edit by AK for multiple level approval -->
                                                                                <td id="tdtxtPVWaiting" runat="server">
                                                                                    <asp:TextBox ID="txtPVWaiting" CssClass="form-control" runat="server" ReadOnly="true">6</asp:TextBox></td>
                                                                                <td id="tdlblPVPending" runat="server">Pending Approval</td>
                                                                                <td id="tdtxtPVPending" runat="server">
                                                                                    <asp:TextBox ID="txtPVPending" CssClass="form-control" runat="server" ReadOnly="true">10</asp:TextBox></td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td>In Use</td>
                                                                                <td colspan="3">
                                                                                    <asp:TextBox ID="txtPVInUse" CssClass="form-control" runat="server" ReadOnly="true">6</asp:TextBox></td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td>Completed</td>
                                                                                <td colspan="3">
                                                                                    <asp:TextBox ID="txtPVCompleted" CssClass="form-control" runat="server" ReadOnly="true">6</asp:TextBox></td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td><b>Total</b></td>
                                                                                <td colspan="3">
                                                                                    <asp:TextBox ID="txtPVTotal" CssClass="form-control" runat="server" ReadOnly="true" Font-Bold="true">6</asp:TextBox></td>
                                                                            </tr>
                                                                        </table>
                                                                    </div>
                                                                </div>
                                                            </td>
                                                        </tr>
                                                    </table>

                                                </ContentTemplate>
                                            </asp:UpdatePanel>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div style="width: 100%; padding-top: 15px;" runat="server" id="MonitorDiv">
                        <div class="panel-group" id="accordion3">
                            <div class="panel panel-primary">
                                <div class="panel-heading">
                                    <div class="panel-title">
                                        <a data-toggle="collapse"
                                            data-parent="#accordion3"
                                            href="#Monitor">SOD/EOD/NCF Monitor</a>
                                    </div>
                                </div>
                                <div id="Monitor" class="panel-collapse collapse in">
                                    <div class="panel-body" style="padding-top: 0; width: 100%;">
                                        <div>
                                            <table id="MonitorLabel">
                                                <tr>
                                                    <th style="width: 25%;">Branches with Start of Day</th>
                                                    <th style="width: 25%;">Branches with End of Day</th>
                                                    <th style="width: 25%;">Branches with NCF</th>
                                                    <th style="width: 25%;">Total Processing Branch</th>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Label ID="lblStartOfDay" runat="server" CssClass="label-text"></asp:Label></td>
                                                    <td>
                                                        <asp:Label ID="lblEndOfDay" runat="server" CssClass="label-text"></asp:Label></td>
                                                    <td>
                                                        <asp:Label ID="lblNCF" runat="server" CssClass="label-text"></asp:Label></td>
                                                    <td>
                                                        <asp:Label ID="lblTotalBranch" runat="server" CssClass="label-text"></asp:Label></td>
                                                </tr>
                                            </table>
                                        </div>
                                        <table id="dataTable2" class="table table-hover table-condensed table-responsive" style="width: 100% !important;">
                                            <thead class="dataTable_HeaderRow">
                                                <tr>
                                                    <th id="firstCol">Processing Branch</th>
                                                    <th class="text-center">Total Batches</th>
                                                    <th class="text-center">Empty Batches</th>
                                                    <th class="text-center">Start of Day</th>
                                                    <th class="text-center">End of Day</th>
                                                    <th class="text-center">NCF Active</th>
                                                </tr>
                                            </thead>
                                            <tbody></tbody>
                                        </table>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div style="width: 30%; float: left; padding-right: 0.2cm">
                    <div style="width: 100%;">
                        <div class="panel-group" id="accordion2">
                            <div class="panel panel-primary">
                                <div class="panel-heading">
                                    <div class="panel-title">
                                        <a data-toggle="collapse"
                                            data-parent="#accordion2"
                                            href="#Statistic">User Activity</a>
                                    </div>
                                </div>
                                <div id="Statistic" class="panel-collapse collapse in">
                                    <div class="panel-body">
                                        <div class="table-responsive">
                                            <div style="width: 100%">
                                                <div id="StatisticTab">
                                                    <table id="dataTable" style="width: 100%" class="table table-hover table-condensed">
                                                        <thead class="dataTable_HeaderRow">
                                                            <tr>
                                                                <th>Active User</th>
                                                                <th>Login Datime</th>

                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                            <% foreach (System.Data.DataRow dr in activeUsrDT.Rows)
                                                                {
                                                                    string userID = dr[0].ToString().Trim();
                                                                    string dt = dr[1].ToString().Trim();
                                                            %>
                                                            <tr>
                                                                <td><i class="fa fa-user" aria-hidden="true"></i>&nbsp; <%: userID %></td>
                                                                <td><%:dt %></td>
                                                            </tr>
                                                            <% } %>
                                                        </tbody>
                                                    </table>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

            </div>
        </div>



        <asp:PlaceHolder runat="server">
            <%: Scripts.Render("~/Tab/js") %>
            <%: Styles.Render("~/Tab/css") %>
        </asp:PlaceHolder>

        <asp:PlaceHolder runat="server">
            <%: Scripts.Render("~/DataTableGrid/js") %>
        </asp:PlaceHolder>

        <script type="text/javascript" class="init">
            $(document).ready(function () {
                $('div.fg-toolbar').hide();
                $("#<%= txtDateTime.ClientID %>").on("focusout", function () {
                    var selectedDate = $("#<%= txtDateTime.ClientID %>").val();
                    var client = $("#<%= ddlClient.ClientID %>").val();
                    initMonitorDataTable(client, selectedDate);
                    $('div.fg-toolbar').hide();
                });

                //$('div#dataTable2_filter').hide();
                Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(
                    function () {
                        if (navigator.appVersion.indexOf("MSIE") != -1)
                            $('select').ieExpandSelectWidth();

                        //Dialog
                        var redirectURL;
                        $("#<%: txtDateTime.ClientID %>").datepicker(
                            {
                                autoclose: true,
                                changeMonth: true,
                                changeYear: true
                            });//.datepicker("setDate", new Date());


                        $("#<%: txtDateTime.ClientID %>").attr("readonly", true);

                        $("#<%= txtDateTime.ClientID %>, #<%= ddlClient.ClientID %>").on("change", function () {
                            var selectedDate = $("#<%= txtDateTime.ClientID %>").val();
                                            var client = $("#<%= ddlClient.ClientID %>").val();
                                            initMonitorDataTable(client, selectedDate);
                                            $('div.fg-toolbar').hide();
                                        });

                        if (document.getElementById("allType") != null)
                            document.getElementById("allType").click();

                    }
                );

                var prm = Sys.WebForms.PageRequestManager.getInstance();

                prm.add_endRequest(function () {
                    if (window.hasRunMonitorTable) {
                        return;
                    }

                    window.hasRunMonitorTable = true; // Mark as executed

                    setTimeout(() => {
                        window.hasRunMonitorTable = false; // Reset after execution
                    }, 500); // Adjust delay if needed

                    var selectedDate = $("#<%= txtDateTime.ClientID %>").val();
                    var client = $("#<%= ddlClient.ClientID %>").val();

                    initMonitorDataTable(client, selectedDate);
                    $('div.fg-toolbar').hide();
                });
            });
        </script>

        <script>
            function loadRejStatus(evt, clickedID) {
                var i, tabcontent, tablinks;
                tabcontent = document.getElementsByClassName("tabcontent");
                for (i = 0; i < tabcontent.length; i++) {
                    tabcontent[i].style.display = "none";
                }
                tablinks = document.getElementsByClassName("tablinks");
                for (i = 0; i < tablinks.length; i++) {
                    tablinks[i].className = tablinks[i].className.replace(" active", "");
                }
                document.getElementById(clickedID).style.display = "block";
                evt.currentTarget.className += " active";
            }

            function showMonitorDiv() {
                document.getElementById('<%= MonitorDiv.ClientID %>').style.display = 'block';  // Show the div
            }

            function hideMonitorDiv() {
                document.getElementById('<%= MonitorDiv.ClientID %>').style.display = 'none';  // Hide the div
            }

            function updateLabels() {
                $.ajax({
                    type: "POST",
                    url: "WebService.asmx/UpdateLabels",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (response) {
                        var data = JSON.parse(response.d);

                        var startOfDayLabel = '<%= lblStartOfDay.ClientID %>';
                        var endOfDayLabel = '<%= lblEndOfDay.ClientID %>';
                        var ncfLabel = '<%= lblNCF.ClientID %>';
                        var totalBranchLabel = '<%= lblTotalBranch.ClientID %>';

                        $("#" + startOfDayLabel).text(data.StartOfDay);
                        $("#" + endOfDayLabel).text(data.EndOfDay);
                        $("#" + ncfLabel).text(data.NCF);
                        $("#" + totalBranchLabel).text(data.TotalBranches);
                    },
                    error: function (error) {
                        console.log("Error calling ASPX method:", error);
                    }
                });
            }

            if (document.getElementById("allType") != null)
                document.getElementById("allType").click();

        </script>

    </form>
</asp:Content>
