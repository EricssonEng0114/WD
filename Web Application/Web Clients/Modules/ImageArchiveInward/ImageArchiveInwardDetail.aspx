<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="ImageArchiveInwardDetail.aspx.cs" Inherits="UBPCWeb.Modules.ImageArchiveInward.ImageArchiveInwardDetail" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">    
<form runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <link href="../../Content/pdsa-collapser.css" rel="stylesheet" />
    <script src="../../Scripts/pdsa-collapser.js"></script>
    <link href="../../Content/toggle.css" rel="stylesheet" type="text/css" />
   <asp:PlaceHolder runat="server">       
        <%: Styles.Render("~/CViewer/css") %>    
        <%: Scripts.Render("~/CViewer/js") %>
    </asp:PlaceHolder>

    <div class="content">     
          <!-- Navigation -->
        <ul class="breadcrumb">
            <li><p><b>Operator Tasks</b></p></li>
            <li><b>Image Archive Inward - Image Information</b></li>
        </ul>

        <div class="">
            <div class="grid-body ">
                <asp:UpdatePanel ID="ajaxPanel1" runat="server">             
                    <ContentTemplate>


                        <!-- 1st Row: Show Reason and Back to Listing Button -->
                        <div style="width: 100%;">

                            <div style="width: 60%; float: left; visibility:hidden">
                                <!-- Title Shows Reject Reason-->
                                <div style="width: 99%; float: left; padding-right: 15px">
                                    <div class="page-title">
                                        <i class="icon-custom-right"></i>
                                        <h3><span class="semi-bold">
                                            <asp:Label ID="lblRejReason" runat="server" Text="REJECT REASON: RL-Amount Breakdown Not Tally" Font-Bold="True" Font-Size="Large" ForeColor="Black"></asp:Label></span></h3>
                                    </div>
                                </div>
                            </div>

                           <div style="width: 40%; float: right; display: flex; gap:2%; padding-right:20px">
                               <!-- Print Button-->
                               <div style="width: 100%; float: left; padding: 1px; margin-bottom: 12px;">
                                   <asp:LinkButton runat="server" ID="btnPrint" Text="<i class='fa fa fa-print'></i> Print" CssClass="btn btn-primary no-drag" Style="float: right" OnClick="btnPrintTransaction_Click"  OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"/>
                               </div>
                               <!-- Back To Listing-->
                               <div style="float: left; padding: 1px; margin-bottom: 12px">
                                   <asp:LinkButton runat="server" ID="btnBackToListing" Text="<i class='fa fa fa-backward'></i> Back To Listing" CssClass="btn btn-primary no-drag" Style="float: right" OnClick="btnBackToListing_Click"  OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"/>
                               </div>
                           </div>
                        </div>
                
                         <!-- 2nd Row: Show Content -->
                        <div style="width:100%;">
                              <!--  Left Content  -->
                            <div style="width:50%; float:left;">                        
                                <!--  Left Image  -->
                                <div id="panelLeft" style="width:99%; float:left;padding-right:15px">
                                    <!--Left Item-->
                                    <div style="background:#F7F9F9;width:100%;" id="leftImage">
                                        <div>
                                           <div class="wrapper"> 
                                               <div id="ImgArcViewerLeft" class="viewerC" style="width: 100%"></div>
                                           </div>
                                        </div>
                                        <div id="ImgArcFlipToggleLeft" style="background: #F7F9F9; width: 100%; float: right; border: 1px solid black; padding: 8px; margin-bottom: 20px">
                                            <button type="button" id="leftImgFlip" class="btn btn-link" title="Flip" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"><i class="fa fa fa-retweet fa-2x"></i></button>
                                            <button type="button" id="leftImgToggle" class="btn btn-link" title="Toggle JPEG/TIFF" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"><i class="fa fa fa-eye-slash fa-2x"></i></button>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div style="width: 50%; float: right;">
                                <!--  Right Image  -->
                                <div id="panelRight" style="width: 99%; float: left; padding-right: 15px">
                                    <!--Right Item-->
                                    <div style="background: #F7F9F9; width: 100%;" id="rightImage">
                                        <div>
                                            <div class="wrapper">
                                                <div id="ImgArcViewerRight" class="viewerC" style="width: 100%"></div>
                                            </div>
                                        </div>
                                        <div id="ImgArcFlipToggleRight" style="background: #F7F9F9; width: 100%; float: right; border: 1px solid black; padding: 8px; margin-bottom: 20px">
                                            <button type="button" id="rightImgFlip" class="btn btn-link" title="Flip" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"><i class="fa fa fa-retweet fa-2x"></i></button>
                                            <button type="button" id="rightImgToggle" class="btn btn-link" title="Toggle JPEG/TIFF" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"><i class="fa fa fa-eye-slash fa-2x"></i></button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                    
                            <!--  Bottom Content  -->
                            <div style="width: 100%; float: left; padding-right:20px">
                                <!--  Transaction info  -->
                                <div style="width: 100%; float: right;">
                                    <div style="width: 100%; float: right;">
                                        <div class="panel-group" id="accordion1">
                                            <div class="panel panel-primary">
                                                <div class="panel-heading">
                                                    <div class="panel-title">
                                                        <a data-toggle="collapse"
                                                            data-parent="#accordion"
                                                            href="#transInfo">Image Information</a>
                                                        <a class="pdsa-panel-toggle"></a>
                                                    </div>
                                                </div>
                                                <div id="transInfo" class="panel-collapse collapse in">
                                                    <div class="panel-body">
                                                        <div class="table-responsive">
                                                            <div style="width: 100%">
                                                                <table style="width: 100%; table-layout: fixed;">
                                                                    <%--<tr>
                                                                        <td>
                                                                            <asp:Label ID="ImageInfo" runat="server" Text="Image Information" Font-Bold="True"></asp:Label></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="1" style="padding-bottom: 8px;"></td>
                                                                    </tr>--%>
                                                                </table>

                                                                <table style="width: 100%; table-layout: fixed;">

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 20%">Amount Paid (RM)</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 80%">
                                                                            <asp:TextBox ID="lblAmount" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>


                                                                    <tr style="width: 100%">
                                                                        <td style="width: 20%">Tran Code</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 80%">
                                                                            <asp:TextBox ID="lblTranCode" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 20%">Cheque Account Number</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 80%">
                                                                            <asp:TextBox ID="lblChequeAccNum" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 20%">Cheque BSB</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 80%">
                                                                            <asp:TextBox ID="lblChequeBSB" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 20%">Cheque Serial</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 80%">
                                                                            <asp:TextBox ID="lblChequeSerial" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 20%">Check Digit</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 80%">
                                                                            <asp:TextBox ID="lblCheckDigit" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>


                                                                    <tr style="width: 100%">
                                                                        <td style="width: 20%">Batch Number</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 80%">
                                                                            <asp:TextBox ID="lblBatchNum" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>


                                                                    <tr style="width: 100%">
                                                                        <td style="width: 20%">NCF Tag</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 80%">
                                                                            <asp:TextBox ID="lblNCFTag" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>


                                                                    <tr style="width: 100%">
                                                                        <td style="width: 20%">Return Count</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 80%">
                                                                            <asp:TextBox ID="lblReturnCount" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>


                                                                    <tr style="width: 100%">
                                                                        <td style="width: 20%">Transaction Type</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 80%">
                                                                            <asp:TextBox ID="lblTransactionType" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>

                                                                </table>
                                                            </div>
                                                            <input type="hidden" id="isInward" value="<%= Session["isInward"] %>" />
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div style="width: 100%;">
                            <div style="width: 60%; float: left; visibility: hidden">
                                <!-- Title Shows Reject Reason-->
                                <div style="width: 99%; float: left; padding-right: 15px">
                                    <div class="page-title">
                                        <i class="icon-custom-right"></i>
                                        <h3><span class="semi-bold">
                                            <asp:Label ID="Label1" runat="server" Text="REJECT REASON: RL-Amount Breakdown Not Tally" Font-Bold="True" Font-Size="Large" ForeColor="Black"></asp:Label></span></h3>
                                    </div>
                                </div>
                            </div>

                            <div style="width: 40%; float: right; display: flex; gap: 2%; padding-right:20px">
                                <!-- Print Button-->
                                <div style="width: 100%; float: left; padding: 1px; margin-bottom: 12px;">
                                    <asp:LinkButton runat="server" ID="LinkButton1" Text="<i class='fa fa fa-print'></i> Print" CssClass="btn btn-primary no-drag" Style="float: right" OnClick="btnPrintTransaction_Click" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"/>
                                </div>
                                <!-- Back To Listing-->
                                <div style="float: left; padding: 1px; margin-bottom: 12px">
                                    <asp:LinkButton runat="server" ID="LinkButton2" Text="<i class='fa fa fa-backward'></i> Back To Listing" CssClass="btn btn-primary no-drag" Style="float: right" OnClick="btnBackToListing_Click"  OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"/>
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
        $("#leftImgToggle").off('click').on('click', function () {
            toggleImage("IAI", "TOP", "#ImgArcViewerLeft");
        });

        $("#leftImgFlip").off('click').on('click', function () {
            flipImage("IAI", "TOP", "#ImgArcViewerLeft");
        });

        $("#rightImgToggle").off('click').on('click', function () {
            toggleImage("IAI", "BOT", "#ImgArcViewerRight");
        });

        $("#rightImgFlip").off('click').on('click', function () {
            flipImage("IAI", "BOT", "#ImgArcViewerRight");
        });

        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(
            function () {

                LoadLeftImage();
                LoadRightImage();

                $("#leftImgToggle").off('click').on('click', function () {
                    toggleImage("IAI", "TOP", "#ImgArcViewerLeft");
                });

                $("#leftImgFlip").off('click').on('click', function () {
                    flipImage("IAI", "TOP", "#ImgArcViewerLeft");
                });

                $("#rightImgToggle").off('click').on('click', function () {
                    toggleImage("IAI", "BOT", "#ImgArcViewerRight");
                });

                $("#rightImgFlip").off('click').on('click', function () {
                    flipImage("IAI", "BOT", "#ImgArcViewerRight");
                });
            }
        );
    });


