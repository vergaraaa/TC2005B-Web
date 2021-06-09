using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace reto.Pages
{
    public class PlayModel : PageModel
    {

        public int ID { get; set; }
        public string Token { get; set; }
        public IList<string> Departments { get; set; }

        private readonly ILogger<PlayModel> _logger;

        public PlayModel(ILogger<PlayModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("username")))
            {
                return new RedirectToPageResult("./Index");
            }
            else
            {
                ID = (int) HttpContext.Session.GetInt32("local_id");
                Token = HttpContext.Session.GetString("token");
                Departments = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("departments"));

                return Page();
            }
        }
    }
}
