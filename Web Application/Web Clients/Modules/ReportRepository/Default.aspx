<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb.Modules.ReportRepository.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">    

<form runat="server">

 <div class="content">
    <ul class="breadcrumb">
        <li>
            <p>Operator Tasks</p>
        </li>
        <li><a href="/ReportRepository" class="active">Report Repository</a></li>
    </ul>
    <div class="page-title">
        <i class="icon-custom-right"></i>
        <h3><span class="semi-bold">Report File(s)</span></h3>
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
                                <span id="Date_Invalid" class="help" style="display: none; color: red;">*Invalid Date Selected</span>

                            </div>
                        </div>
                                            
                        <div class="col-md-2">
                            <div class="form-group">
                                <label class="form-label">Client Code:</label>
                                <asp:DropDownList ID="ddlClient" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlClient_SelectedIndexChanged1"></asp:DropDownList>
                            </div>
                        </div>
                        
                        <div class="col-md-6">
                            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-success" style="margin-top:25px;margin-right:1px;"  OnClick="btnSearch_Click" OnClientClick="return checkSeachData()" />
                            <asp:HiddenField ID="hfUploadFlag" runat="server" />

                            <span class="fileinput-button btn-primary" id="UploadSection">
                                <i class="fa fa-plus"></i>
                                <span>Upload</span>
                                <asp:FileUpload ID="uploadFile" CssClass="custom-file-input" AllowMultiple="true" runat="server" accept=".rpt" />
                            </span>
                            <div style="margin-top:1px;margin-right:1px;">
                             <asp:Label ID="lblInvalidDataSelected" runat="server" Text="*Please select a date up to 3 days of history date" ForeColor="Red" Font-Bold="True" Visible="False"></asp:Label>
                            </div>
                            
                         </div>
                        
                        
                    </div>
                

    

                </div>
                
                <div class="grid-body">

