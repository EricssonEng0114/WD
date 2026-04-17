using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.UI;

namespace UBPCWeb
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkID=303951
        public static void RegisterBundles(BundleCollection bundles)
        {

            #region Login Master Page
            bundles.Add(new StyleBundle("~/bundles/loginCss").Include(
                            "~/css/style.css",
                            "~/css/custom.css",
                            "~/css/font-awesome.min.css",
                            "~/css/jquery.datetimepicker.css",
                            "~/css/chosen.css",
                            "~/css/google-font-droid-serif.css",
                            "~/css/google-font-montserrat.css",
                            "~/css/bootstrap3-dialog.min.css",
                            "~/css/colors/default.css",
                            "~/js/jquery-ui/jquery-ui.min.css",
                            "~/Content/Login.css"));

            bundles.Add(new ScriptBundle("~/bundles/LoginJs").Include(
                         //"~/js/jquery.js",
                         //PE-WD-23-001  update jquery version
                         "~/admincss/assets/plugins/jquery/jquery-3.6.2.min.js",
                          "~/js/jquery-ui/jquery-ui.min.js",
                          "~/js/jquery-migrate.min.js",
                          "~/js/modernizr-2.7.1.min.js",
                          "~/js/jquery.cookie.js",
                          "~/js/jquery.blockUI.min.js",
                          "~/js/imagesloaded.pkgd.min.js",
                          "~/js/isotope-2.0.0.min.js",
                          "~/js/jquery.touchSwipe.min.js",
                          "~/js/bootstrap.min.js",
                       //   "~/js/hoverIntent-r7.min.js",
                       //   "~/js/superfish-1.7.4.min.js",
                          "~/js/script.js",
                          "~/js/chosen.jquery.min.js",
                          "~/js/jquery.datetimepicker.js",
                          "~/js/jquery.parallax-1.1.3.js",
                          "~/js/jquery.carouFredSel-6.2.1-packed.js",
                          "~/js/custom.js",
                          "~/js/bootstrap3-dialog.min.js"
                          ));
            #endregion


            #region Admin css

            bundles.Add(new StyleBundle("~/bundles/adminCss").Include(
                          "~/admincss/assets/plugins/bootstrap-tag/bootstrap-tagsinput.css",
                          "~/admincss/assets/plugins/bootstrap-datepicker/css/datepicker.css",
                          "~/admincss/assets/plugins/bootstrapv3/css/bootstrap.min.css",
                          "~/admincss/assets/plugins/bootstrapv3/css/bootstrap-theme.min.css",
                          "~/admincss/assets/plugins/font-awesome/css/font-awesome.min.css",
                          "~/admincss/assets/plugins/animate.min.css",
                          "~/admincss/assets/plugins/jquery-scrollbar/jquery.scrollbar.css",
                //"~/admincss/webarch/css/webarch.css",
                          "~/admincss/assets/plugins/bootstrap-select2/select2.css",
                          "~/admincss/assets/plugins/jquery-datatable/css/jquery.dataTables.css",
                          "~/admincss/assets/plugins/datatables-responsive/css/datatables.responsive.css",
                          "~/admincss/assets/plugins/bootstrap-multiselect/bootstrap-multiselect.css",
                          "~/css/globalFont.css",
                          //
                           "~/css/bootstrap3-dialog.min.css"
                          )
                         .Include("~/admincss/webarch/css/webarch.css", new CssRewriteUrlTransform())
                    );

            //   bundles.Add(new StyleBundle("~/Content/adminCSSFontAwesome").Include("~/Content/font-awesome.css", new CssRewriteUrlTransform()));

            bundles.Add(new ScriptBundle("~/bundles/adminJs").Include(
                          //PE-WD-23-001  update jquery version
                          "~/admincss/assets/plugins/jquery/jquery-3.6.2.min.js",
                          "~/admincss/assets/plugins/bootstrapv3/js/bootstrap.min.js",
                          "~/admincss/assets/plugins/jquery-block-ui/jqueryblockui.min.js",
                          "~/admincss/assets/plugins/jquery-unveil/jquery.unveil.min.js",
                          "~/admincss/assets/plugins/jquery-scrollbar/jquery.scrollbar.min.js",
                          "~/admincss/assets/plugins/jquery-numberAnimate/jquery.animateNumbers.js",
                          "~/admincss/assets/plugins/jquery-validation/js/jquery.validate.min.js",
                          "~/admincss/assets/plugins/bootstrap-select2/select2.min.js",
                          "~/Scripts/jquery-ui.js",
                          "~/admincss/webarch/js/webarch.js",
                          "~/admincss/assets/plugins/bootstrap-datepicker/js/bootstrap-datepicker.js",
                          "~/Scripts/antiXssChecking.js",
                          "~/Scripts/check_browser_close.js",
                          "~/Scripts/load_spinner.js",
                          "~/Scripts/SessionTimeoutHandler.js",
                          "~/js/bootstrap3-dialog.min.js",
                          "~/Scripts/ClientValidation.js",
                          "~/Scripts/ImageLoader.js"));
            #endregion

            

            bundles.Add(new ScriptBundle("~/DataTables/js").Include(
                "~/DataTables-1.10.3/media/js/jquery.dataTables.min.js",
                "~/DataTables-1.10.3/extensions/FixedColumns/js/dataTables.fixedColumns.min.js",
                "~/DataTables-1.10.3/extensions/RowReorder/js/jquery.dataTables.rowReordering.js",
                "~/DataTables-1.10.3/plugins/integration/jqueryui/dataTables.jqueryui.js"));

            bundles.Add(new ScriptBundle("~/DataTables/adminjs").Include(
                "~/admincss/assets/plugins/bootstrap-select2/select2.min.js",
                "~/admincss/assets/plugins/jquery-datatable/extra/js/dataTables.tableTools.min.js",
                "~/admincss/assets/plugins/datatables-responsive/js/datatables.responsive.js",
                "~/admincss/assets/plugins/datatables-responsive/js/lodash.min.js",
                "~/admincss/assets/js/datatables.js",
                "~/admincss/assets/plugins/bootstrap-multiselect/bootstrap-multiselect.js"));



            bundles.Add(new StyleBundle("~/DataTables/css").Include(
                "~/DataTables-1.10.3/plugins/integration/jqueryui/dataTables.jqueryui.css",
                "~/DataTables-1.10.3/extensions/FixedColumns/css/dataTables.fixedColumns.min.css",
                "~/DataTables-1.10.3/dataTables.custom.css"));

            bundles.Add(new StyleBundle("~/Form/css").Include(
                "~/Content/formDesign.css"));

            bundles.Add(new StyleBundle("~/Form/js").Include(
               "~/Scripts/functionDialog.js"));
            
            #region Dashboard Tab Style
            bundles.Add(new StyleBundle("~/Tab/js").Include(
                "~/Scripts/fnLoadTab.js"));

            bundles.Add(new StyleBundle("~/Tab/css").Include(
              "~/css/tabStyle.css"));
            #endregion

            bundles.Add(new ScriptBundle("~/DataTableGrid/js").Include(
                "~/Scripts/ie_expand_select_width.js",
                "~/Scripts/fnFilterClear.js",
             "~/Scripts/datatablegrid.js"));

            #region Cheque Image Viewer
            bundles.Add(new StyleBundle("~/CViewer/css").Include(
              "~/js/ImgViewer/jquery.iviewer.css",
              "~/css/iViewerStyle.css"));

            bundles.Add(new ScriptBundle("~/CViewer/js").Include(
                //ADD START JBCHONG 20220222 PE-WD-22-001
                "~/js/ImgViewer/tiff.js",
                //ADD E N D JBCHONG 20220222 PE-WD-22-001
             "~/js/ImgViewer/jquery.mousewheel.min.js",
          "~/js/ImgViewer/jquery.iviewer-v2.js"));

            #endregion

           //false - minification off, css wont work if it's true set to false only when debugging
            BundleTable.EnableOptimizations = true;// true;
        }
    }
}