using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdministrationPlat.Data;
using AdministrationPlat.Models;
using System.Linq;

namespace AdministrationPlat.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty] public string LoginUsername { get; set; } = string.Empty;
        [BindProperty] public string LoginPassword { get; set; } = string.Empty;
        [BindProperty] public string RegisterUsername { get; set; } = string.Empty;
        [BindProperty] public string RegisterPassword { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
        public string MessageCssClass { get; private set; } = string.Empty;

        public void OnGet()
        {
        }

        public IActionResult OnPostLogin()
        {
            var username = LoginUsername?.Trim() ?? string.Empty;
            var password = LoginPassword?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Message = "Enter both username and password.";
                MessageCssClass = "validation-summary";
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.Username == username && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                Message = $"Welcome back, {user.Username}!";
                return RedirectToPage("/Teacher/TeacherIndex");
            }

            Message = "Invalid username or password.";
            MessageCssClass = "validation-summary";
            return Page();
        }

        public IActionResult OnPostRegister()
        {
            var username = RegisterUsername?.Trim() ?? string.Empty;
            var password = RegisterPassword?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Message = "Choose a username and password.";
                MessageCssClass = "validation-summary";
                return Page();
            }

            var exists = _context.Users.Any(u => u.Username == username);
            if (exists)
            {
                Message = "Username already exists.";
                MessageCssClass = "validation-summary";
                return Page();
            }

            var user = new User
            {
                Username = username,
                Password = password
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            Message = "Registration successful! You can now log in.";
            MessageCssClass = "status-message";
            return Page();
        }
    }
}
