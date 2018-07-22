using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Kalium.Shared
{
    public class ImageViewModel
    {
        public int Id { get; set; }
        public List<IFormFile> Files { get; set; }
    }
    public class AvatarViewModel
    {
        public string Id { get; set; }
        public List<IFormFile> Files { get; set; }
    }
}
