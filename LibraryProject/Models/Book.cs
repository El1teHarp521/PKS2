using System.Collections.Generic;
using System.Linq;

namespace LibraryProject.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int PublishYear { get; set; }
        public string ISBN { get; set; }
        public int QuantityInStock { get; set; }
        public virtual ICollection<Author> Authors { get; set; } = new List<Author>();
        public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();

        public string AuthorsDisplay => string.Join(", ", Authors.Select(a => a.FullName));
        public string GenresDisplay => string.Join(", ", Genres.Select(g => g.Name));
    }
}