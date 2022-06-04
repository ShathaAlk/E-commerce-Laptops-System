using Laptop_Ecommerce_Shop.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laptop_Ecommerce_Shop.Controllers
{
    public class HomeController : Controller
    {
        private Laptop_EcommerceEntities db = new Laptop_EcommerceEntities();

        //Fuction to count the cart content
        public int TheCart_OnLoad()
        {
            int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
            var CartNumbers = (from d in db.CustomerOrderTables
                               where CustomerID == d.CustomerID && d.Purchased == 0
                               select d).Count();
            TempData["CartNum"] = CartNumbers;
            return (CartNumbers);
        }

        public ActionResult Index()
        {
            //Check if the cookie exists to show the cart content
            if (Request.Cookies["CustomerID"] != null)
            {
                TheCart_OnLoad();

            }
            return View();
        }

        public ActionResult Products(string brandFilter, string searchString)
        {
            //Check if the cookie exists to show the cart content
            if (Request.Cookies["CustomerID"] != null)
            {
                TheCart_OnLoad();
            }
            ViewData["Dell"] = String.IsNullOrEmpty(brandFilter) ? "dell" : "dell";
            ViewData["HP"] = String.IsNullOrEmpty(brandFilter) ? "hp" : "hp";
            ViewData["Apple"] = String.IsNullOrEmpty(brandFilter) ? "Apple" : "Apple";
            ViewData["Asus"] = String.IsNullOrEmpty(brandFilter) ? "Asus" : "Asus";
            ViewData["Huawei"] = String.IsNullOrEmpty(brandFilter) ? "Huawei" : "Huawei";
            ViewData["SearchFilter"] = searchString;

            var productsListDetails = from p in db.ProductItemsDetails_FN().ToList()
                                      select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                var searchingProducts = productsListDetails.Where(p => p.ProductName.Contains(searchString.ToLower())
                                        || p.Brand.Contains(searchString.ToLower()));
                return View(searchingProducts);
            }

            if (brandFilter != null)
            {               
                switch (brandFilter)
                {
                    case "dell":
                        productsListDetails = productsListDetails.Where(p => p.Brand.Contains("Dell".ToLower()));
                        //TempData["LoginMsg"] = "<script>divModal.style.display = 'block';</script>";
                        TempData["divDell"] = "<script>divDell.classList.add('iso-active');</script>";

                        break;
                    case "hp":
                        productsListDetails = productsListDetails.Where(p => p.Brand.Contains("HP".ToLower()));
                        TempData["divHP"] = "<script>divHP.classList.add('iso-active');</script>";
                        break;
                    case "Apple":
                        productsListDetails = productsListDetails.Where(p => p.Brand.Contains("Apple".ToLower()));
                        TempData["divApple"] = "<script>divApple.classList.add('iso-active');</script>";
                        break;
                    case "Asus":
                        productsListDetails = productsListDetails.Where(p => p.Brand.Contains("Asus".ToLower()));
                        TempData["divAsus"] = "<script>divAsus.classList.add('iso-active');</script>";
                        break;
                    case "Huawei":
                        productsListDetails = productsListDetails.Where(p => p.Brand.Contains("Huawei".ToLower()));
                        TempData["divHuawei"] = "<script>divHuawei.classList.add('iso-active');</script>";
                        break;

                    default:
                        productsListDetails.ToList();
                        TempData["divAll"] = "<script>divAll.classList.add('iso-active');</script>";
                        break;
                }
            }

            ViewBag.Current = "Products";
            return View(productsListDetails);
        }

        public ActionResult AddToCart(int id)
        {
            //check if the customer has already login before adding items to the cart by checking the cookie exists
            if (Request.Cookies["CustomerID"] == null)
            {
                TempData["LoginMsg"] = "<script>divModal.style.display = 'block';</script>";
            }
            else
            {
                ProductItem PID = db.ProductItems.Find(id);
                int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
                if (ModelState.IsValid)
                {
                    using (db)
                    {
                        //check if the item already added to the cart by chicking the existence of PID and CID
                        var addedToCart = db.CustomerOrderTables.Where(model => model.ProductID.Equals(PID.ProductID) && model.CustomerID.Equals(CustomerID) && model.Purchased == 0).FirstOrDefault();
                        if (addedToCart != null)
                        {
                            TempData["ProductAdded"] = "<script>divModal.style.display = 'block';</script>";
                        }
                        else
                        {
                            //insert the values to the table
                            CustomerOrderTable CCT = new CustomerOrderTable();
                            // fields to be insert
                            CCT.ProductID = PID.ProductID;
                            CCT.CustomerID = CustomerID;
                            CCT.Quantity = 1;
                            CCT.Purchased = 0;
                            db.CustomerOrderTables.Add(CCT);
                            db.SaveChanges();
                        }
                    }
                }
            }
            return RedirectToAction("Products", "Home");
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
                        TempData["EmailExisted"] = "<script> divModal.style.display = 'block'</script>";
                        return View();
                    }
                    else
                    {
                        //When the conditions are met, new data will be inserted successfully.
                        db.Customers.Add(CustomerUser);
                        db.SaveChanges();
                        //Setting cookies for the new customer for 24 hours. 
                        var CustomerInfo = db.Customers.Where(model => model.Email.Equals(CustomerUser.Email)).FirstOrDefault();
                        {
                            HttpCookie CustomerIDCookie = new HttpCookie("CustomerID", CustomerInfo.CustomerID.ToString());
                            HttpCookie CustomerNameCookie = new HttpCookie("CustomerName", CustomerInfo.CustomerName.ToString());
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
            ViewBag.PreviousUrl = System.Web.HttpContext.Current.Request.UrlReferrer.ToString();
            if (ModelState.IsValid)
            {
                using (db)
                {
                    //Check if the email address and the password are available.
                    var availableCustomerInfo = db.Customers.Where(model => model.Email.Equals(CustomerUser.Email) && model.Password.Equals(CustomerUser.Password)).FirstOrDefault();
                    if (availableCustomerInfo != null)
                    {
                        HttpCookie CustomerIDCookie = new HttpCookie("CustomerID", availableCustomerInfo.CustomerID.ToString());
                        HttpCookie CustomerNameCookie = new HttpCookie("CustomerName", availableCustomerInfo.CustomerName.ToString());
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
                        TempData["LoginFailed"] = "<script> divModal.style.display = 'block';</script>";
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