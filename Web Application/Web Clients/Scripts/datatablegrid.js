//Dialog
var redirectURL;
var table;
var deleteTableID;
var editTableID;


function initAnnouncementDataTable(client, title, desc) {
    var paramList = client + "|" + title + "|" + desc;

    table = $('#dataTable').DataTable({
        "initComplete": function (settings, json) {
            //// CSS
            $("#dataTable_filter input").addClass("textbox");
            //Append Show [DropDown Select] Entries
            $("#dataTable_length select").before("Show ");
            $("#dataTable_length select").after(" entries.");

        },
        "autoWidth": true,
        "bProcessing": true,
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "ModuleName", "value": "AnnouncementMaintenance" });
            aoData.push({ "name": "Param", "value": paramList });
            aoData.push({ "name": "ParamCount", "value": 3 });
        },
        "sServerMethod": "post",
        "aoColumns": [
            {
                "class": "details-content",
                "data": "AnnID",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "10%"
            },
            {
                "class": "details-content",
                "data": "Client",
                "orderable": true,
                "width": "10%"
            },
            {
                "class": "details-content",
                "data": "Title",
                "orderable": true,
                "width": "10%"
            },
            {
                "class": "details-content",
                "data": "Desc",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "20%"
            },
            {
                "class": "details-content",
                "data": "DateFrom",
                "render": function (value) {
                    if (value === null) return "";
                    var pattern = /Date\(([^)]+)\)/;
                    var results = pattern.exec(value);
                    var dtFrom = new Date(parseFloat(results[1]));

                    return pad((dtFrom.getMonth() + 1), 2, '0') + "/" + pad(dtFrom.getDate(), 2, '0') + "/" + dtFrom.getFullYear();
                },
                "width": "10%"
            },
            {
                "class": "details-content",
                "data": "DateTo",
                "render": function (value) {
                    if (value === null) return "";
                    var pattern = /Date\(([^)]+)\)/;
                    var results = pattern.exec(value);
                    var dtFrom = new Date(parseFloat(results[1]));

                    return pad((dtFrom.getMonth() + 1), 2, '0') + "/" + pad(dtFrom.getDate(), 2, '0') + "/" + dtFrom.getFullYear();
                },
                "width": "10%"
            },
            {
                "class": "details-content",
                "data": null,
                // "defaultContent": '<div><img id="Edit" src="../../images/pencil.png" alt="Edit" title="Edit">&nbsp&nbsp&nbsp<img id="DeleteOp" src="../../images/delete.png" alt="Delete" title="Delete" ></div>',
                "render": function (data, type, row, meta) {
                    return '<div><img id="Edit" src="../../images/pencil.png" alt="Edit" title="Edit"  onclick="onSetTempParam(' + row.AnnID + ');">&nbsp&nbsp&nbsp<img id="Delete" src="../../images/delete.png" alt="Delete" title="Delete" onclick="onSetDelTempID(' + row.AnnID + ');"><input class="Mod_' + row.AnnID + '"type="hidden" name="DirName" value="AM"/><input class="Row_' + row.AnnID + '"type="hidden" name="DirName" value="' + row.AnnID + '"/></div>'
                },

                "width": "15%"
            }


            //{
            //    "class": "details-content",
            //    "data": null,
            //    "defaultContent": '<div><img id="Edit" src="../../images/pencil.png" alt="Edit" title="Edit">&nbsp&nbsp&nbsp<img id="Delete" src="../../images/delete.png" alt="Delete" title="Delete" ></div>',
            //    "width": "15%"
            // }
        ],
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": true,
        "order": [[0, "asc"]]
    });

}

function initSystemDataTable(parName, parval1, parval2) {
    var paramList = parName + "|" + parval1 + "|" + parval2;

    table = $('#dataTable').DataTable({
        "initComplete": function (settings, json) {
            //// CSS
            $("#dataTable_filter input").addClass("textbox");
            //Append Show [DropDown Select] Entries
            $("#dataTable_length select").before("Show ");
            $("#dataTable_length select").after(" entries.");

        },
        "autoWidth": true,
        "bProcessing": true,
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "ModuleName", "value": "SystemMaintenance" });
            aoData.push({ "name": "Param", "value": paramList });
            aoData.push({ "name": "ParamCount", "value": 3 });
        },
        "sServerMethod": "post",
        "aoColumns": [

            {
                "class": "details-content",
                "data": "ParamName",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "20%"
            },
            {
                "class": "details-content",
                "data": "Value1",
                "orderable": true,
                "width": "21%"
            },
            {
                "class": "details-content",
                "data": "Value2",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "21%"
            },
            {
                "class": "details-content",
                "data": "Comment",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "22%"
            },
            {
                "class": "details-content",
                "data": null,
                //"defaultContent": '<div><img id="Edit" src="../../images/pencil.png" alt="Edit" title="Edit">&nbsp&nbsp&nbsp<img id="Delete" src="../../images/delete.png" alt="Delete" title="Delete" ></div>',
                "render": function (data, type, row, meta) {
                    return '<div><img id="Edit" src="../../images/pencil.png" alt="Edit" title="Edit"  onclick="onSetTempParam(' + row.TempID + ');">&nbsp&nbsp&nbsp<img id="Delete" src="../../images/delete.png" alt="Delete" title="Delete" onclick="onSetDelTempID(' + row.TempID + ');"><input class="Mod_' + row.TempID + '"type="hidden" name="DirName" value="SM"/><input class="Row_' + row.TempID + '"type="hidden" name="DirName" value="' + row.ParamName + '"/></div>'
                },

                "width": "10%"
            }
        ],
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": true,
        "order": [[0, "asc"]]
    });

}

function initOperatorDataTable(usrID, usrName, client, group) {
    var paramList = usrID + "|" + usrName + "|" + client + "|" + group;

    table = $('#dataTable').DataTable({
        "initComplete": function (settings, json) {
            //// CSS
            $("#dataTable_filter input").addClass("textbox");
            //Append Show [DropDown Select] Entries
            $("#dataTable_length select").before("Show ");
            $("#dataTable_length select").after(" entries.");

        },
        "autoWidth": true,
        "bProcessing": true,
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "ModuleName", "value": "OperatorMaintenance" });
            aoData.push({ "name": "Param", "value": paramList });
            aoData.push({ "name": "ParamCount", "value": 4 });
        },
        "sServerMethod": "post",
        "aoColumns": [
            {
                "class": "details-content",
                "data": "UsrID",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "10%"
            },
            {
                "class": "details-content",
                "data": "UsrName",
                "orderable": true,
                "width": "20%"

            },
            {
                "class": "details-content",
                "data": "UsrGp",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "15%"
            },
            {
                "class": "details-content",
                "data": "UsrClnts",
                "orderable": true,
                "width": "17%"
            },
            {
                "class": "details-content",
                "data": "UsrComment",
                "orderable": true,
                "width": "17%"
            },
            {
                "class": "details-content",
                "data": null,
                // "defaultContent": '<div><img id="Edit" src="../../images/pencil.png" alt="Edit" title="Edit">&nbsp&nbsp&nbsp<img id="DeleteOp" src="../../images/delete.png" alt="Delete" title="Delete" ></div>',
                "render": function (data, type, row, meta) {
                    return '<div><img id="Edit" src="../../images/pencil.png" alt="Edit" title="Edit"  onclick="onSetTempParam(' + row.TempID + ');">&nbsp&nbsp&nbsp<img id="Delete" src="../../images/delete.png" alt="Delete" title="Delete" onclick="onSetDelTempID(' + row.TempID + ');"><input class="Mod_' + row.TempID + '"type="hidden" name="DirName" value="OP"/><input class="Row_' + row.TempID + '"type="hidden" name="DirName" value="' + row.UsrID + '"/></div>'
                },

                "width": "20%"
            }
        ],
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": true,
        "order": [[0, "asc"]]
    });

}

