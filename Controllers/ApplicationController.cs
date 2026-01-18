using JobApplicationTracker.Data;
using JobApplicationTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace JobApplicationTracker.Controllers;

public class ApplicationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ApplicationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string searchTerm, string filterStatus, int page = 1)
    {
        ViewData["CurrentSearch"] = searchTerm;
        ViewData["CurrentFilter"] = filterStatus;

        var applications = _context.Applications.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            applications = applications.Where(a =>
                a.CompanyName.Contains(searchTerm) ||
                a.Position.Contains(searchTerm) ||
                a.Location.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(filterStatus))
        {
            var status = Enum.Parse<ApplicationStatus>(filterStatus);
            applications = applications.Where(a => a.Status == status);
        }

        applications = applications.OrderByDescending(a => a.ApplicationDate);

        var pageSize = 10;
        var totalItems = await applications.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;

        var paginatedApps = await applications
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(paginatedApps);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var application = await _context.Applications.FirstOrDefaultAsync(m => m.Id == id);
        if (application == null) return NotFound();

        return View(application);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CompanyName,Position,Location,Salary,ApplicationDate,Status,Notes,ContactPerson,ContactEmail,ContactPhone")] Application application)
    {
        if (ModelState.IsValid)
        {
            application.CreatedAt = DateTime.Now;
            _context.Add(application);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Başvuru başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(application);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var application = await _context.Applications.FindAsync(id);
        if (application == null) return NotFound();

        return View(application);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName,Position,Location,Salary,ApplicationDate,Status,Notes,ContactPerson,ContactEmail,ContactPhone")] Application application)
    {
        if (id != application.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                application.UpdatedAt = DateTime.Now;
                _context.Update(application);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Başvuru başarıyla güncellendi!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationExists(application.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(application);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var application = await _context.Applications.FirstOrDefaultAsync(m => m.Id == id);
        if (application == null) return NotFound();

        return View(application);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application != null)
        {
            application.IsDeleted = true;
            application.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Başvuru başarıyla silindi!";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportCSV()
    {
        var applications = await _context.Applications
            .OrderByDescending(a => a.ApplicationDate)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Firma,Pozisyon,Konum,Maaş,Başvuru Tarihi,Durum,İlgili Kişi,E-posta,Telefon,Notlar");

        foreach (var app in applications)
        {
            csv.AppendLine($"\"{app.CompanyName}\",\"{app.Position}\",\"{app.Location}\",\"{app.Salary}\",\"{app.ApplicationDate:yyyy-MM-dd}\",\"{app.Status}\",\"{app.ContactPerson}\",\"{app.ContactEmail}\",\"{app.ContactPhone}\",\"{app.Notes}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"basvurular_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    public async Task<IActionResult> GetStats()
    {
        var stats = new
        {
            Total = await _context.Applications.CountAsync(),
            Applied = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Başvuruldu),
            Interview = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Mülakat),
            Rejected = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Red),
            Accepted = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Kabul)
        };

        return Json(stats);
    }

    private bool ApplicationExists(int id)
    {
        return _context.Applications.Any(e => e.Id == id);
    }
}