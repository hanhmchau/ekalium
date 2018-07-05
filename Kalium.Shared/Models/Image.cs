using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Kalium.Shared.Models
{
    public class Image
    {
        [Key]
        public string Url { get; set; }
    }
}
