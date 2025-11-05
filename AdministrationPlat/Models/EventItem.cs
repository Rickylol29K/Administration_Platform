using System;
using System.ComponentModel.DataAnnotations;

namespace AdministrationPlat.Models
{
    public class EventItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Time { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int UserId { get; set; } // 👈 Add this
    }

}