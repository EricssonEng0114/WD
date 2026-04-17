<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ErrorPage401.aspx.cs" Inherits="UBPCWeb.Errors.ErrorPage401" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    
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
    <p  style="font-size:160%;"><b>Error 401 - Unauthorized</b></p>
    <p>
    Dear Customer,<br />
    You do not have permission to access this page.<br />
    Please ensure the login credential is corectly entered.<br /><br />
    To login into your account, please click "Back to Login Page".<br />
    </p>
<input type="button" value="Back to Login Page" class="errbutton" onclick="location.href='/';"  />

</div>
</asp:Content>
