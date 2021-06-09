using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using reto.Models;

namespace reto.Pages
{
    public class LeaderboardModel : PageModel
    {

        private readonly ILogger<LeaderboardModel> _logger;

        public LeaderboardModel(ILogger<LeaderboardModel> logger)
        {
            _logger = logger;
        }
        // Variable que guarda el nombre de usuario
        public string Username { get; set; }

        public async Task<IActionResult> OnGet()
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
                Username = HttpContext.Session.GetString("username");

                // Preparamos variables para hacer request a la API
                Uri baseURL = new Uri("https://chatarrap-api.herokuapp.com/attempts/scoresWeek");
                HttpClient client = new HttpClient();

                // Añadimos los headers necesarios
                client.DefaultRequestHeaders.Add("auth_key", HttpContext.Session.GetString("token"));

                // Hacemos la request y la convertimos a json
                var response = await client.GetAsync(baseURL);
                var json = await response.Content.ReadAsStringAsync();

                Leaderboard = JsonConvert.DeserializeObject<List<Helper>>(json);
                return Page();
            }
        }

        // Lista de la clase Helper, la cual nos ayuda a hacer la lista del leaderboard 
        [BindProperty]
        public IList<Helper> Leaderboard { get; set; }
        
        // Variable que guarda la opción de Exámenes o Prácticas
        [BindProperty]
        public int OptionType { get; set; }

        //  Variable que guarda la opción de si es Semanal o Mensual
        [BindProperty]
        public int OptionTime { get; set; }

        public async Task OnPost()
        {
            // Obtenemos datos del usuario que está en la sesión
            Username = HttpContext.Session.GetString("username");

            // Si se quiere ver por exámenes
            if (OptionType == 0)
            { 
                string url = "";

                // Si se quiere ver semanal
                if (OptionTime == 0)
                {
                    url = "https://chatarrap-api.herokuapp.com/attempts/scoresWeek";
                }
                // Si se quiere ver mensual
                else if (OptionTime == 1)
                {
                    url = "https://chatarrap-api.herokuapp.com/attempts/scores";
                }

                // Se forma la petición
                Uri baseURL = new Uri(url);
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Add("auth_key", HttpContext.Session.GetString("token"));

                // Hacemos la request y la convertimos a json
                var response = await client.GetAsync(baseURL);
                var json = await response.Content.ReadAsStringAsync();

                // Deserializamos el json en nuestra lista de leaderboard
                Leaderboard = JsonConvert.DeserializeObject<List<Helper>>(json);
            
            }
            // Si se quiere ver por prácticas
            else if (OptionType == 1)
            {
                
                string connectionString = "Server=127.0.0.1;Port=3306;Database=db_ternium;Uid=root;password=Tijuana13!;";

                MySqlConnection conexion = new MySqlConnection(connectionString);
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;

                // Si se quiere ver semanal
                if (OptionTime == 0)
                {
                    cmd.CommandText = @"SELECT username, SUM(score) AS score FROM user INNER JOIN exercise ON user.id_user = exercise.id_user WHERE YEARWEEK(NOW()) = YEARWEEK(exercise.timestamp) GROUP BY (user.id_user) ORDER BY SUM(exercise.score) DESC, username DESC";
                }
                // Si se quiere ver mensual
                else if (OptionTime == 1)
                {
                    cmd.CommandText = @"SELECT username, SUM(score) AS score FROM user INNER JOIN exercise ON user.id_user = exercise.id_user WHERE MONTH(NOW()) = MONTH(exercise.timestamp) AND YEAR(NOW()) = YEAR(exercise.timestamp) GROUP BY (user.id_user) ORDER BY SUM(exercise.score) DESC, user.username DESC";
                }

                // Variable temporal de usuario para añadir a la lista del leaderboard
                Helper usr;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usr = new Helper
                        {
                            Username = reader["username"].ToString(),
                            Score = Convert.ToInt32(reader["score"])
                        };
                        Leaderboard.Add(usr);
                    }
                }
                conexion.Dispose();
            }
        }
    }
}
