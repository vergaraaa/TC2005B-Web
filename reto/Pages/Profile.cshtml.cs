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

        public int Medal1 { get; set; }
        public int Medal2 { get; set; }
        public int Medal3 { get; set; }

        public int ScoreMedal1 { get; set; }
        public int ScoreMedal2 { get; set; }
        public int ScoreMedal3 { get; set; }

        public class Attempts
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string ExamName { get; set; }
            public string ExamID { get; set; }
            public int Score { get; set; }
            public int Attempt { get; set; }
            public DateTime Date { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public int V { get; set; }
        }

        [BindProperty]
        public IList<Attempts> ListAttempts { get; set; }

        public async Task OnGet()
        {
            Username = HttpContext.Session.GetString("username");
            Departments = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("departments"));
            await ActualizaTopDiez();

            // Medalla 3
            // Calcular indicador top 10 con base a la diferencia del campo entry_date y last_check


            string url = "https://chatarrap-api.herokuapp.com/attempts/";
            Uri baseURL = new Uri(url);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("auth_key", HttpContext.Session.GetString("token"));
            var response = await client.GetAsync(baseURL);
            var json = await response.Content.ReadAsStringAsync();

            ListAttempts = JsonConvert.DeserializeObject<List<Attempts>>(json);

            // Medalla 1 y medalla 2
            foreach (var attempt in ListAttempts)
            {
                if (attempt.Username == Username)
                {
                    ScoreMedal1++;
                    if (attempt.Score == 100 && attempt.Attempt == 1)
                    {
                        ScoreMedal2++;
                    }
                }
            }

            string connectionString = "Server=127.0.0.1;Port=3306;Database=db_ternium;Uid=root;password=Al.730550;";
            MySqlConnection conexion = new MySqlConnection(connectionString);
            conexion.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conexion;

            cmd.CommandText = @"SELECT id_user, id_medal, user_medal.rank FROM user_medal WHERE id_user = @id_user";
            cmd.Parameters.AddWithValue("@id_user", HttpContext.Session.GetInt32("local_id"));

            // ver si cumple para medalla 1 y medalla 2 y si sí hacer un insert y si no pues nada
            // si sí está ver si el rank actual es mayor que el rank guardado en la base de datos

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if ((int)reader["id_medal"] == 1)
                    {
                        Medal1 = (int)reader["rank"];
                    }
                    else if ((int)reader["id_medal"] == 2)
                    {
                        Medal2 = (int)reader["rank"];
                    }
                    else if ((int)reader["id_medal"] == 3)
                    {
                        Medal3 = (int)reader["rank"];
                    }
                }
            }

            conexion.Dispose();
        }

        public async Task ActualizaTopDiez()
        {
            // string url = "https://chatarrap-api.herokuapp.com/attempts/scoresWeek";
            string url = "https://chatarrap-api.herokuapp.com/attempts/scores";

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
