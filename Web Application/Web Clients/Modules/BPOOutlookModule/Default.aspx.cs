using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Configuration;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using UBPC.Web.Common;

namespace UBPCWeb.Modules.BPOOutlookModule
{
    public partial class Default : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private string clientCode = string.Empty;
        private bool isPageValid = false;
        private string clientList = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString();
                userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString();
                clientList = Session["s_UserClients"] == null ? string.Empty : Session["s_UserClients"].ToString();
                Session["s_IsCutOffTime"] = false;


                if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
                {
                    PageValidatorResult validatorResult;

                    //For password chnge, validate password by encryption class
                    validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "BPOOutlook");
                    isPageValid = validatorResult.Valid;

                    if (!isPageValid)
                    {
                        Session["s_GeneralMsg"] = validatorResult.ReturnMessage;
                        Response.Redirect("/Home", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        Session["s_GeneralMsg"] = string.Empty;
                        isPageValid = true;
                    }
                }
                else
                {
                    //invalid user - back to login page
                    Response.Redirect("/Login", false);
                    Context.ApplicationInstance.CompleteRequest();
                }

                if (isPageValid)
                {
                    Session["s_UploadFileServerPath"] = string.Empty;
                }

                //Load Control for first time
                if (isPageValid & !Page.IsPostBack)
                {
                    LoadControl();

                    if (Session["s_SuccessMsg"] != null)
                    {
                        string successMsg = Session["s_SuccessMsg"].ToString();
                        if (!string.IsNullOrEmpty(successMsg))
                        {
                            ClientScript.RegisterStartupScript(this.GetType(), "showAlert", "<script type='text/javascript'>alert('" + successMsg + "');</script>", false);
                            Session["s_SuccessMsg"] = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.BPOOutlookModule;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }
        protected void LoadControl()
        {
            try
            {
                //Load Client Drop Down
                string[] cltArr = clientList.Split(',');
                List<ListItem> cltitems = new List<ListItem>();
                foreach (string clientCode in cltArr) cltitems.Add(new ListItem(clientCode, clientCode));
                this.ddlClient.DataSource = from i in cltitems select new ListItem() { Text = i.Text, Value = i.Value };
                this.ddlClient.DataTextField = "Text";
                this.ddlClient.DataValueField = "Value";
                this.ddlClient.DataBind();
                this.ddlClient.SelectedIndex = 0;

                bindRptList();
                //This will reload two drop down for recipient: Client Recipient Drop Down , Internal Unisys Recipient Drop Down
                ClientScript.RegisterStartupScript(this.GetType(), "reloadRecipientDropDown", "reloadRecipientDropDown()", true);
                //bindRecipientList();

                //restrict file type for upload
                string fileTypeAllowed = Parameters.GetParamValue(webDBConnStr, Resource.strFileTypeAllowed);
                //uploadFile.Attributes.Add("accept", fileTypeAllowed);
                hfAllowFileType.Value = fileTypeAllowed;

                //restrict maximum number of file allowed to upload
                string hfMaxNumOfFile = Parameters.GetParamValue(webDBConnStr, Resource.strFileCountAllowed);
                hfFileCountAllowed.Value = hfMaxNumOfFile;

                //restrict maximum file size
                HttpRuntimeSection section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
                hfMaxFileSize.Value = (section.MaxRequestLength * 1024).ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void bindRptList()
        {
            cbClientReportList.Items.Clear();
            string mainSite = Parameters.GetParamValue(webDBConnStr, Resources.Resource.MainSiteParam);
            DBConnectionInfo db = new DBConnectionInfo();
            db.webConnStr = webDBConnStr;
            string mainUVRPSConnection = db.GetSiteConnectionString(true, ddlClient.SelectedValue.ToString(), db.GetMainSite(mainSite, ddlClient.SelectedValue.ToString().Trim()));

            DateTime curActiveBusdate = DateTime.Today;
            if (BusinessDate.GetActive(mainUVRPSConnection) != null)
            {
                curActiveBusdate = Convert.ToDateTime(BusinessDate.GetActive(mainUVRPSConnection));
            }

            DataTable dt = CommonFunction.GetReportListByClient(ddlClient.SelectedValue.ToString(), curActiveBusdate);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    cbClientReportList.Items.Add(new ListItem(row["REPO_ReportFileName"].ToString(), row["REPO_FileUploadedPath"].ToString() + "\\" + row["REPO_ReportFileName"].ToString() + ".txt"));
                }
            }
        }

        protected void ddlClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            bindRptList();
            //bindRecipientList();
            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "reloadForm", "reloadForm()", true);
        }

