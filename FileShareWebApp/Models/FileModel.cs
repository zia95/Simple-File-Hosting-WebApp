using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Compression;
using System.IO;
using System.Linq;
using FileShareWebApp.Controllers;

namespace FileShareWebApp.Models
{
    public class FileModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Path { get; set; }

        public byte[]? FileContent { get; set; }

        public bool FileStoredInDb { get; set; }


        
        [NotMapped]
        public IFormFile? FileData { get; set; }

        public string PathDisplay { get { return Path != null ? (Path.Contains("wwwroot") ? Path.Substring(Path.IndexOf("wwwroot") + 8) : Path) : string.Empty; } }

        public bool CanFileBeStoredInDb { get { return FileData?.Length < 2097152; } }


        public FileModel()
        {

        }

        public bool MoveFileToDb()
        {
            if(CanFileBeStoredInDb)
            {
                using (var memoryStream = new MemoryStream())
                {
                    FileData.CopyTo(memoryStream);

                    FileContent = memoryStream.ToArray();

                    if (!System.IO.Path.HasExtension(Name))
                    {
                        Name = Name + System.IO.Path.GetExtension(FileData.FileName);
                    }
                    Path = "{Database}";
                    return true;
                }
            }

            return false;
        }

        public bool MoveFileToPhysicalStorage(string path)
        {
            if(FileData.Length > 0)
            {
                Path = System.IO.Path.Combine(path, System.IO.Path.GetRandomFileName());
                
                if(!System.IO.Path.HasExtension(Name))
                {
                    string ext = System.IO.Path.GetExtension(FileData.FileName);
                    Name = Name + ext;
                }
                
                using (var stream = System.IO.File.Create(Path))
                {
                    FileData.CopyTo(stream);
                }
                return true;
            }
            return false;
        }

    }
}
