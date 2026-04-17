using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using UBPC.Web.Common;

namespace UBPCWeb
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Application_Error() 
        {
            //Get Execptio object
            Exception serverException = Server.GetLastError();


            try
            {
                //Log the exception and notify system operators
                //Call Write Audit Log
                // Audit Log
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Others;
                log.Severity = LogEventType.Error;
                log.Message = serverException.Message;
                log.Exception = serverException;
                log.Write();

                var HEx = serverException as HttpException;
                if (HEx != null)
                {
                    int HttpCode = HEx.GetHttpCode();
                    Server.ClearError();

                    if (HttpCode == 404) // Page Not Found 
                    {
                        Response.StatusCode = 404;
                        Server.Transfer("Errors/ErrorPage400.aspx");
                    }
                    else if (HttpCode == 401) // Access Denied 
                    {
                        Response.StatusCode = 401;
                        Server.Transfer("Errors/ErrorPage401.aspx");
                    }
                    else if (HttpCode == 400)
                    {
                        Response.StatusCode = 400;
                        Server.Transfer("Errors/ErrorPage400.aspx");
                    }
                    else if (HttpCode == 403)
                    {
                        Response.StatusCode = 400;
                        Server.Transfer("Errors/ErrorPage403.aspx");
                    }
                    else if (HttpCode == 500)
                    {
                        Response.StatusCode = 500;
                        Server.Transfer("Errors/ErrorPage500.aspx");
                    }

                }
                else
                {
                    Server.Transfer("CustomErrorPage.aspx");
                }
              
                ////For other kinds of errors give the user some information
                ////but stay on the default page
                //Response.Write("<h2>Global Page Error</h2>" + Environment.NewLine);
                //Response.Write("<p>"+ exc.Message + "</p>" + Environment.NewLine);
                //Response.Write(("Return to the <a href='Default.aspx'>" + "Default Page</a>" + Environment.NewLine));

              

                //Clear the error from the server
                Server.ClearError();
            }
            catch (Exception ex)
            {
                //if failed to 

                Response.Write("<h2>Global Page Error</h2>" + Environment.NewLine);
                Response.Write("<p>" + serverException.Message + "</p>" + Environment.NewLine);
                Response.Write(("Return to the <a href='Default.aspx'>" + "Default Page</a>" + Environment.NewLine));


              
            }
        }

    }
}