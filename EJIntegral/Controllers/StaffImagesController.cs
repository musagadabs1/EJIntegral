using EJIntegral.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace EJIntegral.Controllers
{
    [Authorize]
    public class StaffImagesController : Controller
    {
        private EJIntegralDBEntities db = new EJIntegralDBEntities();
        string imageName = string.Empty;
        // GET: StaffImages
        public async Task<ActionResult> Index()
        {
            return View(await db.StaffImages.ToListAsync());
        }

        [HttpPost]
        public ContentResult GetCapture()
        {
            var url = Session["CapturedImage"].ToString();
            Session["CapturedImage"] = null;
            return Content(url);
        }

        [HttpPost]
        public ActionResult Capture()
        {
            if (Request.InputStream.Length > 0)
            {
                //var empNo =int.Parse( db.GetMaxId().ToString());// 1;// db.Ge;
                using (StreamReader reader = new StreamReader(Request.InputStream))
                {
                    var hexString = Server.UrlEncode(reader.ReadToEnd());
                    imageName = Utility.StaffId +"_"+ DateTime.Now.ToString("dd-MM-yy hh-mm-ss") + ".png";
                    var folderPath = Server.MapPath("~/Captures");
                    var imagePath = Path.Combine(folderPath, imageName);
                    //var imagePath = $"~/Captures/{imageName}.png";
                    Utility.ImagePath = imageName;
                    System.IO.File.WriteAllBytes(imagePath, ConvertHexToBytes(hexString));
                    Session["CapturedImage"] = imagePath;// VirtualPathUtility.ToAbsolute(imagePath);
                }
                //empNo += 1;
            }

            return View();
        }

        private static byte[] ConvertHexToBytes(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        // GET: StaffImages/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var staffImage = await db.StaffImages.FindAsync(id);
            if (staffImage == null)
            {
                return HttpNotFound();
            }
            return View(staffImage);
        }

        // GET: StaffImages/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StaffImages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,ImgPath,StaffId")] StaffImage staffImage)
        {
            //if (ModelState.IsValid)
            //{
            //var staffId = "stf001";

            staffImage.ImgPath = Utility.ImagePath;
            staffImage.StaffId = Utility.StaffId;
            //staffImage.StaffId = staffId;

            db.StaffImages.Add(staffImage);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
            //}

            //return View(staffImage);
        }

        // GET: StaffImages/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var staffImage = await db.StaffImages.FindAsync(id);
            if (staffImage == null)
            {
                return HttpNotFound();
            }
            return View(staffImage);
        }

        // POST: StaffImages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ImgPath,StaffId")] StaffImage staffImage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(staffImage).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(staffImage);
        }

        // GET: StaffImages/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var staffImage = await db.StaffImages.FindAsync(id);
            if (staffImage == null)
            {
                return HttpNotFound();
            }
            return View(staffImage);
        }

        // POST: StaffImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var staffImage = await db.StaffImages.FindAsync(id);
            db.StaffImages.Remove(staffImage);
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
