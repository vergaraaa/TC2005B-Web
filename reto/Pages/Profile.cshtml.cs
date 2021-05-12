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
    public class ProfileModel : PageModel
    {

        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(ILogger<ProfileModel> logger)
        {
            _logger = logger;
        }

        public string Username { get; set; }

        public void OnGet()
        {
            Username = HttpContext.Session.GetString("username");
        }
    }
}
