using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using UBPCWeb.Modules.AccessMaintenance;
using System.Net.Sockets;

namespace UBPCWeb
{
    public partial class Site_Mobile : System.Web.UI.MasterPage
    {
        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
        private string _antiXsrfTokenValue;

        public List<AccessModel> MaintenanceListbyGroup = new List<AccessModel>();
        public List<AccessModel> MonitoringListbyGroup = new List<AccessModel>();
        public List<AccessModel> OperatorListbyGroup = new List<AccessModel>();
        public List<AccessModel> OthersbyGroup = new List<AccessModel>();

        protected void Page_Init(object sender, EventArgs e)
        {
            // The code below helps to protect against XSRF attacks
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            Guid requestCookieGuidValue;
            if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            {
                // Use the Anti-XSRF token from the cookie
                _antiXsrfTokenValue = requestCookie.Value;
                Page.ViewStateUserKey = _antiXsrfTokenValue;
            }
            else
            {
                // Generate a new Anti-XSRF token and save to the cookie
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
                Page.ViewStateUserKey = _antiXsrfTokenValue;

                var responseCookie = new HttpCookie(AntiXsrfTokenKey)
                {
                    HttpOnly = true,
                    Value = _antiXsrfTokenValue
                };
                if (FormsAuthentication.RequireSSL && Request.IsSecureConnection)
                {
                    responseCookie.Secure = true;
                }
                Response.Cookies.Set(responseCookie);
            }

            Page.PreLoad += master_Page_PreLoad;
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set Anti-XSRF token
                ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
                ViewState[AntiXsrfUserNameKey] = Context.User.Identity.Name ?? String.Empty;

                //Added by BOONCHONG PE-WD-24-003
                ClearSessionIfModuleChange();
            }
            else
            {
                // Validate the Anti-XSRF token
                if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
                    || (string)ViewState[AntiXsrfUserNameKey] != (Context.User.Identity.Name ?? String.Empty))
                {
                    throw new InvalidOperationException("Validation of Anti-XSRF token failed.");
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Url Referrer - Make sure it's clicking from within application 
            if (HttpContext.Current.Request.UrlReferrer == null)
            {
                //Session["s_UserID"] = "";
                Response.Redirect("/", false);//login
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                string referrer = HttpContext.Current.Request.UrlReferrer.ToString();


                string usrGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString().Trim();

                if (!string.IsNullOrEmpty(usrGroup))
                {
                    //load menu
                    DataTable dtAccesslist = CommonFunction.GetAllAccessByGroup(Session["s_UserGroup"].ToString());
                    MaintenanceListbyGroup = CommonFunction.GetAccessFunctionName(dtAccesslist, "ACC_Category = 'Maintenance Tasks'");
                    MonitoringListbyGroup = CommonFunction.GetAccessFunctionName(dtAccesslist, "ACC_Category = 'Monitoring Tasks'");
                    OperatorListbyGroup = CommonFunction.GetAccessFunctionName(dtAccesslist, "ACC_Category = 'Operator Tasks'");
                    OthersbyGroup = CommonFunction.GetAccessFunctionName(dtAccesslist, "ACC_Category = 'Others'");
                }
            }
        }

        private void ClearSessionIfModuleChange()
        {
            string currentPath = Request.Path.ToLower();
            string listingOutwardPath = "imagearchiveoutward";
            string listingInwardPath = "imagearchiveinward";

            if (!currentPath.Contains(listingOutwardPath))
            {
                ClearListingOutwardSession();
            }
            if (!currentPath.Contains(listingInwardPath))
            {
                ClearListingInwardSession();
            }
        }

        private void ClearListingOutwardSession()
        {
            // Clear session variables related to the listing module
            Session.Remove("SearchCri_Outward_ClientCode");
            Session.Remove("SearchCri_Outward_BusDate");
            //Session.Remove("SearchCri_Outward_BusDateTo");
            Session.Remove("SearchCri_Outward_DepAcc");
            Session.Remove("SearchCri_Outward_MicrAccNum");
            Session.Remove("SearchCri_Outward_MicrBSB");
            Session.Remove("SearchCri_Outward_ChequeNum");
            Session.Remove("SearchCri_Outward_ItemType");
            Session.Remove("SearchCri_Outward_PresentingBSB");
            Session.Remove("SearchCri_Outward_Amount");
            Session.Remove("SearchCri_Outward_Operate");
            Session.Remove("Outward_totalRecordCount");
            Session.Remove("ImageArchiveInwardFrontImg");
            Session.Remove("ImageArchiveInwardRearImg");
            Session.Remove("ImageArchiveInwardFrontJPEG");
        }

        private void ClearListingInwardSession()
        {
            // Clear session variables related to the listing module
            Session.Remove("SearchCri_Inward_ClientCode");
            Session.Remove("SearchCri_Inward_BusDate");
            //Session.Remove("SearchCri_Inward_BusDateTo");
            Session.Remove("SearchCri_Inward_MicrAccNum");
            Session.Remove("SearchCri_Inward_ChequeNum");
            Session.Remove("SearchCri_Inward_ChequeBSB");
            Session.Remove("SearchCri_Inward_Amount");
            Session.Remove("SearchCri_Inward_Operate");
            Session.Remove("ImageArchiveInwardFrontImg");
            Session.Remove("ImageArchiveInwardRearImg");
            Session.Remove("ImageArchiveInwardFrontJPEG");
        }
    }
}