<%--                    <button id="download-selected" class="btn btn-primary">Download Selected</button>--%>

                    <table id="dataTable" class="table table-hover table-condensed">
                        <thead>
                              <tr class="dataTable_HeaderRow">
                                <th><div class="checkbox-container"><input type="checkbox" id="select-all" class="enlarged-checkbox"></div></th>
                                <th>File Name</th>
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
     
 <script type="text/javascript">
     var  hideRpt = '<%:   Session["HideReport"] %>';
     var isUsysUsr = '<%: Session["s_UserForUnisys"] %>';
     var activeBusdate = '<%: Session["CurBusdate"] %>';
     $(document).ready(function () {

         $('#select-all').click(function () {
             
             var isChecked = $(this).is(':checked');
             $('.file-checkbox').prop('checked', isChecked);
         });

         $('#dataTable').on('draw.dt', function () {            
             $('.file-checkbox').prop('checked', false);
             $('#select-all').prop('checked', false); 
         });

              
         $('#dataTable_paginate a').on('click', function (event) {
             event.preventDefault(); // Prevent default link behavior
             alert('You clicked a pagination link.');
             // Perform any additional actions you need here
         });

         // UploadSection - Non Unisys User cannot upload
         if (isUsysUsr == "False") {
             $('#UploadSection').hide();             
         }
         else {
             $('#UploadSection').show();
             toggleUploadButton();
         }

         validNavigation = true;

         $("#<%:hfUploadFlag.ClientID%>").val("0");

        if (navigator.appVersion.indexOf("MSIE") != -1)
            $('select').ieExpandSelectWidth();

        //Dialog
        var redirectURL;

        $("#<%: txtDateTime.ClientID %>").datepicker(
             {
                
                 autoclose: true,
                 changeMonth: true,
                 changeYear: true,
                 format: 'mm/dd/yyyy'
             }).on("changeDate", function (e) {
                 toggleUploadButton();
                 $("#<%: btnSearch.ClientID %>").click();
             });//.datepicker("setDate", new Date());;
         
         $("#<%: txtDateTime.ClientID %>").attr("readonly", true);
         
         $('input[id*=uploadFile]').off('change').on('change', function (e) {
             if (e.target.files.length > 0) {                
                 uploadFile(e.target.files);
             }
         });
         

         if (hideRpt == "1") {
           
             initRepoReportDataTable($("#<%: txtDateTime.ClientID %>").val(), "N");
         }
         else
         {
             initRepoReportDataTable($("#<%: txtDateTime.ClientID %>").val(), $("#<%: ddlClient.ClientID %>").val());
         }
       
         configureTable();

         // Create download selected button
         var button = $('<button/>', {
             html: '<i class="fa fa-download"></i> Download Selected', 
             id: 'download-selected',
             click: function () {
                 var selectedFiles = [];
                 $('.file-checkbox:checked').each(function () {
                     // Skip the "select all" checkbox
                     if (this.id !== 'select-all') {
                         selectedFiles.push($(this).val());
                     }
                 });

                 if (selectedFiles.length > 0) {
                     downloadFiles(selectedFiles);
                 } else {
                     alert('No files selected for download.');
                 }
             }
         }).css({
             'margin-left': '15px',  
             //'background-color': '#69797e'  // Change the background color to gery
         }).addClass('btn btn-primary');

         // Append the button to the length menu
         $('.dataTables_length').append(button);

     });

     function downloadFiles(selectedFiles) {
         $.ajax({
             url: '/Modules/ReportRepository/Default.aspx/DownloadFiles',
             method: 'post',
             data: JSON.stringify({ files: selectedFiles }),
             contentType: 'application/json; charset=utf-8',
             dataType: 'json',
             success: function (data) {
                 // Redirect to the returned URL to initiate the download
                 window.location.href = data.d;
             },
             error: function (xhr, status, error) {
                 console.log(xhr.responseText);
                 alert("Error connecting to server -> " + xhr.status + "-" + error);
             }
         });
     }

     function toggleUploadButton() {
         var selDateTime = $("#<%: txtDateTime.ClientID %>").val();
         var selDateParts = selDateTime.split('/');
         var month = parseInt(selDateParts[0]);
         var day = parseInt(selDateParts[1]);
         var year = parseInt(selDateParts[2]);

         //If it's not valid date selected, hide the upload button
         if (checkValidDate(month, day, year)) {
             //Busdate selected, allow to click Upload
             $('.fileinput-button').css("pointer-events", "auto").css("background-color", "#0aa699");

         } else {
             //Not busdate, not allow to upload, disable
             $('.fileinput-button').css("pointer-events", "none").css("background-color", "#808080");


         }

     }

     function checkValidDate(m, d, y) {
         try {
             //var currentDate = activeBusdate.split('/');//new Date();
             //alert(currentDate);
             //var currentYear = currentDate.getFullYear();
             //var currentMonth = currentDate.getMonth() + 1;
             //var currentDay = currentDate.getDate();


             var dateParts = activeBusdate.split('/');
             var activeMonth = parseInt(dateParts[0]);
             var activeDay = parseInt(dateParts[1]);
             var activeYear = parseInt(dateParts[2]);

             if (y > activeYear || (y == activeYear && m > activeMonth) || (y == activeYear && m == activeMonth && d > activeDay)) {
                 return false; // The date is in the future
             } else {
                 return true; // The date is not in the future
             }
         } catch (e) {
             return false;
         }
     }

     
     function checkBusDate(m, d, y) {
         try {
             var dateParts = activeBusdate.split('/');
             var activeMonth = parseInt(dateParts[0]);
             var activeDay = parseInt(dateParts[1]);
             var activeYear = parseInt(dateParts[2]);
             //console.log(activeBusdate);

             if (activeMonth == m && activeYear == y && activeDay == d) return true;
             else return false;
         } catch (e) {
             return false;
         }
     }

     function uploadFile(files) {
         var webMaxFileSizeLimit = '<%: Session["s_MaxRequestLength"] %>';
         var fileTypeAllowed = '<%:   Session["s_FileTypes"] %>';
         var fileCountAllowed = '<%:   Session["s_FileCount"] %>';


         var totalSize = 0;
         var totalInvalidFileType = 0;

         if (fileCountAllowed < files.length) {
             alert("Upload failed. You must not upload more than " + fileCountAllowed + " files");
         }

         else {

             for (let i = 0; i < files.length; i++) {
                 totalSize += files[i].size;


                 var fileName = files[i].name;
                 var idxDot = fileName.lastIndexOf(".") + 1;
                 var extFile = fileName.substr(idxDot, fileName.length).toLowerCase();
                 if (fileTypeAllowed.indexOf(extFile) !== -1) {
                     //TO DO
                 } else {
                     totalInvalidFileType += 1;
                 }
             }


             if (totalSize > parseInt(webMaxFileSizeLimit)) {//20000000) {
                 alert("Upload failed. Total file size must not exceeding 20MB!");
             }
             else {

                 if (totalInvalidFileType > 0) {
                     alert("Only allow to upload files with: \n" + fileTypeAllowed);
                 }
                 else {
                     //uploadSingleFile(0, (files.length - 1), files);              
                     uploadMultipleFile(files);
                 }
             }
         }
     }

     function uploadMultipleFile(files)
     {
         const formData = new FormData();
         for(var i = 0,il = files.length;i<il;i++){
             formData.append(files[i].name, files[i]);
         }

         var xhr = new XMLHttpRequest();
         xhr.open('POST', 'FileUploadHandler.ashx');
         //xhr.setRequestHeader("Content-Type", false);
         //xhr.setRequestHeader("X-File-Type", files[i].type);
         //xhr.setRequestHeader("X-File-Size", files[i].size);
         //xhr.setRequestHeader("X-File-Name", files[i].name);
         xhr.onload = function () {
             if (this.status === 200) {
                 console.log(this.responseText);
                 refreshPage(files);
             }
         };

         xhr.send(formData);
     }

     function refreshPage(files)
     {
         if (files.length > 0) {
             //Upon done uploading, reload datagrid
             $("#<%:hfUploadFlag.ClientID%>").val("1");
             alert("Upload Success!");
             document.forms[0].submit();
         }
         else {
             $("#<%:hfUploadFlag.ClientID%>").val("0");
         }
     }

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

    <div id="dialog-delete" title="Delete Report" style="display: none">
    <p>Are you confirm to delete selected Report File?</p>
</div>


 </form>
    <style>
        
        .fileinput-button
        {
            position: relative;
            overflow: hidden;
            display: inline-block;
            padding: 6px 12px;
            margin-bottom: 0;
            font-size: 14px;
            font-weight: 400;
            line-height: 1.42857143;
            text-align: center;
            white-space: nowrap;
            vertical-align: middle;
            -ms-touch-action: manipulation;
            touch-action: manipulation;
            cursor: pointer;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            user-select: none;
            background-image: none;
            border: 1px solid transparent;
            border-radius: 4px;
            margin-top:25px;
        }

        .custom-file-input
        {
            position: absolute;
            top: 0;
            right: 0;
            margin: 0;
            opacity: 0;
            font-size: 200px !important;
            direction: ltr;
            cursor: pointer;
            display:block;
        }

        .checkbox-container {
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100%;
        }

        .enlarged-checkbox {
            width: 18px;
            height: 18px;
        }

      
    </style>
</asp:Content>



