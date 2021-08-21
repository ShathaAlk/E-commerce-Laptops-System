using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Laptop_Ecommerce_Shop;

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
                    var AdminObj = db.Admins.Where(model => model.AdminName.Equals(AdminUser.AdminName) && model.AdminPassword.Equals(AdminUser.AdminPassword)).FirstOrDefault();
                    if (AdminObj != null)
                    {
                        //the cookies contents will be saved for 1 day.
                        HttpCookie AdminIDCookie = new HttpCookie("AdminID", AdminObj.AdminID.ToString());
                        HttpCookie AdminNameCookie = new HttpCookie("AdminName", AdminObj.AdminName.ToString());
                        AdminIDCookie.Expires = DateTime.Now.AddDays(1);
                        AdminNameCookie.Expires = DateTime.Now.AddDays(1);
                        Response.Cookies.Add(AdminIDCookie);
                        Response.Cookies.Add(AdminNameCookie);

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
            if(Request.Cookies["AdminID"] == null)
            {
                return View("AdminLogin");
            }
            else
            {
                return View(db.ProductItems.ToList());
            }
        }

        //Creating CRUD for ProductItem
        // GET: Admin/Details/5
        public ActionResult Details(int? id)
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
            return View(productItem);
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,ProductName,Brand,Description,Price,image")] ProductItem productItem, HttpPostedFileBase Image1)
        {
            if (ModelState.IsValid)
            {
                //Add image to the table
                if (Image1 != null)
                {
                    productItem.image = new byte[Image1.ContentLength];
                    Image1.InputStream.Read(productItem.image, 0, Image1.ContentLength);
                }
                //Insert the values
                db.ProductItems.Add(productItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productItem);
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(int? id)
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
            return View(productItem);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,Brand,Description,Price,image")] ProductItem productItem, HttpPostedFileBase Image1)
        {
            if (ModelState.IsValid)
            {
                if (Image1 != null)
                {
                    productItem.image = new byte[Image1.ContentLength];
                    Image1.InputStream.Read(productItem.image, 0, Image1.ContentLength);
                }

                db.Entry(productItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productItem);
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(int? id)
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
            return View(productItem);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductItem productItem = db.ProductItems.Find(id);
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

        //Remove the Admin cookies.
        public ActionResult Logout()
        {
            if (Request.Cookies["AdminID"] != null)
            {
                Response.Cookies["AdminID"].Expires = DateTime.Now.AddDays(-1);
                Response.Cookies["AdminName"].Expires = DateTime.Now.AddDays(-1);
            }

            return View("AdminLogin");

        }
    }
}
