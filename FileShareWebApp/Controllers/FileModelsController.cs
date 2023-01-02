using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FileShareWebApp.Data;
using FileShareWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.AspNetCore;
using System.Linq;

namespace FileShareWebApp.Controllers
{
    public class FileModelsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webenv;
        private readonly string _physical_storage_path;

        public FileModelsController(IWebHostEnvironment webhostenv, ApplicationDbContext context)
        {
            _webenv = webhostenv;
            _context = context;
            _physical_storage_path = System.IO.Path.Combine(_webenv.WebRootPath, "uploads");
        }


        // GET: FileModels
        public async Task<IActionResult> Index()
        {
            //check if some physical files are not in sync with db
            bool synced = false;
            Array.ForEach(
                System.IO.Directory.GetFiles(_physical_storage_path),
                e =>
                {
                    if(!_context.FileModel.Any( fm => !fm.FileStoredInDb && fm.Path == e))
                    {
                        _context.FileModel.Add(new FileModel() { Name = $"synced_file_{System.IO.Path.GetFileName(e)}", FileStoredInDb = false, Path = e });
                        synced = true;
                    }

                });
            if(synced)
                await _context.SaveChangesAsync();


            return View(await _context.FileModel.ToListAsync());
        }

        // GET: FileModels/SearchFiles
        public IActionResult SearchFiles()
        {
            return View();
        }

        // POST: FileModels/SearchResult
        public async Task<IActionResult> SearchResult(string fileName)
        {
            return View("Index", await _context.FileModel.Where(e => e.Name.Contains(fileName)).ToListAsync());
        }

        // GET: FileModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.FileModel == null)
            {
                return NotFound();
            }

            var fileModel = await _context.FileModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fileModel == null)
            {
                return NotFound();
            }

            return View(fileModel);
        }

        // GET: FileModels/Download/5
        [HttpGet]
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null || _context.FileModel == null)
            {
                return NotFound();
            }

            var fileModel = await _context.FileModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fileModel == null)
            {
                return NotFound();
            }
            if(fileModel.FileStoredInDb)
            {
                if(fileModel.FileContent != null)
                {
                    return File(fileModel.FileContent, "application/force-download", fileModel.Name);
                }
            }
            else
            {
                if(System.IO.File.Exists(fileModel.Path))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileModel.Path);

                    return File(fileBytes, "application/force-download", fileModel.Name);
                }
            }
            ModelState.AddModelError("File", "File not found.");
            return View("Error", new ErrorViewModel() { RequestId = "File not found." });
        }

        // GET: FileModels/Create
        //[Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: FileModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Path,FileContent,FileStoredInDb,FileData")] FileModel fileModel)
        {
            if (ModelState.IsValid)
            {
                if(fileModel.FileData != null)
                {
                    if (fileModel.FileStoredInDb && fileModel.CanFileBeStoredInDb)
                    {
                        fileModel.MoveFileToDb();

                        _context.Add(fileModel);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    else if (!fileModel.FileStoredInDb)
                    {
                        fileModel.MoveFileToPhysicalStorage(_physical_storage_path);

                        _context.Add(fileModel);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("File", "The file is too large.");
                    }
                }
                else
                {
                    ModelState.AddModelError("File", "Select a file to upload.");
                }
            }
            return View(fileModel);
        }

        // GET: FileModels/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.FileModel == null)
            {
                return NotFound();
            }

            var fileModel = await _context.FileModel.FindAsync(id);
            if (fileModel == null)
            {
                return NotFound();
            }
            return View(fileModel);
        }

        // POST: FileModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Path,FileContent,FileStoredInDb,FileData")] FileModel fileModel)
        {
            if (id != fileModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var m = _context.FileModel.First(e => e.Id == id);
                    m.Name = fileModel.Name;
                    //fileModel.Path = m.Path;
                    //fileModel.FileStoredInDb = m.FileStoredInDb;

                    //_context.Update(fileModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FileModelExists(fileModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(fileModel);
        }

        // GET: FileModels/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.FileModel == null)
            {
                return NotFound();
            }

            var fileModel = await _context.FileModel
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (fileModel == null)
            {
                return NotFound();
            }

            return View(fileModel);
        }

        // POST: FileModels/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.FileModel == null)
            {
                return Problem("Entity set 'ApplicationDbContext.FileModel'  is null.");
            }
            var fileModel = await _context.FileModel.FindAsync(id);
            if (fileModel != null)
            {
                //delete physical file....
                if (fileModel.FileStoredInDb == false && System.IO.File.Exists(fileModel.Path))
                    System.IO.File.Delete(fileModel.Path);
                    
                //remove db entry
                _context.FileModel.Remove(fileModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FileModelExists(int id)
        {
          return _context.FileModel.Any(e => e.Id == id);
        }
    }
}
