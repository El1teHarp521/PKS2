using System.Collections.Generic;

namespace TouristGuide.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public int Population { get; set; }
        public string History { get; set; } = string.Empty;
        
        public string CoatOfArmsUrl { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;

        public virtual ICollection<Attraction> Attractions { get; set; } = new List<Attraction>();
    }
}