</script>
    
    <asp:PlaceHolder runat="server">
    <%: Scripts.Render("~/DataTableGrid/js") %>
</asp:PlaceHolder>

<script type="text/javascript">
    window.topBgColor = "lightblue";   // Declare a global variable
    window.ImgBgOriColor = "#F7F9F9";
    window.bottomBgColor = "";
    var isDirty = false;

    window.onload = function () {
        var form = document.forms[0];
        form.onsubmit = function () {
            isDirty = false;
        };
    };

    window.onbeforeunload = function () {
        if (isDirty) {
            return "Changes made, are you sure you want to leave?";
        }
    };

    function disableDirtyCheck() {
        isDirty = false;
    }

    function enableDirtyCheck() {
        isDirty = false;
    }


    function openCR() {
        var urlNet = setImageArchivalPrintRptURLInward();
        var option = 'height=' + screen.availHeight + ', width=' + screen.availWidth + ',scrollbars=1,resizable=1,top=0,left=0';
        myWindow = window.open(urlNet, '_blank', option);
        myWindow.resizeTo(window.screen.availWidth, window.screen.availHeight);
        myWindow.focus();
    }

    function LoadLeftImage() {
        var randomId = GenerateGuid();
        //  alert(randomId);
        LoadImage("/ImageHandlerInward.ashx?" + randomId, "#ImgArcViewerLeft");
    }

    function LoadRightImage() {
        var randomId = GenerateGuid();
        //  alert(randomId);
        LoadImage("/ImageHandlerInwardRear.ashx?" + randomId, "#ImgArcViewerRight");
    }

    //function setBottomBg() {
    //    topBgColor = "";
    //    bottomBgColor = "lightblue"
    //    return true;
    //}

    //function setTopBg() {
    //    bottomBgColor = "";
    //    topBgColor = "lightblue";
    //    return true;
    //}

    //function showhideBottom(strShow) {
    //    if (String(strShow) == "0") {
    //        document.getElementById("panelRight").style.display = "none";
    //    }
    //    else {
    //        document.getElementById("panelRight").style.display = "block";
    //    }
    //}

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

    function setValidNavigation() {
        validNavigation = true;
    }

    //PE-WD-23-001 Obsoleted Version of jQuery in Use
    //function setTopImgBg() {
    //    document.getElementById("ImgArcFlipToggleLeft").style.backgroundColor = "lightblue";
    //    document.getElementById("ImgArcViewerLeft").style.backgroundColor = "lightblue";
    //    document.getElementById('MainAdminContent_btnSelectTop').className = "btn btn-danger";
    //}
</script>

</form>

</asp:Content>

