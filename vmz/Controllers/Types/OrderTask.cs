using System.Collections.Generic;
using vmz.Models;

namespace vmz.Controllers
{
    public struct OrderTask
    {
        public OrderTask(Order order)
        {
            this.Order = order;
            Tasks = new List<Task>();
        }

        public Order Order { get; set; }
        public List<Task> Tasks { get; set; }
    }
}