        protected void btnClientSendEmail_Click(object sender, EventArgs e)
        {
            LogEntry log;
            string value = string.Empty;
            Session["s_SuccessMsg"] = string.Empty;
            try
            {
                // Force TLS1.2 to send email via sendgrid to refer to param om user id and password
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

                string smtpServer = Parameters.GetParamValue(webDBConnStr, "SMTPServer", true);
                string smtpPort = Parameters.GetParamValue(webDBConnStr, "SMTPServer", false);
                bool smtpEnableSSL = Convert.ToBoolean(Parameters.GetParamValue(webDBConnStr, "SMTPEnableSSL"));
                string smtpUserName = Parameters.GetParamValue(webDBConnStr, "SMTPCredential");
                string smtpPassword = Parameters.GetParamValue(webDBConnStr, "SMTPCredential", false);
                string smtpMailBox = Parameters.GetParamValue(webDBConnStr, "SMTPMailBox", true);
                List<Attachment> attachments = new List<Attachment>();
                foreach (ListItem item in cbClientReportList.Items)
                {
                    if (item.Selected)
                    {

                        if (File.Exists(item.Value))
                        {
                            string dest = item.Value.Replace(item.Text + ".txt", item.Text);
                            File.Copy(item.Value, dest, true);
                            attachments.Add(new Attachment(dest));
                        }
                        else
                        {
                            log = new LogEntry();
                            log.Severity = LogEventType.Error;
                            log.Caller = LogCallerID.BPOOutlookModule;
                            log.ClientCode = ddlClient.SelectedValue;
                            log.Message = "Error during attach file." + item.Value + " does not exists.";
                            log.Write();
                        }
                    }
                }

                MailMessage email = new MailMessage();
                email.From = new MailAddress(smtpMailBox);
                email.Subject = txtClientEmailSubject.Text;
                if (attachments.Count > 0)
                {
                    foreach (Attachment att in attachments)
                    {
                        email.Attachments.Add(att);
                    }
                }
                List<string> liRecipient = hfClientRecipient.Value.Split(';').ToList();
                foreach (string recipient in liRecipient)
                {
                    email.To.Add(recipient);
                }

                string body = string.Empty;
                using (StreamReader sr = new StreamReader(Server.MapPath("~/Modules/BPOOutlookModule/DefaultTemplate.html")))
                {
                    body = sr.ReadToEnd();
                }

                LinkedResource unisysLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/unisys.png"));
                unisysLogo.ContentId = "UnisysImage";

                LinkedResource linkedinLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/linkedin.jpg"));
                linkedinLogo.ContentId = "linkedinImage";

                LinkedResource twitterLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/twitter.jpg"));
                twitterLogo.ContentId = "twitterImage";

                LinkedResource youtubeLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/youtube.jpg"));
                youtubeLogo.ContentId = "youtubeImage";

                LinkedResource facebookLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/facebook.jpg"));
                facebookLogo.ContentId = "facebookImage";

                LinkedResource vimeoLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/vimeo.jpg"));
                vimeoLogo.ContentId = "vimeoImage";

                LinkedResource blogLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/blog.jpg"));
                blogLogo.ContentId = "blogImage";

                body = body.Replace("{1}", txtClientEmailMessage.Text);
                AlternateView view = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                view.LinkedResources.Add(unisysLogo);
                view.LinkedResources.Add(linkedinLogo);
                view.LinkedResources.Add(twitterLogo);
                view.LinkedResources.Add(youtubeLogo);
                view.LinkedResources.Add(facebookLogo);
                view.LinkedResources.Add(vimeoLogo);
                view.LinkedResources.Add(blogLogo);
                email.AlternateViews.Add(view);
                email.IsBodyHtml = true;

                SmtpClient smtpClient = new SmtpClient(smtpServer, Convert.ToInt32(smtpPort));
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.EnableSsl = smtpEnableSSL;
                smtpClient.Credentials = new System.Net.NetworkCredential(smtpUserName, smtpPassword);
                smtpClient.Send(email);
                smtpClient.Dispose();
                email.Attachments.Dispose();
                email.Dispose();

                log = new LogEntry();
                log.Caller = LogCallerID.BPOOutlookModule;
                log.ClientCode = ddlClient.SelectedValue;
                log.UserName = userID;
                log.Severity = LogEventType.Information;
                log.Message = "Email successfully send out";
                log.Data = "Subject:" + txtClientEmailSubject.Text.ToString();
                log.Write();

                foreach (ListItem item in cbClientReportList.Items)
                {
                    if (item.Selected)
                    {

                        if (File.Exists(item.Value))
                        {
                            string dest = item.Value.Replace(item.Text + ".txt", item.Text);
                            File.Delete(dest);
                        }
                    }
                }

                Session["s_SuccessMsg"] = "Email successfully send out";

                Response.Redirect("/BPOOutlook", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                log = new LogEntry();
                log.Caller = LogCallerID.BPOOutlookModule;
                log.ClientCode = ddlClient.SelectedValue;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "focusClientSection", "focusClientSection('" + ex.Message + "');", true);
            }
        }

