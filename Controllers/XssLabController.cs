using Microsoft.AspNetCore.Mvc;
using vulnerablewebpage.Data;
using vulnerablewebpage.Models;

namespace vulnerablewebpage.Controllers
{
    public sealed class XssLabController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public XssLabController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Index([FromQuery] string? reflectedInput = null, [FromQuery] string? domInput = null)
        {
            var viewModel = new XssLabPageViewModel
            {
                ReflectedInput = reflectedInput ?? string.Empty,
                DomInput = domInput ?? string.Empty,
                StoredEntries = _dbContext.XssEntries
                    .OrderByDescending(x => x.Id)
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveStored(XssLabPageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.StoredEntries = _dbContext.XssEntries
                    .OrderByDescending(x => x.Id)
                    .ToList();

                return View("Index", model);
            }

            var entity = new XssEntry
            {
                Title = model.StoredTitle.Trim(),
                Content = model.StoredContent,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.XssEntries.Add(entity);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteStored(int id)
        {
            var entity = _dbContext.XssEntries.FirstOrDefault(x => x.Id == id);
            if (entity is not null)
            {
                _dbContext.XssEntries.Remove(entity);
                _dbContext.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}