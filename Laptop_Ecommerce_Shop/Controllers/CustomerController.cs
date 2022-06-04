using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laptop_Ecommerce_Shop.Controllers
{
    public class CustomerController : Controller
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
        public ActionResult CustomerCart()
        {
            //Check if the cookie exists to show the cart content
            if (Request.Cookies["CustomerID"] == null)
            {
                try{
                    TempData["LoginMsg"] = "<script>divModal.style.display = 'block';</script>";
                    //if there is no cookie, the customer will stay in same view
                    return Redirect(Request.UrlReferrer.ToString());
                }
                catch (Exception)
                {
                    return RedirectToAction("Index", "Home");
                }
                               
            }
            else
            {
                int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
                //Call the method (count the cart content)
                TheCart_OnLoad();

                //Calculate the total price of all products that are added to the cart.
                var TotalCartPrice = db.CustomerCartDatails_FN(CustomerID).Sum(x => x.TotalPrice);
                //Check if the cart is empty
                if (TotalCartPrice == 0)
                {
                    TempData["EmptyCart"] = "<script> divEmptyCart.style.display = 'block'</script>";
                }
                else
                {
                    //Save the result in TempData.
                    TempData["TotalCartPrice"] = Convert.ToDouble(TotalCartPrice).ToString("N");
                }               
                //Get the Customer Email and show it in ckeckout view
                var CustomerEmail = (from d in db.Customers
                                     where CustomerID == d.CustomerID
                                     select d.Email).FirstOrDefault();
                Session["CustomerEmail"] = CustomerEmail;

                // show the CustomerCartDatails_FN procedure to displays the cart contents
                return View(db.CustomerCartDatails_FN(CustomerID).ToList());
            }

        }

        //Delete specific item from the cart
        public ActionResult DeleteItem(int id)
        {
            CustomerOrderTable CCID = db.CustomerOrderTables.Find(id);
            db.CustomerOrderTables.Remove(CCID);
            db.SaveChanges();
            return RedirectToAction("CustomerCart");

        }
        //Function to increase the selected product quantity
        public ActionResult IncreaseQuantity(int id)
        {
            //Update the Quantity in CustomerOrderTable Table by using CustomerOrderID and Purchased
            CustomerOrderTable COT = db.CustomerOrderTables.Where(u => u.CustomerOrderID == id && u.Purchased == 0).FirstOrDefault();

            if (ModelState.IsValid)
            {
                using (db)
                {
                    //Update the Quantity by Increasing the value in every click on the plus icon
                    COT.Quantity += 1;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("CustomerCart");
        }
        //Function to decreasee the selected product quantity
        public ActionResult DecreaseQuantity(int id)
        {
            CustomerOrderTable COT = db.CustomerOrderTables.Where(u => u.CustomerOrderID == id && u.Purchased == 0).FirstOrDefault();

            if (ModelState.IsValid)
            {
                using (db)
                {
                    //Check if the value is less than 1 before decreasing
                    if (COT.Quantity > 1)
                    {
                        //Update the Quantity by decreasing the value in every click on the minus icon.
                        COT.Quantity -= 1;
                        db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("CustomerCart");
        }

        public ActionResult ProductDetails(int? id)
        {
            ProductItem PID = db.ProductItems.Find(id);
            var selectedProduct = db.ProductItems.Where(model => model.ProductID.Equals(PID.ProductID)).FirstOrDefault();
            //Show product images
            ViewData["Product images"] = db.ProductFiles.Where(model => model.FileType == "image/jpeg" && model.ProductID == PID.ProductID).ToList();
            //Show product video
            ViewData["Product videos"] = db.ProductFiles.Where(model => model.FileType == "video/mp4" && model.ProductID == PID.ProductID).ToList();
            //Default quantity value
            TempData["CustomerProductQuantity"] = 1;
            //Calculate the total price per each item.
            double TotalPricePerItem = Convert.ToDouble(PID.Price * Convert.ToDouble(TempData["CustomerProductQuantity"]));
            TempData["TotalPricePerItem"] = TotalPricePerItem.ToString("N");

            if (Request.Cookies["CustomerID"] != null)
            {
                int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
                
                var CustomerOrderID = (from d in db.CustomerOrderTables
                                       where CustomerID == d.CustomerID && selectedProduct.ProductID == d.ProductID && d.Purchased == 0
                                       select d.CustomerOrderID).FirstOrDefault();
                TempData["CustomerOrderID"] = CustomerOrderID;
                //Get the Customer Product Quantity and Total Price Per Item
                var CustomerProductQuantity = (from d in db.CustomerOrderTables
                                               where CustomerID == d.CustomerID && selectedProduct.ProductID == d.ProductID && d.Purchased == 0
                                               select d.Quantity).FirstOrDefault();
                TempData["CustomerProductQuantity"] = CustomerProductQuantity;
                //Calculate the total price per each item for customer
                double CusTotalPricePerItem = Convert.ToDouble(PID.Price * CustomerProductQuantity);
                TempData["TotalPricePerItem"] = CusTotalPricePerItem.ToString("N");

                if (CustomerProductQuantity == null)
                {
                    TempData["CustomerProductQuantity"] = 1;
                    TempData["TotalPricePerItem"] = TotalPricePerItem.ToString("N");
                }               
                ViewData["Cart Info"] = db.CustomerOrderTables.Where(model => model.CustomerID.Equals(CustomerID) && selectedProduct.ProductID == PID.ProductID && model.Purchased == 0).FirstOrDefault();
                //Call the method (count the cart content)
                TheCart_OnLoad();
            }
            return View(selectedProduct);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductDetails(int Quantity, int id)
        {
            ProductItem PID = db.ProductItems.Find(id);
            if (Request.Cookies["CustomerID"] == null)
            {
                TempData["LoginMsg"] = "<script>divModal.style.display = 'block';</script>";
                
            }
            else
            {
                int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
               
                //Update the item's quantity by using CustomerOrderID 
                var UpdateQuantity = new CustomerOrderTable()
                {
                    CustomerOrderID = Convert.ToInt32(TempData["CustomerOrderID"]),
                    Quantity = Quantity
                };
                var addedToCart = db.CustomerOrderTables.AsNoTracking().Where(model => model.ProductID.Equals(PID.ProductID) && model.CustomerID.Equals(CustomerID) && model.Purchased == 0).FirstOrDefault();

                if (ModelState.IsValid)
                {
                    using (db)
                    {
                        if (addedToCart != null)
                        {
                            db.CustomerOrderTables.Attach(UpdateQuantity);
                            db.Entry(UpdateQuantity).Property(x => x.Quantity).IsModified = true;
                            db.Configuration.ValidateOnSaveEnabled = false;
                            db.SaveChanges();
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
            return RedirectToAction("ProductDetails");
        }
    
        public ActionResult Payment(string Address)
        {          
            int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
            //Calculate the total price of all products that are added to the cart.
            var TotalOrderPrice = db.CustomerCartDatails_FN(CustomerID).Sum(x => x.TotalPrice);
            //double TotalCartPrice = Convert.ToDouble(TempData["TotalCartPrice"]);
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
                    ord.TotalOrderPrice = TotalOrderPrice;
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
            //Check if the cookie exists
            if (Request.Cookies["CustomerID"] == null)
            {               
                //if there is no cookie, the customer will stay in the Index
                return RedirectToAction("Index", "Home");
            }
            else { 
            int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
            var item = (from d in db.Orders
                          where CustomerID == d.CustomerID
                          select d).ToList();

            //Call the method (count the cart content)
            TheCart_OnLoad();
            ViewBag.Current = "ShowOrders";
            return View(item);
            }
        }
        //Show the the specific order by selecting from the My Orders list by using CustomerID and OrderID.
        public ActionResult CustomerOrder(int id)
        {
            //Check if the cookie exists
            if (Request.Cookies["CustomerID"] == null)
            {
                //if there is no cookie, the customer will stay in the Index
                return RedirectToAction("Index", "Home");
            }
            else
            {
                int CustomerID = Convert.ToInt32(Request.Cookies["CustomerID"].Value);
                ViewData["Orders List"] = db.CustomerOrderDetails_FN(CustomerID, id).ToList();
                //Use FirstOrDefault to show only one order's details
                ViewData["Order"] = db.CustomerOrderDetails_FN(CustomerID, id).FirstOrDefault();
                return View();
            }
        }

        //Show the the final specific order when the customer finished of paying by using CustomerID and OrderID Session.
        public ActionResult SpecificCustomerOrder()
        {
            //Check if the cookie exists
            if (Request.Cookies["CustomerID"] == null)
            {
                //if there is no cookie, the customer will stay in the Index
                return RedirectToAction("Index", "Home");
            }
            else
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
}