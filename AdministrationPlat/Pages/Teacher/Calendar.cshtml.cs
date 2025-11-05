using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using AdministrationPlat.Models;
using AdministrationPlat.Data;

namespace AdministrationPlat.Pages.Teacher
{
    public class CalendarModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CalendarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<int> Days { get; set; } = new();
        public int Year { get; set; }
        public string MonthName { get; set; } = "";
        public int Month { get; set; }

        [BindProperty] public bool ShowOverlay { get; set; }
        [BindProperty] public int SelectedDay { get; set; }
        [BindProperty] public EventItem NewEvent { get; set; } = new();
        [BindProperty] public Guid EditingId { get; set; }

        public List<EventItem> DayEvents { get; set; } = new();

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToPage("/Index");

            GenerateCalendar();
            LoadEvents();
            return Page();
        }

        private void GenerateCalendar()
        {
            var today = DateTime.Now;
            Year = today.Year;
            Month = today.Month;
            MonthName = today.ToString("MMMM");

            int daysInMonth = DateTime.DaysInMonth(Year, Month);
            Days = Enumerable.Range(1, daysInMonth).ToList();
        }

        public IActionResult OnPostShowOverlay(int selectedDay)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToPage("/Index");

            SelectedDay = selectedDay;
            ShowOverlay = true;
            GenerateCalendar();
            LoadEvents();
            return Page();
        }

        public IActionResult OnPostHideOverlay()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToPage("/Index");

            ShowOverlay = false;
            GenerateCalendar();
            LoadEvents();
            return Page();
        }

        public IActionResult OnPostAddEvent(int selectedDay)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToPage("/Index");

            NewEvent.Day = selectedDay;
            NewEvent.Month = DateTime.Now.Month;
            NewEvent.Year = DateTime.Now.Year;
            NewEvent.UserId = userId.Value;

            _context.TeacherEvents.Add(NewEvent);
            _context.SaveChanges();

            NewEvent = new EventItem();
            SelectedDay = selectedDay;
            ShowOverlay = true;
            GenerateCalendar();
            LoadEvents();
            return Page();
        }

        public IActionResult OnPostDeleteEvent(Guid id, int selectedDay)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToPage("/Index");

            var ev = _context.TeacherEvents.FirstOrDefault(e => e.Id == id && e.UserId == userId.Value);
            if (ev != null)
            {
                _context.TeacherEvents.Remove(ev);
                _context.SaveChanges();
            }

            SelectedDay = selectedDay;
            ShowOverlay = true;
            GenerateCalendar();
            LoadEvents();
            return Page();
        }

        public IActionResult OnPostEditEvent(Guid id, int selectedDay)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToPage("/Index");

            var ev = _context.TeacherEvents.FirstOrDefault(e => e.Id == id && e.UserId == userId.Value);
            if (ev != null)
            {
                EditingId = id;
                NewEvent = new EventItem
                {
                    Id = ev.Id,
                    Day = ev.Day,
                    Title = ev.Title,
                    Description = ev.Description,
                    Location = ev.Location,
                    Time = ev.Time
                };
            }

            SelectedDay = selectedDay;
            ShowOverlay = true;
            GenerateCalendar();
            LoadEvents();
            return Page();
        }

        public IActionResult OnPostUpdateEvent(int selectedDay)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToPage("/Index");

            var ev = _context.TeacherEvents.FirstOrDefault(e => e.Id == NewEvent.Id && e.UserId == userId.Value);
            if (ev != null)
            {
                ev.Title = NewEvent.Title;
                ev.Description = NewEvent.Description;
                ev.Location = NewEvent.Location;
                ev.Time = NewEvent.Time;
                _context.SaveChanges();
            }

            EditingId = Guid.Empty;
            SelectedDay = selectedDay;
            ShowOverlay = true;
            GenerateCalendar();
            LoadEvents();
            return Page();
        }

        private void LoadEvents()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return;

            DayEvents = _context.TeacherEvents
                .Where(e => e.Day == SelectedDay && e.Month == Month && e.Year == Year && e.UserId == userId.Value)
                .ToList();
        }
    }
}
