using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookShelf.Data;
using BookShelf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BookShelf.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        [Authorize]
        // GET: Books
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            var books = _context.Books.Where(b => b.ApplicationUserId == user.Id)
                .Include(b => b.ApplicationUser)
                .Include(b => b.Author);

            return View(await books.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var user = await GetCurrentUserAsync();

            if (id == null) return NotFound();

            var book = await _context.Books
                .Where(b => b.ApplicationUserId == user.Id)
                .Include(b => b.ApplicationUser)
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // GET: Books/Create
        public async Task<IActionResult> Create()
        {
            var user = await GetCurrentUserAsync();
            var usersAuthors = _context.Authors.Where(a => a.ApplicationUserId == user.Id);
            ViewData["Authors"] = new SelectList(usersAuthors, "Id", "FullName");

            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ISBN,Title,Genre,PublishDate,AuthorId,ApplicationUserId")] Book book)
        {
            var user = await GetCurrentUserAsync();
            // Get the current user's authors
            var usersAuthors = _context.Authors.Where(a => a.ApplicationUserId == user.Id);

            if (ModelState.IsValid)
            {
                book.ApplicationUserId = user.Id;
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Authors"] = new SelectList(usersAuthors, "Id", "FullName", book.AuthorId);
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var user = await GetCurrentUserAsync();

            var usersAuthors = _context.Authors.Where(a => a.ApplicationUserId == user.Id);

            if (id == null) return NotFound();

            //var book = await _context.Books.FindAsync(id);
            var book = await _context.Books.Where(b => b.ApplicationUserId == user.Id)
                .Include(b => b.ApplicationUser)
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            ViewData["Authors"] = new SelectList(usersAuthors, "Id", "FullName", book.AuthorId);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ISBN,Title,Genre,PublishDate,AuthorId,ApplicationUserId")] Book book)
        {
            var user = await GetCurrentUserAsync();
            var usersAuthors = _context.Authors.Where(a => a.ApplicationUserId == user.Id);

            if (id != book.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    book.ApplicationUserId = user.Id;
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
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
            //ViewData["ApplicationUserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", book.ApplicationUserId);
            ViewData["Authors"] = new SelectList(usersAuthors, "Id", "FullName", book.AuthorId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var user = await GetCurrentUserAsync();

            if (id == null) return NotFound();

            var book = await _context.Books
                .Where(b => b.ApplicationUserId == user.Id)
                .Include(b => b.ApplicationUser)
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
