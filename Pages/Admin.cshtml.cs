using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HealthDataCollector.Services;

namespace HealthDataCollector.Pages
{
    public class AdminModel : PageModel
    {
        private readonly ISourceService _provisioning;

        public AdminModel(ISourceService provisioning)
        {
            _provisioning = provisioning;
        }

        public void OnGet()
        {
        }

        public void OnPost()
        {
            _provisioning.Enable();
        }
    }
}
