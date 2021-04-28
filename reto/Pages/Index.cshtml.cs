using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace reto.Pages
{
    public class IndexModel : PageModel
    {   
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            Error = "";
        }
        
        public string Error { get; set; }
        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }

        public IActionResult OnPost()
        {
            if (Username == "test" && Password == "test")
            {
                return RedirectToPage("/Profile");
            }
            
            Error = Username + Password;
            return Page();
        }
    }
}