function initMonitorDataTable(client, dt) {
    var paramList = client + "|" + dt;

    var monitorTable = $('#dataTable2').DataTable({
        "initComplete": function (settings, json) {
            //// CSS
            $("#dataTable_filter input").addClass("textbox");
            //Append Show [DropDown Select] Entries
            $("#dataTable_length select").before("Show ");
            $("#dataTable_length select").after(" entries.");

        },
        "bDestroy": true,
        "autoWidth": true,
        "bProcessing": true,
        "lengthChange": false,
        "pageLength": -1,
        "scrollY": "500px",
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "ModuleName", "value": "Dashboard" });
            aoData.push({ "name": "Param", "value": paramList });
            aoData.push({ "name": "ParamCount", "value": 2 });
        },
        "sServerMethod": "post",
        "aoColumns": [
            {
                "class": "text-left",
                "data": "ProcessingBranch",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "20%"
            },
            {
                "class": "details-content",
                "data": "TotalBatches",
                "orderable": true,
                "width": "15%"

            },
            {
                "class": "details-content",
                "data": "EmptyBatches",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "15%"
            },
            {
                "class": "details-content",
                "data": "SOD",
                "orderable": true,
                "width": "13%"
            },
            {
                "class": "details-content",
                "data": "EOD",
                "orderable": true,
                "width": "13%"
            },
            {
                "class": "details-content",
                "data": "NCF",
                "orderable": true,
                "width": "13%"
            },
        ],
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": false,
        columnDefs: [
            {
                targets: 3,
                render: function (data, type, row) {
                    return data === 'True'
                        ? '<i class="fa fa-check text-success" style="font-size: 20px;"></i>'
                        : '<i class="fa fa-times text-danger" style="font-size: 20px;"></i>';
                }
            },
            {
                targets: 4,
                render: function (data, type, row) {
                    return data === 'True'
                        ? '<i class="fa fa-check text-success" style="font-size: 20px;"></i>'
                        : '<i class="fa fa-times text-danger" style="font-size: 20px;"></i>';
                }
            },
            {
                targets: 5,
                render: function (data, type, row) {
                    return data === 'True'
                        ? '<i class="fa fa-check text-success" style="font-size: 20px;"></i>'
                        : '<i class="fa fa-times text-danger" style="font-size: 20px;"></i>';
                }
            }
        ]
        //"order": [[0, "asc"]]
    });

    monitorTable.on('init', function () {
        updateLabels();
    });
}

function initReportDataTable(dt, client, worksource) {

    //If different client selected, date need to be updated again to latest client's busdate
    var dtToSelect = dt;
    if (selectedBusdate.length > 1) {
        dtToSelect = selectedBusdate;
    }

    var paramList = dtToSelect + "|" + client + "|" + worksource;

    table = $('#dataTable').DataTable({
        "initComplete": function (settings, json) {
            //// CSS
            $("#dataTable_filter input").addClass("textbox");
            //Append Show [DropDown Select] Entries
            $("#dataTable_length select").before("Show ");
            $("#dataTable_length select").after(" entries.");
            //to hide loading screen when click generate report
            attachEvent();
        },
        "autoWidth": true,
        "bProcessing": true,
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "ModuleName", "value": "Reports" });
            aoData.push({ "name": "Param", "value": paramList });
            aoData.push({ "name": "ParamCount", "value": 3 });
        },
        "sServerMethod": "post",
        "aoColumns": [
            {
                "class": "details-content",
                "data": "ReportCode",
                "style": "vertical-align: top",
                "width": "45%"
            },
            {
                "class": "details-content",
                "data": "ReportDesc",
                "width": "45%"
            },
            {
                "class": "details-content",
                "style": "vertical-align: top",
                "render": function (data, type, row, meta) {
                    return "<form name='viewRpt' target='blank' action='/ViewReport' method='post'>"
                        + "<input type='hidden' name='rptCode' value='" + row.ReportCode + "'/>"
                        + "<input type='hidden' name='clientCode' value='" + row.ReportClient + "'/>"
                        + "<input type='submit' class='btn btn-info btn-small' id='viewRpt' value='View Report' />"
                        + "</form>";
                },
                "width": "10%",
                "orderable": false
            },
        ],
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": true,
        "order": [[0, "asc"]]
    });
}

function initArchivalReportDataTable(dt, client, worksource) {

    //If different client selected, date need to be updated again to latest client's busdate
    var dtToSelect = dt;
    if (selectedBusdate.length > 1) {
        dtToSelect = selectedBusdate;
    }

    var paramList = dtToSelect + "|" + client + "|" + worksource;

    table = $('#dataTable').DataTable({
        "initComplete": function (settings, json) {
            //// CSS
            $("#dataTable_filter input").addClass("textbox");
            //Append Show [DropDown Select] Entries
            $("#dataTable_length select").before("Show ");
            $("#dataTable_length select").after(" entries.");
            //to hide loading screen when click generate report
            attachEvent();
        },
        "autoWidth": true,
        "bProcessing": true,
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "ModuleName", "value": "Reports" });
            aoData.push({ "name": "Param", "value": paramList });
            aoData.push({ "name": "ParamCount", "value": 3 });
        },
        "sServerMethod": "post",
        "aoColumns": [
            {
                "class": "details-content",
                "data": "ReportCode",
                "style": "vertical-align: top",
                "width": "45%"
            },
            {
                "class": "details-content",
                "data": "ReportDesc",
                "width": "45%"
            },
            {
                "class": "details-content",
                "style": "vertical-align: top",
                "render": function (data, type, row, meta) {
                    return "<form name='viewRpt' target='blank' action='/PrintArchivalReport' method='post'>"
                        + "<input type='hidden' name='rptCode' value='" + row.ReportCode + "'/>"
                        + "<input type='hidden' name='clientCode' value='" + row.ReportClient + "'/>"
                        + "<input type='submit' class='btn btn-info btn-small' id='viewRpt' value='View Report' />"
                        + "</form>";
                },
                "width": "10%",
                "orderable": false
            },
        ],
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": true,
        "order": [[0, "asc"]]
    });
}

function attachEvent() {
    $('form[name="viewRpt"]').off('submit').on('submit', function (e) {
        $(".submit-progress").addClass("hidden");
        $("body").removeClass("submit-progress-bg");
        setValidNavigation();
    });
    //$('form[name="viewRpt"]')
    //$(".submit-progress").addClass("hidden");
    //setValidNavigation();
}

function initAuditLogDataTable(dt, sev, caller, client) {
    var paramList = dt + "|" + sev + "|" + caller + "|" + client;

    table = $('#dataTable').DataTable({
        "initComplete": function (settings, json) {
            //// CSS
            $("#dataTable_filter input").addClass("textbox");
            //Append Show [DropDown Select] Entries
            $("#dataTable_length select").before("Show ");
            $("#dataTable_length select").after(" entries.");

        },
        "autoWidth": true,
        "bProcessing": true,
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "ModuleName", "value": "AuditLogViewer" });
            aoData.push({ "name": "Param", "value": paramList });
            aoData.push({ "name": "ParamCount", "value": 4 });
        },
        "sServerMethod": "post",
        "aoColumns": [
            {
                "class": "details-content",
                "data": "LogID",
                "style": "vertical-align: top",
                "width": "8%"
            },
            {
                "class": "details-content",
                "data": "DateTime",
                "render": function (value) {
                    if (value === null) return "";
                    var pattern = /Date\(([^)]+)\)/;
                    var results = pattern.exec(value);
                    var dt = new Date(parseFloat(results[1]));

                    return pad((dt.getMonth() + 1), 2, '0') + "/" + pad(dt.getDate(), 2, '0') + "/" + dt.getFullYear() + "<br\>" + dt.toLocaleTimeString();
                },
                "width": "12%",
                "iDataSort": 0
            },
            {
                "class": "details-content",
                "data": "Status",
                "style": "vertical-align: top",
                "width": "8%"
            },
            {
                "class": "details-content",
                "data": "Caller",
                "style": "vertical-align: top",
                "width": "13%"
            },

            {
                "class": "details-content",
                "data": "Client",
                "style": "vertical-align: top",
                "width": "10%"
            },
            {
                "class": "details-content",
                "data": "UsrName",
                "style": "vertical-align: top",
                "width": "8%"
            },
            {
                "class": "details-content",
                "data": "Msg",
                "style": "vertical-align: top",
                "width": "15%"
            },
            {
                "class": "details-content",
                "data": "Data",
                "style": "vertical-align: top",
                "width": "15%"
            },
            {
                "class": "details-content",
                "data": "ExpMsg",
                "style": "vertical-align: top",
                "render": function (data, type, row, meta) {
                    if (data != "") {
                        return '<button type="button" class="btn btn-danger btn-small" onclick="loadExceptionBox(' + row.LogID + ');" data-toggle="modal" data-target="#myExceptionLog">View Exception</button><input class="ExpMsg_' + row.LogID + '"type="hidden" name="ExpMessage" value="' + data + '">';
                    } else {
                        return '<label>N/A</label>';
                    }
                },
                "width": "10%",
                //disable sorting
                "orderable": false
            }
        ],
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": true,
        "order": [[0, "asc"]]
    });
}

