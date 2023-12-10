using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vezeeta.Core.Models
{
    public enum DiscountType { percentage, value }
    public enum DiscountActivity { active, deactive }
    public class Discount
    {
        public int DiscountID { get; set; }
        public string? discountName { get; set; }
        public DiscountType DiscountType { get; set; }
        public int NumOfRequests { get; set; }
        public int ValueOfDiscount { get; set; }
        public DiscountActivity DiscountActivity { get; set; }

        public List<Booking>? Bookings { get; set; }
    }
}
