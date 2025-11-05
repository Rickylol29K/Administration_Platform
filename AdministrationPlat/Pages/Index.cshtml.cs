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

        [BindProperty] public string LoginUsername { get; set; }
        [BindProperty] public string LoginPassword { get; set; }
        [BindProperty] public string RegisterUsername { get; set; }
        [BindProperty] public string RegisterPassword { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPostLogin()
        {
            var user = _context.Users.FirstOrDefault(u => 
                u.Username == LoginUsername && u.Password == LoginPassword);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                Message = $"Welcome back, {user.Username}!";
                return RedirectToPage("/Teacher/Calendar");
            }

            Message = "Invalid username or password.";
            return Page();
        }

        public IActionResult OnPostRegister()
        {
            var exists = _context.Users.Any(u => u.Username == RegisterUsername);
            if (exists)
            {
                Message = "Username already exists.";
                return Page();
            }

            var user = new User
            {
                Username = RegisterUsername,
                Password = RegisterPassword
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            Message = "Registration successful! You can now log in.";
            return Page();
        }
    }
}