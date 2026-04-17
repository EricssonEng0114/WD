<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb.Modules.Profile.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">

<form runat="server">
    <div class="content">
    <ul class="breadcrumb">
        <li>
            <p>My Login Info</p>
        </li>
        <li><a href="#" class="active">View Profile</a> </li>
    </ul>
    <div class="page-title">
        <i class="icon-custom-right"></i>
        <h3><span class="semi-bold">View Profile</span></h3>
    </div>
    <div class="row">
        <div class="col-md-12">
            <div class="grid simple">
                <div class="grid-title no-border">
                </div>
                <div class="grid-body no-border">
                    <br>
                    <div class="row">
                        <div class="col-md-8 col-sm-8 col-xs-8">
                            <div class="form-group">
                                <label class="form-label">User ID</label>
                                <span class="help"></span>
                                <div class="controls">
                                    <asp:TextBox ID="txtUsrID" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="form-label">User Name</label>
                                <span class="help"></span>
                                <div class="controls">
                                    <asp:TextBox ID="txtUsrName" runat="server"  CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="form-label">User Group</label>
                                <span class="help"></span>
                                <div class="controls">
                                    <asp:TextBox ID="txtUsrGp" runat="server"  CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="form-label">Clients</label>
                                <span class="help"></span>
                                <div class="controls">                                    
                                    <textarea id="txtAreaClnt" rows="2" runat="server" readonly class="form-control" ></textarea>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="form-label">Last Password Changed</label>
                                <span class="help"></span>
                                <div class="controls">
                                    <asp:TextBox ID="txtLastPwsChg" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                </div>
                            </div>     
                             <div class="form-group">
                                <label class="form-label">Last Login At</label>
                                <span class="help"></span>
                                <div class="controls">
                                    <asp:TextBox ID="txtLastLogin" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
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
</asp:Content>
