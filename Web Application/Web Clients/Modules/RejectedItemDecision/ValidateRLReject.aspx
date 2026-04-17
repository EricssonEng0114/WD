<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="ValidateRLReject.aspx.cs" Inherits="UBPCWeb.Modules.RejectedItemDecision.ValidateRLReject" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">
    <link href="../../Content/pdsa-collapser.css" rel="stylesheet" />
    <script src="../../Scripts/pdsa-collapser.js"></script>
<form runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <asp:PlaceHolder runat="server">       
        <%: Styles.Render("~/CViewer/css") %>    
        <%: Scripts.Render("~/CViewer/js") %>
    </asp:PlaceHolder>

    <div class="content">     
          <!-- Navigation -->
        <ul class="breadcrumb">
            <li><p><b>Operator Tasks</b></p></li>
            <li><b>Rejected Item Decision - Validate Transaction</b></li>
        </ul>

        <div class="">
            <div class="grid-body ">
               
                <!-- 1st Row: Show Reason Titl and Back to Listing Button -->
                <div style="width: 100%; padding-right: 15px">
                    <div style="width: 60%; float: left;">
                        <!-- Title Shows Reject Reason-->
                        <div style="width: 99%; float: left; padding-right: 15px">
                            <div class="page-title">
                                <i class="icon-custom-right"></i>
                                <h3><span class="semi-bold">
                                    <asp:Label ID="lblRejReason" runat="server" Text="REJECT REASON: RL-Amount Breakdown Not Tally" Font-Bold="True" Font-Size="Large" ForeColor="Black"></asp:Label></span></h3>
                            </div>
                        </div>
                    </div>

                    <div style="width: 40%; float: right;">
                        <!-- Back To Listing-->
                        <div style="width: 100%; float: left; padding: 1px; margin-bottom: 12px;">
                            <asp:LinkButton runat="server" ID="btnBackToListing" Text="<i class='fa fa fa-backward'></i> Back To Listing" CssClass="btn btn-primary" Style="float: right" OnClick="btnBackToListing_Click" />
                        </div>
                    </div>
                </div>

                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <!-- 2nd Row: Show Transaction Info & Stub Cheque Total -->
                        <div style="width: 100%; padding-right: 15px">
                            <!--  Transaction info  -->
                            <div style="width: 60%; float: left;">
                                <!--  Transaction info  -->
                                <div style="width: 100%; float: right; padding-right: 15px">
                                    <div style="width: 100%; float: right;">
                                        <div class="panel-group" id="accordion1">
                                            <div class="panel panel-primary">
                                                <div class="panel-heading">
                                                    <div class="panel-title">
                                                        <a data-toggle="collapse"
                                                            data-parent="#accordion1"
                                                            href="#transInfo">Transaction Info</a>
                                                        <a class="pdsa-panel-toggle"></a>
                                                    </div>
                                                </div>
                                                <div id="transInfo" class="panel-collapse collapse in">
                                                    <div class="panel-body">
                                                        <div class="table-responsive">
                                                            <div style="width: 100%">
                                                                <table style="width: 100%; table-layout: fixed;">
                                                                    <tr>
                                                                        <td colspan="4">
                                                                            <asp:Label Font-Bold="True" ID="lblMode" runat="server" Text="CHEQUE ONLY MODE" Font-Size="Medium"></asp:Label> 
                                                                             &nbsp; 
                                                                            (<asp:Label ID="lblAllowBD" runat="server" Text="Allow Breakdown:No" Font-Bold="True"></asp:Label>                                                                            
                                                                            <asp:Label ID="lblAllowTol" runat="server" Text=", Allow Tolerance:No" Font-Bold="True"></asp:Label>)
                                                                           </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="4" style="border-bottom: dotted; padding-bottom: 15px;"></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="4" style="padding-top: 15px;"></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>Business Date:
                                                                            <asp:Label ID="lblBusdate" runat="server" Text=""></asp:Label></td>
                                                                        <td align="center">Transaction No.:
                                                                            <asp:Label ID="lblTransNo" runat="server" Text=""></asp:Label></td>
                                                                        <td>Batch Directory:
                                                                            <asp:Label ID="lblBatchDir" runat="server" Text=""></asp:Label></td>
                                                                        <td align="center">Batch No.:
                                                                            <asp:Label ID="lblBatchNo" runat="server" Text=""></asp:Label></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="4" style="padding-bottom: 8px;"></td>
                                                                    </tr>
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
                            <div style="width: 40%; float: left;">
                                <div style="width: 100%; float: right;">
                                    <div style="width: 100%; float: right;">
                                        <div class="panel-group" id="accordion2">
                                            <div class="panel panel-primary">
                                                <div class="panel-heading">
                                                    <div class="panel-title">
                                                        <a data-toggle="collapse"
                                                            data-parent="#accordion2"
                                                            href="#scInfo">Stub & Cheque Total</a>
                                                        <a class="pdsa-panel-toggle"></a>
                                                    </div>
                                                </div>
                                                <div id="scInfo" class="panel-collapse collapse in">
                                                    <div class="panel-body">
                                                        <div class="table-responsive">


                                                            <div style="width: 100%; margin-top: 1px">
                                                                <table style="width: 100%; table-layout: fixed;">
                                                                    <tr>
                                                                        <td style="width: 48%">
                                                                            <asp:Label ID="lblTotalStubCount" runat="server" Text="Total Stub Count : "></asp:Label></td>
                                                                        <td style="width: 45%">
                                                                            <asp:Label ID="lblTotalChequeCount" runat="server" Text="Total Cheque Count : "></asp:Label></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td colspan="2" style="padding-bottom: 8px;"></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td style="width: 48%; padding-right: 15px">Total Stub Amount:</td>
                                                                        <td style="width: 45%">Total Cheque Amount:</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td style="width: 48%; padding-right: 15px">
                                                                            <asp:TextBox ID="txtTotalStubAmt" runat="server" CssClass="form-control" ReadOnly="true">0.00</asp:TextBox>
                                                                        </td>
                                                                        <td style="width: 45%;">
                                                                            <asp:TextBox ID="txtTotalChequeAmt" runat="server" CssClass="form-control" ReadOnly="true">0.00</asp:TextBox>
                                                                        </td>
                                                                    </tr>
                                                                    
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

                        <!-- 3rd Row: Show Image & Item Amendment -->
                        <div style="width: 100%; padding-right: 15px">
                            <!--  Transaction Image  -->
                            <div style="width: 100%; float: right;">
                                <div style="width: 100%; float: right;">
                                    <div class="panel-group" id="accordion3">
                                        <div class="panel panel-primary">
                                            <div class="panel-heading">
                                                <div class="panel-title">
                                                    <a data-toggle="collapse"
                                                        data-parent="#accordion3"
                                                        href="#scDetailInfo">Stub & Cheque Detail Info</a>
                                                    <a class="pdsa-panel-toggle"></a>
                                                </div>
                                            </div>
                                            <div id="scDetailInfo" class="panel-collapse collapse in">
                                                <div class="panel-body">
                                                    <div class="table-responsive">
                                                        <div style="width: 100%">
                                                            <!-- 3Rd Row: Show Image and Table Selector -->
                                                            <div style="width: 100%;">
                                                                <!--  Left Content : Image  -->
                                                                <div style="width: 60%; float: left;">
                                                                    <!--  Top Image  -->
                                                                    <div style="width: 99%; float: left; padding-right: 15px">
                                                                        <!--Top Item-->
                                                                        <div style="background: #F7F9F9; width: 100%;" id="topImage">
                                                                            <div>
                                                                                <div class="wrapper">
                                                                                    <div id="RLImgViewerTop" class="viewerC" style="width: 100%"></div>
                                                                                </div>
                                                                            </div>
                                                                            <div style="background: #F7F9F9; width: 100%; float: right; border: 1px solid black; padding: 8px; margin-bottom: 20px">
                                                                                <button type="button" id="topImgFlip" class="btn btn-link" title="Flip"><i class="fa fa fa-retweet fa-2x"></i></button>
                                                                                <button type="button" id="topImgToggle" class="btn btn-link" title="Toggle JPEG/TIFF"><i class="fa fa fa-eye-slash fa-2x"></i></button>

                                                                            </div>
                                                                        </div>

                                                                        <!--bsb/account & Amount-->
                                                                        <div style="background: #F7F9F9; width: 100%; float: left; border: 1px solid black; padding: 8px; margin-bottom: 20px">
                                                                            <table>
                                                                                 <tr>
                                                                                    <td colspan="2">
                                                                                        <asp:Label ID="lblRejReasonForStub" runat="server" Text="" ForeColor="Red" Font-Bold="true"></asp:Label>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="width: 70%">
                                                                                        <asp:Label ID="lblTopItem1" runat="server" Text="Depositor Account No:"></asp:Label>
                                                                                        <asp:TextBox ID="txtTopItem1" runat="server" CssClass="form-control" Style="text-align: left" MaxLength="20" onkeypress="return onlyNumbers(event);" OnTextChanged="txtTopItem1_TextChanged" AutoPostBack="true"></asp:TextBox></td>
                                                                                    <td style="width: 25%; padding-left: 20px">Stub Amount:
                                                                                        <asp:TextBox ID="txtTopItemAmount" runat="server" CssClass="form-control" Style="text-align: right" onkeypress="return onlyDotsAndNumbers(event);" MaxLength="15" OnTextChanged="txtTopItemAmount_TextChanged" AutoPostBack="true">0.00</asp:TextBox>
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </div>

                                                                        <br />
                                                                       

                                                                    </div>

                                                                      <!--  Bottom Image  -->
                                                                    <div style="width: 99%; float: left; padding-right: 15px">
                                                                        <!--Bottom Item-->
                                                                        <div style="background: #F7F9F9; width: 100%;" id="bottomImage">
                                                                            <div>
                                                                                <div class="wrapper">
                                                                                    <div id="RLImgViewerBottom" class="viewerC" style="width: 100%"></div>
                                                                                </div>
                                                                            </div>
                                                                            <div style="background: #F7F9F9; width: 100%; float: right; border: 1px solid black; padding: 8px; margin-bottom: 20px">
                                                                                <button type="button" id="bottomImgFlip" class="btn btn-link" title="Flip"><i class="fa fa fa-retweet fa-2x"></i></button>
                                                                                <button type="button" id="bottomImgToggle" class="btn btn-link" title="Toggle JPEG/TIFF"><i class="fa fa fa-eye-slash fa-2x"></i></button>

                                                                            </div>
                                                                        </div>

                                                                        <!--cheque bsb & Amount-->
                                                                        <div style="background: #F7F9F9; width: 100%; float: left; border: 1px solid black; padding: 8px; margin-bottom: 20px">
                                                                            <table>
                                                                                 <tr>
                                                                                    <td colspan="2">
                                                                                        <asp:Label ID="lblItemRejReason" runat="server" Text="" ForeColor="Red" Font-Bold="true"></asp:Label>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="width: 70%">
                                                                                        <asp:Label ID="lblBottom" runat="server" Text="Cheque BSB:"></asp:Label>
                                                                                        <asp:TextBox ID="txtBottomBSB" runat="server" CssClass="form-control" Style="text-align: left" MaxLength="20" onkeypress="return onlyNumbers(event);" OnTextChanged="txtTopItem1_TextChanged" AutoPostBack="true"></asp:TextBox></td>
                                                                                    <td style="width: 25%; padding-left: 20px">Cheque Amount:
                                                                                        <asp:TextBox ID="txtBottomAmount" runat="server" CssClass="form-control" Style="text-align: right" onkeypress="return onlyDotsAndNumbers(event);" OnTextChanged="txtTopItemAmount_TextChanged" AutoPostBack="true" MaxLength="15">0.00</asp:TextBox></td>
                                                                                </tr>
                                                                            </table>
                                                                        </div>

                                                                        <br />
                                                                       

                                                                    </div>
                                                                </div>

                                                                <!--  Right Content : Table Selector  -->
                                                                <div style="width: 40%; float: right;">

                                                                    <!--  Selected Account Name Info  -->
                                                                    <div style="width: 100%; float: right;">
                                                                        <div style="width: 100%">
                                                                            <div class="panel-title" style="background-color: #007ACC; width: 90%; padding: 10px; border: 0px solid; border-radius: 10px 10px 0px 0px">
                                                                                <asp:Label ID="Label3" runat="server" Text="Select Stub Item at following list <i class='fa fa fa-level-down'></i>" Font-Bold="True" Font-Size="Medium" ForeColor="White"></asp:Label>
                                                                            
                                                                                <div style="float:right">
                                                                                    <asp:ImageButton class="signup" ID="btnAddStub" ToolTip="Add Stub" ImageUrl="../../images/plus24.png"  runat="server" Visible="False" OnClick="btnAddStub_Click" BorderStyle="None"/>&nbsp
                                                                                    <asp:ImageButton class="signup" ID="btnDelStub" ToolTip="Delete Stub" ImageUrl="../../images/minus24.png"  runat="server" Visible="False" OnClick="btnDelStub_Click" BorderStyle="None"/>
                                                                                </div>
                                                                                 </div>
                                                                            <div style="width: 100%; float: left">
                                                                                <asp:ListBox ID="LstBSelectItem" runat="server" CssClass="form-control"
                                                                                    Font-Names="Lucida Console,Courier New" Font-Bold="true" OnSelectedIndexChanged="ListBox_SelectedIndexChanged"
                                                                                    Rows="26" Style="font-size: 17px; width: 90%; border: 1px solid groove" AutoPostBack="true">
                                                                                </asp:ListBox>
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
                            </div>
                        </div>

                  

                <!-- 3rd Row: Show Action Taken -->
                <div style="width: 100%; padding-right: 15px">
                    <div style="width: 100%; float: left;">
                        <div style="width: 100%; float: left;">
                            <div class="panel-group" id="accordionF">
                                <div class="panel panel-primary">
                                    <div class="panel-heading">
                                        <div class="panel-title">
                                            <a data-toggle="collapse"
                                                data-parent="#accordionF"
                                                href="#actionTab">Action</a>
                                            <a class="pdsa-panel-toggle"></a>
                                        </div>
                                    </div>
                                    <div id="actionTab" class="panel-collapse collapse in">
                                        <div class="panel-body">
                                            <div class="table-responsive">
                                                <div style="width: 85%; float: left">
                                                    <asp:Label ID="lblRemark" runat="server" Text="Additional Remark(s):" ForeColor="Black"></asp:Label>
                                                    <textarea id="txtARemark" maxlength="1000" rows="4" runat="server" class="form-control" style="width: 100%"></textarea>
                                                </div>
                                                <div style="width: 12%; float: right">
                                                    <asp:LinkButton runat="server" ID="btnAccept" Text="<i class='fa fa fa-check'></i>&nbsp ACCEPT " CssClass="btn btn-success"  OnClick="btnAccept_Click" Visible="False"/>
                                                     <asp:LinkButton runat="server" ID="btnConfirmAccept" Text=""   OnClick="btnConfirmAccept_Click" Visible="True"/>

                                                    <br />
                                                    <br />
                                                    <input type="hidden" id="isbpc" name="isbpc" value="<%: Session["isBatchBPC"].ToString() %>">

                                                    <asp:LinkButton runat="server" ID="btnReject" Text="<i class='fa fa fa-times'></i>&nbsp  REJECT &nbsp" CssClass="btn btn-danger" OnClick="btnReject_Click"  OnClientClick="return confirmReject();"/>
                                               
                                                    
                                                     </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                  </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>

    </div>
   
       
