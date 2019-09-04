using FingerPrintModule.DAO;
using SecuGen.FDxSDKPro.Windows;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FingerPrintModule.Facade
{
    public class FingerPrintFacade
    {
        private SGFingerPrintManager m_FPM;
        private SGFPMSecurityLevel m_SecurityLevel;
        private Int32 m_ImageWidth;
        private Int32 m_ImageHeight;
        private Int32 m_Dpi;
        private int serialNo;

        private bool m_DeviceOpened;
        Int32 max_template_size = 0;

        public void MergeTemplate()
        {
             
        }

        public FingerPrintInfo Capture(int fingerPosition, out string err, bool populateImagebytes = false)
        {
            InitializeDevice();
            err = "";

            var fp_image = new Byte[m_ImageWidth * m_ImageHeight];
            var m_fingerprinttemplate = new Byte[max_template_size];

            var error = (int)SGFPMError.ERROR_NONE;
            var img_qlty = 0;

            if (m_DeviceOpened)
            {
                error = m_FPM.GetImage(fp_image);
            }

            if (error == (int)SGFPMError.ERROR_NONE)
            {
                m_FPM.GetImageQuality(m_ImageWidth, m_ImageHeight, fp_image, ref img_qlty);


                var finger_info = new SGFPMFingerInfo
                {
                    FingerNumber = (SGFPMFingerPosition)fingerPosition,
                    ImageQuality = (short)img_qlty,
                    ImpressionType = (short)SGFPMImpressionType.IMPTYPE_LP,
                    ViewNumber = 1
                };

                // CreateTemplate
                error = m_FPM.CreateTemplate(finger_info, fp_image, m_fingerprinttemplate);

                if (error == (int)SGFPMError.ERROR_NONE)
                {
                    return new FingerPrintInfo
                    {
                        Manufacturer = "",
                        Model = "",
                        SerialNumber = "",
                        ImageWidth = m_ImageWidth,
                        ImageHeight = m_ImageHeight,
                        ImageDPI = m_Dpi,
                        ImageQuality = img_qlty,
                        Image = ToBase64String(fp_image, ImageFormat.Bmp),
                        ImageByte = populateImagebytes ? fp_image : null,
                        Template = Convert.ToBase64String(m_fingerprinttemplate),
                        FingerPositions = (FingerPositions)fingerPosition
                    };
                }
            }
            err = DisplayError(error);
            return null; 
        }

        public string Verify(FingerPrintMatchInputModel input)
        {
            InitializeDevice();

            var matchedRecord = "";
            var err = 0;
            var fingerprint = Convert.FromBase64String(input.FingerPrintTemplate);

            foreach (var data in input.FingerPrintTemplateListToMatch)
            {
                var sample_info = new SGFPMISOTemplateInfo();

                var byteTemplate = Convert.FromBase64String(data.Template);
                err = m_FPM.GetIsoTemplateInfo(byteTemplate, sample_info);

                for (int i = 0; i < sample_info.TotalSamples; i++)
                {
                    var matched = false;
                    err = m_FPM.MatchIsoTemplate(byteTemplate, i, fingerprint, 0, m_SecurityLevel, ref matched);
                    if (matched)
                    {
                        matchedRecord = data.staffId;
                        break;
                    }
                }
            }
            return matchedRecord;
        }

        private void InitializeDevice()
        {
            m_FPM = new SGFingerPrintManager();

            Int32 error;
            var device_name = SGFPMDeviceName.DEV_AUTO;
            var device_id = (Int32)SGFPMPortAddr.USB_AUTO_DETECT;
            m_SecurityLevel = SGFPMSecurityLevel.NORMAL;

            m_DeviceOpened = false;


            error = m_FPM.Init(device_name);
            if (error != (Int32)SGFPMError.ERROR_NONE)
            {
                error = m_FPM.InitEx(m_ImageWidth, m_ImageHeight, m_Dpi);
            }

            if (error == (Int32)SGFPMError.ERROR_NONE)
            {
                m_FPM.CloseDevice();
                error = m_FPM.OpenDevice(device_id);
            }
            else
            {
                throw new ApplicationException("Unable to initialize scanner." + DisplayError(error));
            }

            if (error == (Int32)SGFPMError.ERROR_NONE)
            {
                var pInfo = new SGFPMDeviceInfoParam();
                m_FPM.GetDeviceInfo(pInfo);
                m_ImageWidth = pInfo.ImageWidth;
                m_ImageHeight = pInfo.ImageHeight;
                m_Dpi = pInfo.ImageDPI;
                serialNo = pInfo.DeviceID;
            }

            error = m_FPM.SetTemplateFormat(SGFPMTemplateFormat.ISO19794);
            error = m_FPM.GetMaxTemplateSize(ref max_template_size);

            if (device_name != SGFPMDeviceName.DEV_UNKNOWN)
            {
                error = m_FPM.OpenDevice(device_id);
                if (error == (Int32)SGFPMError.ERROR_NONE)
                {
                    m_DeviceOpened = true;
                }
            }
        }


        public string ToBase64String(Byte[] imgData, ImageFormat imageFormat)
        {
            int colorval;
            var bmp = new Bitmap(m_ImageWidth, m_ImageHeight);

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    colorval = (int)imgData[(j * m_ImageWidth) + i];
                    bmp.SetPixel(i, j, Color.FromArgb(colorval, colorval, colorval));
                }
            }

            var base64String = string.Empty;

            var memoryStream = new MemoryStream();
            bmp.Save(memoryStream, imageFormat);

            memoryStream.Position = 0;
            var byteBuffer = memoryStream.ToArray();

            memoryStream.Close();

            base64String = Convert.ToBase64String(byteBuffer);
            byteBuffer = null;

            return base64String;
        }

        public byte[] Base64StringByteArray(string base64String)
        {
            var byteBuffer = Convert.FromBase64String(base64String);
            return byteBuffer;

        }

        string DisplayError(int iError)
        {
            var text = "";

            switch (iError)
            {
                case 0:                             //SGFDX_ERROR_NONE				= 0,
                    {
                        text = "Error none";
                        break;
                    }
                case 1:                             //SGFDX_ERROR_CREATION_FAILED	= 1,
                    {
                        text = "Can not create object";
                        break;
                    }
                case 2:                             //   SGFDX_ERROR_FUNCTION_FAILED	= 2,
                    {
                        text = "Function Failed";
                        break;
                    }
                case 3:                             //   SGFDX_ERROR_INVALID_PARAM	= 3,
                    {
                        text = "Invalid Parameter";
                        break;
                    }
                case 4:                          //   SGFDX_ERROR_NOT_USED			= 4,
                    {
                        text = "Not used function";
                        break;
                    }
                case 5:                                //SGFDX_ERROR_DLLLOAD_FAILED	= 5,
                    {
                        text = "Can not create object";
                        break;
                    }
                case 6:                                //SGFDX_ERROR_DLLLOAD_FAILED_DRV	= 6,
                    {
                        text = "Can not load device driver";
                        break;
                    }
                case 7:                                //SGFDX_ERROR_DLLLOAD_FAILED_ALGO = 7,
                    {
                        text = "Can not load sgfpamx.dll";
                        break;
                    }
                case 51:                //SGFDX_ERROR_SYSLOAD_FAILED	   = 51,	// system file load fail
                    {
                        text = "Can not load driver kernel file";
                        break;
                    }
                case 52:                //SGFDX_ERROR_INITIALIZE_FAILED  = 52,   // chip initialize fail
                    {
                        text = "Failed to initialize the device";
                        break;
                    }
                case 53:                //SGFDX_ERROR_LINE_DROPPED		   = 53,   // image data drop
                    {
                        text = "Data transmission is not good";
                        break;
                    }
                case 54:                //SGFDX_ERROR_TIME_OUT			   = 54,   // getliveimage timeout error
                    {
                        text = "Time out";
                        break;
                    }
                case 55:                //SGFDX_ERROR_DEVICE_NOT_FOUND	= 55,   // device not found
                    {
                        text = "Device not found";
                        break;
                    }
                case 56:                //SGFDX_ERROR_DRVLOAD_FAILED	   = 56,   // dll file load fail
                    {
                        text = "Can not load driver file";
                        break;
                    }
                case 57:                //SGFDX_ERROR_WRONG_IMAGE		   = 57,   // wrong image
                    {
                        text = "Wrong Image";
                        break;
                    }
                case 58:                //SGFDX_ERROR_LACK_OF_BANDWIDTH  = 58,   // USB Bandwith Lack Error
                    {
                        text = "Lack of USB Bandwith";
                        break;
                    }
                case 59:                //SGFDX_ERROR_DEV_ALREADY_OPEN	= 59,   // Device Exclusive access Error
                    {
                        text = "Device is already opened";
                        break;
                    }
                case 60:                //SGFDX_ERROR_GETSN_FAILED		   = 60,   // Fail to get Device Serial Number
                    {
                        text = "Device serial number error";
                        break;
                    }
                case 61:                //SGFDX_ERROR_UNSUPPORTED_DEV		   = 61,   // Unsupported device
                    {
                        text = "Unsupported device";
                        break;
                    }
                // Extract & Verification error
                case 101:                //SGFDX_ERROR_FEAT_NUMBER		= 101, // utoo small number of minutiae
                    {
                        text = "The number of minutiae is too small";
                        break;
                    }
                case 102:                //SGFDX_ERROR_INVALID_TEMPLATE_TYPE		= 102, // wrong template type
                    {
                        text = "Template is invalid";
                        break;
                    }
                case 103:                //SGFDX_ERROR_INVALID_TEMPLATE1		= 103, // wrong template type
                    {
                        text = "1st template is invalid";
                        break;
                    }
                case 104:                //SGFDX_ERROR_INVALID_TEMPLATE2		= 104, // vwrong template type
                    {
                        text = "2nd template is invalid";
                        break;
                    }
                case 105:                //SGFDX_ERROR_EXTRACT_FAIL		= 105, // extraction fail
                    {
                        text = "Minutiae extraction failed";
                        break;
                    }
                case 106:                //SGFDX_ERROR_MATCH_FAIL		= 106, // matching  fail
                    {
                        text = "Matching failed";
                        break;
                    }

                default:
                    break;
            }

            text = " Error # " + iError + " :" + text;
            return text;
        }
    }
}
