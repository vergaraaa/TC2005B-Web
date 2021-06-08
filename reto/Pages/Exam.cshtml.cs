using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace reto.Pages
{

    public class ExamModel : PageModel
    {

        private readonly ILogger<ExamModel> _logger;

        public ExamModel(ILogger<ExamModel> logger)
        {
            _logger = logger;
        }

        public bool IsOnPost { get; set; }
        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("username")))
            {
                return new RedirectToPageResult("./Index");
            }
            else {
                IsOnPost = false;
                return Page(); 
            }
        }

        public void OnPost()
        {
            IsOnPost = true;
        }
    }
}
