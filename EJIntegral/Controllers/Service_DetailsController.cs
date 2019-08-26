using EJIntegral.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EJIntegral.Controllers
{
    [Authorize]
    public class Service_DetailsController : Controller
    {
        private EJIntegralDBEntities db = new EJIntegralDBEntities();

        // GET: Service_Details
        public async Task<ActionResult> Index()
        {
            return View(await db.Service_Details.ToListAsync());
        }

        public ActionResult GetDesignationList(string deptId)
        {
            var lstDesignation = new List<Designation>();
            var depId = Convert.ToInt32(deptId);


            lstDesignation = (db.Designations.Where(x => x.DeparmentId == depId)).ToList();

            var javaScriptSerializer = new JavaScriptSerializer();
            var result = javaScriptSerializer.Serialize(lstDesignation);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: Service_Details/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var service_Details = await db.Service_Details.FindAsync(id);
            if (service_Details == null)
            {
                return HttpNotFound();
            }
            return View(service_Details);
        }
        private double GetSalaryFromDb(int gradeId,int stepId)
        {
            try
            {
                var salary = 0.0;

                var result = (from sal in db.Emp_Salaries where sal.Grade==gradeId && sal.Step==stepId select sal.Salary).FirstOrDefault();
                salary = Convert.ToDouble(result);
                return salary;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }

        public ActionResult GetSalary(string gradeId,string stepId)
        {
            var salaryAmount = 0.0;
            var gradId = Convert.ToInt32(gradeId);
            var steId = Convert.ToInt32(stepId);


            salaryAmount =GetSalaryFromDb(gradId,steId);
            var javaScriptSerializer = new JavaScriptSerializer();
            var result = javaScriptSerializer.Serialize(salaryAmount);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: Service_Details/Create
        public ActionResult Create()
        {
            ViewBag.Departments= new SelectList(db.GetAllDepartment(), "Id", "DepartmentName");
            ViewBag.GradeLevel = new List<SelectListItem>{
                new SelectListItem { Value="1",Text="GL01"},
                new SelectListItem { Value="2",Text="GL02"},
                new SelectListItem { Value="3",Text="GL03"},
                new SelectListItem { Value="4",Text="GL04"},
                new SelectListItem { Value="5",Text="GL05"},
                new SelectListItem { Value="6",Text="GL06"},
                new SelectListItem { Value="7",Text="GL07"},
                new SelectListItem { Value="8",Text="GL08"},
                new SelectListItem { Value="9",Text="GL09"},
                new SelectListItem { Value="10",Text="GL10"},
                new SelectListItem { Value="11",Text="GL11"},
                new SelectListItem { Value="12",Text="GL12"},
                new SelectListItem { Value="13",Text="GL13"},
                new SelectListItem { Value="14",Text="GL14"},
                new SelectListItem { Value="15",Text="GL15"},
                new SelectListItem { Value="16",Text="GL16"}
                };
            ViewBag.Qualification = new List<SelectListItem>{
                new SelectListItem { Value="FSLC",Text="FSLC"},
                new SelectListItem { Value="SSCE",Text="SSCE"},
                new SelectListItem { Value="DIPLOMA",Text="DIPLOMA"},
                new SelectListItem { Value="ND",Text="NATIONAL DIPLOMA"},
                new SelectListItem { Value="HND",Text="HND"},
                new SelectListItem { Value="Bsc",Text="BSC"},
                new SelectListItem { Value="PGD",Text="PGD"},
                new SelectListItem { Value="Msc",Text="MSC"},
                new SelectListItem { Value="Phd",Text="Phd"}
                };
            ViewBag.Step = new List<SelectListItem>{
                new SelectListItem { Value="1",Text="STEP01"},
                new SelectListItem { Value="2",Text="STEP02"},
                new SelectListItem { Value="3",Text="STEP03"},
                new SelectListItem { Value="4",Text="STEP04"},
                new SelectListItem { Value="5",Text="STEP05"},
                new SelectListItem { Value="6",Text="STEP06"},
                new SelectListItem { Value="7",Text="STEP07"},
                new SelectListItem { Value="8",Text="STEP08"},
                new SelectListItem { Value="9",Text="STEP09"},
                new SelectListItem { Value="10",Text="STEP10"},
                new SelectListItem { Value="11",Text="STEP11"},
                new SelectListItem { Value="12",Text="STEP12"},
                new SelectListItem { Value="13",Text="STEP13"},
                new SelectListItem { Value="14",Text="STEP14"},
                new SelectListItem { Value="15",Text="STEP15"}
                };
            return View();
        }

        // POST: Service_Details/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "StaffId,FileNo,Entry_Rank,DateOfFirstAppt,Entry_GradeLevel,Deparment,Designation,BankName,AccountNumber,BVN,ConsolidatedSalary,PlaceOfPosting,ConfirmationDate,YearOfLastPromotion,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,Qualification,Step,Allowances,CurrentRank,CurrentQualification,CurrentGradeLevel,CurrentStep,SignatoryToApptLetter,CurrentConsolidatedSalary,CurrentDesignation")] Service_Details service_Details)
        {
            if (ModelState.IsValid)
            {
                service_Details.CreatedOn = DateTime.Now;
                service_Details.CreatedBy = User.Identity.Name;
                service_Details.StaffId = Utility.StaffId;

                db.Service_Details.Add(service_Details);
                await db.SaveChangesAsync();
                return RedirectToAction("Create","Documents");
            }
            ViewBag.Qualification = new List<SelectListItem>{
                new SelectListItem { Value="FSLC",Text="FSLC"},
                new SelectListItem { Value="SSCE",Text="SSCE"},
                new SelectListItem { Value="DIPLOMA",Text="DIPLOMA"},
                new SelectListItem { Value="ND",Text="NATIONAL DIPLOMA"},
                new SelectListItem { Value="HND",Text="HND"},
                new SelectListItem { Value="Bsc",Text="BSC"},
                new SelectListItem { Value="PGD",Text="PGD"},
                new SelectListItem { Value="Msc",Text="MSC"},
                new SelectListItem { Value="Phd",Text="Phd"}
                };
            ViewBag.GradeLevel = new List<SelectListItem>{
                new SelectListItem { Value="1",Text="GL01"},
                new SelectListItem { Value="2",Text="GL02"},
                new SelectListItem { Value="3",Text="GL03"},
                new SelectListItem { Value="4",Text="GL04"},
                new SelectListItem { Value="5",Text="GL05"},
                new SelectListItem { Value="6",Text="GL06"},
                new SelectListItem { Value="7",Text="GL07"},
                new SelectListItem { Value="8",Text="GL08"},
                new SelectListItem { Value="9",Text="GL09"},
                new SelectListItem { Value="10",Text="GL10"},
                new SelectListItem { Value="11",Text="GL11"},
                new SelectListItem { Value="12",Text="GL12"},
                new SelectListItem { Value="13",Text="GL13"},
                new SelectListItem { Value="14",Text="GL14"},
                new SelectListItem { Value="15",Text="GL15"},
                new SelectListItem { Value="16",Text="GL16"}
                };
            ViewBag.Step = new List<SelectListItem>{
                new SelectListItem { Value="1",Text="STEP01"},
                new SelectListItem { Value="2",Text="STEP02"},
                new SelectListItem { Value="3",Text="STEP03"},
                new SelectListItem { Value="4",Text="STEP04"},
                new SelectListItem { Value="5",Text="STEP05"},
                new SelectListItem { Value="6",Text="STEP06"},
                new SelectListItem { Value="7",Text="STEP07"},
                new SelectListItem { Value="8",Text="STEP08"},
                new SelectListItem { Value="9",Text="STEP09"},
                new SelectListItem { Value="10",Text="STEP10"},
                new SelectListItem { Value="11",Text="STEP11"},
                new SelectListItem { Value="12",Text="STEP12"},
                new SelectListItem { Value="13",Text="STEP13"},
                new SelectListItem { Value="14",Text="STEP14"},
                new SelectListItem { Value="15",Text="STEP15"}
                };
            return View(service_Details);
        }

        // GET: Service_Details/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var service_Details = await db.Service_Details.FindAsync(id);
            ViewBag.GradeLevel = new List<SelectListItem>{
                new SelectListItem { Value="1",Text="GL01"},
                new SelectListItem { Value="2",Text="GL02"},
                new SelectListItem { Value="3",Text="GL03"},
                new SelectListItem { Value="4",Text="GL04"},
                new SelectListItem { Value="5",Text="GL05"},
                new SelectListItem { Value="6",Text="GL06"},
                new SelectListItem { Value="7",Text="GL07"},
                new SelectListItem { Value="8",Text="GL08"},
                new SelectListItem { Value="9",Text="GL09"},
                new SelectListItem { Value="10",Text="GL10"},
                new SelectListItem { Value="11",Text="GL11"},
                new SelectListItem { Value="12",Text="GL12"},
                new SelectListItem { Value="13",Text="GL13"},
                new SelectListItem { Value="14",Text="GL14"},
                new SelectListItem { Value="15",Text="GL15"},
                new SelectListItem { Value="16",Text="GL16"}
                };
            ViewBag.Step = new List<SelectListItem>{
                new SelectListItem { Value="1",Text="STEP01"},
                new SelectListItem { Value="2",Text="STEP02"},
                new SelectListItem { Value="3",Text="STEP03"},
                new SelectListItem { Value="4",Text="STEP04"},
                new SelectListItem { Value="5",Text="STEP05"},
                new SelectListItem { Value="6",Text="STEP06"},
                new SelectListItem { Value="7",Text="STEP07"},
                new SelectListItem { Value="8",Text="STEP08"},
                new SelectListItem { Value="9",Text="STEP09"},
                new SelectListItem { Value="10",Text="STEP10"},
                new SelectListItem { Value="11",Text="STEP11"},
                new SelectListItem { Value="12",Text="STEP12"},
                new SelectListItem { Value="13",Text="STEP13"},
                new SelectListItem { Value="14",Text="STEP14"},
                new SelectListItem { Value="15",Text="STEP15"}
                };
            ViewBag.Qualification = new List<SelectListItem>{
                new SelectListItem { Value="FSLC",Text="FSLC"},
                new SelectListItem { Value="SSCE",Text="SSCE"},
                new SelectListItem { Value="DIPLOMA",Text="DIPLOMA"},
                new SelectListItem { Value="ND",Text="NATIONAL DIPLOMA"},
                new SelectListItem { Value="HND",Text="HND"},
                new SelectListItem { Value="Bsc",Text="BSC"},
                new SelectListItem { Value="PGD",Text="PGD"},
                new SelectListItem { Value="Msc",Text="MSC"},
                new SelectListItem { Value="Phd",Text="Phd"}
                };
            if (service_Details == null)
            {
                return HttpNotFound();
            }
            return View(service_Details);
        }

        // POST: Service_Details/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "StaffId,FileNo,Entry_Rank,DateOfFirstAppt,Entry_GradeLevel,Deparment,Designation,BankName,AccountNumber,BVN,ConsolidatedSalary,PlaceOfPosting,ConfirmationDate,YearOfLastPromotion,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,Qualification,Step,Allowances,CurrentRank,CurrentQualification,CurrentGradeLevel,CurrentStep,SignatoryToApptLetter,CurrentConsolidatedSalary,CurrentDesignation")] Service_Details service_Details)
        {
            if (ModelState.IsValid)
            {
                service_Details.StaffId = Utility.StaffId;
                service_Details.ModifiedBy = User.Identity.Name;
                service_Details.ModifiedOn = DateTime.Now;

                db.Entry(service_Details).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.Qualification = new List<SelectListItem>{
                new SelectListItem { Value="FSLC",Text="FSLC"},
                new SelectListItem { Value="SSCE",Text="SSCE"},
                new SelectListItem { Value="DIPLOMA",Text="DIPLOMA"},
                new SelectListItem { Value="ND",Text="NATIONAL DIPLOMA"},
                new SelectListItem { Value="HND",Text="HND"},
                new SelectListItem { Value="Bsc",Text="BSC"},
                new SelectListItem { Value="PGD",Text="PGD"},
                new SelectListItem { Value="Msc",Text="MSC"},
                new SelectListItem { Value="Phd",Text="Phd"}
                };
            ViewBag.GradeLevel = new List<SelectListItem>{
                new SelectListItem { Value="1",Text="GL01"},
                new SelectListItem { Value="2",Text="GL02"},
                new SelectListItem { Value="3",Text="GL03"},
                new SelectListItem { Value="4",Text="GL04"},
                new SelectListItem { Value="5",Text="GL05"},
                new SelectListItem { Value="6",Text="GL06"},
                new SelectListItem { Value="7",Text="GL07"},
                new SelectListItem { Value="8",Text="GL08"},
                new SelectListItem { Value="9",Text="GL09"},
                new SelectListItem { Value="10",Text="GL10"},
                new SelectListItem { Value="11",Text="GL11"},
                new SelectListItem { Value="12",Text="GL12"},
                new SelectListItem { Value="13",Text="GL13"},
                new SelectListItem { Value="14",Text="GL14"},
                new SelectListItem { Value="15",Text="GL15"},
                new SelectListItem { Value="16",Text="GL16"}
                };
            ViewBag.Step = new List<SelectListItem>{
                new SelectListItem { Value="1",Text="STEP01"},
                new SelectListItem { Value="2",Text="STEP02"},
                new SelectListItem { Value="3",Text="STEP03"},
                new SelectListItem { Value="4",Text="STEP04"},
                new SelectListItem { Value="5",Text="STEP05"},
                new SelectListItem { Value="6",Text="STEP06"},
                new SelectListItem { Value="7",Text="STEP07"},
                new SelectListItem { Value="8",Text="STEP08"},
                new SelectListItem { Value="9",Text="STEP09"},
                new SelectListItem { Value="10",Text="STEP10"},
                new SelectListItem { Value="11",Text="STEP11"},
                new SelectListItem { Value="12",Text="STEP12"},
                new SelectListItem { Value="13",Text="STEP13"},
                new SelectListItem { Value="14",Text="STEP14"},
                new SelectListItem { Value="15",Text="STEP15"}
                };
            return View(service_Details);
        }

        // GET: Service_Details/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var service_Details = await db.Service_Details.FindAsync(id);
            if (service_Details == null)
            {
                return HttpNotFound();
            }
            return View(service_Details);
        }

        // POST: Service_Details/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var service_Details = await db.Service_Details.FindAsync(id);
            db.Service_Details.Remove(service_Details);
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
        }
    }
}
