
function isAlphaNumeric(str) {
    var code, i, len;

    for (i = 0, len = str.length; i < len; i++) {
        code = str.charCodeAt(i);
        if (!(code > 47 && code < 58) && // numeric (0-9)
            !(code > 64 && code < 91) && // upper alpha (A-Z)
            !(code > 96 && code < 123)) { // lower alpha (a-z)
            return false;
        }
    }
    return true;
};

function checkPassword(param) {

    var returnMsg = "";
    $.ajax({
        url: '/Modules/OperatorMaintenance/Upsert.aspx/validatePassword',
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

function checkIsUserLogin(param) {
    var returnMsg = "";

    $.ajax({
        url: '/Modules/OperatorMaintenance/Upsert.aspx/checkUserLogin',
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
            returnMsg = "Error";
        }
    });

    return returnMsg;
}

function checkUserID(param) {
    var returnMsg = "";

    $.ajax({
        url: '/Modules/OperatorMaintenance/Upsert.aspx/validateUsrID',
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

function checkEmailAddrExist(param) {
    var returnMsg = "";

    $.ajax({
        url: '/Modules/OutlookRecipientMaintenance/Detail.aspx/validateEmailAddr',
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

function GenerateGuid()
{
    var guid = (Gid() + Gid() + Gid() + "-4" + Gid().substr(0, 3) + Gid() + Gid() + Gid() + Gid()).toLowerCase();
    return guid;
}

function Gid() {
    return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
}

function formatCurr(txt) {
    if (txt.value.indexOf(',') > -1) {
        var tmpCommaFormat = new Array();
        tmpCommaFormat = txt.value.split(",");

        for (var j in tmpCommaFormat) {
            txt.value = txt.value.replace(",", "");
            txt.value = txt.value.replace(".", "");
        }
    }

    txt.value = txt.value.replace(".", "00");
    if (txt.value.indexOf('.') > -1) {
        var tmpDelDecimal = new Array();
        tmpDelDecimal = txt.value.split(".");

        for (i = 0; i < tmpDelDecimal.length; i++) {
            txt.value = txt.value.replace(".", "00");
        }
    }

    if (isNaN(txt.value)) {
        txt.value = "0";
    }
    var validateNumber = parseFloat(txt.value);

    if (validateNumber < 1) {
        validateNumber = validateNumber * 100;
    }

    validateNumber = validateNumber / 100;

    var p = validateNumber.toFixed(2).split(".");


    var t = p[0].split("").reverse().reduce(function (acc, num, i, orig) {
        return num == "-" ? acc : num + (i && !(i % 3) ? "," : "") + acc;
    }, "") + "." + p[1];

    //txt.value = t.trim();
    if (t.trim().indexOf("undefined") > -1) {
        txt.value = "0.00";
    }
    else {
        txt.value = t.trim();
    }

}

function deformatAmt(txt)
{
    var currency = txt.value;// "2,123,456,777.89";
    var final = currency.replace(".", "");
    var number = Number(final.replace(/[^0-9\.]+/g, ""));
    // alert(txt.value);
    txt.value = ""+ number;
}

function replaceDot(txt)
{
    txt.value = txt.value.replace(".", "00");
}

function onlyDotsAndNumbers(event) {
    
    var charCode = (event.which) ? event.which : event.keyCode
    if (charCode == 46) {
        return true;
    }
    if (charCode > 31 && (charCode < 48 || charCode > 57))
        return false;

    return true;
}

function onlyNumbers(event) {
    var charCode = (event.which) ? event.which : event.keyCode
    if (charCode > 31 && (charCode < 48 || charCode > 57))
        return false;

    return true;
}

function checkEmailAddress(value) {
    var valid = false;
    const pattern = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/g;
    valid = pattern.test(value);
    return valid;
}