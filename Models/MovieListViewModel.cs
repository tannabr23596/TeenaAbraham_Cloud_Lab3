namespace _301222912_abraham_mehta_Lab3.Models
{
    public class MoviewListViewModel
    {
        public List<Movie> Movies { get; set; } // List of movies
        public List<string> Genres { get; set; } // List of distinct genres
        public List<double> Ratings { get; set; } // List of distinct ratings
        public string SelectedGenre { get; set; } // Selected genre for filtering
        public double SelectedRating { get; set; } // Selected rating for filtering
    }
}
}
