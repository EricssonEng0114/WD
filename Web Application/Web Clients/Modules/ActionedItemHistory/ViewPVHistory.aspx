<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="ViewPVHistory.aspx.cs" Inherits="UBPCWeb.Modules.ActionedItemHistory.ViewPVHistory" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">    
<form runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <link href="../../Content/pdsa-collapser.css" rel="stylesheet" />
    <script src="../../Scripts/pdsa-collapser.js"></script>
   <asp:PlaceHolder runat="server">       
        <%: Styles.Render("~/CViewer/css") %>    
        <%: Scripts.Render("~/CViewer/js") %>
    </asp:PlaceHolder>

    <div class="content">     
          <!-- Navigation -->
        <ul class="breadcrumb">
            <li><p><b>Operator Tasks</b></p></li>
            <li><b>Actioned Item History - Details</b></li>
        </ul>

        <div class="">
            <div class="grid-body ">
                <asp:UpdatePanel ID="ajaxPanel1" runat="server">             
                    <ContentTemplate>
                         

                        <!-- 1st Row: Show Reason and Back to Listing Button -->
                       <div style="width:100%;">
                            <div style="width:60%; float:left;">                        
                                <!-- Title Shows Reject Reason-->
                                <div style="width:99%; float:left;padding-right:15px">
                                     <div class="page-title">
                                         <i class="icon-custom-right"></i>
                                         <h3><span class="semi-bold"><asp:Label ID="lblRejReason" runat="server" Text="REJECT REASON: REJECT - No Account Information (AIF)" Font-Bold="True" Font-Size="Large" ForeColor="Black"></asp:Label></span></h3>
                                     </div>
                                </div>
                            </div>

                            <div style="width:40%;float:right;">
                                 <!-- Back To Listing-->
                                <div style="width:100%; float:left;padding:1px;margin-bottom:12px">
                                    <asp:LinkButton runat="server" ID="btnBackToListing" Text="<i class='fa fa fa-backward'></i> Back To Listing" CssClass="btn btn-primary" style="float:right" OnClick="btnBackToListing_Click"  />
                                 </div>

                                 <!-- Accept/reject status-->

                                <div style="width:100%; float:left;padding:1px;margin-bottom:12px">
                                 </div>
                            </div>
                        </div>
                
                         <!-- 2nd Row: Show Content -->
                        <div style="width:100%;">
                              <!--  Left Content  -->
                            <div style="width:60%; float:left;">                        
                                <!--  Top Image  -->
                                <div id="panelTop" style="width:99%; float:left;padding-right:15px">
                                    <!--Top Item-->
                                    <div style="background:#F7F9F9;width:100%;" id="topImage">
                                        <div>
                                           <div class="wrapper"> 
                                               <div id="PVImgViewerTop" class="viewerC" style="width: 100%"></div>
                                           </div>
                                        </div>
                                        <div id="PVImgFlipToggleTop" style="background:#F7F9F9;width:100%;float:right;border:1px solid black;padding:8px;margin-bottom:20px">
                                            <button type="button" id="topImgFlip" class="btn btn-link" title="Flip"><i class="fa fa fa-retweet fa-2x"></i></button>
                                            <button type="button" id="topImgToggle" class="btn btn-link" title="Toggle JPEG/TIFF"><i class="fa fa fa-eye-slash fa-2x"></i></button>
                            
                                             <div style="float:right;padding-top:5px">
                                                 <asp:LinkButton runat="server" ID="btnSelectTop" Text="<i class='fa fa fa-sign-in'></i> Select" CssClass="btn btn-primary" OnClick="btnSelectTop_Click" />
                                             </div>    
                                        </div>
                                    </div>
                            
                                     <!--Account Name & Top Item Amount-->
                                    <div style="background:#F7F9F9;width:100%; float:left;border:1px solid black;padding:8px;margin-bottom:20px">
                                        <table>
                                             <tr>
                                                 <td style="width:70%"> 1ST: <textarea id="txtA1stName" rows="2" runat="server" class="form-control" readonly>xxxx xxxx xxxx - I/C# 00000000</textarea></td>
                                                 <td style="width:25%;padding-left:20px"> Amount: <asp:TextBox ID="txtTopItemAmount" runat="server" CssClass="form-control" style="text-align:right"  ReadOnly="true">0.00</asp:TextBox></td>
                                             </tr>
                                        </table>
                                    </div>

                                    <!-- Bottom Item -->
                                    <div id="panelBottom" style="width:100%; float:left;">                          
                                        <div style="background:#F7F9F9;width:100%;" id="bottomImage">
                                             <div>
                                               <div class="wrapper"> 
                                                   <div id="PVImgViewerBottom" class="viewerC" style="width: 100%"></div>
                                               </div>
                                             </div>
                                        </div>
                                        <div id="PVImgFlipToggleBottom" style="background:#F7F9F9;width:100%;float:right;border:1px solid black;padding:8px;margin-bottom:20px">
                                            <button type="button" id="bottomImgFlip" class="btn btn-link" title="Flip"><i class="fa fa fa-retweet fa-2x"></i></button>
                                            <button type="button" id="bottomImgToggle" class="btn btn-link" title="Toggle JPEG/TIFF"><i class="fa fa fa-eye-slash fa-2x"></i></button>

                                            <div style="float:right;padding-top:5px">
                                                 <asp:LinkButton runat="server" ID="btnSelectBottom" Text="<i class='fa fa fa-sign-in'></i> Select" CssClass="btn btn-primary" OnClick="btnSelectBottom_Click" />
                                             </div>
                                        </div>
                                    
                         
                                    <!-- Bottom Item Amount -->
                                    <div style="background: #F7F9F9; width: 100%; float: left; border: 1px solid black; padding: 8px; margin-bottom: 20px">
                                        <table>
                                            <tr>
                                                <td style="width: 70%"></td>
                                                <td style="width: 25%; padding-left: 20px">&nbsp;
                                                    <asp:TextBox ID="txtBottomItemAmount" runat="server" Style="text-align: right; background-color: #eee" ReadOnly="true">0.00</asp:TextBox></td>
                                            </tr>
                                        </table>
                                    </div>
                                        </div>
                                </div>
                            </div>
                    
                            <!--  Right Content  -->
                            <div style="width: 40%; float: right;">
                                 <!--  Transaction info  -->
                                <div style="width: 100%; float: right;">
                                    <div style="width: 100%; float: right;">
                                        <div class="panel-group" id="accordion">
                                            <div class="panel panel-primary">
                                                <div class="panel-heading">
                                                    <div class="panel-title">
                                                        <a data-toggle="collapse"
                                                            data-parent="#accordion"
                                                            href="#transInfo">Transaction Info</a>
                                                        <a class="pdsa-panel-toggle"></a>
                                                      <%--  <a data-toggle="collapse"
                                                            data-parent="#accordion"
                                                            href="#transInfo" class="pdsa-panel-toggle glyphicon glyphicon-chevron-up"></a>--%>
                                                    </div>
                                                </div>
                                                <div id="transInfo" class="panel-collapse collapse in">
                                                    <div class="panel-body">
                                                        <div class="table-responsive">
                                                            <div style="width: 100%">
                                                                <table style="width: 100%; table-layout: fixed;">
                                                                    <tr>
                                                                        <td>
                                                                            <asp:Label ID="lblDecision" runat="server" Text="ACCEPTED/REJECTED" Font-Bold="True" Font-Size="Large" ForeColor="Red" style="float:left;"></asp:Label>

                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            <asp:Label ID="lblAllowBD" runat="server" Text="Allow Breakdown:No" Font-Bold="True"></asp:Label>                                                                            
                                                                            <asp:Label ID="lblAllowTol" runat="server" Text=", Allow Tolerance:No" Font-Bold="True"></asp:Label>
                                                                           </td>
                                                                    </tr>
                                                                     <tr>
                                                                        <td colspan="1" style="padding-bottom: 8px;"></td>
                                                                    </tr>
                                                                    </table>
                                                                <table style="width: 100%; table-layout: fixed;">
                                                                    <tr>
                                                                        <td>Business Date</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td>
                                                                            <asp:Label ID="lblBusdate" runat="server" Text="xx/xx/xxxx"></asp:Label></td>
                                                                        <td>Transaction No</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td>
                                                                            <asp:Label ID="lblTransNo" runat="server" Text="x"></asp:Label></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>Batch Directory</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td>
                                                                            <asp:Label ID="lblBatchDir" runat="server" Text="xxxxxxxx"></asp:Label></td>
                                                                        <td>Batch No:</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td>
                                                                            <asp:Label ID="lblBatchNo" runat="server" Text="xxxxxxxx"></asp:Label></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="border-bottom: dotted; padding-bottom: 15px;"></td>
                                                                    </tr>
                                                                </table>
                                                            </div>

                                                            <div style="width: 100%; margin-top: 10px">
                                                                <table style="width: 100%; table-layout: fixed;">
                                                                    <tr>
                                                                        <td style="width: 48%">
                                                                            <asp:Label ID="lblTotalStubCount" runat="server" Text="Total Stub Count : x"></asp:Label></td>
                                                                        <td style="width: 45%">
                                                                            <asp:Label ID="lblTotalChequeCount" runat="server" Text="Total Cheque Count : x"></asp:Label></td>
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
                                                                        <td style="width: 45%">
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

                                <!--  Selected Item Info  -->
                                <div style="width: 100%; float: right;">
                                    <div style="width: 100%; float: right;">
                                        <div class="panel-group" id="accordion2">
                                            <div class="panel panel-primary">
                                                <div class="panel-heading">
                                                    <div class="panel-title">
                                                        <a data-toggle="collapse"
                                                            data-parent="#accordion2"
                                                            href="#selItemInfo">Selected Item Info</a>
                                                        <a class="pdsa-panel-toggle"></a>
                                                    </div>
                                                </div>
                                                <div id="selItemInfo" class="panel-collapse collapse in">
                                                    <div class="panel-body">
                                                        <div style="width: 100%; padding-bottom: 10px">
                                                            <asp:Label ID="lblSelType" runat="server" Text="STUB/CHEQUE" Font-Italic="True" Font-Underline="true" Font-Bold="True" Font-Size="Large" ForeColor="Black"></asp:Label>
                                                        </div>

                                                        <div style="width: 100%">
                                                            <table style="width: 100%; table-layout: fixed;">
                                                                <tr>
                                                                    <td>Sequence No</td>
                                                                    <td style="width: 11px">:</td>
                                                                    <td>
                                                                        <asp:Label ID="lblSeqNo" runat="server" Text="x"></asp:Label></td>
                                                                    <td align="right">DIN No</td>
                                                                    <td style="width: 11px">:</td>
                                                                    <td><asp:Label ID="lblDIN" runat="server" Text="x"></asp:Label></td>
                                                                </tr>
                                                                <tr>
                                                                    <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                </tr>
                                                                <tr>
                                                                    <td style="width: 90px">Account No</td>
                                                                    <td style="width: 11px">:</td>
                                                                    <td colspan="4">
                                                                        <asp:TextBox ID="txtSelAcctNo" runat="server" ReadOnly="true" CssClass="form-control">xxxxxxxxxxxx</asp:TextBox>

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

                                <!--  Selected Account Name Info  -->
                                <div style="width: 100%; float: right;">
                                    <div style="width: 100%; float: right;">
                                        <div class="panel-group" id="accordion3">
                                            <div class="panel panel-primary">
                                                <div class="panel-heading">
                                                    <div class="panel-title">
                                                        <a data-toggle="collapse"
                                                            data-parent="#accordion3"
                                                            href="#selAcctName">Selected Account Information</a>
                                                        <a class="pdsa-panel-toggle"></a>
                                                    </div>
                                                </div>
                                                <div id="selAcctName" class="panel-collapse collapse in">
                                                    <div class="panel-body">
                                                        <div class="table-responsive">
                                                            <asp:Label ID="lbl2nd" runat="server" Text="2ND:" ForeColor="Black"></asp:Label>
                                                            <textarea id="txtA2ndAcctName" rows="2" runat="server" class="form-control" readonly>XXXXXXXXXXXX - I/C# 0000000000</textarea>
                                                            <br />
                                                            <asp:Label ID="lbl3rd" runat="server" Text="3RD:" ForeColor="Black"></asp:Label>
                                                            <textarea id="txtA3rdAcctName" rows="2" runat="server" class="form-control" readonly>XXXXXXXXXXXX - I/C# 0000000000</textarea>
                                                            <br />
                                                            <asp:Label ID="lbl4th" runat="server" Text="4TH:" ForeColor="Black"></asp:Label>
                                                            <textarea id="txtA4thAcctName" rows="2" runat="server" class="form-control" readonly>XXXXXXXXXXXX - I/C# 0000000000</textarea>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                 <!--  Previous and Next Page   -->
                        <div style="width: 100%; float: right;">
                            <table style="width: 100%; table-layout: fixed;">
                                <tr>
                                    <td></td>
                                    <td>
                                        <asp:LinkButton runat="server" ID="btnPrevPage" Text="<i class='fa fa fa-arrow-circle-left fa-2x'></i></br> Previous Page " CssClass="btn btn-info" OnClick="btnPrevPage_Click" /></td>
                                    <td></td>
                                    <td>
                                        <asp:LinkButton runat="server" ID="btnNextPage" Text="<i class='fa fa fa-arrow-circle-right fa-2x'></i></br> &nbsp; &nbsp; Next Page &nbsp; &nbsp;  " CssClass="btn btn-info" OnClick="btnNextPage_Click" />
                                    </td>
                                    <td></td>
                                </tr>
                            </table>
                        </div>

                            </div>
                        </div>
                        </ContentTemplate>
                </asp:UpdatePanel>

                


                <!-- 3rd Row: Show Action Taken -->
                <div style="width:100%;">
                    <div style="width:100%; float:left;padding-top:15px">
                            <div style="width:100%; float:left;">
                                <div class="panel-group" id="accordionF">
                                    <div class="panel panel-primary">
                                        <div class="panel-heading">
                                            <div class="panel-title">
                                                <a data-toggle="collapse"
                                                   data-parent="#accordionF"
                                                   href="#actionTab">Action                                     <a class="pdsa-panel-toggle"></a>
                                            </div>
                                        </div>
                                        <div id="actionTab" class="panel-collapse collapse in">
                                            <div class="panel-body">
                                                <div class="table-responsive">
                                                    <div style="width:100%;float:left">
                                                        <table style="width: 100%">
                                                            <tr id="SingleActionReview" runat="server">
                                                                <td>Action By: <asp:Label ID="lblActionBy" runat="server" Text=""></asp:Label>
                                                                    , Action Date Time: <asp:Label ID="lblActionDateTime" runat="server" Text=""></asp:Label>
                                                                </td>
                                                            </tr>
                                                            <tr id="FinalActionReview" runat="server">
                                                                <td>Final Action By: <asp:Label ID="lblFinalActionBy" runat="server" Text=""></asp:Label>
                                                                    , Final Action Date Time: <asp:Label ID="lblFinalActionDateTime" runat="server" Text=""></asp:Label>
                                                                </td>
                                                            </tr>
                                                            <tr id="FirstReview" runat="server">
                                                                <td>1st Review Action By: <asp:Label ID="lbl1stReviewBy" runat="server" Text=""></asp:Label>
                                                                    , 1st Review Action Date Time: <asp:Label ID="lbl1stActionDateTime" runat="server" Text=""></asp:Label>
                                                                </td>
                                                            </tr>
                                                            <tr id="SecondReview" runat="server">
                                                                <td>2nd Review Action By: <asp:Label ID="lbl2ndReviewBy" runat="server" Text=""></asp:Label>
                                                                    , 2nd Review Action Date Time: <asp:Label ID="lbl2ndActionDateTime" runat="server" Text=""></asp:Label>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:Label ID="lblRemark" runat="server" Text="Additional Remark(s):" ForeColor="Black"></asp:Label>
                                                                    <textarea id="txtARemark" maxlength="7000" rows="4" runat="server" class="form-control" style="width: 100%" readonly="readonly"></textarea>
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
        </div>

    </div>
   
