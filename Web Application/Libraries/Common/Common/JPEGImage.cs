using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace UBPC.Web.Common
{
    public class JPEGImage
    {

        #region F2 Image File Structure

        public struct udt_TIFF_Header
        {
            public Int16 usByteOrder;
            public Int16 usVersion;
            public Int32 ulIFDOffset;
        }

        //' Structure of Tags in TIFF
        //' 1) Tag
        //' 2) Type
        //' 3) Count
        //' 4) Value/Offset
        public struct udt_TIFF_Tag
        {
            public Int16 usTagIdentifier;
            public Int16 usTagType;
            public Int32 ulTagTypeCount;
            public Int32 ulTagTypeOffset;
        }

        public struct udt_BNM_JPEG_Tiff
        {
            public Int32 lngOriginalRawStart;
            public Int16 intIndexOfStripOffset;
            public Int16 intTagCount;
            public udt_TIFF_Tag[] aryTags;
            public Int32 lngNextIFD;
            public String DocName;
            public Byte[] arrRawImage;
        }

        #endregion

        String _jpgFileName = String.Empty;
        long _jpgOffset = 0;
        int _jpgSize = 0;

        udt_TIFF_Header _tiffHeader;

        String _imgName = String.Empty;

        #region Public Properties Implementation

        public String ImageName
        {
            set { _imgName = value; }
        }

        public String JpgFileName
        {
            set { _jpgFileName = value; }
        }

        public long JpgOffset
        {
            set { _jpgOffset = value; }
        }

        public int JpgSize
        {
            set { _jpgSize = value; }
        }

        #endregion

        public Byte[] CreateJpeg()
        {
            Byte[] JpegImage;
            FileStream fsWriter = new FileStream(_imgName, FileMode.Create);
            BinaryWriter bWriter = new BinaryWriter(fsWriter);
            try
            {
                JpegImage = GetJPEGImageByteArray(bWriter);
            }
            finally
            {
                fsWriter.Close();
            }

            return JpegImage;

        }

        private byte[] ReadFromFileToByte(BinaryReader oBR, long offset, int lenght)
        {
            oBR.BaseStream.Position = offset;
            return oBR.ReadBytes((int)lenght);
        }

        private udt_TIFF_Header BytesTo_TIFF_HeaderStruct(byte[] bRaw)
        {
            GCHandle hRawID3 = GCHandle.Alloc(bRaw, GCHandleType.Pinned);
            udt_TIFF_Header newRecord = (udt_TIFF_Header)Marshal.PtrToStructure((hRawID3.AddrOfPinnedObject()), typeof(udt_TIFF_Header));
            hRawID3.Free();
            return newRecord;
        }

        private udt_TIFF_Tag BytesTo_TIFF_Tag(byte[] bRaw)
        {
            GCHandle hRawID3 = GCHandle.Alloc(bRaw, GCHandleType.Pinned);
            udt_TIFF_Tag newRecord = (udt_TIFF_Tag)Marshal.PtrToStructure((hRawID3.AddrOfPinnedObject()), typeof(udt_TIFF_Tag));
            hRawID3.Free();
            return newRecord;
        }

        private udt_BNM_JPEG_Tiff CreateJPEGImageStruct()
        {
            String[] aryTags = new String[19];
            String[] strValues;
            udt_BNM_JPEG_Tiff udtTiffImg = new udt_BNM_JPEG_Tiff();

            aryTags[0] = "254,4,1,0"; //      ' New Sub File
            aryTags[1] = "256,4,1,0"; //      ' Width
            aryTags[2] = "257,4,1,0"; //       ' Height
            aryTags[3] = "258,3,1,1"; //       ' Bits Per Sample
            aryTags[4] = "259,3,1,4"; //       ' Compression
            aryTags[5] = "262,3,1,1"; //       ' Photometric Interpretation
            aryTags[6] = "269,2,31,1"; //       ' DocName
            aryTags[7] = "273,4,1,0"; //       ' Strip Offsets
            aryTags[8] = "277,3,1,1"; //       ' Samples Per Pixel
            aryTags[9] = "278,4,1,0"; //       ' Rows Per Strip
            aryTags[10] = "279,4,1,0"; //       ' Strip Byte Count
            aryTags[11] = "284,3,1,1"; //       ' Planar Configuration
            aryTags[12] = "512,3,1,0"; //       ' JPEG Proc
            aryTags[13] = "513,4,1,0"; //       ' JPEG Interchange Format
            aryTags[14] = "514,4,1,0"; //       ' JPEG Interchange Format Length
            aryTags[15] = "515,3,1,0"; //       ' JPEG Restart Interval
            aryTags[16] = "519,4,1,0"; //       ' JPEG Q Tables
            aryTags[17] = "520,4,1,0"; //       ' JPEG DC Tables
            aryTags[18] = "521,4,1,0"; //      ' JPEG AC Tables

            udtTiffImg.intTagCount = (Int16)(aryTags.GetUpperBound(0) + 1);

            udtTiffImg.aryTags = new udt_TIFF_Tag[19];
            for (int iCnt = aryTags.GetLowerBound(0); iCnt < aryTags.GetUpperBound(0) + 1; iCnt++)
            {
                strValues = aryTags[iCnt].Split(',');
                udtTiffImg.aryTags[iCnt].usTagIdentifier = Convert.ToInt16(strValues[0]);
                udtTiffImg.aryTags[iCnt].usTagType = Convert.ToInt16(strValues[1]);
                udtTiffImg.aryTags[iCnt].ulTagTypeCount = Convert.ToInt32(strValues[2]);
                udtTiffImg.aryTags[iCnt].ulTagTypeOffset = Convert.ToInt32(strValues[3]);
            }

            udtTiffImg.lngNextIFD = 0;

            return udtTiffImg;
        }

        private Byte[] GetJPEGImageByteArray(BinaryWriter writer)
        {
            FileStream jpgImageFile = new FileStream(_jpgFileName, FileMode.Open, FileAccess.Read);
            long lngTiffStart = _jpgOffset - 1;
            if (lngTiffStart == -1)
                lngTiffStart = 0;
            BinaryReader jpgReader = new BinaryReader(jpgImageFile);

            //read header infor
            byte[] bFileData = ReadFromFileToByte(jpgReader, lngTiffStart, Marshal.SizeOf(typeof(udt_TIFF_Header)));
            udt_TIFF_Header header = BytesTo_TIFF_HeaderStruct(bFileData);
            _tiffHeader = header;

            long iCurrentOffset = lngTiffStart + header.ulIFDOffset;

            byte[] bTagCount = ReadFromFileToByte(jpgReader, iCurrentOffset, Marshal.SizeOf(typeof(Int16)));
            Int16 iTagCount = BitConverter.ToInt16(bTagCount, 0);

            iCurrentOffset += Marshal.SizeOf(typeof(Int16));
            udt_TIFF_Tag[] udtTags = new udt_TIFF_Tag[iTagCount];
            for (int i = 0; i < iTagCount; i++)
            {
                jpgReader.BaseStream.Position = iCurrentOffset;
                byte[] bTag = jpgReader.ReadBytes((int)Marshal.SizeOf(typeof(udt_TIFF_Tag)));
                udtTags[i] = BytesTo_TIFF_Tag(bTag);
                iCurrentOffset += (int)Marshal.SizeOf(typeof(udt_TIFF_Tag));
            }

            udt_BNM_JPEG_Tiff pudtFront2 = CreateJPEGImageStruct();
            Int32 lngRawSize = 0;
            Int32 lngRawStart = 0;
            for (int intCtr1 = udtTags.GetLowerBound(0); intCtr1 < udtTags.GetUpperBound(0) + 1; intCtr1++)
            {
                for (int intCtr2 = pudtFront2.aryTags.GetLowerBound(0); intCtr2 < pudtFront2.aryTags.GetUpperBound(0) + 1; intCtr2++)
                {
                    if (pudtFront2.aryTags[intCtr2].usTagIdentifier == udtTags[intCtr1].usTagIdentifier)
                    {
                        pudtFront2.aryTags[intCtr2].ulTagTypeCount = udtTags[intCtr1].ulTagTypeCount;
                        pudtFront2.aryTags[intCtr2].ulTagTypeOffset = udtTags[intCtr1].ulTagTypeOffset;

                        switch (pudtFront2.aryTags[intCtr2].usTagIdentifier.ToString("X"))
                        {
                            case "111":
                                lngRawStart = (int)pudtFront2.aryTags[intCtr2].ulTagTypeOffset;
                                pudtFront2.lngOriginalRawStart = lngRawStart;
                                pudtFront2.intIndexOfStripOffset = (Int16)intCtr2;
                                break;
                            case "117":
                                lngRawSize = (int)pudtFront2.aryTags[intCtr2].ulTagTypeOffset;
                                break;
                        }
                    }
                }
            }

            if (lngRawSize > 0 && lngRawStart > 0)
            {
                jpgImageFile.Seek(lngTiffStart + lngRawStart, SeekOrigin.Begin);
                pudtFront2.arrRawImage = jpgReader.ReadBytes(lngRawSize);
            }

            Byte[] jpegByte = null;

            if (pudtFront2.arrRawImage != null)
                jpegByte = pudtFront2.arrRawImage;

            jpgReader.Close();
            jpgImageFile.Close();

            return jpegByte;
        }
    }
}
