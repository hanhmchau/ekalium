using System;
using System.Collections.Generic;
using System.Text;
using Kalium.Shared.Models;

namespace Kalium.Shared.Front
{
    public class CouponData
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int ProductId { get; set; }
        public int Type { get; set; }
        public int Quantity { get; set; }
        public DateTime? DateExpired { get; set; }
        public double Reduction { get; set; }
        public bool IsValid =>
            Type == (int)Consts.Consts.CouponType.Date ? DateTime.Now < DateExpired : Quantity > 0;
        public bool Deleted { get; set; }
    }
}