//Modified by boonchong 20240827 CR016-24, Added "AMOUNT" column for CIMB bank
function initRejDecisionDataTable(dt, client, branch, category, batchNo) {
    //Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {

    //If different client selected, date need to be updated again to latest client's busdate
    var dtToSelect = dt;
    if (selectedBusdate.length > 1) {
        dtToSelect = selectedBusdate;
    }

    var paramList = dtToSelect + "|" + client + "|" + branch + "|" + category + "|" + batchNo;
    var defaultOrderColumn = client === "CIMB" ? 6 : 0;
    var defaultOrderDirection = client === "CIMB" ? "desc" : "asc";

    var columns = [
        {
            "class": "details-content",
            "data": "PresentingBranch",
            "style": "vertical-align: top",
            "orderable": true,
            "width": "14%"
        },
        {
            "class": "details-content",
            "data": "BatchNo",
            "orderable": true,
            "width": "10%"

        },
        {
            "class": "details-content",
            "data": "TransNo",
            "style": "vertical-align: top",
            "orderable": true,
            "width": "9%"
        },
        {
            "class": "details-content",
            "data": "RejCategory",
            "orderable": true,
            "width": "13%"
        },
        {
            "class": "details-content",
            "data": "Reason",
            "orderable": true,
            "width": "20%"
        },
        {
            "class": "details-content",
            "data": "RejectTime",
            "render": function (value) {
                if (value === null) return "";
                var pattern = /Date\(([^)]+)\)/;
                var results = pattern.exec(value);
                var dt = new Date(parseFloat(results[1]));

                return dt.toLocaleTimeString();
            },
            "width": "16%",
            "orderable": false
        }
    ];
    if (client === "CIMB") {
        columns.push({
            "class": "details-content",
            "data": "TotalChequeAmount",
            "style": "vertical-align: top",
            "orderable": true,
            "width": "12%"
        });
    }

    columns.push({
        "class": "details-content",
        "data": null,
        "style": "vertical-align: top",
        "render": function (data, type, row, meta) {
            if (row.IsCutOff) {
                return '<button type="submit" class="btn btn-info btn-small" disabled> Validate </button>';
            } else {
                return '<button type="submit" onclick="onSubmitItmRejDec(' + row.TempID + ');" class="btn btn-info btn-small"> Validate </button><input class="Dir_' + row.TempID + '"type="hidden" name="DirName" value="' + row.BatchDirectory + '"/><input class="No_' + row.TempID + '"type="hidden" name="No" value="' + row.BatchNo + '"/><input class="Trans_' + row.TempID + '"type="hidden" name="Trans" value="' + row.TransNo + '"/><input class="Bundle_' + row.TempID + '"type="hidden" name="bundleName" value="' + row.NewBundle + '"/><input class="Site_' + row.TempID + '"type="hidden" name="SiteName" value="' + row.Site + '"><input class="RequiredApproval_' + row.TempID + '"type="hidden" name="RequiredApproval" value="' + row.RequiredApproval + '">';
            }
        },
        "width": "8%",
        "orderable": false
    });

    // alert(paramList);
    table = $('#dataTable').DataTable({
        "initComplete": function (settings, json) {
            //// CSS
            $("#dataTable_filter input").addClass("textbox");
            //Append Show [DropDown Select] Entries
            $("#dataTable_length select").before("Show ");
            $("#dataTable_length select").after(" entries.");
        },
        "autoWidth": true,
        "bProcessing": true,
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "ModuleName", "value": "RejectedItemDecision" });
            aoData.push({ "name": "Param", "value": paramList });
            aoData.push({ "name": "ParamCount", "value": 5 });
        },
        "sServerMethod": "post",
        "aoColumns": columns,
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": true,
        "bDestroy": true,
        "order": [[defaultOrderColumn, defaultOrderDirection]]
    });
    //});
}


//function initRejDecisionDataTable(dt, client, branch, category, batchNo) {
//    //Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {

//    //If different client selected, date need to be updated again to latest client's busdate
//    var dtToSelect = dt;
//    if (selectedBusdate.length > 1) {
//        dtToSelect = selectedBusdate;
//    }

//    var paramList = dtToSelect + "|" + client + "|" + branch + "|" + category + "|" + batchNo;

//       // alert(paramList);
//        table = $('#dataTable').DataTable({
//            "initComplete": function (settings, json) {
//                //// CSS
//                $("#dataTable_filter input").addClass("textbox");
//                //Append Show [DropDown Select] Entries
//                $("#dataTable_length select").before("Show ");
//                $("#dataTable_length select").after(" entries.");
//            },
//            "autoWidth": true,
//            "bProcessing": true,
//            "sAjaxSource": "WebService.asmx/LoadGridDataP",
//            "fnServerParams": function (aoData) {
//                aoData.push({ "name": "ModuleName", "value": "RejectedItemDecision" });
//                aoData.push({ "name": "Param", "value": paramList });
//                aoData.push({ "name": "ParamCount", "value": 5 });
//            },
//            "sServerMethod": "post",
//            "aoColumns": [
//                            {
//                                "class": "details-content",
//                                "data": "PresentingBranch",
//                                "style": "vertical-align: top",
//                                "orderable": true,
//                                "width": "14%"
//                            },
//                                  {
//                                      "class": "details-content",
//                                      "data": "BatchNo",
//                                      "orderable": true,
//                                      "width": "10%"

//                                  },
//                                   {
//                                       "class": "details-content",
//                                       "data": "TransNo",
//                                       "style": "vertical-align: top",
//                                       "orderable": true,
//                                       "width": "9%"
//                                   },
//                                  {
//                                      "class": "details-content",
//                                      "data": "RejCategory",
//                                      "orderable": true,
//                                      "width": "12%"
//                                  },
//                                  {
//                                      "class": "details-content",
//                                      "data": "Reason",
//                                      "orderable": true,
//                                      "width": "20%"
//                                  },
//                                   {
//                                       "class": "details-content",
//                                       "data": "RejectTime",
//                                       "render": function (value) {
//                                           if (value === null) return "";
//                                        var pattern = /Date\(([^)]+)\)/;
//                                        var results = pattern.exec(value);
//                                        var dt = new Date(parseFloat(results[1]));

//                                        return dt.toLocaleTimeString();
//                                    },
//                                        "width": "15%",
//                                        "orderable": false
//                                    },
//                                    {
//                                        "class": "details-content",
//                                        "data": "TotalChequeAmount",
//                                        "style": "vertical-align: top",
//                                        "orderable": true,
//                                        "width": "12%",
//                                        "visible": client === "CIMB"
//                                    },
//                                   {
//                                       "class": "details-content",
//                                       "data": null,
//                                       "style": "vertical-align: top",
//                                       //"defaultContent": '<button type="submit" onclick="onSubmit(this);" class="btn btn-info btn-small"> Validate </button>',
//                                       //"defaultContent": '<asp:Button ID="btnValidate" runat="server" Text="Validate" CssClass="btn btn-info btn-small" OnClick="btnValidate_Click"  />',
//                                       "render": function (data, type, row, meta) {
//                                           if (row.IsCutOff) {
//                                               return ' <button type="submit" class="btn btn-info btn-small" disabled> Validate </button>'
//                                           }
//                                           else {
//                                             //  return ' <button type="submit" onclick="onSubmitItmActionHis(' + row.TempID + ');" class="btn btn-info btn-small"> View </button><input class="Dir_' + row.TempID + '"type="hidden" name="DirName" value="' + row.BatchDirectory + '"/><input class="No_' + row.TempID + '"type="hidden" name="No" value="' + row.BatchNo + '"/><input class="Trans_' + row.TempID + '"type="hidden" name="Trans" value="' + row.TransNo + '"/><input class="Bundle_' + row.TempID + '"type="hidden" name="bundleName" value="' + row.NewBundle + '"/><input class="Site_' + row.TempID + '"type="hidden" name="SiteName" value="' + row.Site + '">'
//                                               return ' <button type="submit" onclick="onSubmitItmRejDec(' + row.TempID + ');" class="btn btn-info btn-small"> Validate </button><input class="Dir_' + row.TempID + '"type="hidden" name="DirName" value="' + row.BatchDirectory + '"/><input class="No_' + row.TempID + '"type="hidden" name="No" value="' + row.BatchNo + '"/><input class="Trans_' + row.TempID + '"type="hidden" name="Trans" value="' + row.TransNo + '"/><input class="Bundle_' + row.TempID + '"type="hidden" name="bundleName" value="' + row.NewBundle + '"/><input class="Site_' + row.TempID + '"type="hidden" name="SiteName" value="' + row.Site + '"><input class="RequiredApproval_' + row.TempID + '"type="hidden" name="RequiredApproval" value="' + row.RequiredApproval + '">'
//                                           }


//                                           //' <button type="submit" onclick="onSubmitRejDec(' + row.TempID + ');" class="btn btn-info btn-small"> Validate </button>'
//                                       },

//                                       "width": "8%",
//                                       "orderable": false
//                                   }
//            ],
//            "oLanguage": {
//                "sProcessing": "<div>Please wait while your request is being processed...</div>",
//                "sInfoFiltered": ""
//            },
//            "bSort": true,
//            "bDestroy": true,
//            "order": [[0, "asc"]]
//        });
//    //});
//}

