using System.Collections.Generic;

namespace Task.Data
{
    public class Statistic
    {
        public Customer Customer { get; set; }
        public IEnumerable<Order> Orders { get; set; }
    }
}
