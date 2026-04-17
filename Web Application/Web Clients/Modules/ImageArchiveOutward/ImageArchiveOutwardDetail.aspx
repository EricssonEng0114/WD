<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="ImageArchiveOutwardDetail.aspx.cs" Inherits="UBPCWeb.Modules.ImageArchiveOutward.ImageArchiveOutwardDetail" %>
<%@ Register assembly="CrystalDecisions.Web, Version=13.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" namespace="CrystalDecisions.Web" tagprefix="CR" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">    
<form runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <link href="../../Content/pdsa-collapser.css" rel="stylesheet" />
    <link href="../../Content/toggle.css" rel="stylesheet" type="text/css" />
    <script src="../../Scripts/pdsa-collapser.js"></script>
    <script src="../../ImgViewer/jquery.viewer-v2.js"></script>
   <asp:PlaceHolder runat="server">       
        <%: Styles.Render("~/CViewer/css") %>    
        <%: Scripts.Render("~/CViewer/js") %>
    </asp:PlaceHolder>

    <div class="content">     
          <!-- Navigation -->
        <ul class="breadcrumb">
            <li><p><b>Operator Tasks</b></p></li>
            <li><b>Image Archive Outward - Image Information</b></li>
        </ul>

        <div class="">
            <div class="grid-body ">
                <asp:UpdatePanel ID="ajaxPanel1" runat="server">             
                    <ContentTemplate>
                         

                        <!-- 1st Row: Show Reason and Back to Listing Button -->
                       <div style="width:100%;">
                            <div runat="server" style="width:60%; float:left;">                        
                                <!-- Title Shows Reject Reason-->
                                <div style="width:99%; float:left;padding-right:15px">
                                     <div id="title" class="page-title" runat="server">
                                         <i class="icon-custom-right"></i>
                                         <h3><span class="semi-bold"><asp:Label ID="lblItemStatus" runat="server" Text="REJECT REASON: REJECT - No Account Information (AIF)" Font-Bold="True" Font-Size="Large" ForeColor="Black"></asp:Label></span></h3>
                                     </div>
                                </div>
                            </div>

                           <div style="width:100%; float: right; display: flex; gap:0.5%; justify-content:flex-end; padding-right:13px">
                               <!-- Print Transaction Button-->
                               <div style="float: left; padding: 1px; margin-bottom: 12px;">
                                   <asp:LinkButton runat="server" ID="btnPrintTransaction" Text="<i class='fa fa fa-print'></i> Print Transaction " CssClass="btn btn-primary no-drag" Style="float: right" OnClick="btnPrintTransaction_Click" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"/>
                               </div>

                               <!-- Print Current Item Button-->
                               <div style="float: left; padding: 1px; margin-bottom: 12px;">
                                   <asp:LinkButton runat="server" ID="btnPrintCurrentItem" Text="<i class='fa fa fa-print'></i> Print Current Item " CssClass="btn btn-primary no-drag" Style="float: right" OnClick="btnPrintItemOnly_Click" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"/>
                               </div>
                               <!-- Back To Listing-->
                               <div style="float: left; padding: 1px; margin-bottom: 12px">
                                   <asp:LinkButton runat="server" ID="btnBackToListing" Text="<i class='fa fa fa-backward'></i> Back To Listing" CssClass="btn btn-primary no-drag" Style="float: right" OnClick="btnBackToListing_Click" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"/>
                               </div>
                           </div>
                        </div>
                
                         <!-- 2nd Row: Show Content -->
                        <div style="width:100%;">
                              <!--  Left Content  -->
                            <div style="width:50%; float:left;">
                                <!--  Left Image  -->
                                <div id="panelLeft" style="width: 99%; float: left; padding-right: 15px" runat="server">
                                    <!--Left Item-->
                                    <%-- Show Virtual Stub image when no image --%>
                                    <div style="width: 100%; height: 100%">
                                        <div style="background: #F7F9F9; width: 100%;" id="leftImage" runat="server">
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
                                <div style="width: 97%; display: flex; gap: 2%; justify-content: flex-end">
                                    <!-- View Batch Header-->
                                    <div style="float: left; padding: 1px; margin-bottom: 12px">
                                        <asp:LinkButton runat="server" ID="btnViewBatchHeader" Text="<i class='fa fa'></i> View Batch Header" CssClass="btn btn-success no-drag" Style="float: right" OnClick="btnViewBatchHeader_Click" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"/>
                                    </div>
                                </div>


                                <%-- Image Information --%>
                                <div style="width: 99%; float: left; padding-right:15px">
                                    <div style="width: 100%; float: left;">
                                        <div class="panel-group" id="topInfo" runat="server">
                                            <div class="panel panel-primary">
                                                <div class="panel-heading">
                                                    <div class="panel-title">
                                                        <a data-toggle="collapse"
                                                            data-parent="#accordion"
                                                            href="#transInfoImage">Image Information</a>
                                                        <a class="pdsa-panel-toggle"></a>
                                                    </div>
                                                </div>
                                                <div id="transInfoImage" class="panel-collapse collapse in">
                                                    <div class="panel-body">
                                                        <div class="table-responsive">
                                                            <div style="width: 100%">
                                                                <table style="width: 100%; table-layout: fixed;">

                                                                    <tr>
                                                                        <td colspan="1" style="padding-bottom: 8px;"></td>
                                                                    </tr>
                                                                </table>

                                                                <table style="width: 100%; table-layout: fixed;">                                                                    
                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Business Date</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblBusDate" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Depositor A/C Number</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblDepositorACNum" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Amount Paid (RM)</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblAmount" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Tran Code</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblTranCode" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Cheque A/C Number</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblChequeAccNum" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Cheque BSB</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblChequeBSB" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Cheque Serial</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblChequeSerial" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Primary Name</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblPrimaryName" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr
                                                                    
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">1st Secondary Name</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lbl1stSecondaryName" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">2nd Secondary Name</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lbl2ndSecondaryName" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                        <td></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">3rd Secondary Name</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lbl3rdSecondaryName" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                        <td></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Batch Number</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblBatchNum" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                        <td></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Trans Number</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblTransNum" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                        <td></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Din</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblDin" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                        <td></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Worksource</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblWorksource" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                        <td></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Bundle Number</td>
                                                                        <td style="width: 11px"></td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblBundleNum" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                        <td></td>
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


                                <%-- Image Information --%>

                            </div>

                            <!--  Right Content  -->
                            <div style="width: 50%; float: right;">
                                <!--  Transaction info  -->
                                <!-- Right Item (Display Rear Outward Image)-->
                                <div id="panelRight" style="width: 99%; float: right; padding-right: 15px;" runat="server">
                                    <div style="background: #F7F9F9; width: 100%;" id="rightImage" runat="server">
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

                                <div style="width: 100%; display: flex; gap: 2%; justify-content: flex-start">
                                    <!-- Prev Item-->
                                    <div style="float: left; padding: 1px; margin-bottom: 12px;">
                                        <asp:LinkButton runat="server" ID="btnPrevItem" Text="<i class='fa fa fa-backward'></i> Prev Item" CssClass="btn btn-success no-drag" Style="float: right" OnClick="btnPrevItem_Click" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"/>
                                    </div>
                                    <!-- Next Item-->
                                    <div style="float: left; padding: 1px; margin-bottom: 12px">
                                        <asp:LinkButton runat="server" ID="btnNextItem" Text="<i class='fa fa fa-forward'></i> Next Item" CssClass="btn btn-success no-drag" Style="float: right" OnClick="btnNextItem_Click" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"/>
                                    </div>
                                </div>

                                <%-- Other information --%>
                                <div style="width: 99%; float: right; padding-right:15px">
                                    <div style="width: 100%; float: right;">
                                        <div class="panel-group">
                                            <div class="panel panel-primary">
                                                <div class="panel-heading">
                                                    <div class="panel-title">
                                                        <a data-toggle="collapse"
                                                            data-parent="#accordion"
                                                            href="#transInfoOthers">Others Information</a>
                                                        <a class="pdsa-panel-toggle"></a>
                                                    </div>
                                                </div>
                                                <div id="transInfoOthers" class="panel-collapse collapse in">
                                                    <div class="panel-body">
                                                        <div class="table-responsive">
                                                            <div style="width: 100%">
                                                                <table style="width: 100%; table-layout: fixed;">

                                                                    <tr>
                                                                        <td colspan="1" style="padding-bottom: 8px;"></td>
                                                                    </tr>
                                                                </table>

                                                                <table style="width: 100%; table-layout: fixed;">
                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Rejected Reason</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblRejectedReason" runat="server" Text="x" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">NCF Tag</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblNCFTag" runat="server" Text="x" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>
                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Reference Number</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblRefNum" runat="server" Text="x" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Presenting BSB</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblPresentingBSB" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Processing Mode</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblProcessingMode" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">PE Run Number</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblPERunNumber" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Run Number</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblRunNum" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Product Type</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblProductType" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Reference 1</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblRef1" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Reference 2</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblRef2" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Reference 3</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblRef3" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Reference 4</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblRef4" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Box No</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblBoxNo" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr style="width: 100%">
                                                                        <td style="width: 30%">Return Status</td>
                                                                        <td style="width: 11px">:</td>
                                                                        <td style="width: 70%">
                                                                            <asp:TextBox ID="lblReturnStatus" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <div id="itemReturned" runat="server">
                                                                        <tr style="width: 100%">
                                                                            <td style="width: 30%">Returned Date</td>
                                                                            <td style="width: 11px">:</td>
                                                                            <td style="width: 70%">
                                                                                <asp:TextBox ID="lblReturnedDate" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                        </tr>

                                                                        <tr style="width: 100%">
                                                                            <td style="width: 30%">Returned Reason</td>
                                                                            <td style="width: 11px">:</td>
                                                                            <td style="width: 70%">
                                                                                <asp:TextBox ID="lblReturnedReason" runat="server" Text="xxxxxxxx" ReadOnly="true" MaxLength="40" TextMode="MultiLine" Width="100%" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"></asp:TextBox></td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                        </tr>
                                                                    </div>

                                                                    <tr>
                                                                        <td></td>
                                                                        <td style="width: 11px"></td>
                                                                        <td>
                                                                            <%--<asp:Label ID="Label13" runat="server" Text="xxxxxxxx"></asp:Label>--%></td>
                                                                        <td></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td></td>
                                                                        <td style="width: 11px"></td>
                                                                        <td>
                                                                            <%--<asp:Label ID="Label14" runat="server" Text="xxxxxxxx"></asp:Label>--%></td>
                                                                        <td></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td></td>
                                                                        <td style="width: 11px"></td>
                                                                        <td>
                                                                            <%--<asp:Label ID="Label15" runat="server" Text="xxxxxxxx"></asp:Label>--%></td>
                                                                        <td></td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="padding-bottom: 8px;"></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td></td>
                                                                        <td style="width: 11px"></td>
                                                                        <td>
                                                                            <%--<asp:Label ID="Label16" runat="server" Text="xxxxxxxx"></asp:Label>--%></td>
                                                                        <td></td>
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
                                <%-- Other information --%>

                                <div style="float: right; display: flex; gap: 2%; justify-content: flex-end; position:absolute; bottom:0; right:0; padding-right:35px">
                                    <!-- Print Transaction Button-->
                                    <div style="float: left; padding: 1px; margin-bottom: 12px;">
                                        <asp:LinkButton runat="server" ID="LinkButton1" Text="<i class='fa fa fa-print'></i> Print Transaction " CssClass="btn btn-primary no-drag" Style="float: right" OnClick="btnPrintTransaction_Click" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"/>
                                    </div>

                                    <!-- Print Current Item Button-->
                                    <div style="float: left; padding: 1px; margin-bottom: 12px;">
                                        <asp:LinkButton runat="server" ID="LinkButton2" Text="<i class='fa fa fa-print'></i> Print Current Item " CssClass="btn btn-primary no-drag" Style="float: right" OnClick="btnPrintItemOnly_Click" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"/>
                                    </div>
                                    <!-- Back To Listing-->
                                    <div style="float: left; padding: 1px; margin-bottom: 12px">
                                        <asp:LinkButton runat="server" ID="LinkButton3" Text="<i class='fa fa fa-backward'></i> Back To Listing" CssClass="btn btn-primary no-drag" Style="float: right" OnClick="btnBackToListing_Click" OnFocus="disableDirtyCheck()" OnBlur="enableDirtyCheck()"/>
                                    </div>
                                </div>

                                <input type="hidden" id="isInward" value="<%= Session["isInward"] %>" />
                            </div>
                            
                            
                        </div>
                        </ContentTemplate>
                </asp:UpdatePanel>

              
            </div>
        </div>

    </div>
   
