using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Laptop_Ecommerce_Shop.Models
{
    public class MultiModels
    {
        public ProductItem ProductItem { get; set; }
        public CustomerOrderTable CustomerOrderTable { get; set; }
    }
}

