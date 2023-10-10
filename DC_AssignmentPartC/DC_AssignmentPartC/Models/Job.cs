using Microsoft.Extensions.Primitives;

namespace DC_AssignmentPartC.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string data { get; set; }
        public string result { get; set; }
        public int completedBy { get; set; }
    }
}
