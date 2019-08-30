using EJIntegral.Models;
using EJIntegral.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EJIntegral.Controllers
{
    [Authorize]
    public class Staff_DetailsController : Controller
    {
        private EJIntegralDBEntities db = new EJIntegralDBEntities();
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
                new SelectListItem { Value="Single",Text="Single"},
                 new SelectListItem { Value="Widow",Text="Widow"},
                new SelectListItem { Value="Widower",Text="Widower"}
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

                if (StaffExist(staff_Details.StaffId.ToUpper()))
                {
                    ViewBag.Error = " Staff with this Id already exist. " + staff_Details.StaffId;
                    ViewBag.LGA = new SelectList(db.GetAllLGAS(), "Id", "LGA");
                    ViewBag.Gender = new List<SelectListItem>{
                new SelectListItem { Value="F",Text="Female"},
                new SelectListItem { Value="M",Text="Male"}
                };
                    ViewBag.MaritalStatus = new List<SelectListItem>{
                new SelectListItem { Value="Divorce",Text="Divorce"},
                new SelectListItem { Value="Married",Text="Married"},
                new SelectListItem { Value="Single",Text="Single"},
                 new SelectListItem { Value="Widow",Text="Widow"},
                new SelectListItem { Value="Widower",Text="Widower"}
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
                    return View(staff_Details);
                }

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
            ViewBag.LGA = new SelectList(db.GetAllLGAS(), "Id", "LGA");
            ViewBag.Gender = new List<SelectListItem>{
                new SelectListItem { Value="F",Text="Female"},
                new SelectListItem { Value="M",Text="Male"}
                };
            ViewBag.MaritalStatus = new List<SelectListItem>{
                new SelectListItem { Value="Divorce",Text="Divorce"},
                new SelectListItem { Value="Married",Text="Married"},
                new SelectListItem { Value="Single",Text="Single"},
                 new SelectListItem { Value="Widow",Text="Widow"},
                new SelectListItem { Value="Widower",Text="Widower"}
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
            return View(staff_Details);
        }

        private bool StaffExist(string staffId)
        {
            var exist = false;
            var stafId = db.Staff_Details.FirstOrDefault(x => x.StaffId.ToUpper().Equals(staffId.ToUpper()));
            if (stafId != null)
            {
                exist = true;
            }
            return exist;
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

        public List<StaffDetailsViewModel> GetData()
        {
            var conString = ConfigurationManager.ConnectionStrings["EJIntegralConnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(conString))
            {
                var dt = new DataTable();
                var details = new List<StaffDetailsViewModel>();

                var cmd = new SqlCommand("GetEmployeeInfo", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);

                    foreach (DataRow dr in dt.Rows)
                    {
                        var obj = new StaffDetailsViewModel();

                        obj.StaffId = dr["StaffId"].ToString();
                        obj.Fullname = dr["Fullname"].ToString();
                        obj.DateOfBirth =Convert.ToDateTime( dr["DateOfBirth"].ToString());
                        obj.ContactNumber = dr["ContactNumber"].ToString();
                        obj.DepartmentName = dr["DepartmentName"].ToString();
                        obj.EntryDesignation = dr["EntryDesignation"].ToString();
                        obj.EntryGradeLevel = dr["EntryGradeLevel"].ToString();
                        obj.EntryStep =Convert.ToInt32( dr["EntryStep"].ToString());
                        obj.CurrentGradeLevel = dr["CurrentGradeLevel"].ToString();
                        obj.CurrentStep =Convert.ToInt32( dr["CurrentStep"].ToString());
                        obj.EntryQualification = dr["EntryQualification"].ToString();
                        obj.AdditionalQualification = dr["AdditionalQualification"].ToString();
                        obj.DateOfFirstAppt =Convert.ToDateTime( dr["DateOfFirstAppt"].ToString());

                        details.Add(obj);
                    }


                   
                }
                return details;
            }
        }

        public ActionResult Report()
        {
            var gv = new GridView();
            gv.DataSource = GetData();
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + "StaffDetails_" + DateTime.Today.ToShortTimeString() + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            var objStringWriter = new StringWriter();
            var objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
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

        // GET: Backup
        public ActionResult BackupDatabase()
        {
            var dbPath = Server.MapPath("~/App_Data/DBBackup.bak");
            using (var db = new EJIntegralDBEntities())
            {
                var cmd = $"BACKUP DATABASE {"EJIntegralDB"} TO DISK='{dbPath}' WITH FORMAT, MEDIANAME='DbBackups', MEDIADESCRIPTION='Media set for {"EJIntegralDB"} database';";
                db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, cmd);
            }
            return new FilePathResult(dbPath, "application/octet-stream");
        }


        public ActionResult RestoreDatabase()
        {
            var dbPath = Server.MapPath("~/App_Data/DBBackup.bak");
            using (var db = new EJIntegralDBEntities())
            {
                var cmd = $"USE master restore WITH REPLACE DATABASE {"EJIntegralDB"} from DISK='{dbPath}';";
                db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, cmd);
            }
            return View();
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
        }
    }
}
