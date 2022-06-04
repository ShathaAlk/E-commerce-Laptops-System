using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Laptop_Ecommerce_Shop;
using Laptop_Ecommerce_Shop.Models;

namespace Laptop_Ecommerce_Shop.Controllers
{
    public class AdminController : Controller
    {
        private Laptop_EcommerceEntities db = new Laptop_EcommerceEntities();

        public ActionResult AdminLogin()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminLogin(Admin AdminUser)
        {
            if (ModelState.IsValid)
            {
                using (db)
                {
                    //Check if the AdminName and the password are available.
                    var AdminInfo = db.Admins.Where(model => model.AdminName.Equals(AdminUser.AdminName) && model.AdminPassword.Equals(AdminUser.AdminPassword)).FirstOrDefault();
                    if (AdminInfo != null)
                    {
                        Session["AdminID"] = AdminInfo.AdminID.ToString();
                        Session["AdminName"] = AdminInfo.AdminName.ToString();                       
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        TempData["Message"] = "Login Failed, Username or Password is Incorrect.";
                    }
                }
            }
            return View(AdminUser);
        }
        //Show the list of products
        public ActionResult Index()
        {
            //if the Admin cookies does not exist, Index will redirect to AdminLogin view 
            if(Session["AdminID"] == null)
            {
                return View("AdminLogin");
            }
            else
            {
                List<ProductItemsDetails> productItemDetails = db.ProductItemsDetails_FN().ToList();                
                return View(productItemDetails);
            }
        }

        public void ShowFiles(int id)
        {
            ProductItem productItem = db.ProductItems.Find(id);
            //Show product images
            ViewData["Product images"] = db.ProductFiles.Where(model => model.FileType == "image/jpeg" && model.ProductID == productItem.ProductID).ToList();
            //Show product video
            ViewData["Product videos"] = db.ProductFiles.Where(model => model.FileType == "video/mp4" && model.ProductID == productItem.ProductID).ToList();
        }

