using FingerPrintModule.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FingerPrintModule.Facade
{
    public class FingerPrintFacade
    {
        public void MergeTemplate()
        {
             
        }

        public FingerPrintInfo Capture(int fingerPosition, out string err, bool populateImagebytes = false)
        {
            if (!TryCreateSecuGenContext(out var ctx, out err))
            {
                return new FingerPrintInfo
                {
                    ErrorMessage = err,
                    FingerPositions = (FingerPositions)fingerPosition
                };
            }

            try
            {
                if (!InitializeDevice(ctx, out var imageWidth, out var imageHeight, out var imageDpi, out var maxTemplateSize, out err))
                {
                    return new FingerPrintInfo { ErrorMessage = err, FingerPositions = (FingerPositions)fingerPosition };
                }

                var image = new byte[imageWidth * imageHeight];
                var template = new byte[maxTemplateSize];
                var imageQualityArgs = new object[] { imageWidth, imageHeight, image, 0 };

                var getImageErr = Convert.ToInt32(Invoke(ctx.Manager, "GetImage", image));
                if (getImageErr != 0)
                {
                    err = DisplayError(getImageErr);
                    return new FingerPrintInfo { ErrorMessage = err, FingerPositions = (FingerPositions)fingerPosition };
                }

                InvokeWithByRef(ctx.Manager, "GetImageQuality", imageQualityArgs);
                var imageQuality = Convert.ToInt32(imageQualityArgs[3]);

                var fingerInfo = Activator.CreateInstance(ctx.FingerInfoType);
                SetProperty(fingerInfo, "FingerNumber", Enum.ToObject(ctx.FingerPositionType, fingerPosition));
                SetProperty(fingerInfo, "ImageQuality", (short)imageQuality);
                SetProperty(fingerInfo, "ImpressionType", (short)ctx.ImpressionTypeLpValue);
                SetProperty(fingerInfo, "ViewNumber", 1);

                var createTemplateErr = Convert.ToInt32(Invoke(ctx.Manager, "CreateTemplate", fingerInfo, image, template));
                if (createTemplateErr != 0)
                {
                    err = DisplayError(createTemplateErr);
                    return new FingerPrintInfo { ErrorMessage = err, FingerPositions = (FingerPositions)fingerPosition };
                }

                err = "";
                return new FingerPrintInfo
                {
                    Manufacturer = "",
                    Model = "",
                    SerialNumber = "",
                    ImageWidth = imageWidth,
                    ImageHeight = imageHeight,
                    ImageDPI = imageDpi,
                    ImageQuality = imageQuality,
                    Image = Convert.ToBase64String(image),
                    ImageByte = populateImagebytes ? image : null,
                    Template = Convert.ToBase64String(template),
                    FingerPositions = (FingerPositions)fingerPosition
                };
            }
            catch (Exception ex)
            {
                err = "Scanner capture failed: " + ex.Message;
                return new FingerPrintInfo { ErrorMessage = err, FingerPositions = (FingerPositions)fingerPosition };
            }
        }

        public string Verify(FingerPrintMatchInputModel input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.FingerPrintTemplate))
            {
                return "";
            }

            if (TryCreateSecuGenContext(out var ctx, out _))
            {
                try
                {
                    if (InitializeDevice(ctx, out _, out _, out _, out _, out _))
                    {
                        var sdkResult = VerifyWithSecuGen(ctx, input);
                        if (!string.IsNullOrWhiteSpace(sdkResult))
                        {
                            return sdkResult;
                        }
                    }
                }
                catch
                {
                    // Fall through to deterministic non-SDK fallback.
                }
            }

            var matches = input.FingerPrintTemplateListToMatch ?? new List<FingerPrintInfo>();
            var matched = matches.FirstOrDefault(x => string.Equals(x.Template, input.FingerPrintTemplate, StringComparison.Ordinal));
            return matched?.staffId ?? "";
        }

        private string VerifyWithSecuGen(SecuGenContext ctx, FingerPrintMatchInputModel input)
        {
            var target = Convert.FromBase64String(input.FingerPrintTemplate);
            var templates = input.FingerPrintTemplateListToMatch ?? new List<FingerPrintInfo>();

            foreach (var item in templates.Where(x => !string.IsNullOrWhiteSpace(x.Template)))
            {
                var sample = Convert.FromBase64String(item.Template);
                var isoInfo = Activator.CreateInstance(ctx.IsoTemplateInfoType);
                var getInfoErr = Convert.ToInt32(Invoke(ctx.Manager, "GetIsoTemplateInfo", sample, isoInfo));
                if (getInfoErr != 0)
                {
                    continue;
                }

                var totalSamples = Convert.ToInt32(GetProperty(isoInfo, "TotalSamples") ?? 0);
                for (var i = 0; i < totalSamples; i++)
                {
                    var args = new object[] { sample, i, target, 0, ctx.SecurityLevelNormalValue, false };
                    var matchErr = Convert.ToInt32(InvokeWithByRef(ctx.Manager, "MatchIsoTemplate", args));
                    var matched = Convert.ToBoolean(args[5]);
                    if (matchErr == 0 && matched)
                    {
                        return item.staffId ?? "";
                    }
                }
            }

            return "";
        }

        private bool InitializeDevice(SecuGenContext ctx, out int imageWidth, out int imageHeight, out int imageDpi, out int maxTemplateSize, out string err)
        {
            imageWidth = 0;
            imageHeight = 0;
            imageDpi = 0;
            maxTemplateSize = 0;
            err = "";

            var initErr = Convert.ToInt32(Invoke(ctx.Manager, "Init", ctx.DeviceNameAutoValue));
            if (initErr != 0)
            {
                err = DisplayError(initErr);
                return false;
            }

            var openErr = Convert.ToInt32(Invoke(ctx.Manager, "OpenDevice", ctx.PortUsbAutoDetectValue));
            if (openErr != 0)
            {
                err = DisplayError(openErr);
                return false;
            }

            var deviceInfo = Activator.CreateInstance(ctx.DeviceInfoType);
            Invoke(ctx.Manager, "GetDeviceInfo", deviceInfo);
            imageWidth = Convert.ToInt32(GetProperty(deviceInfo, "ImageWidth") ?? 0);
            imageHeight = Convert.ToInt32(GetProperty(deviceInfo, "ImageHeight") ?? 0);
            imageDpi = Convert.ToInt32(GetProperty(deviceInfo, "ImageDPI") ?? 0);

            Invoke(ctx.Manager, "SetTemplateFormat", ctx.TemplateFormatIsoValue);
            var args = new object[] { 0 };
            InvokeWithByRef(ctx.Manager, "GetMaxTemplateSize", args);
            maxTemplateSize = Convert.ToInt32(args[0]);
            if (maxTemplateSize <= 0)
            {
                err = "Invalid template size returned by SDK.";
                return false;
            }

            return true;
        }

        private static bool TryCreateSecuGenContext(out SecuGenContext context, out string err)
        {
            context = null;
            err = "";
            try
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                                   .FirstOrDefault(a => string.Equals(a.GetName().Name, "SecuGen.FDxSDKPro.Windows", StringComparison.OrdinalIgnoreCase))
                               ?? Assembly.Load("SecuGen.FDxSDKPro.Windows");

                var managerType = assembly.GetType("SecuGen.FDxSDKPro.Windows.SGFingerPrintManager", true);
                var fingerInfoType = assembly.GetType("SecuGen.FDxSDKPro.Windows.SGFPMFingerInfo", true);
                var isoTemplateInfoType = assembly.GetType("SecuGen.FDxSDKPro.Windows.SGFPMISOTemplateInfo", true);
                var deviceInfoType = assembly.GetType("SecuGen.FDxSDKPro.Windows.SGFPMDeviceInfoParam", true);
                var fingerPositionType = assembly.GetType("SecuGen.FDxSDKPro.Windows.SGFPMFingerPosition", true);

                var impressionType = assembly.GetType("SecuGen.FDxSDKPro.Windows.SGFPMImpressionType", true);
                var deviceNameType = assembly.GetType("SecuGen.FDxSDKPro.Windows.SGFPMDeviceName", true);
                var portType = assembly.GetType("SecuGen.FDxSDKPro.Windows.SGFPMPortAddr", true);
                var securityType = assembly.GetType("SecuGen.FDxSDKPro.Windows.SGFPMSecurityLevel", true);
                var templateFormatType = assembly.GetType("SecuGen.FDxSDKPro.Windows.SGFPMTemplateFormat", true);

                context = new SecuGenContext
                {
                    Manager = Activator.CreateInstance(managerType),
                    FingerInfoType = fingerInfoType,
                    IsoTemplateInfoType = isoTemplateInfoType,
                    DeviceInfoType = deviceInfoType,
                    FingerPositionType = fingerPositionType,
                    ImpressionTypeLpValue = Convert.ToInt32(Enum.Parse(impressionType, "IMPTYPE_LP")),
                    DeviceNameAutoValue = Enum.Parse(deviceNameType, "DEV_AUTO"),
                    PortUsbAutoDetectValue = Convert.ToInt32(Enum.Parse(portType, "USB_AUTO_DETECT")),
                    SecurityLevelNormalValue = Enum.Parse(securityType, "NORMAL"),
                    TemplateFormatIsoValue = Enum.Parse(templateFormatType, "ISO19794")
                };

                return true;
            }
            catch (Exception ex)
            {
                err = "SecuGen scanner SDK is not available: " + ex.Message;
                return false;
            }
        }

        private static object Invoke(object target, string method, params object[] args)
        {
            var mi = target.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Instance);
            if (mi == null) throw new MissingMethodException(target.GetType().FullName, method);
            return mi.Invoke(target, args);
        }

        private static object InvokeWithByRef(object target, string method, object[] args)
        {
            var mi = target.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Instance);
            if (mi == null) throw new MissingMethodException(target.GetType().FullName, method);
            return mi.Invoke(target, args);
        }

        private static object GetProperty(object target, string property)
            => target?.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance)?.GetValue(target);

        private static void SetProperty(object target, string property, object value)
            => target?.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance)?.SetValue(target, value);

        private sealed class SecuGenContext
        {
            public object Manager { get; set; }
            public Type FingerInfoType { get; set; }
            public Type IsoTemplateInfoType { get; set; }
            public Type DeviceInfoType { get; set; }
            public Type FingerPositionType { get; set; }
            public int ImpressionTypeLpValue { get; set; }
            public object DeviceNameAutoValue { get; set; }
            public int PortUsbAutoDetectValue { get; set; }
            public object SecurityLevelNormalValue { get; set; }
            public object TemplateFormatIsoValue { get; set; }
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
