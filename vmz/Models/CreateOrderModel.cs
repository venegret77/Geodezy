using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace vmz.Models
{
    public class CreateOrderModel
    {
        public string name { get; set; }
        public string description { get; set; }
        public string priority { get; set; }
        public DateTime datestart { get; set; }
        public string oid { get; set; }
    }
}