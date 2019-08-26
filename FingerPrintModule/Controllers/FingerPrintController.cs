using FingerPrintModule.DAO;
using FingerPrintModule.Facade;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace FingerPrintModule.Controllers
{
    public class FingerPrintController : ApiController
    {

        [HttpGet]
        public FingerPrintInfo CapturePrint(int fingerPosition)
        {
            FingerPrintFacade fingerPrintFacade = new FingerPrintFacade();
            var data = fingerPrintFacade.Capture(fingerPosition, out string err, false);

            if (string.IsNullOrEmpty(err))
            {
                var db = new DataAccess();
                var previously = db.GetBiometricinfo();

                var matchedStaffId = fingerPrintFacade.Verify(new FingerPrintMatchInputModel
                {
                    FingerPrintTemplate = data.Template,
                    FingerPrintTemplateListToMatch = new List<FingerPrintInfo>(previously)
                });

                if (matchedStaffId != "")
                {
                    string info = db.RetrieveStaffIdByID(matchedStaffId);
                    string name = info.Split('|')[0];
                    string UniqueId = info.Split('|')[1];
                    data.ErrorMessage = string.Format("Finger print record already exist for this staff {0} Name : {1} {2}  Identifier : {3}", Environment.NewLine, name, Environment.NewLine, UniqueId);
                }
            }
            else
            {
                data = new FingerPrintInfo();
                data.ErrorMessage = err;
            }
            return data;
        }

        [HttpGet]
        public List<FingerPrintInfo> CheckForPreviousCapture(string staffId)
        {
            if (ConfirmDBSetting())
            {
                var db = new DataAccess();
                var staffInfo = db.RetrieveStaffIdByID(staffId);

                if (staffInfo != null && Int32.TryParse(staffInfo.Split('|')[1], out int sid))
                {
                    var previously = db.GetBiometricinfo(sid);
                    return previously;
                }
                else
                {
                    return null;
                }
            }
            else
                throw new ApplicationException("Invalid database connection ");
        }


        [HttpPost]
        public string MatchFingerPrint(FingerPrintMatchInputModel input)
        {

            var fingerPrintFacade = new FingerPrintFacade();
            var matchedStaffId = fingerPrintFacade.Verify(input);
            
            return JsonConvert.SerializeObject(new
            {
                PatientId = matchedStaffId,
                Matched = matchedStaffId != ""
            });
        }


        [HttpPost]
        public ResponseModel SaveToDatabase(SaveModel model)
        {
            var db = new DataAccess();
            var staffId = model.StaffId;
            var staffInfo = db.RetrieveStaffIdByID(staffId);

            if (staffInfo != null && !string.IsNullOrEmpty(staffInfo.Split('|')[1]))
            {
                model.FingerPrintList.ForEach(x =>
                    {
                        x.staffId = staffInfo.Split('|')[1];
                    }
                );
                return db.SaveToDatabase(model.FingerPrintList);
            }
            else
            {
                return new ResponseModel
                {
                    ErrorMessage = "Invalid staffId supplied",
                    IsSuccessful = false
                };
            }
        }


        public bool ConfirmDBSetting()
        {
            var db = new DataAccess(DataAccess.GetConnectionString());
            //check if connection is valid
            //if connection string is wrong an error would occur here and will return error
            db.ExecuteScalar(string.Format("Select DB_ID('EJIntegralDB');"));

            db.ExecuteQuery("CheckBiometricTable");
            return true;
        }
    }
}