        //Creating CRUD for ProductItem
        // GET: Admin/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["AdminID"] == null)
            {
                return View("AdminLogin");
            }
            else
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                ProductItem productItem = db.ProductItems.Find(id);
                if (productItem == null)
                {
                    return HttpNotFound();
                }
                ShowFiles(productItem.ProductID);
                return View(productItem);
            }
                
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            if (Session["AdminID"] == null)
            {
                return View("AdminLogin");
            }
            else
            {
                return View();
            }
        }

        // POST: Admin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,ProductName,Brand,Description,Price,image,video,keyFeatures")] ProductItem productItem, ProductFile productFile)
        {         
            foreach (var item in productFile.imgFile) {
                productItem.ProductFiles.Add(new ProductFile()
                {
                    FileType = item.ContentType,
                    FileContent = ConvertToBytes(item),
                    ProductID = productItem.ProductID,
                    SelectedMainImage = 0,

                });
            }
            try
            {
                foreach (var item in productFile.videoFile)
                {
                    productItem.ProductFiles.Add(new ProductFile()
                    {
                        FileType = item.ContentType,
                        FileContent = ConvertToBytes(item),
                        ProductID = productItem.ProductID,
                        SelectedMainImage = 0,

                    });
                }

            }
            catch (Exception)
            {

            }
           
            db.ProductItems.Add(productItem); // parent and its children gets added
            db.SaveChanges();
            Session["PID"] = productItem.ProductID;
            HttpCookie PIDCookie = new HttpCookie("PID", productItem.ProductID.ToString());
            PIDCookie.Expires = DateTime.Now.AddHours(24);
            Response.Cookies.Add(PIDCookie);
            return RedirectToAction("SelectMainImage");
        }

        //GET: Admin/Create
        public ActionResult SelectMainImage()
        {
            if (Session["AdminID"] == null)
            {
                return View("AdminLogin");
            }
            else
            {
                if (Session["PID"] == null)
                {
                    //if there is no cookie, the customer will stay in the Index
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    int ProductID = Convert.ToInt32(Session["PID"]);
                    var item = (from d in db.ProductFiles
                                where ProductID == d.ProductID
                                select d).ToList();
                    ShowFiles(ProductID);
                    return View(item);

                }
            }            
        }
        public ActionResult SelectingMainImage(string SelectedValue)
        {
            int SelectedValueNum = Convert.ToInt32(SelectedValue);
            int PID = Convert.ToInt32(Session["PID"]);
            var UpdateOldImage = db.ProductFiles.Where(model => model.ProductID == PID && model.SelectedMainImage == 1).FirstOrDefault();
            var UpdateMainImage = db.ProductFiles.Where(model => model.FileID == SelectedValueNum).FirstOrDefault();
            var UpdateDefaultImage = db.ProductFiles.Where(model => model.ProductID == PID && model.FileType == "image/jpeg").FirstOrDefault();

            if (ModelState.IsValid)
            {
                using (db)
                {
                    if(SelectedValue == null)
                    {
                        if (UpdateDefaultImage != null)
                        {
                            UpdateDefaultImage.FileID = UpdateDefaultImage.FileID;
                            UpdateDefaultImage.SelectedMainImage = 1;
                            db.SaveChanges();
                            TempData["UpdateDefaultMessage"] = "Note: First Image was Selected.";
                        }
                    }
                    else
                    {
                        if (UpdateOldImage != null)
                        {
                            UpdateOldImage.FileID = UpdateOldImage.FileID;
                            UpdateOldImage.SelectedMainImage = 0;
                            db.SaveChanges();
                        }
                        if (UpdateMainImage != null)
                        {
                            UpdateMainImage.FileID = SelectedValueNum;
                            UpdateMainImage.SelectedMainImage = 1;
                            db.SaveChanges();
                            TempData["SelectImageMessage"] = "<script>divModal.style.display = 'block';</script>";
                        }
                    }                          
                }
            }
            return RedirectToAction("Index");
        }
       
            public byte[] ConvertToBytes(HttpPostedFileBase file)
            {
                byte[] imageBytes = null;
                BinaryReader reader = new BinaryReader(file.InputStream);
                imageBytes = reader.ReadBytes((int)file.ContentLength);
                return imageBytes;
            }

        public ActionResult Edit(int? id)
        {
            if (Session["AdminID"] == null)
            {
                return View("AdminLogin");
            }
            else
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                ProductItem productItem = db.ProductItems.Find(id);
                MultiModelsForProducts model = new MultiModelsForProducts();
                model.ProductItem = productItem;
                Session["PID"] = productItem.ProductID;

                return View(model);
            }                       
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,Brand,Description,Price,keyFeatures")] ProductItem productItem, ProductFile productFile, HttpPostedFileBase file)
        {          
            var productFiles = db.ProductFiles.Where(f => f.ProductID == productItem.ProductID && f.FileType == "image/jpeg").ToList();
            ProductFile UpdateFile = new ProductFile();

            db.Entry(productItem).State = EntityState.Modified;
            db.SaveChanges();
            TempData["UpdateMessage"] = "It is updated successfully.";           
            return View();

        }

        public ActionResult UpdateFiles()
        {
            if (Session["AdminID"] == null)
            {
                return View("AdminLogin");
            }
            else
            {
                ShowFiles(Convert.ToInt32(Session["PID"]));
                return View();
            }
           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //Update multiple images' files
        public ActionResult UpdateFiles(ProductFile productFile, HttpPostedFileBase videoFiles)
        {
            int PID = Convert.ToInt32(Session["PID"]);
            var DeleteProductImages = db.ProductFiles.Where(f => f.ProductID == PID && f.FileType == "image/jpeg").ToList();
            var DeleteProductVideos = db.ProductFiles.Where(f => f.ProductID == PID && f.FileType == "video/mp4").ToList();
            ProductFile UpdateFile = new ProductFile();

            if (productFile.imgFile != null)
            {
                foreach (var pf in DeleteProductImages)
                {
                    db.ProductFiles.Remove(pf);
                    db.SaveChanges();
                }
               
            }

            foreach (var item in productFile.imgFile)
            {
                productFile.FileType = item.ContentType;
                productFile.FileContent = ConvertToBytes(item);
                productFile.ProductID = Convert.ToInt32(Session["PID"]);
                productFile.SelectedMainImage = 0;

                db.ProductFiles.Add(productFile);
                db.SaveChanges();

            }
                       
            HttpCookie PIDCookie = new HttpCookie("PID", Session["PID"].ToString());
            PIDCookie.Expires = DateTime.Now.AddHours(24);
            Response.Cookies.Add(PIDCookie);
            ShowFiles(Convert.ToInt32(Session["PID"]));
            TempData["UpdateImageMessage"] = "It is updated successfully.";
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateVideo(ProductFile productFile)
        {
            int PID = Convert.ToInt32(Session["PID"]);
            var DeleteProductVideos = db.ProductFiles.Where(f => f.ProductID == PID && f.FileType == "video/mp4").ToList();
            ProductFile UpdateFile = new ProductFile();
            try
            {
                if (productFile.videoFile != null)
            {
                foreach (var pf in DeleteProductVideos)
                {
                    db.ProductFiles.Remove(pf);
                    db.SaveChanges();
                }

            }
                    foreach (var item in productFile.videoFile)
                    {
                        productFile.FileType = item.ContentType;
                        productFile.FileContent = ConvertToBytes(item);
                        productFile.ProductID = Convert.ToInt32(Session["PID"]);
                        productFile.SelectedMainImage = 0;

                        db.ProductFiles.Add(productFile);
                        db.SaveChanges();

                    }
                }
                catch (Exception)
                {

                }
            
            Session["PID"] = Session["PID"];
            HttpCookie PIDCookie = new HttpCookie("PID", Session["PID"].ToString());
            PIDCookie.Expires = DateTime.Now.AddHours(24);
            Response.Cookies.Add(PIDCookie);
            ShowFiles(Convert.ToInt32(Session["PID"]));
            TempData["UpdateVideoMessage"] = "It is updated successfully.";
            return RedirectToAction("UpdateFiles");
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["AdminID"] == null)
            {
                return View("AdminLogin");
            }
            else
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                ProductItem productItem = db.ProductItems.Find(id);
                if (productItem == null)
                {
                    return HttpNotFound();
                }
                ShowFiles(productItem.ProductID);
                return View(productItem);
            }            
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductItem productItem = db.ProductItems.Find(id);
            var productFiles  = db.ProductFiles.Where(f => f.ProductID == productItem.ProductID).ToList();
            if(productFiles != null)
            {
                foreach (var pf in productFiles)
                {
                    db.ProductFiles.Remove(pf);
                }               
            }
           
            db.ProductItems.Remove(productItem);
            db.SaveChanges();
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

        //Remove the Admin Session.
        public ActionResult Logout()
        {           
            Session["AdminID"] = null;            
            return View("AdminLogin");
        }
    }
}
