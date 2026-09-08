using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HealthDataCollector.Services;

namespace HealthDataCollector.Pages
{
    public class AdminModel : PageModel
    {
        private readonly IProvisioningService _provisioning;

        public AdminModel(IProvisioningService provisioning)
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
