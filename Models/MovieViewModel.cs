﻿using System.Diagnostics.Eventing.Reader;

namespace _301222912_abraham_mehta_Lab3.Models
{
    public class MovieViewModel
    {
        public String MovieId { get; set; }
        public string MovieTitle { get; set; }
        public string MovieGenre { get; set; }
        public double MovieRating { get; set; }
        public List<string> MovieDirectors { get; set; }

        public string MovieDirectorsInput { get; set; }
        public string MovieReleaseTime { get; set; }
        public IFormFile MovieVideo { get; set; } 

        public IFormFile NewMovieVideo { get; set; }

        public string MovieURL {  get; set; }
        public bool ReplaceVideo { get; set; }
    }
}
