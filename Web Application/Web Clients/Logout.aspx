<%--<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Logout.aspx.cs" Inherits="UBPCWeb.Logout" %>--%>
<%@ Page Title="Logout" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Logout.aspx.cs" Inherits="UBPCWeb.Logout" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    
<style type="text/css">
     /* Back button */
    .errbutton {
	    padding: 5px 10px;
		    font-weight: 300;
	    font-size: 14px;
	    color: #fff;
	    text-shadow: 0px 1px 0 rgba(0,0,0,0.25);
	
	    background: #004b97;
	    border: 1px solid #004b97;
	    border-radius: 2px;
	    cursor: pointer;
	
	    box-shadow: inset 0 0 2px rgba(256,256,256,0.75);
	    -moz-box-shadow: inset 0 0 2px rgba(256,256,256,0.75);
	    -webkit-box-shadow: inset 0 0 2px rgba(256,256,256,0.75);
                width: 147px;
                height: 35px;
            }

    .errbutton:hover {
	    background: #3f9db8;
	    border: 1px solid rgba(256,256,256,0.75);
	
	    box-shadow: inset 0 1px 3px rgba(0,0,0,0.5);
	    -moz-box-shadow: inset 0 1px 3px rgba(0,0,0,0.5);
	    -webkit-box-shadow: inset 0 1px 3px rgba(0,0,0,0.5);
    }

    .errbutton:focus {
	    position: relative;
	    bottom: -1px;
	
	    background: #56c2e1;
	
	    box-shadow: inset 0 1px 6px rgba(256,256,256,0.75);
	    -moz-box-shadow: inset 0 1px 6px rgba(256,256,256,0.75);
	    -webkit-box-shadow: inset 0 1px 6px rgba(256,256,256,0.75);
    }

</style>
        
<div id="wrapper">
    <p  style="font-size:160%;"><b>Your Session Expired</b></p>
    <p>
    Dear Customer,<br />
    Your browser was idle for more than 15 minutes.<br />
    For increased security on this site, sessions are expired after 15 minutes of inactivity<br />
    If you wish to login again, please click "Back to Login Page".<br />
    </p>
<input type="button" value="Back to Login Page" class="errbutton" onclick="location.href='/';"  />

</div>
</asp:Content>
