using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UBPC.Web.Common
{
    public class ImageConverter
    {
        private string _imageFileName;
        public string ImgFileName
        {
            get { return _imageFileName; }
            set { _imageFileName = value; }
        }

        private long _imageOffsetValue;
        public long ImgOffsetValue
        {
            get { return _imageOffsetValue; }
            set { _imageOffsetValue = value; }
        }

        private long _imageSize;
        public long ImgSize
        {
            get { return _imageSize; }
            set { _imageSize = value; }
        }

        private string _imageOutputPath;
        public string ImgOutputPath
        {
            get { return _imageOutputPath; }
            set { _imageOutputPath = value; }
        }

        private DateTime _busdate;
        public DateTime BusinessDate
        {
            get { return _busdate; }
            set { _busdate = value; }
        }

        public ImageConverter(){}

        public Byte[] RetrieveImageByteByOffsetValue(string fileName, Int64 offsetValue, Int32 imgSize)
        {
            byte[] BitmapData = null;
            FileStream imagefile = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            imagefile.Seek(offsetValue - 1, SeekOrigin.Begin);
            BinaryReader br = new BinaryReader(imagefile);
            BitmapData = br.ReadBytes(Convert.ToInt32(imgSize) - 1);

            return BitmapData;
        }
        public Byte[] RetrieveImageByteByOffsetValueExcludeSize(string fileName, Int64 offsetValue)
        {
            byte[] BitmapData = null;
            using (FileStream imagefile = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                imagefile.Seek(offsetValue - 1, SeekOrigin.Begin);
                using (BinaryReader br = new BinaryReader(imagefile))
                {
                    // Calculate the number of remaining bytes to read
                    long remainingBytes = imagefile.Length - (offsetValue - 1);
                    BitmapData = br.ReadBytes((int)remainingBytes);
                }
            }
            return BitmapData;
        }

        public Byte[] RetrieveJPEGImageByteByOffset(BinaryWriter writer)
        {
            JPEGImage jpgImage = new JPEGImage();
            jpgImage.JpgFileName = _imageFileName;
            jpgImage.JpgOffset = Convert.ToInt64(_imageOffsetValue);
            jpgImage.JpgSize = Convert.ToInt32(_imageSize);
            jpgImage.ImageName = Path.Combine(GetTempDirectory(), _imageOutputPath + "F.IMG");
            return jpgImage.CreateJpeg();

        }

        private String GetTempDirectory()
        {
            String tempDirectory = Path.Combine(Resources.TempDirectory.Trim(), _busdate.ToString("yyyyMMdd"));
            if (Directory.Exists(tempDirectory) == false)
            {//temporary directory doesnot exist, create directory
                Directory.CreateDirectory(tempDirectory);
            }
            return tempDirectory;
        }
    }
}
