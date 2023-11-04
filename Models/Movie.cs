using Amazon.DynamoDBv2.DataModel;

namespace _301222912_abraham_mehta_Lab3.Models
{
    [DynamoDBTable("Movies")]
    public class Movie
    {
        [DynamoDBHashKey("movieId")] // Partition key
        public string movieId { get; set; }

        [DynamoDBProperty("movieTitle")] 
        public string movieTitle { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("movieGenre-index")] // Secondary index on movieGenre
        public string movieGenre { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("movieRating-index")] // Secondary index on movieRating
        public double movieRating { get; set; }

        [DynamoDBProperty("movieComments")]
        public List<MovieComment> movieComments { get; set; }

        [DynamoDBProperty("movieDirectors")]
        public List<string> movieDirectors { get; set; }

        [DynamoDBProperty("movieReleaseTime")]
        public string movieReleaseTime { get; set; }

        [DynamoDBProperty("userId")]
        public string userId { get; set; }

        [DynamoDBProperty("movieVideoURL")]
        public string movieVideoURL { get; set; }
    }

    public class MovieComment
    {
        public string userId { get; set; }
        public string comment { get; set; }
        public string commentedTime { get; set; }
    }
}
