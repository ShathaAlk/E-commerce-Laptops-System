using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Laptop_Ecommerce_Shop.Controllers
{
    public class CustomerController : Controller
    {
        private Laptop_EcommerceEntities db = new Laptop_EcommerceEntities();
        public ActionResult AddToCart(int id)
        {
            //check if the customer has already login before adding items to the cart by checking the cookie exists
            if (Request.Cookies["CustomerID"] == null) {
                TempData["msg"] = "<script>alert('Please Login first to continue');</script>";
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
                        var obj = db.CustomerOrderTables.Where(model => model.ProductID.Equals(PID.ProductID) && model.CustomerID.Equals(CustomerID) && model.Purchased == 0).FirstOrDefault();
                        if (obj != null)
                        {
                            TempData["msg"] = "<script>alert('It is Already in The Cart');</script>";
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
            return RedirectToAction("Index", "Home");
        }

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

       
        public ActionResult CustomerCart()
        {
            //Check if the cookie exists to show the cart content
            if (Request.Cookies["CustomerID"] == null)
            {
                TempData["msg"] = "<script>alert('Please Login first to continue');</script>";
            }
            else
            {
                int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
                //Call the method (count the cart content)
                TheCart_OnLoad();

                //Calculate the total price of all products that are added to the cart.
                var TheTotalPrice = db.CustomerCartDatails_FN(CustomerID).Sum(x => x.TotalPrice);
                //Save the result in session.
                Session["TotalPrice"] = TheTotalPrice;

                // show the CustomerCartDatails_FN procedure to displays the cart contents
                return View(db.CustomerCartDatails_FN(CustomerID).ToList());
            }
            //if there is no cookie, the customer will stay in the Index
            return RedirectToAction("Index", "Home");

        }

        //Delete specific item from the cart
        public ActionResult DeleteItem(int id)
        {
            CustomerOrderTable CCID = db.CustomerOrderTables.Find(id);
            db.CustomerOrderTables.Remove(CCID);
            db.SaveChanges();
            return RedirectToAction("CustomerCart");

        }
        
        public ActionResult ModifyProductDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerOrderTable CCT = db.CustomerOrderTables.Find(id);
            if (CCT == null)
            {
                return HttpNotFound();
            }
            //Call the method (count the cart content)
            TheCart_OnLoad();
            return View(CCT);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ModifyProductDetails(int CustomerOrderID, int Quantity)
        {
            //Update the item's quantity by using CustomerOrderID 
            var UpdateQuantity = new CustomerOrderTable()
            {
                CustomerOrderID = CustomerOrderID,
                Quantity = Quantity
            };
            using (db)
            {
                db.CustomerOrderTables.Attach(UpdateQuantity);
                db.Entry(UpdateQuantity).Property(x => x.Quantity).IsModified = true;
                db.SaveChanges();
            }

            return RedirectToAction("CustomerCart");

        }

        //Create the customer's details and payment view.
        public ActionResult Checkout()
        {
            int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
            //Get the Customer Email and show it in ckeckout view
            var CustomerEmail = (from d in db.Customers
                                 where CustomerID == d.CustomerID
                                 select d.Email).FirstOrDefault();
            Session["CustomerEmail"] = CustomerEmail;
            //Call the method (count the cart content)
            TheCart_OnLoad();
            return View();
        }

        public ActionResult Payment(string Address)
        {
            int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
            int TheTotalPrice = Convert.ToInt32(Session["TotalPrice"]);
            List<CustomerOrderTable> COT = db.CustomerOrderTables.Where(u => u.CustomerID == CustomerID && u.Purchased ==0).ToList();
            Order ord = new Order();

            var UpdateAddress = new Customer()
            {
                CustomerID = CustomerID,
                Address = Address
            };
            if (ModelState.IsValid)
            {
                using (db)
                {
                    //Insert the values to the Order table
                    ord.CustomerID = CustomerID;
                    ord.TotalOrderPrice = TheTotalPrice;
                    ord.OrderDate = DateTime.Now;
                    db.Orders.Add(ord);
                    db.SaveChanges();

                    //Update the Purchased and OrderID columns in CustomerOrderTable Table by using CustomerID
                    foreach (var item in COT)
                    {
                        item.OrderID = ord.OrderID;
                        item.Purchased = 1;
                        db.SaveChanges();
                        Session["OrderID"] = item.OrderID;
                    }

                    //Update the Customer's address by using CustomerID
                    db.Customers.Attach(UpdateAddress);
                    db.Entry(UpdateAddress).Property(x => x.Address).IsModified = true;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChanges();
                }

            }

            return RedirectToAction("SpecificCustomerOrder", "Customer");
        }

        //Show the customer's list of orders by using CustomerID
        public ActionResult ShowOrders()
        {
            int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
            var item = (from d in db.Orders
                          where CustomerID == d.CustomerID
                          select d).ToList();

            //Call the method (count the cart content)
            TheCart_OnLoad();
            return View(item); 
        }
        //Show the the specific order by selecting from the My Orders list by using CustomerID and OrderID.
        public ActionResult CustomerOrder(int id)
        {
            int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
            ViewData["Orders List"] = db.CustomerOrderDetails_FN(CustomerID, id).ToList();
            //Use FirstOrDefault to show only one order's details
            ViewData["Order"] = db.CustomerOrderDetails_FN(CustomerID, id).FirstOrDefault();
            return View();
        }

        //Show the the final specific order when the customer finished of paying by using CustomerID and OrderID Session.
        public ActionResult SpecificCustomerOrder()
        {
            int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
            int OrderID = Convert.ToInt32(Session["OrderID"]);
            ViewData["Orders List"] = db.CustomerOrderDetails_FN(CustomerID, OrderID).ToList();
            //Use FirstOrDefault to show only one order's details
            ViewData["Order"] = db.CustomerOrderDetails_FN(CustomerID, OrderID).FirstOrDefault();
            return View();
        }

        
    }
}