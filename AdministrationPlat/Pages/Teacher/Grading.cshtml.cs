using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages.Teacher;

public class Grading : PageModel
{
    
    [BindProperty]
    public bool ShowOverlay { get; set; }
    
    public void OnPostShowOverlay()
    {
        ShowOverlay = true;
    }
    
    [BindProperty]
    public bool ShowOverlay2 { get; set; }
    
    public void OnPostShowOverlay2()
    {
        ShowOverlay = true;
        ShowOverlay2 = true;
    }
    
    [BindProperty]
    public bool submitClose { get; set; }
    
    public void OnPostsubmitClose()
    {
        submitClose = false;
    }
}