function initBatchMaintDataTable(dt, client, branch, category, batchNo) {

    //If different client selected, date need to be updated again to latest client's busdate
    var dtToSelect = dt;
    if (selectedBusdate.length > 1) {
        dtToSelect = selectedBusdate;
    }

    var paramList = dtToSelect + "|" + client + "|" + branch + "|" + category + "|" + batchNo;

    table = $('#dataTable').DataTable({
        "initComplete": function (settings, json) {
            //// CSS
            $("#dataTable_filter input").addClass("textbox");
            //Append Show [DropDown Select] Entries
            $("#dataTable_length select").before("Show ");
            $("#dataTable_length select").after(" entries.");

        },
        "autoWidth": true,
        "bProcessing": true,
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "ModuleName", "value": "BatchMaintenance" });
            aoData.push({ "name": "Param", "value": paramList });
            aoData.push({ "name": "ParamCount", "value": 5 });
        },
        "sServerMethod": "post",
        "aoColumns": [
            {
                "class": "details-content",
                "data": "PresentingBranch",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "14%"
            },
            {
                "class": "details-content",
                "data": "BatchNo",
                "orderable": true,
                "width": "10%"

            },
            {
                "class": "details-content",
                "data": "TransNo",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "11%"
            },
            {
                "class": "details-content",
                "data": "RejCategory",
                "orderable": true,
                "width": "12%"
            },
            {
                "class": "details-content",
                "data": "Reason",
                "orderable": true,
                "width": "20%"
            },
            {
                "class": "details-content",
                "data": "InUsedBy",
                "orderable": true,
                "width": "15%"
            },
            {
                "class": "details-content",
                "data": null,
                "style": "vertical-align: top",
                "render": function (data, type, row, meta) {
                    return ' <button type="button" onclick="onSubmitBatchMaint(' + row.TempID + ');" class="btn btn-info btn-small"> Reset </button><input class="Dir_' + row.TempID + '"type="hidden" name="DirName" value="' + row.BatchDirectory + '"/><input class="No_' + row.TempID + '"type="hidden" name="No" value="' + row.BatchNo + '"/><input class="Trans_' + row.TempID + '"type="hidden" name="Trans" value="' + row.TransNo + '"/><input class="Bundle_' + row.TempID + '"type="hidden" name="bundleName" value="' + row.NewBundle + '"/><input class="Site_' + row.TempID + '"type="hidden" name="SiteName" value="' + row.Site + '">'
                    //' <button type="submit" onclick="onSubmitRejDec(' + row.TempID + ');" class="btn btn-info btn-small"> Validate </button>'
                },

                "width": "8%",
                "orderable": false
            }
        ],
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": true,
        "order": [[0, "asc"]]
    });

}

function initActionedHistoryDataTable(dt, client, branch, category, batchNo) {

    //If different client selected, date need to be updated again to latest client's busdate
    var dtToSelect = dt;
    if (selectedBusdate.length > 1) {
        dtToSelect = selectedBusdate;
    }

    var paramList = dtToSelect + "|" + client + "|" + branch + "|" + category + "|" + batchNo;

    table = $('#dataTable').DataTable({
        "initComplete": function (settings, json) {
            //// CSS
            $("#dataTable_filter input").addClass("textbox");
            //Append Show [DropDown Select] Entries
            $("#dataTable_length select").before("Show ");
            $("#dataTable_length select").after(" entries.");

        },
        "autoWidth": true,
        "bProcessing": true,
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "ModuleName", "value": "ActionedItemHistory" });
            aoData.push({ "name": "Param", "value": paramList });
            aoData.push({ "name": "ParamCount", "value": 5 });
        },
        "sServerMethod": "post",
        "aoColumns": [
            {
                "class": "details-content",
                "data": "PresentingBranch",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "14%"
            },
            {
                "class": "details-content",
                "data": "BatchNo",
                "orderable": true,
                "width": "10%"

            },
            {
                "class": "details-content",
                "data": "TransNo",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "11%"
            },
            {
                "class": "details-content",
                "data": "RejCategory",
                "orderable": true,
                "width": "12%"
            },
            {
                "class": "details-content",
                "data": "Reason",
                "orderable": true,
                "width": "20%"
            },
            {
                "class": "details-content",
                "data": "ActionStatus",
                "orderable": true,
                "width": "15%"
            },
            {
                "class": "details-content",
                "data": null,
                "style": "vertical-align: top",
                "render": function (data, type, row, meta) {
                    return ' <button type="submit" onclick="onSubmitItmActionHis(' + row.TempID + ');" class="btn btn-info btn-small"> View </button><input class="Dir_' + row.TempID + '"type="hidden" name="DirName" value="' + row.BatchDirectory + '"/><input class="No_' + row.TempID + '"type="hidden" name="No" value="' + row.BatchNo + '"/><input class="Trans_' + row.TempID + '"type="hidden" name="Trans" value="' + row.TransNo + '"/><input class="Bundle_' + row.TempID + '"type="hidden" name="bundleName" value="' + row.NewBundle + '"/><input class="Site_' + row.TempID + '"type="hidden" name="SiteName" value="' + row.Site + '"><input class="RequiredApproval_' + row.TempID + '"type="hidden" name="RequiredApproval" value="' + row.RequiredApproval + '">'
                },
                "width": "8%",
                "orderable": false
            }
        ],
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": true,
        "order": [[0, "asc"]]
    });

}

function initOutlookRecpMaintDataTable(client, emailAddr, site) {
    var paramList = `${client}|${emailAddr}|${site}`;// client + "|" + emailAddr + "|" + site;

    table = $('#dataTable').DataTable({
        "initComplete": function (settings, json) {
            //// CSS
            $("#dataTable_filter input").addClass("textbox");
            //Append Show [DropDown Select] Entries
            $("#dataTable_length select").before("Show ");
            $("#dataTable_length select").after(" entries.");

        },
        "autoWidth": true,
        "bProcessing": true,
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "ModuleName", "value": "OutlookRecipientMaintenance" });
            aoData.push({ "name": "Param", "value": paramList });
            aoData.push({ "name": "ParamCount", "value": 3 });
        },
        "sServerMethod": "post",
        "aoColumns": [
            {
                "class": "details-content",
                "data": "ClientCode",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "15%"
            },
            {
                "class": "details-content",
                "data": "EmailAddr",
                "orderable": true,
                "width": "50%"

            },
            {
                "class": "details-content",
                "data": "Site",
                "orderable": true,
                "width": "10%"

            },
            {
                "class": "details-content",
                "data": null,
                "style": "vertical-align: top",
                "render": function (data, type, row, meta) {
                    return '<div><img id="Edit" src="../../images/pencil.png" alt="Edit" title="Edit"  onclick="onSetTempParam(' + row.Id + ');">&nbsp&nbsp&nbsp<img id="Delete" src="../../images/delete.png" alt="Delete" title="Delete" onclick="onSetDelTempID(' + row.Id + ');"><input class="Mod_' + row.Id + '"type="hidden" name="DirName" value="OR"/><input class="Row_' + row.Id + '"type="hidden" name="DirName" value="' + row.Id + '"/></div>'
                },
                "width": "15%",
                "orderable": false
            }
        ],
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": true,
        "order": [[0, "asc"]]
    });

}

function initRepoReportDataTable(dt, client) {
    //Report Repository allowed to choose history date
    var dtToSelect = dt;

    var paramList = dtToSelect + "|" + client;

    table = $('#dataTable').DataTable({
        "initComplete": function (settings, json) {
            //// CSS
            $("#dataTable_filter input").addClass("textbox");
            //Append Show [DropDown Select] Entries
            $("#dataTable_length select").before("Show ");
            $("#dataTable_length select").after(" entries.");
            //to hide loading screen when click generate report
            attachEvent();
        },
        "autoWidth": true,
        "bProcessing": true,
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "fnServerParams": function (aoData) {
            aoData.push({ "name": "ModuleName", "value": "ReportRepository" });
            aoData.push({ "name": "Param", "value": paramList });
            aoData.push({ "name": "ParamCount", "value": 2 });
        },
        "sServerMethod": "post",
        "aoColumns": [
            {
                "class": "details-content",
                "data": null,
                "render": function (data, type, row, meta) {
                    return '<div class="checkbox-container"><input type="checkbox" class="file-checkbox enlarged-checkbox" value="' + row.ReportFileUploadedFullPath + '"></div>';
                },
                "width": "10%",
                "orderable": false
            },

            {
                "class": "details-content",
                "data": "ReportFileName",
                "style": "vertical-align: top",
                "width": "70%"
            },
            {
                "class": "details-content",
                "data": null,
                "render": function (data, type, row, meta) {



                    if (row.ReportFileAllowDelete == "1") {
                        return '<div><a href="' + row.ReportFileUploadedFullPath + '" download="' + row.ReportFileName + '" ><img id="Download" src="../../images/download.png" alt="Download" title="Download"  onclick="onSetTempParam(' + row.TempID + ');"/></a>&nbsp&nbsp&nbsp<img id="Delete" src="../../images/delete.png" alt="Delete" title="Delete" onclick="onSetDelTempID(' + row.TempID + ');"><input class="Mod_' + row.TempID + '"type="hidden" name="DirName" value="RP"/><input class="Row_' + row.TempID + '"type="hidden" name="DirName" value="' + row.ReportFileName + '"/></div>'
                    }
                    else {
                        return '<div><a href="' + row.ReportFileUploadedFullPath + '" download="' + row.ReportFileName + '" ><img id="Download" src="../../images/download.png" alt="Download" title="Download"  onclick="onSetTempParam(' + row.TempID + ');"/></a>&nbsp&nbsp&nbsp<input class="Mod_' + row.TempID + '"type="hidden" name="DirName" value="RP"/><input class="Row_' + row.TempID + '"type="hidden" name="DirName" value="' + row.ReportFileName + '"/></div>'

                    }
                },

                "width": "10%"
            }
        ],
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": true,
        "order": [[1, "asc"]]
    });
}

