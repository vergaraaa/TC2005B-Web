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
    public class PlayModel : PageModel
    {

        private readonly ILogger<PlayModel> _logger;

        public PlayModel(ILogger<PlayModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            if (HttpContext.Session.GetString("username") == "")
            {
                RedirectToAction("Login", "/");
            }
        }
    }
}