<asp:PlaceHolder runat="server">
    <%: Scripts.Render("~/DataTableGrid/js") %>
</asp:PlaceHolder>

<script type="text/javascript">
    var $ = jQuery;

    var reportCode = '';
    var reportClient = '';

    function render(data, type, row, meta) {
        return "<form name='viewRpt' target='_blank' action='/PrintArchivalReport' method='post'>"
            + "<input type='hidden' name='rptCode' value='" + reportCode + "'/>"
            + "<input type='hidden' name='clientCode' value='" + reportClient + "'/>"
            + "<input type='submit' class='btn btn-info btn-small' id='viewRpt' value='View Report' />"
            + "</form>";
    }
    function printTransaction() {
        // Call render function with sample data
        var data = null; // You may need to define 'data' based on your actual usage
        var type = null; // You may need to define 'type' based on your actual usage
        var row = { ReportCode: reportCode, ReportClient: reportClient }; // Construct row object with reportCode and reportClient
        var meta = null; // You may need to define 'meta' based on your actual usage

        var result = render(data, type, row, meta);
        console.log("Rendered HTML:", result);

        // Optionally, append result HTML to a container on your page
        // Example: document.getElementById('renderContainer').innerHTML = result;
    }
    $(document).ready(function () {
        $("#leftImgToggle").off('click').on('click', function () {
            toggleImage("IAO", "TOP", "#ImgArcViewerLeft");
        });

        $("#leftImgFlip").off('click').on('click', function () {
            flipImage("IAO", "TOP", "#ImgArcViewerLeft");
        });

        $("#rightImgToggle").off('click').on('click', function () {
            toggleImage("IAO", "BOT", "#ImgArcViewerRight");
        });

        $("#rightImgFlip").off('click').on('click', function () {
            flipImage("IAO", "BOT", "#ImgArcViewerRight");
        });

        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(
            function () {
                var isRepresented = '<%= curRepresented.ToString() %>'.value;
                var imageExist = '<%= imageExist.ToString() %>'.value;
                if (imageExist == "true") {

                    if (isRepresented == 'False') {
                        LoadLeftImage();
                        LoadRightImage();
                    }
                    else {
                        LoadLeftImage2();
                        LoadRightImage2();
                    }
                }

                $("#leftImgToggle").off('click').on('click', function () {
                    toggleImage("IAO", "TOP", "#ImgArcViewerLeft");
                });

                $("#leftImgFlip").off('click').on('click', function () {
                    flipImage("IAO", "TOP", "#ImgArcViewerLeft");
                });

                $("#rightImgToggle").off('click').on('click', function () {
                    toggleImage("IAO", "BOT", "#ImgArcViewerRight");
                });

                $("#rightImgFlip").off('click').on('click', function () {
                    flipImage("IAO", "BOT", "#ImgArcViewerRight");
                });
            }
        );
    });


