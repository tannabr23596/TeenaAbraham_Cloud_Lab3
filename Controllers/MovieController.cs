using _301222912_abraham_mehta_Lab3.Models;
using _301222912_abraham_mehta_Lab3.Services;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
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
                    //movieComments = comments,
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
        [HttpPost("/filteredMovie")]
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

            return View("ListMovies",model);
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


        [HttpPost("/editMoviePost")]
        public async Task<IActionResult> EditMoviePost(MovieViewModel model,bool replaceVideo)
        {
            MovieListViewModel mView = new MovieListViewModel();
            if (Request.Cookies.TryGetValue("MovieId", out string movieId))
            {
                // Load the existing movie from DynamoDB
                Movie existingMovie = await GetMovieByMovieIdAsync(movieId);

                if (existingMovie == null)
                {
                    return NotFound();
                }


                if (replaceVideo && model.NewMovieVideo != null)
                {
                    // Replace the existing video URL with the new one
                    string newVideoUrl = await s3Service.saveFiles(model.NewMovieVideo);
                    existingMovie.movieVideoURL = newVideoUrl;
                }

                // Update other properties as needed

                if (!string.IsNullOrWhiteSpace(model.MovieTitle))
                {
                    existingMovie.movieTitle = model.MovieTitle;
                }

                if (!string.IsNullOrWhiteSpace(model.MovieGenre))
                {
                    existingMovie.movieGenre = model.MovieGenre;
                }

                if (model.MovieRating.ToString() != null)
                {
                    existingMovie.movieRating = model.MovieRating;
                }

                if (model.MovieDirectors != null && model.MovieDirectors.Any())
                {
                    existingMovie.movieDirectors = model.MovieDirectorsInput.Split(',').Select(d => d.Trim()).ToList();
                }

                
                    existingMovie.movieReleaseTime = model.MovieReleaseTime;
                

                // Save the edited movie back to DynamoDB
                await dBContext.SaveAsync(existingMovie);

                var allMovies = await dBContext.ScanAsync<Movie>(new List<ScanCondition>()).GetRemainingAsync();
                var (distinctGenres, distinctRatings) = await FetchDistinctGenresAndRatingsAsync();

                // Create a model that includes distinct genres, distinct ratings, and the list of all movies
                var modelView = new MovieListViewModel
                {
                    Genres = distinctGenres,
                    Ratings = distinctRatings,
                    Movies = allMovies
                };
                mView = modelView;
            }
                return View("ListMovies", mView);
            

                   
        }


         [HttpGet]
        public async Task<IActionResult> EditMovie(string movieId)
        {
            if (string.IsNullOrEmpty(movieId))
            {
                return BadRequest();
            }

            var movie = await GetMovieByMovieIdAsync(movieId);

            if (movie == null)
            {
                return NotFound();
            }
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1), // Set the expiration date (adjust as needed)
                IsEssential = true // Make the cookie essential for sessions
            };

            Response.Cookies.Append("MovieId", movieId, cookieOptions);

            IFormFile video = await ConvertVideoUrlToIFormFileAsync(movie.movieVideoURL);
            var movieViewModel = new MovieViewModel
            {
                MovieId = movie.movieId,
                MovieTitle = movie.movieTitle,
                MovieGenre = movie.movieGenre,
                MovieRating = movie.movieRating,
                MovieDirectorsInput = string.Join(", ", movie.movieDirectors),
                MovieReleaseTime = movie.movieReleaseTime,
                MovieURL = movie.movieVideoURL
            };

            // You can pass the movie to the view for editing
            return View(movieViewModel);
        }
        private async Task<Movie> GetMovieByMovieIdAsync(string movieId)
        {
            return await dBContext.LoadAsync<Movie>(movieId);
        }
        public async Task<IFormFile> ConvertVideoUrlToIFormFileAsync(string videoUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(videoUrl);

                if (response.IsSuccessStatusCode)
                {
                    Stream videoStream = await response.Content.ReadAsStreamAsync();

                    // Create an IFormFile from the downloaded stream
                    IFormFile videoFile = new FormFile(videoStream, 0, videoStream.Length, "videoFile", Path.GetFileName(videoUrl));

                    return videoFile;
                }

                // Handle the case when the download fails or the URL is invalid
            }

            return null;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMovie(string movieId)
        {
            bool dynamoDeleteStatus;
            // Use the 'movie' object in your controller action
            // You can perform any necessary processing here
            try
            {
                dynamoDeleteStatus = await dynamoServe.deleteMovie(movieId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
        public async Task<IActionResult> GiveComments(string movieId)
        {
            //movieId = HttpContext.Request.Cookies["MovieId"];
            // Create a cookie with the UserId
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1), // Set the expiration date (adjust as needed)
                IsEssential = true // Make the cookie essential for sessions
            };
            string userId = HttpContext.Request.Cookies["userId"];
            Response.Cookies.Append("MovieId", movieId, cookieOptions);

            var movie = await GetMovieByMovieIdAsync(movieId);
            List<MovieComment> comments = movie.movieComments;
                CommentViewModel viewModel = new CommentViewModel
            {
                UserId = userId,
                MovieId = movieId,
                MovieComments = comments
            };

            if (viewModel == null)
            {
                // Handle the case where the movie is not found (e.g., show an error message or redirect).
                return NotFound();
            }

            return View("ListComments", viewModel);
        }
        // POST method to add a comment
        [HttpPost("/addComments")]
        public async Task<IActionResult> AddComment(string comment)
        {
            var movie = new Movie();
            if (HttpContext.Request.Cookies.TryGetValue("userId", out string userIdSaved)
                && HttpContext.Request.Cookies.TryGetValue("MovieId", out string movieIdSaved))
            {
                int seed = GenerateUniqueSeed();  // Implement GenerateUniqueSeed() with your unique logic
                Random random = new Random(seed);
                int randomCommentId = random.Next();
                // Create a new MovieComment object with the comment details
                MovieComment newComment = new MovieComment
                {
                    userId = userIdSaved, // You should replace this with the actual user ID of the commenter
                    comment = comment,
                    commentedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    commentId = randomCommentId.ToString()// Generate a unique comment ID
                };

                // Fetch the movie based on the movie ID
                movie = await GetMovieByMovieIdAsync(movieIdSaved);

                if (movie == null)
                {
                    // Handle the case where the movie is not found (e.g., show an error message or redirect).
                    return NotFound();
                }

                // Add the new comment to the movie's comment list
                if (movie.movieComments == null)
                {
                    List<MovieComment> cmnts = new List<MovieComment>();
                    cmnts.Add(newComment);
                    movie.movieComments = cmnts;

                }
                else
                {
                    movie.movieComments.Add(newComment);
                }
                

                // Save the updated movie back to your data store (e.g., DynamoDB)
                await dBContext.SaveAsync(movie);
            }
            // Redirect back to the "givecomments" view with the updated movie data
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
        [HttpGet]
        public async Task<IActionResult> EditCommentGet(string commentId)
        {
            CommentViewModel viewModelSaved = new CommentViewModel();
            if (string.IsNullOrEmpty(commentId))
            {
                return BadRequest();
            }
            if (HttpContext.Request.Cookies.TryGetValue("userId", out string userIdSaved)
               && HttpContext.Request.Cookies.TryGetValue("MovieId", out string movieIdSaved))
            {
              
                var movie = await GetMovieByMovieIdAsync(movieIdSaved);
                List<MovieComment> comments = movie.movieComments;
                CommentViewModel viewModel = new CommentViewModel
                {
                    UserId = userIdSaved,
                    MovieId = movieIdSaved,
                    IsEditing = true,
                    MovieComments = comments
                };
                viewModelSaved = viewModel;
            }
            // Return the view for editing the comment
            return View("ListComments", viewModelSaved);
        }

       
    }
}
