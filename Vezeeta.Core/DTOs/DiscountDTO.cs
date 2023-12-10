using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vezeeta.Core.Models;

namespace Vezeeta.Core.DTOs
{
    public class DiscountDTO
    {
        public string? DiscountCode { get; set; }
        public int NoOfReq { get; set; }
        public DiscountType DiscountType { get; set; }
        public int Value { get; set; }
    }
}
