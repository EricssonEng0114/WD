<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb.Modules.BPOOutlookModule.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">
    <form runat="server" id="submitForm">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <link href="../../Content/pdsa-collapser.css" rel="stylesheet" />
        <script src="../../Scripts/pdsa-collapser.js"></script>
        <script type="text/javascript">
            function debounce(func, delay) {
                let timer;
                return function (...args) {
                    clearTimeout(timer);
                    timer = setTimeout(() => func.apply(this, args), delay);
                };
            }

            const reloadDebounced = debounce(function (values) {
                reloadInternalRecipientDropDown(values);
            }, 400); // 300–500ms is ideal

            document.addEventListener("DOMContentLoaded", () => {
                var divIntEmailRecipient = document.getElementById('divInternalEmailRecipient');

                $('#multi_internalsite').multiselect({
                    includeSelectAllOption: true,
                    numberDisplayed: 3,
                    maxHeight: 200,
                    enableFiltering: true,
                    buttonWidth: '100%',
                    enableCaseInsensitiveFiltering: true,
                    template: {
                        li: '<a class"multiselect-option dropdown-item"><a><label style="display:inline;"></label><input type="text" /></a></a>'
                    },

                    onChange: function () {
                        const values = $('#multi_internalsite').val() || [];
                        reloadDebounced(values);
                        divIntEmailRecipient.style.display = values.length > 0 ? 'block' : 'none';
                    },

                    onSelectAll: function () {
                        const values = $('#multi_internalsite').val() || [];
                        reloadDebounced(values);
                        divIntEmailRecipient.style.display = values.length > 0 ? 'block' : 'none';
                    },

                    onDeselectAll: function () {
                        reloadDebounced([]);
                        divIntEmailRecipient.style.display = 'none';
                    }
                });
            });

            function focusClientSection(msg) {
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

            function focusInternalSection(msg) {
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

                $('div[id=clientReportTab]').removeClass('in');
                $('div[id=internalReportTab]').addClass('in');
                return false;
            }



            function reloadRecipientDropDown() {
                var clientCode = $("#<%:ddlClient.ClientID %>").val();
                var obj = { clientCode: clientCode, siteLst: [] };
                var param = JSON.stringify(obj);

                $.ajax({
                    url: '/Modules/BPOOutlookModule/Default.aspx/getRecipientByClient',
                    method: 'post',
                    contentType: 'application/json; charset=utf-8',
                    data: param,
                    dataType: 'json',
                    async: false,
                    cache: false,
                    success: function (data) {
                        var result = data.d;
                        $('#multi_clientrecipient').multiselect('destroy');
                        if (result.length) {
                            $('#multi_clientrecipient').children().remove();
                            for (var i = 0; i < result.length; i++) {
                                $('#multi_clientrecipient').append('<option values="' + result[i] + '">' + result[i] + '</option>');
                            }
                        }

                        $('#multi_clientrecipient').multiselect({
                            includeSelectAllOption: true,
                            numberDisplayed: 3,
                            maxHeight: 200,
                            enableFiltering: true,
                            buttonWidth: '100%',
                            template: {
                                li: '<a class"multiselect-option dropdown-item"><a><label style="display:inline;"></label><input type="text" /></a></a>'
                            }
                        });

                        // Call the second drop down here
                        //reloadInternalRecipientDropDown();
                    },
                    error: function (xhr, status, error) {
                        console.log(xhr.responseText);  // to see the error message
                        console.log("Error connecting to server -> " + xhr.status + "-" + error);
                    }
                });
            }

            function reloadInternalRecipientDropDown(siteLst) {
                var clientCode = 'USYS';
                var obj = { clientCode: clientCode, siteLst: siteLst };
                var param = JSON.stringify(obj);

                $.ajax({
                    url: '/Modules/BPOOutlookModule/Default.aspx/getRecipientByClient',
                    method: 'post',
                    contentType: 'application/json; charset=utf-8',
                    data: param,
                    dataType: 'json',
                    async: false,
                    cache: false,
                    success: function (data) {
                        var result = data.d;
                        $('#multi_internalrecipient').multiselect('destroy');
                        if (result.length) {
                            $('#multi_internalrecipient').children().remove();
                            for (var i = 0; i < result.length; i++) {
                                $('#multi_internalrecipient').append('<option values="' + result[i] + '">' + result[i] + '</option>');
                            }
                        }

                        $('#multi_internalrecipient').multiselect({
                            includeSelectAllOption: true,
                            numberDisplayed: 3,
                            maxHeight: 200,
                            enableFiltering: true,
                            buttonWidth: '100%',
                            template: {
                                li: '<a class"multiselect-option dropdown-item"><a><label style="display:inline;"></label><input type="text" /></a></a>'
                            }
                        });
                    },
                    error: function (xhr, status, error) {
                        console.log(xhr.responseText);  // to see the error message
                        console.log("Error connecting to server -> " + xhr.status + "-" + error);
                    }
                });
            }

        </script>
        <div class="content">
            <ul class="breadcrumb">
                <li>
                    <p>Maintenance Tasks</p>
                </li>
                <li><a href="/BPOOutlook" class="active">BPO Outlook Module</a> </li>
            </ul>
            <div class="page-title">
                <i class="icon-custom-right"></i>
                <h3><span class="semi-bold">
                    <asp:Label ID="lblTitle" runat="server" Text="Send Email"></asp:Label>
                </span></h3>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <div class="panel-group" id="clientReportAccordion">
                        <div class="panel panel-primary">
                            <div class="panel-heading">
                                <div class="panel-title">
                                    <a data-toggle="collapse"
                                        data-parent="#clientReportAccordion"
                                        href="#clientReportTab">Client Report</a>
                                    <a class="pdsa-panel-toggle"></a>
                                </div>
                            </div>
                            <div id="clientReportTab" class="panel-collapse collapse in">
                                <div class="panel-body">
                                    <div class="table-responsive">
                                        <div class="grid simple">
                                            <div class="grid-title no-border">
                                                <h4>Please fill in all mandatory to send email.</h4>
                                            </div>
                                            <div class="grid-body no-border">
                                                <asp:UpdatePanel ID="ajaxPanel1" runat="server">
                                                    <ContentTemplate>
                                                        <div class="tab">
                                                            <button type="button" id="btnClientEmailContent" onclick="showClientTab(event,0)" class="tablinks">Email Contents</button>
                                                            <button type="button" id="btnClientEmailAttch" onclick="showClientTab(event,1)" class="tablinks">Attachments</button>
                                                        </div>
                                                        <div id="ClientEmailContent" class="tabcontent">
                                                            <div class="form-group">
                                                                <label class="form-label">*Client Code</label>
                                                                <span id="ClientCode_Required" class="help" style="display: none; color: red;">*Required</span>
                                                                <div class="controls">
                                                                    <asp:DropDownList ID="ddlClient" runat="server" CssClass="form-control" OnSelectedIndexChanged="ddlClient_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
                                                                </div>
                                                            </div>

                                                            <div class="form-group">
                                                                <label class="form-label" style="width: 100%">*Select Recipient(s)</label>
                                                                <span id="ClientRecipient_Required" class="help" style="display: none; color: red;">*Required</span>
                                                                <select id="multi_clientrecipient" multiple></select>
                                                            </div>





                                                            <div class="form-group">
                                                                <label class="form-label">*Email Subject</label>
                                                                <span id="ClientEmailSubject_Required" class="help" style="display: none; color: red;">*Required</span>
                                                                <div class="controls">
                                                                    <asp:TextBox ID="txtClientEmailSubject" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                                                                </div>
                                                            </div>
                                                            <div class="form-group">
                                                                <label class="form-label">Email Message</label>
                                                                <span id="ClientEmailMessage_Required" class="help" style="display: none; color: red;">*Required</span>
                                                                <div class="controls">
                                                                    <asp:TextBox ID="txtClientEmailMessage" TextMode="MultiLine" Rows="5" onkeyDown="return checkMaxLength(this,event);" runat="server" CssClass="form-control"></asp:TextBox>
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <div id="ClientEmailAttch" class="tabcontent">
                                                            <div class="form-group">
                                                                <label class="form-label" style="width: 100%;">*Select Report to attach</label>
                                                                <span id="ClientAttachment_Required" class="help" style="display: none; color: red;">*Required</span>
                                                                <br />
                                                                <asp:CheckBoxList ID="cbClientReportList"
                                                                    RepeatDirection="Vertical"
                                                                    RepeatLayout="Flow"
                                                                    CellPadding="5"
                                                                    CellSpacing="5"
                                                                    RepeatColumns="3"
                                                                    TextAlign="Right" CssClass="checkboxlist_nowrap"
                                                                    runat="server">
                                                                </asp:CheckBoxList>
                                                            </div>
                                                        </div>
                                                    </ContentTemplate>
                                                </asp:UpdatePanel>
                                                <asp:HiddenField ID="hfClientRecipient" runat="server" />
                                                <asp:Button ID="btnClientSendEmail" runat="server" Text="Send Email" CssClass="btn btn-info" OnClick="btnClientSendEmail_Click" OnClientClick="return checkClientFormField()" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="row">
                <div class="col-md-12">
                    <div class="panel-group" id="internalReportAccordion">
                        <div class="panel panel-primary">
                            <div class="panel-heading">
                                <div class="panel-title">
                                    <a data-toggle="collapse"
                                        data-parent="#internalReportAccordion"
                                        href="#internalReportTab">Internal Report</a>
                                    <a class="pdsa-panel-toggle"></a>
                                </div>
                            </div>
                            <div id="internalReportTab" class="panel-collapse collapse">
                                <div class="panel-body">
                                    <div class="table-responsive">
                                        <div class="grid simple">
                                            <div class="grid-title no-border">
                                                <h4>Please fill in all mandatory to send email.</h4>
                                            </div>
                                            <div class="grid-body no-border">
                                                <div class="form-group">
                                                    <label class="form-label">*Email Subject</label>
                                                    <span id="InternalEmailSubject_Required" class="help" style="display: none; color: red;">*Required</span>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtInternalEmailSubject" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                                                    </div>
                                                </div>

                                                <%--   <div class="form-group">
                                                    <label class="form-label">*Email Recipient (Please key in ';' before enter another recipient email address)</label>
                                                    <span id="InternalEmailRecipient_Required" class="help" style="display: none; color: red;">*Required</span>
                                                    <span id="InternalEmailAddress_Invalid" class="help" style="display: none; color: red;">*Invalid Email Address</span>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtInternalEmailRecipient" TextMode="MultiLine" Rows="3" runat="server" CssClass="form-control"></asp:TextBox>
                                                    </div>
                                                </div>--%>

                                                <div class="form-group">
                                                    <label class="form-label" style="width: 100%">*Select Sites</label>
                                                    <span id="InternalEmailSite_Required" class="help" style="display: none; color: red;">*Required</span>
                                                    <select id="multi_internalsite" multiple>
                                                        <option value="KL2">KL2</option>
                                                        <option value="KL1">KL1</option>
                                                        <option value="AS">AS</option>
                                                        <option value="PP">PP</option>
                                                        <option value="IP">IP</option>
                                                        <option value="KN">KN</option>
                                                        <option value="ML">ML</option>
                                                        <option value="JB">JB</option>
                                                        <option value="OTH">Others</option>
                                                    </select>
                                                </div>

                                                <div id="divInternalEmailRecipient" class="form-group" style="display: none">
                                                    <label class="form-label" style="width: 100%">*Select Internal Recipient(s)</label>
                                                    <span id="InternalEmailRecipient_Required" class="help" style="display: none; color: red;">*Required</span>
                                                    <select id="multi_internalrecipient" multiple></select>
                                                </div>

                                                <div class="form-group">
                                                    <label class="form-label">Email Message</label>
                                                    <span id="InternalEmailMessage_Required" class="help" style="display: none; color: red;">*Required</span>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtInternalEmailMessage" TextMode="MultiLine" Rows="5" onkeyDown="return checkMaxLength(this,event);" runat="server" CssClass="form-control"></asp:TextBox>
                                                    </div>
                                                </div>

                                                <div class="form-group">
                                                    <label class="form-label">Upload File</label>
                                                    <span id="InternalEmailAttachment_Required" class="help" style="display: none; color: red;">*Required</span>
                                                    <div class="controls">
                                                        <span class="fileinput-button btn-primary">
                                                            <i class="fa fa-plus"></i>
                                                            <span>Add files</span>
                                                            <!-- use html input file instead of asp control file upload because to unable to clear out the value if using asp control file input -->
                                                            <input type="file" id="uploadFile" multiple="multiple" class="custom-file-input" runat="server" />
                                                        </span>
                                                    </div>

                                                    <table id="fileList">
                                                        <thead>
                                                            <tr>
                                                                <th style="width: 40%"></th>
                                                                <th style="width: 5%"></th>
                                                                <th style="width: 55%"></th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                        </tbody>
                                                    </table>
                                                </div>
                                                <asp:HiddenField ID="hfMaxFileSize" runat="server" />
                                                <asp:HiddenField ID="hfAllowFileType" runat="server" />
                                                <asp:HiddenField ID="hfFileCountAllowed" runat="server" />
                                                <asp:HiddenField ID="hfInternalAttachment" runat="server" />
                                                <asp:HiddenField ID="hfInternalRecipient" runat="server" />

                                                <asp:Button ID="btnInternalSendEmail" runat="server" Text="Send Email" CssClass="btn btn-info" OnClick="btnInternalSendEmail_Click" OnClientClick="return checkInternalFormField()" />
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
    </form>
    <asp:PlaceHolder runat="server">
        <%: Scripts.Render("~/Tab/js") %>
        <%: Styles.Render("~/Tab/css") %>    
    </asp:PlaceHolder>
    <script type="text/javascript">
        $(document).ready(function () {

            $('input[id*=uploadFile]').attr('accept', $('input[id*=hfAllowFileType]').val());

            if (document.getElementById("btnClientEmailContent") != null)
                document.getElementById("btnClientEmailContent").click();

            $('input[id*=uploadFile]').off('change').on('change', function (e) {
                var valid = true;

                if (e.target.files.length > 0) {
                    var fileCountAllowed = document.getElementById('<%:hfFileCountAllowed.ClientID%>').value;
                    var fileCountAllowed = Number(fileCountAllowed);
                    var maxFileSize = document.getElementById('<%:hfMaxFileSize.ClientID%>').value;
                    var maxFileSize = Number(maxFileSize);

                    if (e.target.files.length > fileCountAllowed) {
                        alert("Only can upload maximum " + fileCountAllowed + " file at the same time");
                        valid = false;
                    }

                    var totalSize = 0;
                    for (var i = 0, il = e.target.files.length; i < il; i++) {
                        totalSize += e.target.files[i].size;
                    }

                    if (totalSize > maxFileSize) {
                        alert("Upload failed. Total file size must not exceeding 20MB!");
                        valid = false;
                    }

                    var table = document.getElementById('fileList').getElementsByTagName('tbody')[0];
                    var fileTypeAllowed = document.getElementById('<%:hfAllowFileType.ClientID%>').value.split(',');

                    for (var l = 0; l < e.target.files.length; l++) {
                        var filename = e.target.files[l].name;
                        var ext = filename.substr(filename.lastIndexOf('.')).toLowerCase();
                        if (fileTypeAllowed.indexOf(ext) < 0) {
                            alert("Only allow to upload files with: \n" + fileTypeAllowed);
                            valid = false;
                            break;
                        }
                    }

                    if (!valid) {
                        $el = $(e.target);
                        $el.wrap('<form>').closest('form').get(0).reset();
                        $el.unwrap();
                    }
                    else {
                        for (var l = 0; l < e.target.files.length; l++) {
                            var filename = e.target.files[l].name;
                            var ext = filename.substr(filename.lastIndexOf('.')).toLowerCase();
                            if (fileTypeAllowed.indexOf(ext) < 0) {
                                continue;
                            }

                            var id = "tr-" + e.target.files[l].name;
                            var tr = $('tr[id="' + id + '"]');
                            if (tr.length > 0) {
                                tr.remove();
                            }

                            var tr = table.insertRow(-1);
                            tr.id = "tr-" + e.target.files[l].name;
                            var td = document.createElement('td');
                            td = tr.insertCell(-1);

                            var label = document.createElement('label');
                            label.innerText = e.target.files[l].name;
                            td.appendChild(label);

                            td = tr.insertCell(-1);
                            var div = document.createElement('div');
                            div.className = "progress progress-striped active";
                            div.style.width = "100%";
                            var innerDiv = document.createElement('div');
                            innerDiv.id = "progress-bar-" + e.target.files[l].name;
                            innerDiv.className = "progress-bar progress-bar-success";
                            innerDiv.style.width = "0%";
                            div.appendChild(innerDiv);
                            td.appendChild(div);

                            var btnDelete = document.createElement('button');
                            btnDelete.id = "btn-delete-" + e.target.files[l].name;
                            btnDelete.style.display = "none";
                            btnDelete.type = "button";
                            btnDelete.onclick = function () {
                                removeRow(this);
                                return false;
                            }

                            var i = document.createElement('i');
                            i.className = "fa fa-times";
                            btnDelete.appendChild(i);
                            td.appendChild(btnDelete);
                        }

                        uploadMultipleFile(e.target.files);
                    }
                }
            });

            //$('[id*=txtInternalEmailRecipient]').off('change').on('change', function (e) {
            //    $('[id*=txtInternalEmailRecipient]').val(e.target.value.replace(/\s/g, ''));
            //});
        });

        function removeRow(btn) {
            var trID = btn.id.replace('btn-delete-', 'tr-');
            var tr = document.getElementById(trID);
            if (tr != null) {
                tr.remove();
            }
        }

        function uploadMultipleFile(files) {
            const formData = new FormData();
            for (var i = 0, il = files.length; i < il; i++) {
                formData.append(files[i].name, files[i])
            }

            $.ajax({
                url: "FileUploadHandler.ashx",
                type: 'POST',
                async: false,
                data: formData,
                contentType: false,
                processData: false,
                complete: function (data) {
                    for (var i = 0, il = files.length; i < il; i++) {
                        var fileName = files[i].name;
                        var progress = document.getElementById('progress-bar-' + fileName);
                        var btnDelete = document.getElementById('btn-delete-' + fileName);

                        if (btnDelete != null) btnDelete.style.display = "";
                        if (progress != null) progress.parentElement.style.display = "none";
                    }
                },

                success: function (data) {
                    /*
                    if (current < max)
                    {
                        var next = current + 1;
                        uploadFile(next, max, files);
                    }
                    */
                },
                error: function (err) {
                    console.log(err);
                    console.log("error:" + err.responseText);
                },
                failure: function (fail) {
                    console.log("fail:" + fail.responseText);
                }/*,
                xhr: function () {
                    
                    var fileXhr = $.ajaxSettings.xhr();
                    var progress = document.getElementById('progress-bar-' + fileName);
                    var btnDelete = document.getElementById('btn-delete-' + fileName);

                    if (fileXhr.upload) {
                        fileXhr.upload.addEventListener("progress", function (e) {
                            if (e.lengthComputable) {
                                var percentage = Math.ceil(((e.loaded / e.total) * 100));
                                if (progress != null) progress.style.width = percentage + "%";
                                if (percentage == 100) {
                                    if (btnDelete != null) btnDelete.style.display = "";
                                    if (progress != null) progress.parentElement.style.display = "none";
                                }
                            }
                        }, false);
                    }
                    return fileXhr;
                    
                }
                */
            });
        }

        function showClientTab(evt, tabNo) {
            var tabContent = document.getElementsByClassName('tabcontent');
            var tabButton = document.getElementsByClassName('tablinks');
            var targetDiv = evt.target.id.replace('btn', '');

            for (var i = 0; i < tabContent.length; i++) {
                if (tabContent[i].id.indexOf('Client') > -1) {
                    if (tabContent[i].id == targetDiv) {
                        tabContent[i].style.display = "block";
                    }
                    else {
                        tabContent[i].style.display = "none";
                    }
                }
            }

            for (var i = 0; i < tabButton.length; i++) {
                if (tabButton[i].id.indexOf('Client') > -1) {
                    if (tabButton[i].id === evt.target.id) {
                        tabButton[i].className = "tablinks active";
                    }
                    else {
                        tabButton[i].className = "tablinks";
                    }
                }
            }
        }

        function reloadForm() {
            if (document.getElementById("btnClientEmailContent") != null)
                document.getElementById("btnClientEmailContent").click();

            reloadRecipientDropDown();
        }

        function checkClientFormField() {
            var valid = true;
            var bFocusClientAttachmentTab = false;
            var bFocusClientEmailFormTab = false;
            var clientCode = $("#<%:ddlClient.ClientID %>").val();
            var emailSubject = $("#<%:txtClientEmailSubject.ClientID %>").val();
            var emailMessage = $("#<%:txtClientEmailMessage.ClientID %>").val();
            var selectedRecipient = $('ul.multiselect-container>li>a>label>input[type=checkbox]:checked');
            var selectedReport = $('input[id*=cbClientReportList_]:checked');

            loadSpinner();

            $('#ClientCode_Required').hide();
            $('#ClientEmailMessage_Required').hide();
            $('#ClientEmailSubject_Required').hide();
            $('#ClientAttachment_Required').hide();
            $('#ClientRecipient_Required').hide();

            if (clientCode.length == 0) {
                valid = false;
                $('#ClientCode_Required').show();
                bFocusClientEmailFormTab = true;
            }

            if (selectedRecipient.length == 0) {
                valid = false;
                $('#ClientRecipient_Required').show();
                bFocusClientEmailFormTab = true;
            }
            else {
                var str = "";
                for (var i = 0; i < selectedRecipient.length; i++) {
                    if ($(selectedRecipient[i]).val() == "multiselect-all") continue;
                    if (str.length > 0) str += ";";

                    str += $(selectedRecipient[i]).val();

                    $("#<%:hfClientRecipient.ClientID%>").val(str);
                }
            }

            if (emailSubject.length == 0) {
                valid = false;
                $('#ClientEmailSubject_Required').show();
                bFocusClientEmailFormTab = true;
            }

            if (emailMessage.length == 0) {
                valid = false;
                $('#ClientEmailMessage_Required').show();
                bFocusClientEmailFormTab = true;
            }

            if (selectedReport.length == 0) {
                if (valid) bFocusClientAttachmentTab = true;
                valid = false;
                $('#ClientAttachment_Required').show();
            }

            if (!valid) {
                isLoadSpinner = false;
                unloadSpinner();

                if (!bFocusClientEmailFormTab) {
                    if (bFocusClientAttachmentTab) {
                        $('#btnClientEmailAttch').click();
                    }
                }
                else {
                    $('#btnClientEmailContent').click();
                }
            }

            return valid;
        }

        function checkInternalFormField() {
            var valid = true;
            loadSpinner();
            $('#InternalEmailSubject_Required').hide();
            $('#InternalEmailRecipient_Required').hide();
            $('#InternalEmailAddress_Invalid').hide();
            $('#InternalEmailMessage_Required').hide();
            $('#InternalEmailAttachment_Required').hide();
            $('#InternalEmailSite_Required').hide();

            var emailSubject = $('#<%:txtInternalEmailSubject.ClientID%>').val();
            var emailRecipient = $('#multi_internalrecipient>option:selected');
            var emailSite = $('#multi_internalsite>option:selected');


<%--            $('#<%:txtInternalEmailRecipient.ClientID%>').val();--%>
            var emailMessage = $('#<%:txtInternalEmailMessage.ClientID%>').val();

            var attachmentList = $('table[id=fileList]>tbody>tr');

            if (emailSubject.length == 0) {
                valid = false;
                $('#InternalEmailSubject_Required').show();
            }

            //if (emailRecipient.length == 0) {
            //    valid = false;
            //    $('#InternalEmailRecipient_Required').show();
            //}
            //else
            //{
            //    //check email address
            //    var arrEmail = emailRecipient.split(';');
            //    for (var i = 0, il = arrEmail.length; i < il; i++) {
            //        var ret = checkEmailAddress(arrEmail[i]);
            //        if(!ret)
            //        {
            //            $('#InternalEmailAddress_Invalid').show();
            //            valid = false;
            //            break;
            //        }
            //    }
            //}

            if (emailSite.length === 0) {
                valid = false;
                $('#InternalEmailSite_Required').show();
            }

            if (emailRecipient.length == 0) {
                valid = false;
                $('#InternalEmailRecipient_Required').show();
            }
            else {

                var str = "";

                for (var i = 0; i < emailRecipient.length; i++) {
                    if ($(emailRecipient[i]).val() == "multiselect-all") continue;
                    if (str.length > 0) str += ";";

                    str += $(emailRecipient[i]).val();
                }

                $("#<%:hfInternalRecipient.ClientID%>").val(str);

            }


            if (emailMessage.length == 0) {
                valid = false;
                $('#InternalEmailMessage_Required').show();
            }

            if (attachmentList.length == 0) {
                valid = false;
                $('#InternalEmailAttachment_Required').show();
            }
            else {
                var str = "";
                for (var i = 0; i < attachmentList.length; i++) {
                    var fileName = attachmentList[i].id.replace('tr-', '');

                    if (str != "") str += ";";

                    str += fileName;
                }

                $('#<%:hfInternalAttachment.ClientID%>').val(str);
            }

            if (!valid) {
                isLoadSpinner = false;
                unloadSpinner();
            }

            return valid;
        }

        function checkMaxLength(txtBox, e) {
            if (!checkSpecialKeys(e)) {
                if (txtBox.value.length <= 150) {
                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                return true;
            }
        }

        function checkSpecialKeys(e) {
            if (e.keyCode != 8 && e.keyCode != 46 && e.keyCode != 37 && e.keyCode != 38 && e.keyCode != 39 && e.keyCode != 40)
                return false;
            else
                return true;
        }
    </script>
    <style>
        .checkboxlist_nowrap label {
            display: inline;
            margin-left: 10px !important;
        }

        input[id*=cbClientReportList_] {
            margin-left: 1.5em !important;
        }

        input[id*=btnClientSendEmail], input[id*=btnInternalSendEmail] {
            margin-top: 1.5em;
        }

        .form-check .form-check-label {
            display: inline-block !important;
        }

        button.multiselect {
            border: 1px solid #8b91a0;
            border-radius: 4px;
        }

        .multiselect-container > li > a > label.checkbox > input[type=checkbox] {
            display: block !important;
            margin-top: 1px;
        }

        ul.multiselect-container {
            border: 1px solid rgba(0,0,0,.15);
            width: 100%;
        }

        .fileinput-button {
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
        }

        .custom-file-input {
            position: absolute;
            top: 0;
            right: 0;
            margin: 0;
            opacity: 0;
            font-size: 200px !important;
            direction: ltr;
            cursor: pointer;
            display: block;
        }

        table[id*=fileList] {
            width: 100%;
            max-width: 100%;
            margin-bottom: 20px;
            border-spacing: 0;
            border-collapse: collapse;
            margin-top: 10px;
        }

            table[id*=fileList] > tbody > tr > td {
                margin: 0;
                padding: 0;
            }

        button[id*=btn-delete-] {
            background-color: #FFFFFF;
            color: #f35958;
            border: none;
        }
    </style>
</asp:Content>