//Added by boonchong PE - WD-24-003
function initImageArchiveOutwardDataTable(dt, amount, depositorAcc, operate, micrAccNum, micrBSB, chequeNum, itemType, presentingBSB, clientCode, totalCount) {
    var dtToSelect = dt;

    var paramList = dtToSelect + "|" + amount + "|" + depositorAcc + "|" + operate + "|" + micrAccNum + "|" + micrBSB + "|" + chequeNum + "|" + itemType + "|" + presentingBSB + "|" + clientCode;

    // alert(paramList);
    table = $('#dataTable').DataTable({
        "lengthChange": false,
        "pageLength": 25, // Set the number of entries to display per page to 25
        "initComplete": function (settings, json) {
            //// CSS
            // Ensure the parent container is set to 100% width
            $("label").css({
                'width': '100%',
                'margin-bottom': '-1%'
            });
            $("#dataTable_wrapper").find(".dataTables_filter").before(function () {
                return '<a id="printButton" href="#" class="btn btn-info btn-small" style="position:absolute; margin-top:1%; width:6%; padding:0.35%;">Print</a>' +
                    '<div style="position:absolute; margin-top:1.5%;margin-left:6.5%;font-size:small;color:blue;font-style:italic"> Note: Only items from the same page can be printed at a time.</div > ';
            });
            $("#dataTable_wrapper").find(".dataTables_filter").after(function () {
                return '<div style="margin-top: -2%; text-align: right;">Total Count: ' + totalCount + '</div>';
            });


            // Add click event listener for the print button
            $("#printButton").on("click", function (event) {
                event.preventDefault();
                var selectedData = [];
                $('#dataTable').find('.file-checkbox:checked').each(function () {
                    var row = $(this).closest('tr');
                    var data = {
                        Date: row.find('.Date').val(),
                        DirName: row.find('.DirName').val(),
                        BatchNo: row.find('.No').val(),
                        Trans: row.find('.Trans').val().trim(),
                        TransSeqNum: row.find('.TransSeqNum').val().trim(),
                        Din: row.find('.Din').val(),
                        Client: clientCode,
                        SiteName: row.find('.SiteName').val()
                    };
                    selectedData.push(data);
                });

                if (selectedData.length > 0) {
                    // Send the selected data to getreporturl
                    $.ajax({
                        url: "/Modules/ImageArchiveOutward/Default.aspx/PrintFunction", // Replace with your actual URL
                        method: "POST",
                        data: JSON.stringify({ selectedData: selectedData }),
                        contentType: 'application/json; charset=utf-8',
                        dataType: 'json',
                        success: function (response) {
                            if (response.d === "Success") {
                                openCR();
                            }
                            // Handle the response from the server
                            console.log("Report generated:", response);
                        },
                        error: function (error) {
                            // Handle any errors
                            console.error("Error generating report:", error);
                        }
                    });
                } else {
                    alert("Please select at least one checkbox.");
                }
            });
        },
        "autoWidth": true,
        "bProcessing": true,
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "serverSide": true,
        "fnServerParams": function (aoData) {
            var valueToDisplay = "25";//$('select[name="dataTable_length"]').val();//how many display in one page
            var order = $('#dataTable').DataTable().order().toString();//get table ascending
            var splitOrder = order.split(',');//split the order and get 0 | asc
            var pageInfo = $('#dataTable').DataTable().page.info();
            var currentPage = pageInfo.page + 1;
            aoData.push({ "name": "ModuleName", "value": "ImageArchiveOutward" });
            aoData.push({ "name": "Param", "value": paramList + "|" + currentPage + "|" + valueToDisplay + "|" + splitOrder[0] + "|" + splitOrder[1] });
            aoData.push({ "name": "ParamCount", "value": 14 });
        },
        "sServerMethod": "post",
        "aoColumns": [
            {
                "class": "details-content",
                "data": null,
                "render": function (data, type, row, meta) {
                    return '<div class="checkbox-container" >' +
                        '<input type="checkbox" class="file-checkbox enlarged-checkbox" value="' + row.ReportFileUploadedFullPath + '">' +
                        '<input class="DirName" type="hidden" name="DirName" value="' + row.BatchDirectory + '"/>' +
                        '<input class="No" type="hidden" name="No" value="' + row.BatchNo + '"/>' +
                        '<input class="Trans" type="hidden" name="Trans" value="' + row.TransNo + '"/>' +
                        '<input class="Din" type="hidden" name="Din" value="' + row.Din + '"/>' +
                        '<input class="TransSeqNum" type="hidden" name="TransSeqNum" value="' + row.TransSeqNum + '"/>' +
                        '<input class="Client" type="hidden" name="Client" value="' + row.ClientCode + '"/>' +
                        '<input class="Date" type="hidden" name="Date" value="' + row.date + '"/>' +
                        '<input class="SiteName" type="hidden" name="SiteName" value="' + row.Site + '"/>'
                    '</div>';
                },
                "width": "2%",
                "orderable": false
            },
            {
                "class": "details-content",
                "data": "BatchNo",
                "orderable": true,
                "width": "8%"
            },
            {
                "class": "details-content",
                "data": "ItemType",
                "orderable": true,
                "width": "7%"

            },
            {
                "class": "details-content",
                "data": "ChequeNo",
                "orderable": true,
                "width": "9%"

            },
            {
                "class": "details-content",
                "data": "ChequeBSB",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "8%"
            },
            {
                "class": "details-content",
                "data": "AccountNumber",
                "orderable": true,
                "width": "10%"
            },
            {
                "class": "details-content",
                "data": "TransCode",
                "orderable": true,
                "width": "8%"
            },
            {
                "class": "details-content",
                "data": "Amount",
                "orderable": true,
                "width": "9%"
            },
            {
                "class": "details-content",
                "data": "DepositorAccount",
                "orderable": true,
                "width": "13%"
            },
            {
                "class": "details-content",
                "data": "TxnNo",
                "orderable": true,
                "width": "7%"
            },
            {
                "class": "details-content",
                "data": null,
                "style": "vertical-align: top",
                "render": function (data, type, row, meta) {
                    //  return ' <button type="submit" onclick="onSubmitItmActionHis(' + row.TempID + ');" class="btn btn-info btn-small"> View </button><input class="Dir_' + row.TempID + '"type="hidden" name="DirName" value="' + row.BatchDirectory + '"/><input class="No_' + row.TempID + '"type="hidden" name="No" value="' + row.BatchNo + '"/><input class="Trans_' + row.TempID + '"type="hidden" name="Trans" value="' + row.TransNo + '"/><input class="Bundle_' + row.TempID + '"type="hidden" name="bundleName" value="' + row.NewBundle + '"/><input class="Site_' + row.TempID + '"type="hidden" name="SiteName" value="' + row.Site + '">'
                    return ' <button type="submit" onclick="onSubmitImgArcOutward(' + row.TempID + ');" class="btn btn-info btn-small"> View </button><input class="Dir_' + row.TempID + '"type="hidden" name="DirName" value="' + row.BatchDirectory + '"/><input class="No_' + row.TempID + '"type="hidden" name="No" value="' + row.BatchNo + '"/><input class="Trans_' + row.TempID + '"type="hidden" name="Trans" value="' + row.TransNo + '"/><input class="Din_' + row.TempID + '"type="hidden" name="Din" value="' + row.Din + '"/><input class="Client_' + row.TempID + '"type="hidden" name="Client" value="' + row.ClientCode + '"/><input class="Date_' + row.TempID + '"type="hidden" name="Date" value="' + row.date + '"/><input class="Site_' + row.TempID + '"type="hidden" name="SiteName" value="' + row.Site + '"><input class="TransSeqNum_' + row.TempID + '"type="hidden" name="TransSeqNum" value="' + row.TransSeqNum + '"><input class="ItemType_' + row.TempID + '"type="hidden" name="ItemType" value="' + row.ItemType + '">'
                },

                "width": "5%",
                "orderable": false
            }
        ],
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": true,
        "bDestroy": true,
        "order": [[1, "asc"]]
    });
    //});
}

