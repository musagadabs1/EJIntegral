using EJIntegral.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EJIntegral.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private EJIntegralDBEntities db = new EJIntegralDBEntities();

        // GET: Documents
        public async Task<ActionResult> Index()
        {
            return View(await db.Documents.ToListAsync());
        }

        // GET: Documents/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var document = await db.Documents.FindAsync(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // GET: Documents/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Documents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Description,DocPath,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,StaffId")] Document document)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var documentDetails = new List<DocumentDetail>();
                    //var staffId = "STF005";
                    var fileName = string.Empty;
                    var filePath = string.Empty;

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (file !=null && file.ContentLength>0)
                        {
                            fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Docs"), fileName);
                            file.SaveAs(path);
                            document.CreatedBy = User.Identity.Name;
                            document.CreatedOn = DateTime.Now;
                            document.DocPath = fileName;
                            document.StaffId = Utility.StaffId;
                            //document.StaffId = staffId;

                            db.Documents.Add(document);
                            await db.SaveChangesAsync();

                        }
                    }
                    return RedirectToAction("Create","StaffImages");
                }
                catch (Exception ex)
                {

                    throw ex;
                }
                
            }

            return View(document);
        }

        // GET: Documents/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var document = await db.Documents.FindAsync(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Description,DocPath,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,StaffId")] Document document,HttpPostedFileBase fileBase)
        {
            if (ModelState.IsValid)
            {
                var fileName = string.Empty;
                var filePath = string.Empty;

                if (fileBase.ContentLength > 0 && fileBase != null)
                {
                    filePath = fileBase.FileName;
                    fileName = Path.GetFileName(fileBase.FileName);
                }
                var folderPath = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/Docs";
                var docPath = Path.Combine(folderPath, filePath);
                fileBase.SaveAs(Server.MapPath(docPath));

                document.CreatedBy = User.Identity.Name;
                document.CreatedOn = DateTime.Now;
                document.DocPath = fileName;
                document.StaffId = Utility.StaffId;

                document.ModifiedBy = User.Identity.Name;
                document.ModifiedOn = DateTime.Now;

                db.Entry(document).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(document);
        }

        // GET: Documents/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var document = await db.Documents.FindAsync(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var document = await db.Documents.FindAsync(id);
            db.Documents.Remove(document);
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
