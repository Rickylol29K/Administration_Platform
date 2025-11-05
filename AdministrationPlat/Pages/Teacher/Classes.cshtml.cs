using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages.Teacher;

public class Classes : PageModel
{
    [BindProperty]
    public int SelectedClassId { get; set; }

    [BindProperty]
    public bool ShowOverlay { get; set; }

    public void OnPostShowOverlay(int classId)
    {
        SelectedClassId = classId;
        ShowOverlay = true;
    }

}