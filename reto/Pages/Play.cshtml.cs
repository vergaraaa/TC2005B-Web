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
            // Revisamos la variable de sesión es nula
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("username")))
            {
                // Si es nula la redireccionamos al login para que se autentique
                return new RedirectToPageResult("./Index");
            }
            else
            {
                // Obtenemos los datos del usuario que están en la sesión
                ID = (int) HttpContext.Session.GetInt32("local_id");
                Token = HttpContext.Session.GetString("token");
                Departments = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("departments"));

                return Page();
            }
        }
    }
}
