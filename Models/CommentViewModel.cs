using System.Diagnostics.Eventing.Reader;

namespace _301222912_abraham_mehta_Lab3.Models
{
    public class CommentViewModel
    {
        public string UserId { get; set; }
        public string MovieId { get; set; }

        public bool IsEditing { get; set; }
        public List<MovieComment> MovieComments { get; set; }
    }
}