<script type="text/javascript">
    var $ = jQuery;

    $(document).ready(function () {
        $("#topImgToggle").off('click').on('click', function () {
            toggleImage("APV", "TOP", "#PVImgViewerTop");
        });

        $("#topImgFlip").off('click').on('click', function () {
            flipImage("APV", "TOP", "#PVImgViewerTop");
        });

        $("#bottomImgToggle").off('click').on('click', function () {
            toggleImage("APV", "BOT", "#PVImgViewerBottom");
        });

        $("#bottomImgFlip").off('click').on('click', function () {
            flipImage("APV", "BOT", "#PVImgViewerBottom");
        });

        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(
          function () {

              LoadTopImage();
              LoadBottomImage();

              if (topBgColor.length > 1) {
                  //top selected
                  document.getElementById("PVImgFlipToggleTop").style.backgroundColor = topBgColor;
                  document.getElementById("PVImgViewerTop").style.backgroundColor = topBgColor;
                  document.getElementById('MainAdminContent_btnSelectTop').className = "btn btn-danger";

                  //bottom de-selected
                  document.getElementById("PVImgFlipToggleBottom").style.backgroundColor = ImgBgOriColor;
                  document.getElementById("PVImgViewerBottom").style.backgroundColor = ImgBgOriColor;
                  document.getElementById('MainAdminContent_btnSelectBottom').className = "btn btn-primary";
              }
              else {
                  //bottom seected
                  document.getElementById("PVImgFlipToggleBottom").style.backgroundColor = bottomBgColor;
                  document.getElementById("PVImgViewerBottom").style.backgroundColor = bottomBgColor;
                  document.getElementById('MainAdminContent_btnSelectBottom').className = "btn btn-danger";

                  //top de-selected
                  document.getElementById("PVImgFlipToggleTop").style.backgroundColor = ImgBgOriColor;
                  document.getElementById("PVImgViewerTop").style.backgroundColor = ImgBgOriColor;
                  document.getElementById('MainAdminContent_btnSelectTop').className = "btn btn-primary";
              }


              $("#topImgToggle").off('click').on('click', function () {
                  toggleImage("APV", "TOP", "#PVImgViewerTop");
              });

              $("#topImgFlip").off('click').on('click', function () {
                  flipImage("APV", "TOP", "#PVImgViewerTop");
              });

              $("#bottomImgToggle").off('click').on('click', function () {
                  toggleImage("APV", "BOT", "#PVImgViewerBottom");
              });

              $("#bottomImgFlip").off('click').on('click', function () {
                  flipImage("APV", "BOT", "#PVImgViewerBottom");
              });





          }
        );
    });


