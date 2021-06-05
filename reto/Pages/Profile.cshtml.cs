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
using System.Net.Http;
using System.Net.Http.Headers;
using MySql.Data.MySqlClient;
using System.Data;

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
        public class Helper
        {
            public string Username { get; set; }
            public int Score { get; set; }
        }

        async public void OnGet()
        {
            Username = HttpContext.Session.GetString("username");
            Departments = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("departments"));
            await ActualizaTopDiez();
        }

        public async Task ActualizaTopDiez()
        {
            string url = "https://chatarrap-api.herokuapp.com/attempts/scoresWeek";
            // string url = "https://chatarrap-api.herokuapp.com/attempts/scores";

            Uri baseURL = new Uri(url);
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("auth_key", HttpContext.Session.GetString("token"));
            var respuesta = await client.GetAsync(baseURL);
            var json = await respuesta.Content.ReadAsStringAsync();
            
            // Obtiene los primeros 10 lugares al momento del leaderboard, o menos si hay menos
            var scores = JsonConvert.DeserializeObject<List<Helper>>(json);
            int n = Math.Min(scores.Count, 10);
            var top10 = scores.Take(n);

            // Abre la conexión a la base de datos del sistema 
            string connectionString = "Server=127.0.0.1;Port=3306;Database=db_ternium;Uid=root;password=Al.730550;";
            MySqlConnection conexion = new MySqlConnection(connectionString);
            conexion.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conexion;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CheckTopTen";
            cmd.Parameters.AddWithValue("@Username", "'temp'");

            // Itera el top 10 actual, llamando un stored procedure para cada uno
            foreach (var user in top10)
            {
                cmd.Parameters["@Username"].Value = user.Username;
                cmd.ExecuteNonQuery();
            }

            // Se deshace de la conexión existente
            conexion.Dispose();
        }
    }
}
