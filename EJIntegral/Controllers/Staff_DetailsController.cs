using CrystalDecisions.CrystalReports.Engine;
using EJIntegral.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EJIntegral.Controllers
{
    [Authorize]
    public class Staff_DetailsController : Controller
    {
        private EJIntegralDBEntities db = new EJIntegralDBEntities();
        private readonly ReportDocument reportDocument = new ReportDocument();
        // GET: Staff_Details
        public async Task<ActionResult> Index()
        {
            return View(await db.Staff_Details.ToListAsync());
        }

        // GET: Staff_Details/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var staff_Details = await db.Staff_Details.FindAsync(id);
            if (staff_Details == null)
            {
                return HttpNotFound();
            }
            return View(staff_Details);
        }

        // GET: Staff_Details/Create
        public ActionResult Create()
        {
            ViewBag.LGA = new SelectList(db.GetAllLGAS(), "Id", "LGA");
            ViewBag.Gender = new List<SelectListItem>{
                new SelectListItem { Value="F",Text="Female"},
                new SelectListItem { Value="M",Text="Male"}
                };
            ViewBag.MaritalStatus = new List<SelectListItem>{
                new SelectListItem { Value="Divorce",Text="Divorce"},
                new SelectListItem { Value="Married",Text="Married"},
                new SelectListItem { Value="Single",Text="Single"}
                };
            ViewBag.Relationship = new List<SelectListItem>
            {
                new SelectListItem{Value="Brother",Text="Brother"},
                new SelectListItem{Value="Father",Text="Father"},
                new SelectListItem{Value="Sister",Text="Sister"},
                new SelectListItem{Value="Mother",Text="Mother"},
                new SelectListItem{Value="Uncle",Text="Uncle"},
                new SelectListItem{Value="Son",Text="Son"},
                new SelectListItem{Value="Daughter",Text="Daughter"},
            };

            return View();
        }

        public ActionResult GetDAList(string lgId)
        {
            var lstDa = new List<DevelopmentArea>();
            var lgaId = Convert.ToInt32(lgId);
            
            
                lstDa = (db.DevelopmentAreas.Where(x => x.lgaId == lgaId)).ToList();
            
            var javaScriptSerializer = new JavaScriptSerializer();
            var result = javaScriptSerializer.Serialize(lstDa);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // POST: Staff_Details/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "StaffId,FirstName,MiddleName,LastName,Gender,DateOfBirth,ResidenceAddress,MaritalStatus,ContactPhone,EmailAddress,StateOfOrigin,LGA,NFullName,NAddress,NRelationship,NContactNumber,Status,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,DevArea")] Staff_Details staff_Details)
        {
            if (ModelState.IsValid)
            {
                staff_Details.CreatedBy = User.Identity.Name;
                staff_Details.CreatedOn = DateTime.Now;
                staff_Details.StateOfOrigin = "Nasawara";
                staff_Details.Status = true;

                Utility.StaffId = staff_Details.StaffId;
                staff_Details.StaffId = staff_Details.StaffId.ToUpper();

                db.Staff_Details.Add(staff_Details);
                await db.SaveChangesAsync();
                return RedirectToAction("Create","Service_Details");
            }

            return View(staff_Details);
        }

        // GET: Staff_Details/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.LGA = new SelectList(db.GetAllLGAS(), "Id", "LGA");
            ViewBag.Gender = new List<SelectListItem>{
                new SelectListItem { Value="F",Text="Female"},
                new SelectListItem { Value="M",Text="Male"}
                };
            ViewBag.MaritalStatus = new List<SelectListItem>{
                new SelectListItem { Value="Divorce",Text="Divorce"},
                new SelectListItem { Value="Married",Text="Married"},
                new SelectListItem { Value="Single",Text="Single"}
                };
            ViewBag.Relationship = new List<SelectListItem>
            {
                new SelectListItem{Value="Brother",Text="Brother"},
                new SelectListItem{Value="Father",Text="Father"},
                new SelectListItem{Value="Sister",Text="Sister"},
                new SelectListItem{Value="Mother",Text="Mother"},
                new SelectListItem{Value="Uncle",Text="Uncle"},
                new SelectListItem{Value="Son",Text="Son"},
                new SelectListItem{Value="Daughter",Text="Daughter"},
            };
            var staff_Details = await db.Staff_Details.FindAsync(id);
            if (staff_Details == null)
            {
                return HttpNotFound();
            }
            return View(staff_Details);
        }

        // POST: Staff_Details/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "StaffId,FirstName,MiddleName,LastName,Gender,DateOfBirth,ResidenceAddress,MaritalStatus,ContactPhone,EmailAddress,StateOfOrigin,LGA,NFullName,NAddress,NRelationship,NContactNumber,Status,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,DevArea")] Staff_Details staff_Details)
        {
            if (ModelState.IsValid)
            {
                staff_Details.ModifiedBy = User.Identity.Name;
                staff_Details.ModifiedOn = DateTime.Now;
                staff_Details.StateOfOrigin = "Nasawara";
                staff_Details.Status = true;

                Utility.StaffId = staff_Details.StaffId;

                db.Entry(staff_Details).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(staff_Details);
        }
        public ActionResult Print(string id)
        {
            //staffId = staffId.ToUpper();
            var staffDetails = db.GetEmployeeInfoForPrint(id).FirstOrDefault();
            //var empDetails = new StaffDetailViewModel();
            //empDetails = staffDetails;

            return View(staffDetails);
        }

        public ActionResult Report()
        {
            ViewBag.ReportType = new List<SelectListItem>
            {
                new SelectListItem{Text="Excel",Value="1"},
                new SelectListItem{Text="Pdf",Value="2"},
                new SelectListItem{Text="Word",Value="3"}
            };
            return View();
        }

        [HttpPost]
        public ActionResult Report(FormCollection form)
        {
            var reportType = int.Parse(Request.Form["ReportType"]);
            if (reportType == 1)//1 for Excel
            {
                using (var stream1 = new MemoryStream())
                {
                    var Path = Server.MapPath("~/Reports/EmployeeList.rpt");


                    reportDocument.Load(Path);
                    Stream oStream = null;
                    byte[] byteArray = null;
                    oStream = reportDocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    byteArray = new byte[oStream.Length];
                    oStream.Read(byteArray, 0, Convert.ToInt32(oStream.Length - 1));
                    oStream.Seek(0, SeekOrigin.Begin);
                    return File(oStream, "application/excel", string.Format("Staff {0}.xls", DateTime.Now.ToLongTimeString()));
                }
            }
            else if (reportType == 2)//for Pdf
            {
                using (var stream1 = new MemoryStream())
                {
                    var Path = Server.MapPath("~/Reports/EmployeeList.rpt");


                    reportDocument.Load(Path);
                    Stream oStream = null;
                    byte[] byteArray = null;
                    oStream = reportDocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    byteArray = new byte[oStream.Length];
                    oStream.Read(byteArray, 0, Convert.ToInt32(oStream.Length - 1));
                    oStream.Seek(0, SeekOrigin.Begin);
                    return File(oStream, "application/pdf", string.Format("Staff {0}.pdf", DateTime.Now.ToLongTimeString()));
                }
            }
            else if (reportType == 3)//For Word
            {
                using (var stream1 = new MemoryStream())
                {
                    var Path = Server.MapPath("~/Reports/EmployeeList.rpt");


                    reportDocument.Load(Path);
                    Stream oStream = null;
                    byte[] byteArray = null;
                    oStream = reportDocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    byteArray = new byte[oStream.Length];
                    oStream.Read(byteArray, 0, Convert.ToInt32(oStream.Length - 1));
                    oStream.Seek(0, SeekOrigin.Begin);
                    return File(oStream, "application/msword", string.Format("Staff {0}.doc", DateTime.Now.ToLongTimeString()));
                }
            }

            return View();
        }
        // GET: Staff_Details/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var staff_Details = await db.Staff_Details.FindAsync(id);
            if (staff_Details == null)
            {
                return HttpNotFound();
            }
            return View(staff_Details);
        }

        // POST: Staff_Details/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            var staff_Details = await db.Staff_Details.FindAsync(id);
            db.Staff_Details.Remove(staff_Details);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
            reportDocument.Dispose();
        }
    }
}
