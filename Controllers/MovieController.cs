using _301222912_abraham_mehta_Lab3.Models;
using _301222912_abraham_mehta_Lab3.Services;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using static System.Formats.Asn1.AsnWriter;

namespace _301222912_abraham_mehta_Lab3.Controllers
{
    public class MovieController : Controller
    {
        public MovieDynamoService dynamoServe = new MovieDynamoService();
        public MovieS3Service s3Service = new MovieS3Service();
        public static DynamoDBContext dBContext = new DynamoDBContext(Helper.dynamoClient);
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(MovieViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (HttpContext.Request.Cookies.TryGetValue("userId", out string userId))
                {

                    String movieUrl = await s3Service.saveFiles(model.MovieVideo);
                    var newMovie = new Movie
                    {
                        movieId = Guid.NewGuid().ToString(),
                        movieTitle = model.MovieTitle,
                        movieGenre = model.MovieGenre,
                        movieRating = model.MovieRating,
                        movieDirectors = model.MovieDirectors,
                        movieReleaseTime = model.MovieReleaseTime,
                        userId = userId, // Add userId from cookies
                        movieVideoURL = movieUrl
                    };
                    await dynamoServe.SaveNewMovie(newMovie);

                }
            }
            return RedirectToAction("ListMovies");
        }
       /* [HttpGet]
        public async Task<IActionResult> ListMoviesGet()
        {
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

            return View(model);
        }*/


        private async Task<(List<string> genres, List<double> ratings)> FetchDistinctGenresAndRatingsAsync()
        {
            var genres = new List<string>();
            var ratings = new List<double>();

            using (var client = new AmazonDynamoDBClient())
            {
                var config = new DynamoDBContextConfig { ConsistentRead = true };
                var context = new DynamoDBContext(client, config);

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
            }

            return (genres, ratings);
        }
        [HttpPost]
        public async Task<IActionResult> ListMovies(MovieListViewModel model)
        {
            // Fetch distinct genres and ratings
            var (distinctGenres, distinctRatings) = await FetchDistinctGenresAndRatingsAsync();

            // Query DynamoDB to fetch all movies
            var allMovies = await dBContext.ScanAsync<Movie>(new List<ScanCondition>()).GetRemainingAsync();

            // Filter movies based on the selected criteria
            var filteredMovies = FilterMovies(allMovies, model.SelectedGenre, model.SelectedRating);

            // Update the model with the filtered movies, distinct genres, and distinct ratings
            model.Movies = filteredMovies;
            model.Genres = distinctGenres;
            model.Ratings = distinctRatings;

            return View(model);
        }
        private List<Movie> FilterMovies(List<Movie> movies, string selectedGenre, double selectedRating)
        {
            if (!string.IsNullOrEmpty(selectedGenre))
            {
                movies = movies.Where(movie => movie.movieGenre == selectedGenre).ToList();
            }

            if (selectedRating > 0)
            {
                movies = movies.Where(movie => movie.movieRating >= selectedRating).ToList();
            }

            return movies;
        }

    }
}
