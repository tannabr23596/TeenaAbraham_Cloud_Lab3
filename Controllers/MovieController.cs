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
        
        
        [HttpGet("/addMovie")]
        public IActionResult AddMovie()
        {
            return View();
        }

        [HttpPost("/addMoviePost")]
        public async Task<IActionResult> AddMovie(MovieViewModel model)
        {
           
                if (HttpContext.Request.Cookies.TryGetValue("userId", out string userId))
                {

                    String movieUrl = await s3Service.saveFiles(model.MovieVideo);
                    if (!string.IsNullOrEmpty(model.MovieDirectorsInput))
                    {
                        model.MovieDirectors = model.MovieDirectorsInput.Split(',').Select(d => d.Trim()).ToList();
                    }
                MovieComment movieComment = new MovieComment();
                List<MovieComment> comments = new List<MovieComment>();
                comments.Add(movieComment);
                int seed = GenerateUniqueSeed();  // Implement GenerateUniqueSeed() with your unique logic
                Random random = new Random(seed);
                int randomMovieId = random.Next();
                var newMovie = new Movie
                {
                    movieId = randomMovieId.ToString(),
                    movieTitle = model.MovieTitle,
                    movieGenre = model.MovieGenre,
                    movieRating = model.MovieRating,
                    movieDirectors = model.MovieDirectors,
                    movieReleaseTime = model.MovieReleaseTime,
                    userId = userId, // Add userId from cookies
                    movieVideoURL = movieUrl,
                    movieComments = comments
                };
                    await dynamoServe.SaveNewMovie(newMovie);

                }
            var allMovies = await dBContext.ScanAsync<Movie>(new List<ScanCondition>()).GetRemainingAsync();
            var (distinctGenres, distinctRatings) = await FetchDistinctGenresAndRatingsAsync();

            // Create a model that includes distinct genres, distinct ratings, and the list of all movies
            var modelView = new MovieListViewModel
            {
                Genres = distinctGenres,
                Ratings = distinctRatings,
                Movies = allMovies
            };

            return View("ListMovies", modelView);
        }
        private int GenerateUniqueSeed()
        {
            // Generate a seed based on the current timestamp and an application-specific identifier (e.g., your application's assembly name).
            string applicationIdentifier = System.Reflection.Assembly.GetEntryAssembly().GetName().Name; // Get your application's assembly name

            // Combine the current timestamp with the application identifier
            string seedString = DateTime.Now.Ticks + applicationIdentifier;

            // Use a hash function to generate a hash code from the combined string
            int hashCode = seedString.GetHashCode();

            return hashCode;
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
