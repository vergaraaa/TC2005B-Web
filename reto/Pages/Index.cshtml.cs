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
            // Search in data base
            string db_string = @"server=127.0.0.1;uid=root;password=Tijuana13!;database=db_ternium";

            MySqlConnection conexion = new MySqlConnection(db_string);
            conexion.Open();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conexion;
            cmd.CommandText = "Select id_user from user where username = '" + Username + "'";

            var reader = cmd.ExecuteReader();

            conexion.Close();

            // Errores por todos lados :( :c D:
            if (reader.HasRows)
            {
                conexion.Open();

                int id_user = Convert.ToInt32(reader["id_user"]);
                var query = "insert into session(id_user, timestamp) values(@id_user, @timestamp)";
                using var cmd2 = new MySqlCommand(query, conexion);

                cmd2.Parameters.AddWithValue("@id_user", id_user);
                cmd2.Parameters.AddWithValue("@timestamp", DateTime.Now);
                cmd2.Prepare();

                cmd2.ExecuteNonQuery();

                conexion.Close();

                return RedirectToPage("/Profile");
            }
            else
            {
                conexion.Open();
                // Crear usuario
                var query = "insert into user(username, type) values(@username, @type)";
                using var cmd2 = new MySqlCommand(query, conexion);

                cmd2.Parameters.AddWithValue("@username", Username);
                cmd2.Parameters.AddWithValue("@type", "Chatarreria");
                cmd2.Prepare();

                cmd2.ExecuteNonQuery();

                conexion.Close();

                // Encontrar id del usuario creado
                conexion.Open();

                query = "select id_user from user where username = " + User;
                cmd2.Connection = conexion;
                cmd2.CommandText = query;

                var reader2 = cmd.ExecuteReader();


                int id_user = Convert.ToInt32(reader2["id_user"]);
                conexion.Close();
                // Insert new user when loggs
                conexion.Open();
                query = "insert into session(id_user, timestamp) values(@id_user, @timestamp)";
                cmd2.Connection = conexion;
                cmd2.CommandText = query;

                cmd2.Parameters.AddWithValue("@id_user", id_user);
                cmd2.Parameters.AddWithValue("@timestamp", DateTime.Now);
                cmd2.Prepare();

                cmd2.ExecuteNonQuery();

                conexion.Close();

                return RedirectToPage("/Profile");
            }
            //Error = "Error";
            //return Page();
        }
    }
}