//Added by boonchong PE - WD-24-003
function initImageArchiveInwardDataTable(dt, amount, operate, clientCode, micrAccNum, chequeNum, presentingBSB, totalCount) {
    var dtToSelect = dt;

    var paramList = dtToSelect + "|" + amount + "|" + operate + "|" + clientCode + "|" + micrAccNum + "|" + chequeNum + "|" + presentingBSB;
    console.log(totalCount);
    table = $('#dataTable').DataTable({
        //"lengthChange": false,
        //"pageLength": 25, // Set the number of entries to display per page to 25
        "initComplete": function (settings, json) {
            //// CSS
            // Ensure the parent container is set to 100% width
            //$("label").css({
            //    'width': '100%',
            //    'margin-bottom': '-1.5%'
            //});
            $("#dataTable_filter input").addClass("textbox");
            //Append Show [DropDown Select] Entries
            $("#dataTable_length select").before("Show ");
            $("#dataTable_length select").after(" entries.");
            $("#dataTable_wrapper").find(".dataTables_filter").after(function () {
                return '<div style="position:absolute; right:0;margin-right:5%;margin-top:1%">Total Count: ' + totalCount + '</div>';
            });
        },
        "autoWidth": true,
        "bProcessing": true,
        "serverSide": true,
        "sAjaxSource": "WebService.asmx/LoadGridDataP",
        "fnServerParams": function (aoData) {
            var valueToDisplay = $('select[name="dataTable_length"]').val();//how many display in one page
            var order = $('#dataTable').DataTable().order().toString();//get table ascending
            var splitOrder = order.split(',');//split the order and get 0 | asc
            var pageInfo = $('#dataTable').DataTable().page.info();
            var currentPage = pageInfo.page + 1;

            aoData.push({ "name": "ModuleName", "value": "ImageArchiveInward" });
            aoData.push({ "name": "Param", "value": paramList + "|" + currentPage + "|" + valueToDisplay + "|" + splitOrder[0] + "|" + splitOrder[1] });
            aoData.push({ "name": "ParamCount", "value": 11 });
        },
        "sServerMethod": "post",
        "aoColumns": [
            {
                "class": "details-content",
                "data": "BatchNo",
                "orderable": true,
                "width": "10%"
            },
            {
                "class": "details-content",
                "data": "CheckNo",
                "orderable": true,
                "width": "11%"

            },
            {
                "class": "details-content",
                "data": "IssuingBranch",
                "style": "vertical-align: top",
                "orderable": true,
                "width": "10%"
            },
            {
                "class": "details-content",
                "data": "AcctNo",
                "orderable": true,
                "width": "10%"
            },
            {
                "class": "details-content",
                "data": "TRCode",
                "orderable": true,
                "width": "8%"
            },
            {
                "class": "details-content",
                "data": "Amount",
                "orderable": true,
                "width": "8%"
            },
            {
                "class": "details-content",
                "data": null,
                "style": "vertical-align: top",
                "render": function (data, type, row, meta) {
                    //  return ' <button type="submit" onclick="onSubmitItmActionHis(' + row.TempID + ');" class="btn btn-info btn-small"> View </button><input class="Dir_' + row.TempID + '"type="hidden" name="DirName" value="' + row.BatchDirectory + '"/><input class="No_' + row.TempID + '"type="hidden" name="No" value="' + row.BatchNo + '"/><input class="Trans_' + row.TempID + '"type="hidden" name="Trans" value="' + row.TransNo + '"/><input class="Bundle_' + row.TempID + '"type="hidden" name="bundleName" value="' + row.NewBundle + '"/><input class="Site_' + row.TempID + '"type="hidden" name="SiteName" value="' + row.Site + '">'
                    return ' <button type="submit" onclick="onSubmitImgArcInward(' + row.TempID + ');" class="btn btn-info btn-small"> View </button><input class="No_' + row.TempID + '"type="hidden" name="No" value="' + row.BatchNo + '"/><input class="Date_' + row.TempID + '"type="hidden" name="BusDate" value="' + row.date + '"/><input class="CheckNum_' + row.TempID + '"type="hidden" name="CheckNum" value="' + row.CheckNo + '"/><input class="UIC_' + row.TempID + '"type="hidden" name="UIC" value="' + row.UIC + '"/>'
                },
                "width": "5%",
                "orderable": false
            }
        ],
        "oLanguage": {
            "sProcessing": "<div>Please wait while your request is being processed...</div>",
            "sInfoFiltered": ""
        },
        "bSort": true,
        "bDestroy": true,
        "order": [[0, "asc"]]
    });
    //});
}


function configureTable() {
    //Re-Draw
    $('#dataTable').on('draw.dt', function () {
        var bExpandAll = false;

        if ($("#Expand").text() != 'Expand All') {
            bExpandAll = true;
        }

        $('#dataTable tbody td.details-control').each(function (i) {
            var tr = $(this).closest('tr');
            var row = table.row(tr);

            if (bExpandAll) {
                // Open this row
                row.child(format(row.data())).show();
                tr.addClass('shown');
            }
            else {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
            }
        });
    });

    //Add Event Listener for edit Operator
    $('#dataTable tbody').on('click', 'img#Edit', function () {
        var moduleName = $(".Mod_" + editTableID).val();
        var editID = $(".Row_" + editTableID).val();

        var pobj = { selID: editID };
        var param = JSON.stringify(pobj);

        if (assignSessionMainDT(moduleName, param) != "0") {

            var tagInfo = document.createElement("INPUT");
            tagInfo.type = "hidden";
            tagInfo.name = "tag";
            tagInfo.value = "EDIT";

            var selectedRow = document.createElement("INPUT");
            selectedRow.type = "hidden";
            selectedRow.name = "tableID";

            validNavigation = true;
            isLoadSpinner = true;
            loadSpinner();
            //  var $td = $(this).closest('tr').children('td');
            // var tblID = $td.eq(0).text();
            //    alert(editTableID);
            selectedRow.value = editTableID;
            document.forms[0].appendChild(tagInfo);
            document.forms[0].appendChild(selectedRow);
            document.forms[0].submit();
        }

    });

    ////Add event listener for delete others
    $('#dataTable tbody').on('click', 'img#Delete', function () {
        isLoadSpinner = false;
        unloadSpinner();

        //var $td = $(this).closest('tr').children('td');
        //deleteTableID = $td.eq(0).text();

        //Check if module is operator maintenance
        //Check if user is current login user 
        //SAdmin cannot be deleted
        var moduleName = $(".Mod_" + deleteTableID).val();

        if (moduleName == "OP") {
            var usrID = $(".Row_" + deleteTableID).val().trim();
            if (usrID == "SAdmin" || usrID == "OAdmin") {
                //dialog-info
                $("#dialog-info").dialog("open");
            }
            else {
                if (validateUsrLogin(usrID) != "0") {
                    $("#dialog-usrLogin").dialog("open");
                }
                else {
                    $("#dialog-delete").dialog("open");
                }
            }
        }
        else {
            $("#dialog-delete").dialog("open");
        }
    });

    $("#dialog-delete").dialog({
        autoOpen: false,
        buttons: {
            "Yes": {
                text: "Yes",
                id: "dialogBtnYes",
                click: function () {
                    var moduleName = $(".Mod_" + deleteTableID).val();
                    var usrID = $(".Row_" + deleteTableID).val();

                    var pobj = { selID: usrID };
                    var param = JSON.stringify(pobj);

                    if (assignSessionMainDT(moduleName, param) != "0") {

                        isLoadSpinner = true;
                        loadSpinner();
                        validNavigation = true;
                        var tagInfo = document.createElement("INPUT");
                        tagInfo.type = "hidden";
                        tagInfo.name = "tag";
                        tagInfo.value = "DEL";

                        var selectedRow = document.createElement("INPUT");
                        selectedRow.type = "hidden";
                        selectedRow.name = "tableID";
                        selectedRow.value = deleteTableID;

                        document.forms[0].appendChild(tagInfo);
                        document.forms[0].appendChild(selectedRow);
                        document.forms[0].submit();
                    }
                }
            },
            "No": {
                text: "No",
                id: "dialogBtnNo",
                click: function () {
                    unloadSpinner();
                    $(this).dialog("close");
                }
            }
        }
    });


    //hide all dialog, ok
    $("#dialog-info").dialog({
        autoOpen: false,
        buttons: {

            "OK": function () {
                $(this).dialog("close");
            }
        }
    });

    //hide all dialog, ok
    $("#dialog-usrLogin").dialog({
        autoOpen: false,
        buttons: {

            "OK": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#dialog-general-message").dialog({
        autoOpen: false,
        buttons: {

            "OK": function () {
                $(this).dialog("close");
            }
        }
    });

    // Apply the filter
    $("#dataTable thead select").on('change', function () {
        table
            .column($(this).parent().index() + ':visible')
            .search(this.value)
            .draw();
    });

    $("#dataTable thead input").on('keyup', function () {
        table
            .column($(this).parent().index() + ':visible')
            .search(this.value)
            .draw();
    });

    // Add event listener for opening and closing details
    $('#dataTable tbody').on('click', 'td.details-control', function () {
        var tr = $(this).closest('tr');
        var row = table.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
            tr.removeClass('shown');
        }
        else {
            // Open this row
            row.child(format(row.data())).show();
            tr.addClass('shown');
        }
    });

    //Add Event Listener for ClearFilter
    $("#ClearFilter").click(function (e) {

        // validNavigation = true;
        $("input:text").val('');
        $("select[name^=search_]").val('');

        var oTable = $('#dataTable').dataTable();
        /* Remove all filtering */
        oTable.fnFilterClear();
    });
}

