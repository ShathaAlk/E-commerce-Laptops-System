using Laptop_Ecommerce_Shop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laptop_Ecommerce_Shop.Controllers
{
    public class HomeController : Controller
    {
        private Laptop_EcommerceEntities db = new Laptop_EcommerceEntities();
       
        public ActionResult Index()
        {
            //Check if the cookie exists to show the cart content
            if (Request.Cookies["CustomerID"] != null)
            {
                int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
                var CartNumbers = (from d in db.CustomerOrderTables
                                   where CustomerID == d.CustomerID && d.Purchased == 0
                                   select d).Count();
                TempData["CartNum"] = CartNumbers;
            }

            //Show the products' contents for the customers
            var productsList = (from d in db.ProductItems
                                select d).ToList();
            return View(productsList);
        }

        public ActionResult CustomerSignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerSignUp([Bind(Include = "CustomerID,CustomerName,Email,Password")] Customer CustomerUser)
        {
            if (ModelState.IsValid)
            {
                using (db)
                {
                    //Check if the email address already exists before signing up (inserting new data).
                    var CustomerEmail = db.Customers.Where(model => model.Email.Equals(CustomerUser.Email)).FirstOrDefault();
                    if (CustomerEmail != null)
                    {
                        TempData["Message"] = "This email address already exists.";
                        return View();
                    }
                    else
                    {
                        //When the conditions are met, new data will be inserted successfully.
                        db.Customers.Add(CustomerUser);
                        db.SaveChanges();
                        //Setting cookies for the new customer for 24 hours. 
                        var CustomerObj = db.Customers.Where(model => model.Email.Equals(CustomerUser.Email)).FirstOrDefault();
                        {
                            HttpCookie CustomerIDCookie = new HttpCookie("CustomerID", CustomerObj.CustomerID.ToString());
                            HttpCookie CustomerNameCookie = new HttpCookie("CustomerName", CustomerObj.CustomerName.ToString());
                            CustomerIDCookie.Expires = DateTime.Now.AddHours(24);
                            CustomerNameCookie.Expires = DateTime.Now.AddHours(24);
                            Response.Cookies.Add(CustomerIDCookie);
                            Response.Cookies.Add(CustomerNameCookie);
                        }
                        return RedirectToAction("Index");
                    }
                }
            }
            return View(CustomerUser);
        }

        public ActionResult CustomerLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerLogin(CustomerLoginModel CustomerUser)
        {
            if (ModelState.IsValid)
            {
                using (db)
                {
                    //Check if the email address and the password are available.
                    var CustomerObj = db.Customers.Where(model => model.Email.Equals(CustomerUser.Email) && model.Password.Equals(CustomerUser.Password)).FirstOrDefault();
                    if (CustomerObj != null)
                    {
                        HttpCookie CustomerIDCookie = new HttpCookie("CustomerID", CustomerObj.CustomerID.ToString());
                        HttpCookie CustomerNameCookie = new HttpCookie("CustomerName", CustomerObj.CustomerName.ToString());
                        //if the RememberMe checkbox is checked, the cookies contents will be saved for 100 days.
                        if (CustomerUser.RememberMe)
                        {
                            CustomerIDCookie.Expires = DateTime.Now.AddDays(100);
                            CustomerNameCookie.Expires = DateTime.Now.AddDays(100);
                           
                        }
                        // if not, the cookies contents will be saved for 1 hour.
                        else
                        {
                            CustomerIDCookie.Expires = DateTime.Now.AddHours(1);
                            CustomerNameCookie.Expires = DateTime.Now.AddHours(1);
                           
                        }
                        Response.Cookies.Add(CustomerIDCookie);
                        Response.Cookies.Add(CustomerNameCookie);
                        return RedirectToAction("Index");

                    }
                    else
                    {
                        TempData["Message"] = "Login Failed, Email or Password is Incorrect.";
                    }
                }
            }
            return View(CustomerUser);
        }

        //Remove the Customer cookies.
        public ActionResult Logout()
        {
            if (Request.Cookies["CustomerID"] != null)
            {
                Response.Cookies["CustomerID"].Expires = DateTime.Now.AddDays(-1);
                Response.Cookies["CustomerName"].Expires = DateTime.Now.AddDays(-1);
            }

            return RedirectToAction("Index");

        }
           
    }
}