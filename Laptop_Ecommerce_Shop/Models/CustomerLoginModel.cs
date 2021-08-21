using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Laptop_Ecommerce_Shop.Models
{
    public class CustomerLoginModel
    {
        public int CustomerID { get; set; }
        [Required(ErrorMessage = "Email Required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password Required")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }


    }
}

