using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using UBPC.Web.Common;

namespace UBPCWeb
{
    /// <summary>
    /// Summary description for ImageHandler6
    /// </summary>
    public class ImageHandler6 : IHttpHandler, IRequiresSessionState
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


                bool isCurSelectedProcModeC = HttpContext.Current.Session["s_CurSelectedProcModeImgArc"].ToString() == "C";
                bool isCurSelectedRepresentedTrue = HttpContext.Current.Session["s_CurSelectedRepresentedImgArc"].ToString() == "True";
                bool isCurSelectedItemTypeS = HttpContext.Current.Session["s_CurSelectedItemTypeImgArc"].ToString() == "S";

                curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"].ToString());
                curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString();

                string curSelectedImg = HttpContext.Current.Session["CurSelectedBottomImage"].ToString();
                if (!string.IsNullOrEmpty(curSelectedImg))
                {
                    if ((isCurSelectedProcModeC || isCurSelectedRepresentedTrue))
                    {
                        if (!isCurSelectedItemTypeS)
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
                        else
                        {
                            isJpeg = true;
                        }
                    }
                    else
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
                }


                //write your handler implementation here.
                //load from fim rim
                byte[] imgByte;
                string rlpsImgPath = HttpContext.Current.Server.MapPath(strVirtualDir);//virtual directories

                string initTempPath = @"C:\Temp";
                ChequeViewerCtrl chqImgObj = new ChequeViewerCtrl();

                if ((isCurSelectedProcModeC || isCurSelectedRepresentedTrue))//if is cheque only mode OR is Represented
                {
                    if (!isCurSelectedItemTypeS)//this is check for when it is not cheque only mode and not stub 
                    {
                        imgByte = chqImgObj.LoadImage(isFrontImg, isJpeg, rlpsImgPath.Trim(), strImgFileName.Trim(), intFrontOffset,
                         intRearOffset, intFrontSize, intRearSize, curBusdate, curBatchNo, intTranseqNo,
                         initTempPath);
                    }
                    else//if is cheque only mode and stub come together then show virtual stub
                    {
                        imgByte = chqImgObj.displayImage(@"C:\\Temp\\" + curBusdate, isFrontImg, isJpeg,
                            (byte[])HttpContext.Current.Session["ImageArchiveOutwardVirtualStubImage"], (byte[])HttpContext.Current.Session["ImageArchiveOutwardVirtualStubImage"],
                            (byte[])HttpContext.Current.Session["ImageArchiveOutwardVirtualStubImage"]);
                    }
                }
                else
                {
                    imgByte = chqImgObj.LoadImage(isFrontImg, isJpeg, rlpsImgPath.Trim(), strImgFileName.Trim(), intFrontOffset,
                     intRearOffset, intFrontSize, intRearSize, curBusdate, curBatchNo, intTranseqNo,
                     initTempPath);
                }

                //Remove temp path after done converting jpeg to byte
                if (imgByte != null && imgByte.Length > 0)
                {
                    if (isJpeg && File.Exists(initTempPath))
                    {
                        chqImgObj.DeleteTempDirectory(initTempPath);
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
                else
                {
                    if (context.Request.Headers["Accept"] != null && context.Request.Headers["Accept"] == "*/*")
                    {
                        LogEntry log = new LogEntry();
                        log.Caller = LogCallerID.ImageArchiveOutward;
                        log.Message = "RightImageHandler: No Image is Found";
                        log.Severity = LogEventType.Information;
                        log.Write();
                    }
                }
            }
            catch (Exception ex)
            {
                if (context.Request.Headers["Accept"] != null && context.Request.Headers["Accept"] == "*/*")
                {
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.ImageArchiveOutward;
                    log.Severity = LogEventType.Error;
                    log.Message = "RightImageHandler:" + ex.Message;
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

        public void showNoImage()
        {
            string virtualDirectory = "/Content/images/";
            string virtualStubImagePath = HttpContext.Current.Server.MapPath(virtualDirectory);
            string fullPath = virtualStubImagePath + "NoImage.jpg";
            using (FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                // Create a memory stream to hold the image data
                using (System.Drawing.Image image = System.Drawing.Image.FromStream(fileStream, true, true))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        // Save the image to the memory stream in JPEG format
                        image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                        // Return the byte array from the memory stream
                        HttpContext.Current.Session["ImageArchiveOutwardNoImage"] = memoryStream.ToArray();
                    }
                }
            }
        }
    }
}