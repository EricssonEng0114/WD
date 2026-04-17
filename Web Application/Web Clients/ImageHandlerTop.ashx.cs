using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using UBPC.Web.Common;

namespace UBPCWeb
{
    /// <summary>
    /// Summary description for ImageHandler
    /// </summary>
    public class ImageHandler : IHttpHandler,IRequiresSessionState 
    {
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
                DateTime curBusdate = DateTime.Now;
                string curBatchNo = string.Empty;
                string strVirtualDir = string.Empty;
                string strImgFileName = string.Empty;
                bool isFrontImg = false;
                bool isJpeg = false;
                Int64 intFrontOffset = 0;
                Int32 intFrontSize = 0;
                Int64 intRearOffset = 0;
                Int32 intRearSize = 0;
                Int32 intTranseqNo = 0;

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
                    intFrontOffset = Convert.ToInt64(imgArrInfo[4].ToString().Trim());
                    intFrontSize = Convert.ToInt32(imgArrInfo[5].ToString().Trim());
                    intRearOffset = Convert.ToInt64(imgArrInfo[6].ToString().Trim());
                    intRearSize = Convert.ToInt32(imgArrInfo[7].ToString().Trim());
                    intTranseqNo = Convert.ToInt32(imgArrInfo[8].ToString().Trim());
                }


                //write your handler implementation here.
                //load from fim rim
                byte[] imgByte;
                string rlpsImgPath = HttpContext.Current.Server.MapPath(strVirtualDir);//virtual directories

                string initTempPath = @"C:\Temp";
 
                ChequeViewerCtrl chqImgObj = new ChequeViewerCtrl();
                imgByte =  chqImgObj.LoadImage(isFrontImg, isJpeg, rlpsImgPath.Trim(), strImgFileName, intFrontOffset,
                     intRearOffset, intFrontSize, intRearSize, curBusdate, curBatchNo, intTranseqNo,
                     initTempPath);

                //Remove temp path after done converting jpeg to byte
                if (imgByte != null && imgByte.Length > 0)
                {
                    if (isJpeg)
                    {
                        chqImgObj.DeleteTempDirectory(initTempPath);
                    }                   
                }

                //response.AddHeader("content-disposition", "inline;filename=myimage.jpeg");
                context.Response.AppendHeader("Content-Length", imgByte.Length.ToString());
                //context.Response.ContentType = "image/tiff";
                if (isJpeg)
                {
                    context.Response.ContentType = "image/jpeg";
                }
                else
                {
                    context.Response.ContentType = "image/tiff";
                }
                context.Response.OutputStream.Write(imgByte, 0, imgByte.Length);
            }
            catch (Exception ex) 
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.RejectedItemDecision;
                log.Severity = LogEventType.Error;
                log.Message = "TopImageHandler:" + ex.Message;
                log.Exception = ex;
                log.Write();
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