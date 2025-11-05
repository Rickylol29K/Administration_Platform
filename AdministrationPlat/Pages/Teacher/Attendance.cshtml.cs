using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages.Shared.Teacher;

public class Attendance : PageModel
{
    [BindProperty]
    public bool ShowOverlay { get; set; }

    public void OnPostShowOverlay()
    {
        ShowOverlay = true;
    }

    public void OnPostHideOverlay()
    {
        ShowOverlay = false;
    }

    public void OnPostSaveAttendance()
    {
        // TODO: Save attendance to database
        ShowOverlay = false;
    }
}
