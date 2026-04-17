using DocumentFormat.OpenXml.Office2010.CustomUI;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Web.UI.WebControls;

namespace UBPC.Web.Common
{
    public class ChequeViewerCtrl
    {

        public string DBConnString { get; set; }
        public ChequeViewerCtrl() { }
        public ChequeViewerCtrl(string connectionString) { DBConnString = connectionString; }

        public Image _frontImage;
        public Image _frontJpegImage;
        private Image _rearImage;
        private Image _rearJpegImage;

        public byte[] LoadImage(bool isFront, bool isJPEG, string IFSPath, string imgfileName, long frontOffset,
            long rearOffset, int frontLength,
            int rearlength, DateTime busDate, string batchNo, int transSeqNo, string tempDir)
        {
            byte[] BitmapData;
            JPEGImage jpgImage = new JPEGImage();
            string _TempDirectoryWithBusDate = GetTempDirectory(tempDir, busDate);

            if (frontOffset == 0)
            {
                return null;
            }

            if (isFront)
            {
                if (isJPEG)
                {
                    String FI2fileName = Path.Combine(IFSPath.Trim(), imgfileName.Trim() + ".FI2");

                    //JPEG Img here
                    jpgImage.JpgFileName = FI2fileName;
                    jpgImage.JpgOffset = Convert.ToInt64(frontOffset);
                    jpgImage.JpgSize = Convert.ToInt32(frontLength);
                    jpgImage.ImageName = Path.Combine(_TempDirectoryWithBusDate, batchNo + transSeqNo.ToString() + "F.IMG");
                    BitmapData = jpgImage.CreateJpeg();
                }
                else
                {
                    String FIMfileName = Path.Combine(IFSPath.Trim(), imgfileName.Trim() + ".FIM");

                    //TIFF Img here
                    System.IO.FileStream imagefile = new System.IO.FileStream(FIMfileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    imagefile.Seek(frontOffset - 1, System.IO.SeekOrigin.Begin);
                    //CHG START JBCHONG 20220222 PE-WD-22-001
                    //System.IO.BinaryReader br = new System.IO.BinaryReader(imagefile);
                    //BitmapData = br.ReadBytes(Convert.ToInt32(frontLength) - 1);
                    byte[] bytes = System.IO.File.ReadAllBytes(FIMfileName);
                    imagefile.Read(bytes, 0, Convert.ToInt32(frontLength) - 1);
                    BitmapData = bytes;
                    //CHG E N D JBCHONG 20220222 PE-WD-22-001
                }
            }
            else
            {
                if (isJPEG)
                {
                    //Rear JPEG Img here
                    String RI2fileName = Path.Combine(IFSPath.Trim(), imgfileName.Trim() + ".RI2");
                    //JPEG Img here
                    jpgImage.JpgFileName = RI2fileName;
                    jpgImage.JpgOffset = Convert.ToInt64(rearOffset);
                    jpgImage.JpgSize = Convert.ToInt32(rearlength);
                    jpgImage.ImageName = Path.Combine(_TempDirectoryWithBusDate, batchNo + transSeqNo.ToString() + "R.IMG");
                    BitmapData = jpgImage.CreateJpeg();

                }
                else
                {
                    String RIMfileName = Path.Combine(IFSPath.Trim(), imgfileName.Trim() + ".RIM");

                    //Rear TIFF Img here
                    System.IO.FileStream imagefile = new System.IO.FileStream(RIMfileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    // imagefile.Seek(_frontOffset - 1, System.IO.SeekOrigin.Begin);
                    imagefile.Seek(rearOffset - 1, System.IO.SeekOrigin.Begin);
                    //CHG START JBCHONG 20220222 PE-WD-22-001
                    //System.IO.BinaryReader br = new System.IO.BinaryReader(imagefile);
                    //BitmapData = br.ReadBytes(Convert.ToInt32(rearlength) - 1);
                    byte[] bytes = System.IO.File.ReadAllBytes(RIMfileName);
                    imagefile.Read(bytes, 0, Convert.ToInt32(rearlength) - 1);
                    BitmapData = bytes;
                    //CHG E N D JBCHONG 20220222 PE-WD-22-001
                }
            }

            return BitmapData;
        }

        #region Internal Function

        private String GetTempDirectory(string tempDir, DateTime busDate)
        {
            String tempDirectory = Path.Combine(tempDir, busDate.ToString("yyyyMMdd"));
            if (Directory.Exists(tempDirectory) == false)
            {//temporary directory doesnot exist, create directory
                Directory.CreateDirectory(tempDirectory);
            }
            return tempDirectory;
        }

        public void DeleteTempDirectory(string delPath)
        {
            Directory.Delete(delPath, true);

        }
        #endregion

        //Show Inward Image
        public byte[] displayImage(string IFSPath, bool isFront, bool isJPEG, byte[] frontImg, byte[] rearImg, byte[] frontJpegImg)
        {
            byte[] bitmapdata;
            //creating front image
            String frontTIFFfileName = string.Empty; //Path.Combine(IFSPath, frontImgPath);
            String rearTIFFfileName = string.Empty;// Path.Combine(IFSPath, rearImgPath);
            String jpegFrontfileName = string.Empty;
            if ((frontImg == null || frontImg.Length == 0) && (rearImg == null || rearImg.Length == 0))
            {
                return null;
            }


            if (isFront)
            {
                if (isJPEG)
                {
                    //jpegFrontfileName = System.IO.Path.Combine(IFSPath, frontJpegImg);
                    //_frontJpegImage = System.Drawing.Image.FromFile(jpegFrontfileName);
                    byte[] bytes = frontJpegImg;//System.IO.File.ReadAllBytes(jpegFrontfileName);
                    bitmapdata = bytes;

                }
                else
                {
                    //frontTIFFfileName = System.IO.Path.Combine(IFSPath, frontImgPath);
                    //_frontImage = System.Drawing.Image.FromFile(frontTIFFfileName); 
                    byte[] bytes = frontImg;//System.IO.File.ReadAllBytes(frontTIFFfileName);
                    bitmapdata = bytes;

                }
            }
            else
            {
                if (isJPEG)
                {
                    //rearTIFFfileName = System.IO.Path.Combine(IFSPath, rearImgPath);
                    //_frontJpegImage = System.Drawing.Image.FromFile(jpegFrontfileName);
                    byte[] bytes = rearImg;//System.IO.File.ReadAllBytes(rearTIFFfileName);
                    bitmapdata = bytes;
                }
                else
                {
                    //rearTIFFfileName = System.IO.Path.Combine(IFSPath, rearImgPath);
                    //_rearImage = System.Drawing.Image.FromFile(rearTIFFfileName);
                    byte[] bytes = rearImg;//System.IO.File.ReadAllBytes(rearTIFFfileName);
                    bitmapdata = bytes;

                }
            }

            return bitmapdata;

        }
    }

    public class ChequeViewerCtrlViewModel
    {
        public Boolean IsEditMode { get; set; }
        public string imgType { get; set; }

        public string isFrontImage { get; set; }
        public string isJPEGImage { get; set; }
        public string ImgFileName { get; set; }
        public long frontOffset { get; set; }
        public long rearOffset { get; set; }
        public Int32 frontLength { get; set; }
        public Int32 rearLength { get; set; }
        public DateTime BusinessDate { get; set; }
        public string BatchNo { get; set; }
        public long DinNo { get; set; }
        public int transSeqNo { get; set; }
    }
}
