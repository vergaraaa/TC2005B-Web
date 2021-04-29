using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

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
            if (Username == "edgar" || Username == "charlie" || Username == "fabiana" || Username == "ingrid" || Username == "axel")
            {
                int id_user = 0;

                if (Username == "edgar") id_user = 1;
                else if (Username == "charlie") id_user = 2;
                else if (Username == "fabiana") id_user = 3;
                else if (Username == "ingrid") id_user = 4;
                else if (Username == "axel") id_user = 5;

                /*string db_string = @"server=127.0.0.1;uid=root;password=Tijuana13!;database=db_ternium";
                using var conexion = new MySqlConnection(db_string);
                conexion.Open();

                var query = "insert into session(id_user, timestamp) values(@id_user, @timestamp)";
                using var cmd = new MySqlCommand(query, conexion);

                cmd.Parameters.AddWithValue("@id_user", id_user);
                cmd.Parameters.AddWithValue("@timestamp", DateTime.Now);
                cmd.Prepare();

                cmd.ExecuteNonQuery();*/

                return RedirectToPage("/Profile");
            }

            Error = "Error";
            return Page();
        }
    }
}