function loadExceptionBox(logID) {
    //var span = document.getElementById("messageExp");
    //var txt = document.createTextNode($(".ExpMsg_" + logID).val());
    //span.appendChild(txt);

    $('#messageExp').html($(".ExpMsg_" + logID).val());
}

function onSubmitItmRejDec(tempID) {
    var returnMsg = "";
    isLoadSpinner = true;
    //loadSpinner();
    validNavigation = true;

    var batchDir = $(".Dir_" + tempID).val();
    var batchNo = $(".No_" + tempID).val();
    var transNo = $(".Trans_" + tempID).val();
    var siteName = $(".Site_" + tempID).val();
    var bundleID = $(".Bundle_" + tempID).val();
    var requiredApproval = $(".RequiredApproval_" + tempID).val();

    var obj = { BatchDir: batchDir, BatchNo: batchNo, TransNo: transNo, BundleID: bundleID, SiteName: siteName, RequiredApproval: requiredApproval };
    var param = JSON.stringify(obj);

    //get ajax call to set session value, if return yes
    if (assignSessionMainDT("RD", param) != "0") {
        $(".submit-progress").removeClass("hidden");
        $("body").addClass("submit-progress-bg");

        var strval = tempID;

        var tagI = document.createElement("INPUT");
        tagI.type = "hidden";
        tagI.name = "tag";
        tagI.value = "VALIDATE";

        var selRow = document.createElement("INPUT");
        selRow.type = "hidden";
        selRow.name = "tableID";
        selRow.value = strval;

        document.forms[0].appendChild(tagI);
        document.forms[0].appendChild(selRow);

        //need to set 1 second timer to load form submit to show the "spinner" icon
        var timerId = setInterval(function () { document.forms[0].submit(); }, 1000);

        clearInterval(timerId);
    }


}

function onSubmitItmActionHis(tempID) {
    var returnMsg = "";
    isLoadSpinner = true;
    //loadSpinner();
    validNavigation = true;

    var batchDir = $(".Dir_" + tempID).val();
    var batchNo = $(".No_" + tempID).val();
    var transNo = $(".Trans_" + tempID).val();
    var siteName = $(".Site_" + tempID).val();
    var bundleID = $(".Bundle_" + tempID).val();
    var requiredApproval = $(".RequiredApproval_" + tempID).val();

    var obj = { BatchDir: batchDir, BatchNo: batchNo, TransNo: transNo, BundleID: bundleID, SiteName: siteName, RequiredApproval: requiredApproval };
    var param = JSON.stringify(obj);

    //get ajax call to set session value, if return yes
    if (assignSessionMainDT("AH", param) != "0") {
        $(".submit-progress").removeClass("hidden");
        $("body").addClass("submit-progress-bg");

        var strval = tempID;

        var tagI = document.createElement("INPUT");
        tagI.type = "hidden";
        tagI.name = "tag";
        tagI.value = "VALIDATE";

        var selRow = document.createElement("INPUT");
        selRow.type = "hidden";
        selRow.name = "tableID";
        selRow.value = strval;

        document.forms[0].appendChild(tagI);
        document.forms[0].appendChild(selRow);

        //need to set 1 second timer to load form submit to show the "spinner" icon
        var timerId = setInterval(function () { document.forms[0].submit(); }, 1000);

        clearInterval(timerId);
    }


}
function onSubmitImgArcOutward(tempID) {
    var returnMsg = "";
    isLoadSpinner = true;
    //loadSpinner();
    validNavigation = true;

    sessionStorage.removeItem('ImageArchiveInwardFrontImg');
    sessionStorage.removeItem('ImageArchiveInwardRearImg');
    sessionStorage.removeItem('ImageArchiveInwardFrontJPEG');
    sessionStorage.removeItem('ImageArchiveOutwardVirtualStubImage');

    var batchDir = $(".Dir_" + tempID).val();
    var batchNo = $(".No_" + tempID).val();
    var transNo = $(".Trans_" + tempID).val();
    var clientSite = $(".Site_" + tempID).val();
    var din = $(".Din_" + tempID).val();
    var transseqnum = $(".TransSeqNum_" + tempID).val();
    var clientCode = $(".Client_" + tempID).val();
    var busDate = $(".Date_" + tempID).val();
    var itemtype = $(".ItemType_" + tempID).val();

    var obj = { BatchDir: batchDir, BatchNo: batchNo, TransNo: transNo, Din: din, ClientCode: clientCode, BusDate: busDate, ClientSite: clientSite, TransSeqNum: transseqnum, ItemType: itemtype };
    var param = JSON.stringify(obj);

    //get ajax call to set session value, if return yes
    if (assignSessionMainDT("IAO", param) != "0") {
        $(".submit-progress").removeClass("hidden");
        $("body").addClass("submit-progress-bg");

        var strval = tempID;

        var tagI = document.createElement("INPUT");
        tagI.type = "hidden";
        tagI.name = "tag";
        tagI.value = "VALIDATE";

        var selRow = document.createElement("INPUT");
        selRow.type = "hidden";
        selRow.name = "tableID";
        selRow.value = strval;

        document.forms[0].appendChild(tagI);
        document.forms[0].appendChild(selRow);

        //need to set 1 second timer to load form submit to show the "spinner" icon
        var timerId = setInterval(function () { document.forms[0].submit(); }, 1000);

        clearInterval(timerId);
    }


}

function onSubmitImgArcInward(tempID) {
    var returnMsg = "";
    isLoadSpinner = true;
    //loadSpinner();
    validNavigation = true;

    sessionStorage.removeItem('ImageArchiveInwardFrontImg');
    sessionStorage.removeItem('ImageArchiveInwardRearImg');
    sessionStorage.removeItem('ImageArchiveInwardFrontJPEG');
    sessionStorage.removeItem('ImageArchiveOutwardVirtualStubImage');

    var batchDir = $(".Dir_" + tempID).val();
    var batchNo = $(".No_" + tempID).val();
    var transNo = $(".Trans_" + tempID).val();
    var siteName = $(".Site_" + tempID).val();
    var bundleID = $(".Bundle_" + tempID).val();
    var clientCode = $(".Client_" + tempID).val();
    var busDate = $(".Date_" + tempID).val();
    var checkNo = $(".CheckNum_" + tempID).val()
    var uic = $(".UIC_" + tempID).val()

    var obj = { BatchNo: batchNo, BusDate: busDate, CheckNum: checkNo, UIC: uic };
    var param = JSON.stringify(obj);
    console.log(param);

    //get ajax call to set session value, if return yes
    if (assignSessionMainDT("IAI", param) != "0") {
        $(".submit-progress").removeClass("hidden");
        $("body").addClass("submit-progress-bg");

        var strval = tempID;

        var tagI = document.createElement("INPUT");
        tagI.type = "hidden";
        tagI.name = "tag";
        tagI.value = "VALIDATE";

        var selRow = document.createElement("INPUT");
        selRow.type = "hidden";
        selRow.name = "tableID";
        selRow.value = strval;

        document.forms[0].appendChild(tagI);
        document.forms[0].appendChild(selRow);

        //need to set 1 second timer to load form submit to show the "spinner" icon
        var timerId = setInterval(function () { document.forms[0].submit(); }, 1000);

        clearInterval(timerId);
    }


}


