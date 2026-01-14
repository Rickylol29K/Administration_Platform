using AdministrationPlat.Models;
using Logic;
using Logic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogicService _logic;

        public IndexModel(ILogicService logic)
        {
            _logic = logic;
        }

        [BindProperty]
        public string LoginUsername { get; set; } = string.Empty;

        [BindProperty]
        public string LoginPassword { get; set; } = string.Empty;

        [BindProperty]
        public string RegisterUsername { get; set; } = string.Empty;

        [BindProperty]
        public string RegisterPassword { get; set; } = string.Empty;

        [BindProperty]
        public bool RegisterIsAdmin { get; set; }

        public string Message { get; set; } = string.Empty;
        public string MessageCssClass { get; private set; } = string.Empty;

        public void OnGet()
        {
            HttpContext.Session.Clear();
        }

        public IActionResult OnPostLogin()
        {
            OperationResult<User> result = _logic.Login(LoginUsername, LoginPassword);

            if (result.Success && result.Value != null)
            {
                HttpContext.Session.SetInt32("UserId", result.Value.Id);
                HttpContext.Session.SetString("Username", result.Value.Username);
                HttpContext.Session.SetInt32("IsAdmin", result.Value.IsAdmin ? 1 : 0);
                Message = $"Welcome back, {result.Value.Username}!";

                if (result.Value.IsAdmin)
                {
                    return RedirectToPage("/Admin/AdminIndex");
                }

                return RedirectToPage("/Teacher/TeacherIndex");
            }

            Message = result.Error ?? "Invalid username or password.";
            MessageCssClass = "validation-summary";
            return Page();
        }

        public IActionResult OnPostRegister()
        {
            OperationResult<User> result = _logic.Register(RegisterUsername, RegisterPassword, RegisterIsAdmin);

            if (!result.Success)
            {
                Message = result.Error ?? "Registration failed.";
                MessageCssClass = "validation-summary";
                return Page();
            }

            Message = "Registration successful! You can now log in.";
            MessageCssClass = "status-message";
            return Page();
        }
    }
}
