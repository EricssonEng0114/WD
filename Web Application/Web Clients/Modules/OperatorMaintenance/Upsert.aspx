<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Upsert.aspx.cs" Inherits="UBPCWeb.Modules.OperatorMaintenance.Upsert" %>
<asp:Content ID="ContentUpsert" ContentPlaceHolderID="MainAdminContent" runat="server">
    
    
    
    <form runat="server" id="submitForm">

    <div class="content">
    <ul class="breadcrumb">
        <li>
            <p>Maintenance Tasks</p>
        </li>
        <li><a href="/Operator" class="active">Operator Maintenance</a> </li>
    </ul>
    <div class="page-title">
        <i class="icon-custom-right"></i>
            <h3><span class="semi-bold">
                <asp:Label ID="lblTitle" runat="server" Text="Create/Update Operator"></asp:Label>
                </span></h3>
    </div>
    <div class="row">
        <div class="col-md-12">
            <div class="grid simple">
                <div class="grid-title no-border">
                    <h4> Please fill in all mandatory field(s) below and click Save.</h4>
                </div>
                <div class="grid-body no-border">
                    <div class="row">
                        <div class="col-md-8 col-sm-8 col-xs-8">
                            <input type="hidden" name="saveCommand" id="saveCommand" value="" />

                            <% if(isEditMode){  %>
                            <div class="form-group">                                   
                                  <asp:CheckBox ID="chkIsLogin" runat="server" style = "transform: scale(1.2);-webkit-transform: scale(1.2);" />
                                  <asp:Label ID="lblIsLogin" runat="server" Text="Is Login
/Locked
                                  " Font-Bold="True" Font-Italic="True" ForeColor="Black"></asp:Label>  

                             <div class="controls"></div>
                             </div>
                             <div class="form-group">                                   
                                  <asp:CheckBox ID="chkForcePasswordChange" runat="server" style = "transform: scale(1.2);-webkit-transform: scale(1.2);" />
                                  <asp:Label ID="lblForcedPwdChanged" runat="server" Text="Label" Font-Bold="true" Font-Italic="true" ForeColor="Black">Forced Password Changed
                                  </asp:Label> 
                                 
                                                                
                             <div class="controls"></div>
                             </div>
                            <% }else{ %>
                            <% } %>
                            <div class="form-group">
                                <label class="form-label">*User ID</label>
                                <span id="UserID_Required" class="help" style="display: none; color: red;">*Required</span>
                                <span id="UserIDLength_Required" class="help" style="display: none; color: red;">*At least 6 Characters required</span>
                                <span id="UserIDExist" class="help" style="display: none; color: red;">*This User ID already existed.</span>
                                <span id="Alphanumeric_Required" class="help" style="display: none; color: red;">*Please enter only alphabets and/or numbers.</span>
                                <div class="controls">
                                    <asp:TextBox ID="txtUsrID" runat="server" CssClass="form-control" MaxLength="8"></asp:TextBox>
                                </div>
                            </div>

                            <div class="form-group">
                                <label class="form-label">*User Name</label>
                                <span id="UserName_Required" class="help" style="display: none; color: red;">*Required</span>

                                <div class="controls">
                                    <asp:TextBox ID="txtUsrName" runat="server" CssClass="form-control" MaxLength="20"></asp:TextBox>
                                </div>
                            </div>
                            

                            <% if(isPasswordEditable) { %>
                            <div class="form-group">
                                <asp:Label ID="lblPassword" runat="server" Text="Password" ForeColor="Black"></asp:Label>
                                <span id="Password_Validate" class="help" style="display: none; color: red;"></span>
                                <span id="Password_Required" class="help" style="display: none; color: red;">*Required</span>
                                <div class="controls">
                                    <asp:TextBox ID="txtPassword" runat="server" MaxLength="30" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                </div>
                            </div> 
                            <% } %>
                                
                            <div class="form-group">
                                <label class="form-label">*User Group</label>
                                <span id="UserGp_Required" class="help" style="display: none; color: red;">*Required</span>
                                <div class="controls">
                                    <asp:DropDownList ID="ddlUserGroup" runat="server" CssClass="form-control"></asp:DropDownList>

                                </div>
                            </div>   

                             <div class="form-group">
                                <label class="form-label">*Clients</label>
                                <span id="Client_Required" class="help" style="display: none; color: red;">*Required</span>
                                <div class="controls">
                                    <asp:ListBox ID="lstBoxClients" runat="server" CssClass="form-control" SelectionMode="Multiple"></asp:ListBox>
                                </div>
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Comments</label>
                                <div class="controls">
                                    <textarea id="txtAreaComment" rows="2" runat="server" class="form-control" ></textarea>
                                </div>
                            </div>


            <asp:Button ID="btnSave" runat="server" Text="Save"  CssClass="btn btn-info" OnClick="btnSave_Click" OnClientClick="return checkSaveData()" />

             <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" CssClass="btn btn-danger" OnClientClick="oncancel()" />



    </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

        
 <!-- #region Confirmation Box -->