        protected void btnInternalSendEmail_Click(object sender, EventArgs e)
        {
            LogEntry log;
            string value = string.Empty;
            Session["s_SuccessMsg"] = string.Empty;
            try
            {
                string smtpServer = Parameters.GetParamValue(webDBConnStr, "SMTPServer", true);
                string smtpPort = Parameters.GetParamValue(webDBConnStr, "SMTPServer", false);
                bool smtpEnableSSL = Convert.ToBoolean(Parameters.GetParamValue(webDBConnStr, "SMTPEnableSSL"));
                string smtpUserName = Parameters.GetParamValue(webDBConnStr, "SMTPCredential");
                string smtpPassword = Parameters.GetParamValue(webDBConnStr, "SMTPCredential", false);
                string smtpMailBox = Parameters.GetParamValue(webDBConnStr, "SMTPMailBox", true);
                List<Attachment> attachments = new List<Attachment>();
                List<string> internalAttachments = hfInternalAttachment.Value.Split(';').ToList();
                string tempFolder = "C:\\Temp\\" + userID;
                foreach (string fileName in internalAttachments)
                {
                    string filePath = tempFolder + "\\" + fileName;
                    if (File.Exists(filePath))
                    {
                        attachments.Add(new Attachment(filePath));
                    }
                    else
                    {
                        log = new LogEntry();
                        log.Severity = LogEventType.Error;
                        log.ClientCode = "MBB";
                        log.Caller = LogCallerID.BPOOutlookModule;
                        log.Message = "Error during attach file." + fileName + " does not exists.";
                        log.Write();
                    }
                }

                MailMessage email = new MailMessage();
                email.From = new MailAddress(smtpMailBox);
                email.Subject = txtInternalEmailSubject.Text;
                if (attachments.Count > 0)
                {
                    foreach (Attachment att in attachments)
                    {
                        email.Attachments.Add(att);
                    }
                }

                //List<string> liRecipient = txtInternalEmailRecipient.Text.Split(';').ToList();
                //foreach (string recipient in liRecipient)
                //{
                //    email.To.Add(recipient);
                //}

                List<string> liRecipient = hfInternalRecipient.Value.Split(';').ToList();
                foreach (string recipient in liRecipient)
                {
                    email.To.Add(recipient);
                }

                string body = string.Empty;
                using (StreamReader sr = new StreamReader(Server.MapPath("~/Modules/BPOOutlookModule/DefaultTemplate.html")))
                {
                    body = sr.ReadToEnd();
                }

                LinkedResource unisysLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/unisys.png"));
                unisysLogo.ContentId = "UnisysImage";

                LinkedResource linkedinLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/linkedin.jpg"));
                linkedinLogo.ContentId = "linkedinImage";

                LinkedResource twitterLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/twitter.jpg"));
                twitterLogo.ContentId = "twitterImage";

                LinkedResource youtubeLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/youtube.jpg"));
                youtubeLogo.ContentId = "youtubeImage";

                LinkedResource facebookLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/facebook.jpg"));
                facebookLogo.ContentId = "facebookImage";

                LinkedResource vimeoLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/vimeo.jpg"));
                vimeoLogo.ContentId = "vimeoImage";

                LinkedResource blogLogo = new LinkedResource(Server.MapPath("~/Modules/BPOOutlookModule/TemplateImages/blog.jpg"));
                blogLogo.ContentId = "blogImage";
                body = body.Replace("{1}", txtInternalEmailMessage.Text);
                AlternateView view = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                view.LinkedResources.Add(unisysLogo);
                view.LinkedResources.Add(linkedinLogo);
                view.LinkedResources.Add(twitterLogo);
                view.LinkedResources.Add(youtubeLogo);
                view.LinkedResources.Add(facebookLogo);
                view.LinkedResources.Add(vimeoLogo);
                view.LinkedResources.Add(blogLogo);
                email.AlternateViews.Add(view);
                email.IsBodyHtml = true;

                SmtpClient smtpClient = new SmtpClient(smtpServer, Convert.ToInt32(smtpPort));
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.EnableSsl = smtpEnableSSL;
                smtpClient.Credentials = new System.Net.NetworkCredential(smtpUserName, smtpPassword);
                smtpClient.Send(email);
                smtpClient.Dispose();
                email.Attachments.Dispose();
                email.Dispose();

                log = new LogEntry();
                log.Caller = LogCallerID.BPOOutlookModule;
                log.ClientCode = "MBB";
                log.UserName = userID;
                log.Severity = LogEventType.Information;
                log.Message = "Email successfully send out";
                log.Data = "Subject:" + txtInternalEmailSubject.Text.ToString();
                log.Write();

                Session["s_SuccessMsg"] = "Email successfully sent out";

                Response.Redirect("/BPOOutlook", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                log = new LogEntry();
                log.Caller = LogCallerID.BPOOutlookModule;
                log.ClientCode = "MBB";
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "focusInternalSection", "focusInternalSection('" + ex.Message + "');", true);
            }
        }

        [WebMethod]
        public static List<string> getRecipientByClient(string clientCode, string[] siteLst)
        {
            DataTable dt = UBPCWeb.Modules.OutlookRecipientMaintenance.CommonFunction.GetAllRecpByClient(clientCode, siteLst);
            List<string> liRecipient = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                liRecipient.Add(row["Recp_EmailAddr"].ToString());
            }
            return liRecipient;
        }
    }
}