using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _301222912_abraham_mehta_Lab3.Models;
using Amazon.DynamoDBv2.DataModel;
using Polly;
using Amazon.DynamoDBv2;

namespace _301222912_abraham_mehta_Lab3.Controllers
{
    public class UsersController : Controller
    {
        private readonly MovieDbContext _context;
        public static DynamoDBContext dBContext = new DynamoDBContext(Helper.dynamoClient);
        public UsersController(MovieDbContext context)
        {
            _context = context;
        }

        

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.User == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Email,Password,ConfirmPassword,FirstName,LastName")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.User == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Email,Password,ConfirmPassword,FirstName,LastName")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
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
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.User == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.User == null)
            {
                return Problem("Entity set 'MovieDbContext.User'  is null.");
            }
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return (_context.User?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View("Register");
        }
        [HttpPost]
        public IActionResult Register(User user)
        {

            if (ModelState.IsValid)
            {
                _context.User.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Index", "Home");  // Redirect to login page or another appropriate action.
            }
            else
            {
                
                    return View();
                
            }
               // Return the registration view with validation errors.
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Query the database to find a user with the provided username
            var user = await Task.Run(() => _context.User.FirstOrDefault(u => u.Email == username));

            if (user != null && user.Password == password)
            {
                // Create a cookie with the UserId
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1), // Set the expiration date (adjust as needed)
                    IsEssential = true // Make the cookie essential for sessions
                };

                Response.Cookies.Append("UserId", user.UserId.ToString(), cookieOptions);
                Response.Cookies.Append("Firstname", user.FirstName, cookieOptions);

                // Fetch distinct genres and ratings
                var (distinctGenres, distinctRatings) = await FetchDistinctGenresAndRatingsAsync();

                // Query DynamoDB to fetch all movies
                var allMovies = await dBContext.ScanAsync<Movie>(new List<ScanCondition>()).GetRemainingAsync();

                // Create a model that includes distinct genres, distinct ratings, and the list of all movies
                var model = new MovieListViewModel
                {
                    Genres = distinctGenres,
                    Ratings = distinctRatings,
                    Movies = allMovies
                };

                return View ("ListMovies", model);
                // Redirect to a protected area upon successful login.
            }

            // Authentication failed; show an error message.
            ViewBag.ErrorMessage = "Invalid username or password";
            return RedirectToAction("Index", "Home");  // Return the login view with an error message.
        }
        private async Task<(List<string> genres, List<double> ratings)> FetchDistinctGenresAndRatingsAsync()
        {
            var genres = new List<string>();
            var ratings = new List<double>();

            
                var config = new DynamoDBContextConfig { ConsistentRead = true };
                var context = new DynamoDBContext(Helper.dynamoClient, config);

                var scanConditions = new List<ScanCondition>();
                var search = context.ScanAsync<Movie>(scanConditions);

                List<Movie> movies = new List<Movie>();
                do
                {
                    var searchResponse = await search.GetNextSetAsync();
                    movies.AddRange(searchResponse);
                } while (!search.IsDone);

                genres = movies.Select(movie => movie.movieGenre).Distinct().ToList();
                ratings = movies.Select(movie => movie.movieRating).Distinct().ToList();
            

            return (genres, ratings);
        }
    }
}
