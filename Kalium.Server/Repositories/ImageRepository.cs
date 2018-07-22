using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Kalium.Server.Repositories
{
    public interface IImageRepository
    {
        ICollection<Image> Save(List<IFormFile> files, string folder);
    }

    public class ImageRepository : IImageRepository
    {
        private readonly IHostingEnvironment _environment;

        public ImageRepository(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public ICollection<Image> Save(List<IFormFile> files, string folder)
        {
            var images = new List<Image>();
            if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
            {
                _environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }
            var webRootPath = _environment.WebRootPath;
            string newPath = Path.Combine(webRootPath, folder);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            foreach (IFormFile item in files)
            {
                if (item.Length > 0)
                {
                    string extension = Path.GetExtension(item.FileName);
                    string fileName = Guid.NewGuid() + extension;
                    string relativePath = Path.Combine(folder, fileName);
                    string fullPath = Path.Combine(webRootPath, relativePath);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        item.CopyTo(stream);
                    }
                    images.Add(new Image
                    {
                        Url = relativePath
                    });
                }
            }

            return images;
        }
    }
}