<div id="confirmUpdate" class="modal fade" tabindex="-1" role="dialog" aria-hidden="true">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header ">
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
                <h4 class="modal-title" id="confirmUpdateModel">Update Confirmation</h4>
            </div>
            <div class="modal-body">
                <span>User is currently login, are you sure to proceed with the update?</span>
            </div>
            <div class="modal-actions" style="padding-bottom:5px;padding-top:5px">
                <asp:Button ID="btnForcedSave" runat="server" Text="Yes" CssClass="btn btn-primary"  style="margin-left:10px;width:10%;" OnClick="btnForcedSave_Click" />
                <button type="button" class="btn btn-primary" data-dismiss="modal" style="margin-left:10px;width:10%;">No</button>
            </div>
        </div>
    </div>
</div>

<!-- #endregion -->


</form>
   

    <script>
        function oncancel() {
            isLoadSpinner = true;
        }

        function pad(n, width, z) {
            z = z || '0';
            n = n + '';
            return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
        }


        function checkSaveData() {
          
            var countEmpty = 0;
            var userID = $("#<%: txtUsrID.ClientID %>").val();
            var userName = $("#<%: txtUsrName.ClientID %>").val();
            var password = $("#<%: txtPassword.ClientID %>").val();
            var userGp = $("#<%: ddlUserGroup.ClientID %>").find(":selected").val();
            var userClient = $("#<%: lstBoxClients.ClientID %>").find(":selected").val();
            var isEdit = '<%:isEditMode %>';

            if (userID.length > 0) {
                $('#UserID_Required').hide();

                if (userID.length < 6) {
                    isLoadSpinner = false;
                    unloadSpinner();
                    $('#UserIDLength_Required').show();
                    return false;
                }
                else {
                    
                    $('#UserIDLength_Required').hide();

                    if (isEdit == "False") {
                        //check invalid user
                        if (validateUsrExist() != "0") {
                            isLoadSpinner = false;
                            unloadSpinner();
                            $('#UserIDExist').show();
                            return false;
                        }
                        else {

                            $('#UserIDExist').hide();

                            if (isAlphaNumeric(userID)) {
                                $('#Alphanumeric_Required').hide();
                            }
                            else {
                                isLoadSpinner = false;
                                unloadSpinner();
                                $('#Alphanumeric_Required').show();
                                return false;
                            }                            
                        }
                    }
                   
                }
            }
            else {
                countEmpty = countEmpty + 1;
                $('#UserID_Required').show();
            }

            if(password){

                if (password.length > 1) {
                    var retMsg = validatePassword(password);

                    if (retMsg != '') {
                        var span = document.getElementById("Password_Validate");
                        $("#Password_Validate").html("");

                        var txt = document.createTextNode(retMsg);
                        //if (span.childNodes[0] != null)
                        //{
                        //    span.removeChild(span.childNodes[0]);
                        //}

                        span.appendChild(txt);
                        countEmpty = countEmpty + 1;
                        $('#Password_Validate').show();
                    }
                    else
                    {
                        $('#Password_Validate').hide();
                    }
                }
                else
                {
                    if (isEdit == "False") {
                        countEmpty = countEmpty + 1;
                        $('#Password_Required').show();
                    }
                    else
                    {
                        $('#Password_Required').hide();
                    }
                }
            }

            if (userName.length > 0) {
                $('#UserName_Required').hide();
            }
            else {
                countEmpty = countEmpty + 1;
                $('#UserName_Required').show();
            }


            if (userGp.length > 0) {
                $('#UserGp_Required').hide();
            }
            else {
                countEmpty = countEmpty + 1;
                $('#UserGp_Required').show();
            }


            if (userClient.length > 0) {
                $('#Client_Required').hide();
            }
            else {
                countEmpty = countEmpty + 1;
                $('#Client_Required').show();
            }

            if (countEmpty > 0) {
                isLoadSpinner = false;
                unloadSpinner();
                return false;
            }
            else {

                if (validateUsrLogin() != "0") {
                    //Prompt confirm update 
                    isLoadSpinner = false;
                    unloadSpinner();

                    $('#confirmUpdate').modal();
                    return false;
                }
                else {
                    isLoadSpinner = true;
                    return true;
                }
            }          
        }

       

        function validatePassword(password) {
            loadSpinner();

            var inputUsrID = $("#<%: txtUsrID.ClientID %>").val();
            var obj = { param: password, id: inputUsrID };
            var param = JSON.stringify(obj);

            return checkPassword(param);

            //$.ajax({
            //    url: '/Modules/OperatorMaintenance/Upsert.aspx/validatePassword', // remove the ..
            //    method: 'post',
            //    contentType: 'application/json; charset=utf-8',
            //    data: param,
            //    dataType: 'json',
            //    async: true,
            //    cache: false,
            //    success: function (data) {
            //        return data.d;
            //        //alert(data.d); // true
            //    },
            //    error: function (xhr, status, error) {
            //        alert(xhr.responseText);  // to see the error message
            //        //   alert("Error connecting to server -> " + jqXHR.status + "-" + errorThrown);
            //    }
            //});
        }


        function validateUsrLogin() {
            loadSpinner();

            var inputUsrID = $("#<%: txtUsrID.ClientID %>").val();
            var obj = { id: inputUsrID };
            var param = JSON.stringify(obj);
            return checkIsUserLogin(param);
        }

        function validateUsrExist() {
            loadSpinner();

            var inputUsrID = $("#<%: txtUsrID.ClientID %>").val();
            var obj = { id: inputUsrID };
            var param = JSON.stringify(obj);
            return checkUserID(param);
        }
    </script>

</asp:Content>
