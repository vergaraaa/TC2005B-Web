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

namespace reto.Pages
{
    public class LeaderboardModel : PageModel
    {

        private readonly ILogger<LeaderboardModel> _logger;

        public LeaderboardModel(ILogger<LeaderboardModel> logger)
        {
            _logger = logger;
        }

        public string Username { get; set; }

        public class Helper
        {
            public string Username { get; set; }
            public int Score { get; set; }
        }

        public async Task OnGet()
        {
            Username = HttpContext.Session.GetString("username");

            Uri baseURL = new Uri("https://chatarrap-api.herokuapp.com/attempts/scoresWeek");
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("auth_key", HttpContext.Session.GetString("token"));

            var response = await client.GetAsync(baseURL);
            var json = await response.Content.ReadAsStringAsync();

            Leaderboard = JsonConvert.DeserializeObject<List<Helper>>(json);
        }

        [BindProperty]
        public IList<Helper> Leaderboard { get; set; }
        [BindProperty]
        public int OptionType { get; set; }
        [BindProperty]
        public int OptionTime { get; set; }
        public async Task OnPost()
        {
            Username = HttpContext.Session.GetString("username");
            if (OptionType == 0)
            { 
                string url = "";

                if (OptionTime == 0)
                {
                    url = "https://chatarrap-api.herokuapp.com/attempts/scoresWeek";
                }
                else if (OptionTime == 1)
                {
                    url = "https://chatarrap-api.herokuapp.com/attempts/scores";
                }

                Uri baseURL = new Uri(url);
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Add("auth_key", HttpContext.Session.GetString("token"));

                var response = await client.GetAsync(baseURL);
                var json = await response.Content.ReadAsStringAsync();
                Leaderboard = JsonConvert.DeserializeObject<List<Helper>>(json);
            
            }
            else if (OptionType == 1)
            {
                OptionType = 1;
                string connectionString = "Server=127.0.0.1;Port=3306;Database=db_ternium;Uid=root;password=Al.730550;";

                MySqlConnection conexion = new MySqlConnection(connectionString);
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;

                if (OptionTime == 0)
                {
                    cmd.CommandText = @"SELECT username, SUM(score) AS score FROM user INNER JOIN exercise ON user.id_user = exercise.id_user WHERE YEARWEEK(NOW()) = YEARWEEK(exercise.timestamp) GROUP BY (user.id_user) ORDER BY SUM(exercise.score) DESC, username DESC";
                }
                else if (OptionTime == 1)
                {
                    cmd.CommandText = @"SELECT username, SUM(score) AS score FROM user INNER JOIN exercise ON user.id_user = exercise.id_user WHERE MONTH(NOW()) = MONTH(exercise.timestamp) AND YEAR(NOW()) = YEAR(exercise.timestamp) GROUP BY (user.id_user) ORDER BY SUM(exercise.score) DESC, user.username DESC";
                }
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