</script>
    
<script type="text/javascript">
    window.topBgColor = "lightblue";   // Declare a global variable
    window.ImgBgOriColor = "#F7F9F9";
    window.bottomBgColor = "";

    function LoadTopImage() {
        var randomId = GenerateGuid();
        //  alert(randomId);
        LoadImage("/ImageHandlerTop.ashx?" + randomId, "#PVImgViewerTop");
    }

    function LoadBottomImage() {
        var randomId = GenerateGuid();
        //alert(randomId);
        LoadImage("/ImageHandlerBottom.ashx?" + randomId, "#PVImgViewerBottom");
    }

    function setBottomBg() {
        topBgColor = "";
        bottomBgColor = "lightblue"
        return true;
    }

    function setTopBg() {
        bottomBgColor = "";
        topBgColor = "lightblue";
        // document.getElementById("PVImgFlipToggleTop").style.backgroundColor = "lightblue";
        return true;
    }

    function showhideBottom(strShow) {
        if (String(strShow) == "0") {
            document.getElementById("panelBottom").style.display = "none";
        }
        else {
            document.getElementById("panelBottom").style.display = "block";
        }
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

    //PE-WD-23-001 Obsoleted Version of jQuery in Use
    function setTopImgBg() {
        document.getElementById("PVImgFlipToggleTop").style.backgroundColor = "lightblue";
        document.getElementById("PVImgViewerTop").style.backgroundColor = "lightblue";
        document.getElementById('MainAdminContent_btnSelectTop').className = "btn btn-danger";
    }
</script>

</form>

</asp:Content>




