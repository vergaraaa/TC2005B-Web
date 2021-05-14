using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
        public IList<string> Departments { get; set; }
        public string Token { get; set; }
        public string Id { get; set; }

        public void OnGet()
        {
            Username = HttpContext.Session.GetString("username");
            Departments = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("departments"));
        }
    }
}