<script type="text/javascript">
    var $ = jQuery;

    $(document).ready(function () {
        $("#topImgToggle").off('click').on('click', function () {
            toggleImage("RL", "TOP", "#RLImgViewerTop");
        });

        $("#topImgFlip").off('click').on('click', function () {
            flipImage("RL", "TOP", "#RLImgViewerTop");
        });

        $("#bottomImgToggle").off('click').on('click', function () {
            toggleImage("RL", "BOT", "#RLImgViewerBottom");
        });

        $("#bottomImgFlip").off('click').on('click', function () {
            flipImage("RL", "BOT", "#RLImgViewerBottom");
        });

        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(
          function () {

              //hide button


              LoadTopImage();
              LoadBottomImage();

              $("#topImgToggle").off('click').on('click', function () {
                  toggleImage("RL", "TOP", "#RLImgViewerTop");
              });

              $("#topImgFlip").off('click').on('click', function () {
                  flipImage("RL", "TOP", "#RLImgViewerTop");
              });

              $("#bottomImgToggle").off('click').on('click', function () {
                  toggleImage("RL", "BOT", "#RLImgViewerBottom");
              });

              $("#bottomImgFlip").off('click').on('click', function () {
                  flipImage("RL", "BOT", "#RLImgViewerBottom");
              });
          }
        );
    });