function assignSessionMainDT(module, param) {
    var returnMsg = "";
    var strurl = "";

    switch (module) {
        case "RD":
            strurl = '/Modules/RejectedItemDecision/Default.aspx/assignSessionDT';
            break;
        case "AH":
            strurl = '/Modules/ActionedItemHistory/Default.aspx/assignSessionDT';
            break;
        case "BM":
            strurl = '/Modules/BatchMaintenance/Default.aspx/assignSessionDT';
            break;
        case "OP":
            strurl = '/Modules/OperatorMaintenance/Default.aspx/assignSessionDT';
            break;
        case "AM":
            strurl = '/Modules/Announcement/Default.aspx/assignSessionDT';
            break;
        case "SM":
            strurl = '/Modules/SystemMaintenance/Default.aspx/assignSessionDT';
            break;
        case "OR":
            strurl = '/Modules/OutlookRecipientMaintenance/Default.aspx/assignSessionDT';
            break;
        case "RP":
            strurl = '/Modules/ReportRepository/Default.aspx/assignSessionDT';
            break;
        case "IAO":
            strurl = '/Modules/ImageArchiveOutward/Default.aspx/assignSessionDT';
            break;
        case "IAI":
            strurl = '/Modules/ImageArchiveInward/Default.aspx/assignSessionDT';
            break;
        case "RPTO":
            strurl = '/Modules/ImageArchiveOutward/ImageArchiveOutwardDetail.aspx/getReportUrl';
            break;

    }


    $.ajax({
        url: strurl,
        method: 'post',
        contentType: 'application/json; charset=utf-8',
        data: param,
        dataType: 'json',
        async: false,
        cache: false,
        success: function (data) {
            returnMsg = data.d;
        },
        error: function (xhr, status, error) {
            alert(xhr.responseText);  // to see the error message
            alert("Error connecting to server -> " + xhr.status + "-" + error);
            returnMsg = "Error";
        }
    });

    return returnMsg;
}

var isReset = false;
function onSubmitBatchMaint(obj) {

    var batchDir = $(".Dir_" + obj).val();
    var batchNo = $(".No_" + obj).val();
    var transNo = $(".Trans_" + obj).val();
    var siteName = $(".Site_" + obj).val();
    var bundleID = $(".Bundle_" + obj).val();

    var obj = { BatchDir: batchDir, BatchNo: batchNo, TransNo: transNo, BundleID: bundleID, SiteName: siteName };
    var param = JSON.stringify(obj);

    if (assignSessionMainDT("BM", param) != "0") {
        isLoadSpinner = false;
        // unloadSpinner();
        jQuery("#dialog-batchMaint").dialog(
            {
                modal: false,
                buttons: {
                    "Yes": {
                        text: "Yes",
                        id: "dialogBtnYes",
                        click: function () {
                            validNavigation = true;
                            $(this).dialog("close");
                            loadSpinner();
                            isLoadSpinner = true;
                            //loadSpinner();
                            validNavigation = true;
                            isReset = true;
                            callResetBatchMain(obj);
                        }
                    },
                    "No": {
                        text: "No",
                        id: "dialogBtnNo",
                        click: function () {
                            unloadSpinner();
                            $(this).dialog("close");
                        }
                    }
                }
            });


        //jQuery("#dialog-batchMaint").dialog(
        //    {
        //    modal: false,
        //    buttons: {
        //        "Yes": function () {
        //            validNavigation = true;
        //            $(this).dialog("close");
        //            loadSpinner();
        //            isLoadSpinner = true;
        //            //loadSpinner();
        //            validNavigation = true;
        //            isReset = true;
        //            callResetBatchMain(obj);

        //        },
        //        "No": function () {
        //            validNavigation = true;
        //            unloadSpinner();
        //            isLoadSpinner = false;
        //            isReset = false;
        //            $(this).dialog("close");
        //        }
        //    }
        //    });
    }

    return false;
}

function callResetBatchMain(obj) {
    var strval = obj;

    var tagI = document.createElement("INPUT");
    tagI.type = "hidden";
    tagI.name = "tag";
    tagI.value = "VALIDATE";

    var selRow = document.createElement("INPUT");
    selRow.type = "hidden";
    selRow.name = "tableID";
    selRow.value = strval;

    document.forms[0].appendChild(tagI);
    document.forms[0].appendChild(selRow);
    document.forms[0].submit();
}

function onSetDelTempID(obj) {
    deleteTableID = obj;
}

function onSetTempID(obj) {
    editTableID = obj;
    // alert(obj);


    //isLoadSpinner = true;
    ////loadSpinner();
    //validNavigation = true;

    //var strval = obj;

    //var tagI = document.createElement("INPUT");
    //tagI.type = "hidden";
    //tagI.name = "tag";
    //tagI.value = "VALIDATE";

    //var selRow = document.createElement("INPUT");
    //selRow.type = "hidden";
    //selRow.name = "tableID";
    //selRow.value = strval;

    //document.forms[0].appendChild(tagI);
    //document.forms[0].appendChild(selRow);
    //document.forms[0].submit();



}

function onSetTempParam(obj) {
    editTableID = obj;
}

function setRptURL(reportCode, clientCode, worksourceId, selBusdate) {
    var obj = { rptCode: reportCode, cltCode: clientCode, wsID: worksourceId, busdate: selBusdate };
    var param = JSON.stringify(obj);

    var returnMsg = "";
    $.ajax({
        url: '/Modules/Reports/Default.aspx/getReportUrl',
        method: 'post',
        contentType: 'application/json; charset=utf-8',
        data: param,
        dataType: 'json',
        async: false, //must wait till ajax call done then only return back
        cache: false,
        success: function (data) {
            returnMsg = data.d;
        },
        error: function (xhr, status, error) {
            alert(xhr.responseText);  // to see the error message
            //   alert("Error connecting to server -> " + jqXHR.status + "-" + errorThrown);
            returnMsg = "Error";
        }
    });

    return returnMsg;
}

function setAuditPrintRptURL(selDate, selClient, selSev, selCaller) {
    var obj = { busdate: selDate, clientCode: selClient, severity: selSev, caller: selCaller };
    var param = JSON.stringify(obj);

    var returnMsg = "";
    $.ajax({
        url: '/Modules/AuditLogViewer/Default.aspx/getReportUrl',
        method: 'post',
        contentType: 'application/json; charset=utf-8',
        data: param,
        dataType: 'json',
        async: false, //must wait till ajax call done then only return back
        cache: false,
        success: function (data) {
            returnMsg = data.d;
        },
        error: function (xhr, status, error) {
            alert(xhr.responseText);  // to see the error message
            //   alert("Error connecting to server -> " + jqXHR.status + "-" + errorThrown);
            returnMsg = "Error";
        }
    });

    return returnMsg;
}

function setImageArchivalPrintRptURLOutward() {
    //var obj = { busdate: selDate, clientCode: selClient, severity: selSev, caller: selCaller };
    var obj = {};
    var param = JSON.stringify(obj);

    var returnMsg = "";
    $.ajax({
        url: '/Modules/ImageArchiveOutward/ImageArchiveOutwardDetail.aspx/getReportUrl',
        method: 'post',
        contentType: 'application/json; charset=utf-8',
        data: param,
        dataType: 'json',
        async: false, //must wait till ajax call done then only return back
        cache: false,
        success: function (data) {
            returnMsg = data.d;
        },
        error: function (xhr, status, error) {
            alert(xhr.responseText);  // to see the error message
            //   alert("Error connecting to server -> " + jqXHR.status + "-" + errorThrown);
            returnMsg = "Error";
        }
    });

    return returnMsg;
}

function setImageArchivalPrintRptURLOutward2() {
    //var obj = { busdate: selDate, clientCode: selClient, severity: selSev, caller: selCaller };
    var obj = {};
    var param = JSON.stringify(obj);

    var returnMsg = "";
    $.ajax({
        url: '/Modules/ImageArchiveOutward/Default.aspx/getReportUrl',
        method: 'post',
        contentType: 'application/json; charset=utf-8',
        data: param,
        dataType: 'json',
        async: false, //must wait till ajax call done then only return back
        cache: false,
        success: function (data) {
            returnMsg = data.d;
        },
        error: function (xhr, status, error) {
            alert(xhr.responseText);  // to see the error message
            //   alert("Error connecting to server -> " + jqXHR.status + "-" + errorThrown);
            returnMsg = "Error";
        }
    });

    return returnMsg;
}
function setImageArchivalPrintRptURLInward() {
    //var obj = { busdate: selDate, clientCode: selClient, severity: selSev, caller: selCaller };
    var obj = {};
    var param = JSON.stringify(obj);

    var returnMsg = "";
    $.ajax({
        url: '/Modules/ImageArchiveInward/ImageArchiveInwardDetail.aspx/getReportUrl',
        method: 'post',
        contentType: 'application/json; charset=utf-8',
        data: param,
        dataType: 'json',
        async: false, //must wait till ajax call done then only return back
        cache: false,
        success: function (data) {
            returnMsg = data.d;
        },
        error: function (xhr, status, error) {
            alert(xhr.responseText);  // to see the error message
            //   alert("Error connecting to server -> " + jqXHR.status + "-" + errorThrown);
            returnMsg = "Error";
        }
    });

    return returnMsg;
}