</script>
    
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
        var urlNet = setImageArchivalPrintRptURLOutward();
        var option = 'height=' + screen.availHeight + ', width=' + screen.availWidth + ',scrollbars=1,resizable=1,top=0,left=0';
        myWindow = window.open(urlNet, '_blank', option);
        myWindow.resizeTo(window.screen.availWidth, window.screen.availHeight);
        myWindow.focus();
    }


    function LoadLeftImage() {
        var randomId = GenerateGuid();
          //alert(randomId);
        LoadImage("/ImageHandlerLeft.ashx?" + randomId, "#ImgArcViewerLeft");
    }

    function LoadLeftImage2() {
        var randomId = GenerateGuid();
        LoadImage("/ImageHandlerInward.ashx?" + randomId, "#ImgArcViewerLeft");
    }

    //function LoadLeftImage3() {
    //    var randomId = GenerateGuid();
    //    //  alert(randomId);
    //    LoadImage("/ImageHandlerLeft.ashx?" + randomId, "#ImgArcViewerLeft1");
    //}

    function LoadRightImage() {
        var randomId = GenerateGuid();
        //alert(randomId);
        LoadImage("/ImageHandlerRight.ashx?" + randomId, "#ImgArcViewerRight");
    }

    function LoadRightImage2() {
        var randomId = GenerateGuid();
        //alert(randomId);
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
    //    // document.getElementById("ImgArcFlipToggleLeft").style.backgroundColor = "lightblue";
    //    return true;
    //}

    function showhideBottom(strShow) {
        if (String(strShow) == "0") {
            document.getElementById("panelRight").style.visibility = "hidden";
        }
        else {
            document.getElementById("panelRight").style.visibility = "visible";
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

    function setValidNavigation() {
        validNavigation = true;
    }

    //PE-WD-23-001 Obsoleted Version of jQuery in Use
    function setTopImgBg() {
        //document.getElementById("ImgArcFlipToggleLeft").style.backgroundColor = "lightblue";
        //document.getElementById("ImgArcViewerLeft").style.backgroundColor = "lightblue";
       //document.getElementById('MainAdminContent_btnSelectTop').className = "btn btn-danger";
    }

    function toggleButton(btn) {
        var isOn = btn.getAttribute("aria-checked") === "true";
        btn.classList.toggle('toggle-on');
        var batchHeader = document.getElementById('<%= panelRight.ClientID %>');
        var leftImage = document.getElementById('<%= panelLeft.ClientID %>');
        var leftInfo = document.getElementById('<%= topInfo.ClientID %>');


        if (isOn) { //if current is on
            btn.setAttribute("aria-checked", "false");
            hiddenField.value = "Off";
            batchHeader.style.visibility = "hidden";

            //leftImage
            leftImage.style.width = '60%';
            leftImage.style.position = 'absolute';
            leftImage.style.marginLeft = '20%';
            leftImageHidden.style.visibility = 'hidden';
            leftImageHidden.style.display = 'block';
            //topInfo.style.marginTop = '65.5%'
        } else {
            btn.setAttribute("aria-checked", "true");
            hiddenField.value = "On";
            batchHeader.style.visibility = "visible";

            //topImage
            leftImage.style.width = '99%';
            leftImage.style.position = '';
            leftImage.style.marginLeft = '';
            leftImageHidden.style.display = 'none';
            //topInfo.style.marginTop = ''
        }
    }
</script>
</form>

</asp:Content>

