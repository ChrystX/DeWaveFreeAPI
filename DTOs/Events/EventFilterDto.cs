namespace DeWaveFreeAPI.DTOs.Events
{
    public class EventFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? EventTypes { get; set; }
        public List<string>? Visibilities { get; set; }
        public List<int>? CourseIds { get; set; }
        public bool? OnlyUpcoming { get; set; }
        public bool? OnlyRegistered { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
