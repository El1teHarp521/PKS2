using System;
using System.Collections.Generic;

namespace LibraryProject.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Country { get; set; }

        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
        public string FullName => $"{FirstName} {LastName}";
    }
}