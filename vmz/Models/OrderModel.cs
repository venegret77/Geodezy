using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace vmz.Models
{
    public class OrderModel
    {
        public string name { get; set; }
        public string description { get; set; }
        public string action { get; set; }
        public DateTime datestart { get; set; }
        public string tid { get; set; }
        public List<string> slist { get; set; }
        public string oid { get; set; }
    }
}