using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TuneScout.Pages
{
    public class exeptionModel : PageModel
    {
        public void OnGet()
        {
            Response.StatusCode = 404;
        }
    }
}
