namespace TouristGuide.Models
{
    public class Attraction
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string History { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public string WorkingHours { get; set; } = string.Empty;
        public string TicketPrice { get; set; } = string.Empty;

        public int CityId { get; set; }
        public virtual City? City { get; set; }
    }
}