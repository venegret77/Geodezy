using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace vmz.Models
{
    public class BrigadeModel
    {
        public string action { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string servicen { get; set; }
        public string serviced { get; set; }
        public List<string> ulist { get; set; }
        public List<string> slist { get; set; }
        public string tid { get; set; }
    }
}