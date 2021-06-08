using System;

namespace reto.Models
{
    public class Attempts
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string ExamName { get; set; }
        public string ExamID { get; set; }
        public int Score { get; set; }
        public int Attempt { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int V { get; set; }
    }
}
