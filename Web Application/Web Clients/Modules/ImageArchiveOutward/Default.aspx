<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb.Modules.ImageArchiveOutward.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">

    <form runat="server">
        <link href="../../Content/toggle.css" rel="stylesheet" type="text/css" />
        <div class="content">
            <ul class="breadcrumb">
                <li>
                    <p>Operator Tasks</p>
                </li>
                <li><a href="/ImageArchiveOutward" class="active">Image Archive Outward</a></li>
            </ul>
            <input type="hidden" id="ImageArchiveModule" value="ImageArchiveOutward" />
            <div class="page-title">
                <i class="icon-custom-right"></i>
                <h3><span class="semi-bold">Image Archive - Outward Clearing</span></h3>
                <div>
                    <asp:Label ID="dateErrorMsg" runat="server" Text="*Date Out of range. Please choose a date within one month." Style="display: none" Font-Bold="True" Font-Size="Medium" ForeColor="Red"></asp:Label>
                </div>
            </div>
            <div class="row-fluid">
                <div class="span12">
                    <div class="grid simple">
                        <div class="grid-title">
                            <div class="row">
                                <div class="col-md-2">
                                    <div class="form-group">
                                        <label class="form-label">Client Code:</label>
                                        <asp:DropDownList ID="ddlClient" runat="server" CssClass="form-control" OnSelectedIndexChanged="ddlClient_SelectedIndexChanged">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="form-group">
                                        <label class="form-label">MICR Account Number:</label>
                                        <asp:TextBox ID="txtMicrAccNum" runat="server" style="width:100%" MaxLength="12" onkeypress="return onlyNumbers(event);"></asp:TextBox>
                                    </div>

                                </div>

                                <div class="col-md-2">
                                    <div class="form-group">
                                        <label class="form-label"><span style="color: red;">*</span>Business Date:</label>
                                        <asp:TextBox ID="txtDateTime" runat="server" CssClass="form-control"></asp:TextBox>
                                        <span id="Date_Required" class="help" style="display: none; color: red;">*Required</span>
                                    </div>

                                    <div class="form-group">
                                        <label class="form-label">MICR BSB:</label>
                                        <asp:TextBox ID="txtMicrBSB" runat="server" style="width:100%" MaxLength="12" onkeypress="return onlyNumbers(event);"></asp:TextBox>
                                    </div>
                                </div>

                                <div class="col-md-2">
                                    <%--<div class="form-group">
                                        <label class="form-label"><span style="color: red;">*</span>Business Date (To):</label>
                                        <asp:TextBox ID="txtDateTimeTo" runat="server" CssClass="form-control"></asp:TextBox>
                                        <span id="Date_Required_To" class="help" style="display: none; color: red;">*Required</span>
                                    </div>--%>
                                    <div class="form-group">
                                        <label class="form-label">ItemType:</label>
                                        <asp:DropDownList ID="ddlItemType" runat="server" CssClass="form-control" OnSelectedIndexChanged="ddlItemType_SelectedIndexChanged">
                                        </asp:DropDownList>
                                    </div>

                                    <div class="form-group">
                                        <label class="form-label">Cheque Number:</label>
                                        <asp:TextBox ID="txtChequeNum" runat="server" Style="width: 100%" MaxLength="12" onkeypress="return onlyNumbers(event);"></asp:TextBox>
                                    </div>
                                </div>

                                <div class="col-md-2">
                                    <div class="form-group">
                                        <label class="form-label">Depositor Account:</label>
                                        <asp:TextBox ID="txtDepositorAcc" runat="server" Style="width: 100%" MaxLength="20" onkeypress="return onlyNumbers(event);"></asp:TextBox>
                                    </div>

                                    <div class="form-group">
                                        <label class="form-label">Presenting BSB:</label>
                                        <asp:TextBox ID="txtPresentingBSB" runat="server" Style="width: 100%" MaxLength="12" onkeypress="return onlyNumbers(event);"></asp:TextBox>
                                    </div>
                                </div>

                                <div class="col-md-2">
                                    <div class="form-group" style="visibility: hidden">
                                        <label class="form-label">Presenting BSB:</label>
                                        <asp:TextBox ID="TextBox1" Style="width: 100px!important;" runat="server" MaxLength="12" onkeypress="return onlyNumbers(event);"></asp:TextBox>
                                    </div>

                                    <div class="form-group" style="margin-top: -3%">
                                        <div class="form-container">
                                            <span class="form-label">Amount:
                                            </span>
                                            <asp:RadioButtonList ID="rblAmountType" runat="server" RepeatDirection="Horizontal" OnSelectedIndexChanged="rblAmountType_SelectedIndexChanged">
                                                <asp:ListItem Value="smaller" style="text-align: center; display: flex; align-content: center; margin-right: 7px" Selected="True">
                                                    <span style="margin-top:8px; display:flex">&lt;=</span>
                                                </asp:ListItem>
                                                <asp:ListItem Value="fixed" style="text-align: center; display: flex; align-content: center; margin-right: 7px">
                                                    <span style="margin-top:8px; display:flex">=</span>
                                                </asp:ListItem>
                                                <asp:ListItem Value="larger" style="text-align: center; display: flex; align-content: center; margin-right: 7px">
                                                    <span style="margin-top:8px; display:flex">&gt;=</span>
                                                </asp:ListItem>
                                            </asp:RadioButtonList>
                                        </div>
                                        <div id="divFixedAmount" runat="server">
                                            <asp:TextBox ID="txtAmount" runat="server" style="width:100%" MaxLength="14" onkeypress="return onlyDecimal(event);" oninput="enforceTwoDecimalPlaces(event)"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>

                                <div class="col-md-2">
                                    <div class="form-group" style="visibility: hidden">
                                        <label class="form-label">Presenting BSB:</label>
                                        <asp:TextBox ID="TextBox2" Style="width: 100px!important;" runat="server" MaxLength="12" onkeypress="return onlyNumbers(event);"></asp:TextBox>
                                    </div>

                                    <div class="form-group">
                                        <label class="form-label" style="visibility:hidden; width:100%">Presenting BSB:</label>

                                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-success" Style="margin-right: 5%; width:70px" OnClick="btnSearch_Click" />
                                        <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-success" Style="width:70px" onclick="btnClear_Click" />
                                        <asp:HiddenField ID="hdnClearFlag" runat="server" Value="false" />
                                        <%--<button id="btnClear" type="button" class="btn btn-success" style="width:70px" onclick="clearControls();">Clear</button>--%>
                                    </div>
                                </div>

                                <div class="col-md-2">
                                </div>
                            </div>


                        </div>
                        <div id="bodyContainer" runat="server" class="grid-body">
                            <table id="dataTable" class="table table-hover table-condensed">
                                <thead>
                                    <tr class="dataTable_HeaderRow">
                                        <%--<th>Bus Date</th>--%>
                                        <th>
                                            <div class="checkbox-container" style="margin-left:3.2px">
                                            <input type="checkbox" id="select-all" class="enlarged-checkbox"></div>
                                        </th>
                                        <th>Batch No</th>
                                        <th>Type</th>
                                        <th>CHQ NO</th>
                                        <th>Chq BSB</th>
                                        <th>Account No</th>
                                        <th>Tran Code</th>
                                        <th>Amount</th>
                                        <th>DEP. A/C</th>
                                        <th>Trans</th>
                                        <th></th>
                                    </tr>
                                </thead>
                                <tbody></tbody>
                            </table>
                        </div>
                        <div id="footerContainer" runat="server" class="grid-body" visible="false">
                            <asp:DropDownList ID="ddlPrintRejected" runat="server" CssClass="form-control" Style="display: inline-block; width: auto;">
                            </asp:DropDownList>
                            <asp:LinkButton runat="server" ID="btnPrintRejected" Text="<i class='fa fa fa-print'></i> Print Rejected " CssClass="btn btn-success no-drag" Style="display: inline-block; margin-left: 10px" OnClick="btnPrintRejected_Click" />
                            <asp:Label ID="NoDataMsg" runat="server" Text="No Record Found." Style="display: none; margin-left:10px"  Font-Bold="True" Font-Size="small" ForeColor="Red"></asp:Label>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <asp:PlaceHolder runat="server">
            <%: Scripts.Render("~/DataTableGrid/js") %>
        </asp:PlaceHolder>

        <script type="text/javascript" class="init">
            $('#select-all').click(function () {

                var isChecked = $(this).is(':checked');
                $('.file-checkbox').prop('checked', isChecked);
            });

            $('#dataTable').on('draw.dt', function () {
                $('.file-checkbox').prop('checked', false);
                $('#select-all').prop('checked', false);
            });

            $(document).ready(function () {

                if (navigator.appVersion.indexOf("MSIE") != -1)
                    $('select').ieExpandSelectWidth();

                // Initialize date pickers
                var date = $("#<%= txtDateTime.ClientID %>");
                var searchButton = $("#<%= btnSearch.ClientID %>");

                date.datepicker({
                    autoclose: true,
                    changeMonth: true,
                    changeYear: true,
                    format: 'dd/mm/yyyy'
                }).on("changeDate", function (e) {
                    //// Set the min date for the end date picker
                    //var startDate = date.datepicker('getDate');
                    //var endDate = new Date(startDate);
                    //endDate.setMonth(endDate.getMonth() + 1);
                    //validateDateRange();
                });

                //date.attr("readonly", true);
                //toDate.attr("readonly", true);

                function validateDateRange() {
                    var fromDateVal = fromDate.val();
                    var toDateVal = toDate.val();

                    if (fromDateVal && toDateVal) {
                        var fromDateParts = fromDateVal.split('/');
                        var toDateParts = toDateVal.split('/');

                        var fromDateObj = new Date(fromDateParts[2], fromDateParts[1] - 1, fromDateParts[0]);
                        var toDateObj = new Date(toDateParts[2], toDateParts[1] - 1, toDateParts[0]);

                        var maxEndDate = new Date(fromDateObj);
                        maxEndDate.setMonth(maxEndDate.getMonth() + 1);

                        if (toDateObj > maxEndDate || toDateObj < fromDateObj) {
                            // Show error message
                            $("#<%= dateErrorMsg.ClientID %>").text("*Date Out of range. Please choose a date within one month.").css('display', 'block');
                            toDate.datepicker('hide');
                            searchButton.prop('disabled', true);
                        }
                        else {
                            // Hide error message if valid
                            $("#<%= dateErrorMsg.ClientID %>").css('display', 'none');
                            searchButton.prop('disabled', false);
                        }
                    }
                }
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

        <script type="text/javascript">

            function openCR() {
                var urlNet = setImageArchivalPrintRptURLOutward2();
                var option = 'height=' + screen.availHeight + ', width=' + screen.availWidth + ',scrollbars=1,resizable=1,top=0,left=0';
                myWindow = window.open(urlNet, '_blank', option);
                myWindow.resizeTo(window.screen.availWidth, window.screen.availHeight);
                myWindow.focus();
            }

            function setClearFlag(button) {
                var hiddenField = document.getElementById('hdnClearFlag');
                if (hiddenField) {
                    hiddenField.value = 'true';
                }
            }

            function clearControls() {
                var txtMicrAccNum = document.getElementById('<%= txtMicrAccNum.ClientID %>');
                if (txtMicrAccNum) {
                    txtMicrAccNum.value = '';
                }
                var txtMicrBSB = document.getElementById('<%= txtMicrBSB.ClientID %>');
                if (txtMicrBSB) {
                    txtMicrBSB.value = '';
                }
                var txtChequeNum = document.getElementById('<%= txtChequeNum.ClientID %>');
                if (txtChequeNum) {
                    txtChequeNum.value = '';
                }
                var txtPresentingBSB = document.getElementById('<%= txtPresentingBSB.ClientID %>');
                if (txtPresentingBSB) {
                    txtPresentingBSB.value = '';
                }
                var txtDepositorAcc = document.getElementById('<%= txtDepositorAcc.ClientID %>');
                if (txtDepositorAcc) {
                    txtDepositorAcc.value = '';
                }
                var txtAmount = document.getElementById('<%= txtAmount.ClientID %>');
                if (txtAmount) {
                    txtAmount.value = '';
                }

                var rblAmountType = document.getElementById('<%= rblAmountType.ClientID %>');
                if (rblAmountType) {
                    var fixedOption = rblAmountType.querySelector('input[value="smaller"]');
                    if (fixedOption) {
                        fixedOption.checked = true;
                    }
                }


                //clear datepicker datetime from
                var txtDateTime = document.getElementById('<%= txtDateTime.ClientID %>');
                if (txtDateTime) {
                    var today = new Date();
                    var dd = String(today.getDate()).padStart(2, '0');
                    var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
                    var yyyy = today.getFullYear();

                    todayFormatted = dd + '/' + mm + '/' + yyyy;
                    txtDateTime.value = todayFormatted;
                    var datepicker = $("#<%= txtDateTime.ClientID %>");
                    datepicker.datepicker("update");
                }

                // Clear DropDownList
                var ddlClient = document.getElementById('<%= ddlClient.ClientID %>');
                if (ddlClient) {
                    ddlClient.selectedIndex = 0;
                }

                // Clear DropDownList
                var ddlItemType = document.getElementById('<%= ddlItemType.ClientID %>');
                if (ddlItemType) {
                    ddlItemType.selectedIndex = 0;
                }

                // Ensure date validation error message is hidden and search button is enabled
                $("#<%= dateErrorMsg.ClientID %>").css('display', 'none');
                $("#<%= btnSearch.ClientID %>").prop('disabled', false);

            }


            function onlyDecimal(event) {
                var charCode = (event.which) ? event.which : event.keyCode;
                if (charCode != 46 && charCode > 31
                    && (charCode < 48 || charCode > 57)) {
                    return false;
                }
                var inputValue = event.target.value;
                var dotCount = (inputValue.match(/\./g) || []).length;
                if (dotCount > 0 && charCode == 46) {
                    return false;
                }
                return true;
            }

            function enforceTwoDecimalPlaces(event) {
                var value = event.target.value;
                if (value.includes('.')) {
                    var decimalPart = value.split('.')[1];
                    if (decimalPart.length > 2) {
                        event.target.value = parseFloat(value).toFixed(2);
                    }
                }
            }
        </script>
    </form>
</asp:Content>













