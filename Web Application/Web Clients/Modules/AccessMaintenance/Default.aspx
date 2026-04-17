<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UBPCWeb.Modules.AccessMaintenance.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainAdminContent" runat="server">    

<form runat="server">

 <div class="content">
    <ul class="breadcrumb">
        <li>
            <p>Maintenance Tasks</p>
        </li>
        <li><a href="/Access" class="active">Access Maintenance</a></li>
    </ul>
    <div class="page-title">
        <i class="icon-custom-right"></i>
        <h3><span class="semi-bold"> Access Maintenance - List</span></h3>
    </div>
    <div class="row-fluid">
        <div class="span12">
            <div class="grid simple">
                <div class="grid-title">
<%--                    <asp:Button ID="New" runat="server" Text="Add New" CssClass="btn btn-primary" OnClick="New_Click" />--%>
<%--                    <button id="ClearFilter" class="btn btn-success">Clear Filters</button>--%>
                </div>
                <div class="grid-body ">
                    <table id="dataTable" class="table table-hover table-condensed">
                        <thead>
                             <tr class="dataTable_HeaderRow">
                                    <th>Function Code</th>
                                    <th>Function Name</th>
                                    <th>Group 1</th>
                                    <th>Group 2</th>
                                    <th>Group 3</th>
                                    <th>Group 4</th>
                                    <th>Group 5</th>
                                    <th>Group 6</th>
                                    <th>Group 7</th>
                                    <th>Group 8</th>
                                    
                                 <% if(isForUnisys.Equals(true)){ %>
                                 <th>
                                        Group 9

                                    </th>
                                 <%} %>
                                </tr>
                        </thead>                      
                        <tbody>
                            <% foreach (UBPCWeb.Modules.AccessMaintenance.AccessModel obj in acesssList)
                               {
                                   string ModuleName = obj.AccessCode;
                                   string ModuleDesc = obj.AccessName;
                                   bool group1 = obj.Group1;
                                   bool group2 = obj.Group2;
                                   bool group3 = obj.Group3;
                                   bool group4 = obj.Group4;
                                   bool group5 = obj.Group5;
                                   bool group6 = obj.Group6;
                                   bool group7 = obj.Group7;
                                   bool group8 = obj.Group8;
                                   bool group9 = obj.Group9;

                                   string chkIDNameGp1 = ModuleName + "_1";
                            %>
                            <tr>
                                <td><%: ModuleName %></td>
                                <td><%: ModuleDesc %></td>
                                <td class="text-nowrap">
                                    <% if (group1)
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_1" %>" value="<%: ModuleName + "_1" %>" checked />
                                    <%}
                                       else
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_1" %>" value="<%: ModuleName + "_1" %>" />
                                    <%} %>
                                </td>
                                <td class="text-nowrap">
                                    <% if (group2)
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_2" %>" value="<%: ModuleName + "_2" %>" checked />
                                    <%}
                                       else
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_2" %>" value="<%: ModuleName + "_2" %>" />
                                    <%} %>
                                </td>
                                <td class="text-nowrap">
                                    <% if (group3)
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_3" %>" value="<%: ModuleName + "_3" %>" checked />
                                    <%}
                                       else
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_3" %>" value="<%: ModuleName + "_3" %>" />
                                    <%} %>
                                </td>
                                <td class="text-nowrap">
                                    <% if (group4)
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_4" %>" value="<%: ModuleName + "_4" %>" checked />
                                    <%}
                                       else
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_4" %>" value="<%: ModuleName + "_4" %>" />
                                    <%} %>
                                </td>
                                <td class="text-nowrap">
                                    <% if (group5)
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_5" %>" value="<%: ModuleName + "_5" %>" checked />
                                    <%}
                                       else
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_5" %>" value="<%: ModuleName + "_5" %>" />
                                    <%} %>
                                </td>
                                <td class="text-nowrap">
                                    <% if (group6)
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_6" %>" value="<%: ModuleName + "_6" %>" checked />
                                    <%}
                                       else
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_6" %>" value="<%: ModuleName + "_6" %>" />
                                    <%} %>
                                </td>
                                <td class="text-nowrap">
                                    <% if (group7)
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_7" %>" value="<%: ModuleName + "_7" %>" checked />
                                    <%}
                                       else
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_7" %>" value="<%: ModuleName + "_7" %>" />
                                    <%} %>
                                </td>
                                <td class="text-nowrap">
                                    <% if (group8)
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_8" %>" value="<%: ModuleName + "_8" %>" checked />
                                    <%}
                                       else
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_8" %>" value="<%: ModuleName + "_8" %>" />
                                    <%} %>
                                </td>

                                <% if(isForUnisys.Equals(true)){ %>
                                <td class="text-nowrap">
                                    <% if (group9)
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_9" %>" value="<%: ModuleName + "_9" %>" checked />
                                    <%}
                                       else
                                       { %>
                                    <input type="checkbox" name="<%: "L_"+ ModuleName + "_9" %>" value="<%: ModuleName + "_9" %>" />
                                    <%} %>
                                </td>
                                <% } %>
                            </tr>

                            <%  }                                 %>
                        </tbody>
                    </table>
                     <input type="submit" value="Submit" class="btn btn-info" />
                </div>
            </div>
        </div>
    </div>
</div>
      
    </form>
 <script type="text/javascript" class="init">

 $(document).ready(function () {

        if (navigator.appVersion.indexOf("MSIE") != -1)
            $('select').ieExpandSelectWidth();

        //Dialog
        var redirectURL;

    });

</script>

 
</asp:Content>



