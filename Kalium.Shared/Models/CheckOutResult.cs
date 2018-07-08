using System;
using System.Collections.Generic;
using System.Text;

namespace Kalium.Shared.Models
{
    public class CheckOutResult
    {
        public ICollection<string> Messages { get; set; }
        public int OrderId { get; set; }
        public bool Succeeded { get; set; }
        public ECart ECart { get; set; }
    }
}
