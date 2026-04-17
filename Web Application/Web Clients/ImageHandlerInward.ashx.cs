using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI.WebControls;
using UBPC.Web.Common;

namespace UBPCWeb
{
    /// <summary>
    /// Summary description for ImageHandler3
    /// </summary>
    public class ImageHandler3 : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            bool FrontImgExist = (byte[])HttpContext.Current.Session["ImageArchiveInwardFrontImg"] != null;
            bool RearImgExist = (byte[])HttpContext.Current.Session["ImageArchiveInwardRearImg"] != null;
            bool JpegFrontImgExist = (byte[])HttpContext.Current.Session["ImageArchiveInwardFrontJPEG"] != null;

            try
            {
                //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
                DateTime curBusdate = DateTime.Now;
                string curBatchNo = string.Empty;
                string strVirtualDir = string.Empty;
                string strImgFileName = string.Empty;
                bool isFrontImg = false;
                bool isJpeg = false;

                curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"].ToString());
                curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString();

                string curSelectedImg = HttpContext.Current.Session["CurSelectedTopImage"].ToString();
                if (!string.IsNullOrEmpty(curSelectedImg))
                {
                    String[] imgArrInfo = curSelectedImg.Split('|');
                    strVirtualDir = imgArrInfo[0].ToString();
                    strImgFileName = imgArrInfo[1].ToString();
                    isFrontImg = Convert.ToBoolean(imgArrInfo[2].ToString().Equals("1"));
                    isJpeg = Convert.ToBoolean(imgArrInfo[3].ToString().Equals("1"));
                }


                //write your handler implementation here.
                //load from fim rim
                byte[] imgByte;
                //string rlpsImgPath = HttpContext.Current.Server.MapPath(strVirtualDir);//virtual directories

                string initTempPath = @"C:\Temp";

                ChequeViewerCtrl chqImgObj = new ChequeViewerCtrl();


                imgByte = chqImgObj.displayImage(@"C:\\Temp\\" + curBusdate, isFrontImg, isJpeg,
                    (byte[])HttpContext.Current.Session["ImageArchiveInwardFrontImg"], (byte[])HttpContext.Current.Session["ImageArchiveInwardRearImg"],
                    (byte[])HttpContext.Current.Session["ImageArchiveInwardFrontJPEG"]);

                //Remove temp path after done converting jpeg to byte
                if (imgByte != null && imgByte.Length > 0)
                {
                    if (isJpeg && Directory.Exists(initTempPath))
                    {
                        chqImgObj.DeleteTempDirectory(initTempPath);
                    }


                    //response.AddHeader("content-disposition", "inline;filename=myimage.jpeg");
                    context.Response.AppendHeader("Content-Length", imgByte.Length.ToString());
                    //context.Response.ContentType = "image/tiff";
                    if (isJpeg && isFrontImg)
                    {
                        context.Response.ContentType = "image/jpeg";
                    }
                    else
                    {
                        context.Response.ContentType = "image/tiff";
                    }
                    context.Response.OutputStream.Write(imgByte, 0, imgByte.Length);
                }
                else
                {
                    if (context.Request.Headers["Accept"] != null && context.Request.Headers["Accept"] == "*/*"
                        && (!FrontImgExist && !RearImgExist && !JpegFrontImgExist))
                    {
                        string caller = HttpContext.Current.Session["Logcaller_Inward_Outward"].ToString();
                        LogEntry log = new LogEntry();
                        if (caller == "ImageArchiveOutward")
                        {
                            log.Caller = LogCallerID.ImageArchiveOutward;
                            log.Message = "LeftImageHandler: No Image is Found";
                        }
                        else if (caller == "ImageArchiveInward")
                        {
                            log.Caller = LogCallerID.ImageArchiveInward;
                            log.Message = "LeftImageHandler: No Image is Found";
                        }
                        log.Severity = LogEventType.Information;
                        log.Write();
                    }
                }
            }
            catch (Exception ex)
            {
                if (context.Request.Headers["Accept"] != null && context.Request.Headers["Accept"] == "*/*"
                    && (!FrontImgExist && !RearImgExist && !JpegFrontImgExist))
                {
                    string caller = HttpContext.Current.Session["Logcaller_Inward_Outward"].ToString();
                    LogEntry log = new LogEntry();
                    if (caller == "ImageArchiveOutward")
                    {
                        log.Caller = LogCallerID.ImageArchiveOutward;
                    }
                    else if (caller == "ImageArchiveInward")
                    {
                        log.Caller = LogCallerID.ImageArchiveInward;
                    }
                    log.Severity = LogEventType.Error;
                    log.Message = "LeftImageHandler:" + ex.Message;
                    log.Exception = ex;
                    log.Write();
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}