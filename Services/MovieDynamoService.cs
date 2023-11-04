using _301222912_abraham_mehta_Lab3.Models;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.EntityFrameworkCore;

namespace _301222912_abraham_mehta_Lab3.Services
{
    public class MovieDynamoService
    {
        public static DynamoDBContext dBContext;
        private static Movie  retrievedItem;
        public async Task<List<Movie>> GetMovieList(int userId)
        {
            List<Movie> movieList = new List<Movie>();
            dBContext = new DynamoDBContext(Helper.dynamoClient);

            // Assuming userId is a partition key
            var config = new DynamoDBOperationConfig
            {
                QueryFilter = new List<ScanCondition>
        {
            new ScanCondition("UserId", ScanOperator.Equal, userId)
        }
            };

            try
            {
                var search = dBContext.QueryAsync<Movie>(userId, config);
                movieList = await search.GetRemainingAsync();

                if (movieList.Count > 0)
                {
                    Console.WriteLine("Movies found.");
                }
                else
                {
                    Console.WriteLine("No movies found for this user.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error retrieving movies: " + e.Message);
            }

            return movieList;
        }
        public async Task SaveNewMovie(Movie movie)
        {
            // Save the data to DynamoDB
            await dBContext.SaveAsync(movie);
        }

        public async Task<(List<string> genres, List<double> ratings)> FetchDistinctGenresAndRatings()
        {
            var scanConfig = new ScanOperationConfig();
            var movies = await dBContext.FromScanAsync<Movie>(scanConfig).GetRemainingAsync();

            var distinctGenres = movies.Select(m => m.movieGenre).Distinct().ToList();
            var distinctRatings = movies.Select(m => m.movieRating).Distinct().ToList();

            return (distinctGenres, distinctRatings);
        }
    }
}