</script>
    
<script type="text/javascript">
    function onKeyPressFormat(evt, txt) {
        txt.value = txt.value.replace(".", "00");

        return onlyDotsAndNumbers(evt);
    }

    function onBlurRun(txt) {
        formatCurr(txt);
        <%--        __doPostBack('<%= UpdatePanel1.UniqueID %>');//, txt.name);--%>


        //txtTopItemAmount_TextChanged
        <%--        __doPostBack('<%= txtTopItemAmount.ClientID %>', 'TextChanged');  --%>

<%--        __doPostBack('<%= UpdatePanel1.UniqueID %>', 'RefreshListBoxAndCalTotalAmt');  --%>

    }


    function setValidNavigation() {
        validNavigation = true;
    }

    function unloadSpin() {
        isLoadSpinner = false;
        unloadSpinner();

    }

    function LoadTopImage() {

        var randomId = GenerateGuid();
        //  alert(randomId);
        LoadImage("/ImageHandlerTop.ashx?" + randomId, "#RLImgViewerTop");
    }

    function LoadBottomImage() {
        var randomId = GenerateGuid();
        //  alert(randomId);
        LoadImage("/ImageHandlerBottom.ashx?" + randomId, "#RLImgViewerBottom");
    }

    function padleft(len, pad) {
        if (len === undefined) {
            len = 1;
        } else if (pad === undefined) {
            pad = '0';
        }

        var pads = '';
        while (pads.length < len) {
            pads += pad;
        }

        this.pad = function (what) {
            var s = what.toString();
            return pads.substring(0, pads.length - s.length) + s;
        };
    }

    function confirmAccept() {

        jQuery("#dialog-confirmbox").text("Transaction will be accepted. Are you sure to proceed?");

        jQuery("#dialog-confirmbox").dialog(
        {
            modal: true,
            buttons: {
                "Ok": function () {
                    validNavigation = true;
                    $(this).dialog("close");
                    //call to ajax to update session value as accepted,if return true then only call btnAccept again

                        <%=Page.ClientScript.GetPostBackEventReference(btnConfirmAccept, "") %>
                    },
                    "Cancel": function () {
                        validNavigation = true;
                        $(this).dialog("close");
                        return false;
                    }
                }
            });

            return false;
        }

        function confirmReject() {

            var str = "";
            if (document.getElementById('isbpc').value.toUpperCase() == "TRUE") {
                str = "This BPC Transaction will be Rejected. Are you sure to proceed?";
            }
            else{
                str = "Transaction will be Rejected. Are you sure to proceed?";
            }


            jQuery("#dialog-confirmbox").text(str);

            jQuery("#dialog-confirmbox").dialog(
            {
                modal: true,
                buttons: {
                    "Ok": function () {
                        validNavigation = true;
                        $(this).dialog("close");
                        <%=Page.ClientScript.GetPostBackEventReference(btnReject, "") %>
                    },
                    "Cancel": function () {
                        validNavigation = true;
                        $(this).dialog("close");
                        return false;
                    }
                }
            });

            return false;
        }

        function alertBox(msg) {
            jQuery("#dialog-alertbox").text(msg);

            jQuery("#dialog-alertbox").dialog(
            {
                modal: true,
                buttons: {
                    "Ok": function () {
                        $(this).dialog("close");
                    }
                }
            });

            return false;
        }

        function checkAccept() {
            var totalStubAmount = $("#<%: txtTotalStubAmt.ClientID %>").val();
            var totalChqAmount = $("#<%: txtTotalChequeAmt.ClientID %>").val();

            if (totalStubAmount == totalChqAmount) {
                var depAcctNo = $("#<%: txtTopItem1.ClientID %>").val();
                var bsb = $("#<%: txtBottomBSB.ClientID %>").val();

                if (depAcctNo.length > 0) {
                    if (bsb.length > 0) {
                        return true;
                    }
                    else {
                        alert("Please enter valid cheque BSB.")
                        return false;
                    }
                }
                else {
                    alert("Please enter valid depositor account number.");
                    return false;
                }
            }
            else {
                alert("Total Stub Amount is not tallied with Total Cheque Amount. Please amend accordingly.");
                return false;
            }
        }

</script>

</form>

</asp:Content>



