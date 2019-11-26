using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookShelf.Data;
using BookShelf.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BookShelf.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthorsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        [Authorize]
        // GET: Authors
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();

            var authors = _context.Authors.Where(a => a.ApplicationUserId == user.Id).Include(a => a.ApplicationUser);
            return View(await authors.ToListAsync());
        }

            //var applicationDbContext = _context.Books.Include(b => b.ApplicationUser).Include(b => b.Author);
            //return View(await applicationDbContext.ToListAsync());

        // GET: Authors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var user = await GetCurrentUserAsync();

            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .Where(a => a.ApplicationUserId == user.Id)
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // GET: Authors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Authors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,PenName,PreferredGenre,ApplicationUserId")] Author author)
        {
            var user = await GetCurrentUserAsync();

            if (ModelState.IsValid)
            {
                author.ApplicationUserId = user.Id;
                _context.Add(author);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Authors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var user = await GetCurrentUserAsync();

            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .Where(a => a.ApplicationUserId == user.Id)
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // POST: Authors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,PenName,PreferredGenre,ApplicationUserId")] Author author)
        {
            var user = await GetCurrentUserAsync();

            if (id != author.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    author.ApplicationUserId = user.Id;
                    _context.Update(author);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.Id))
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
            //ViewData["ApplicationUserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", author.ApplicationUserId);
            return View(author);
        }

        // GET: Authors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var user = await GetCurrentUserAsync();

            if (id == null) return NotFound();

            var author = await _context.Authors
                .Where(a => a.ApplicationUserId == user.Id)
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (author == null) return NotFound();

            return View(author);
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await GetCurrentUserAsync();
            var author = await _context.Authors.FindAsync(id);

            // Check to see if the user has any books under the author
            var books = await _context.Books.Where(b => b.AuthorId == author.Id).ToListAsync();

            if (books.Count() == 0)
            {
                // No books under the author, so the author can be deleted
                _context.Authors.Remove(author);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewData["ErrorMsg"] = "This author cannot be deleted because they have books in the database.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            
        }

        private bool AuthorExists(int id)
        {
            return _context.Authors.Any(e => e.Id == id);
        }
    }
}
