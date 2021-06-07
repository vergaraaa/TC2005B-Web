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

        private class Medals
        {
            public int Id_user { get; set; }
            public int Id_medal { get; set; }
            public int Rank { get; set; }
        }

        public async Task OnGet()
        {
            Username = HttpContext.Session.GetString("username");
            Departments = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("departments"));
            await ActualizaTopDiez();
            int daysInTop10 = 2;

            string url = "https://chatarrap-api.herokuapp.com/attempts/";
            Uri baseURL = new Uri(url);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("auth_key", HttpContext.Session.GetString("token"));
            var response = await client.GetAsync(baseURL);
            var json = await response.Content.ReadAsStringAsync();

            ListAttempts = JsonConvert.DeserializeObject<List<Attempts>>(json);

            // Score actual de Medalla 1 y medalla 2
            foreach (var attempt in ListAttempts)
            {
                if (attempt.Username == "alberto")
                {
                    ScoreMedal1++;
                    if (attempt.Score == 100 && attempt.Attempt == 1)
                    {
                        ScoreMedal2++;
                    }
                }
            }

            // Gets current rank of medal 1
            int rank_medal1 = 0;
            if (ScoreMedal1 >= 1 && ScoreMedal1 < 5) rank_medal1 = 1;
            else if (ScoreMedal1 >= 5 && ScoreMedal1 < 10) rank_medal1 = 2;
            else if (ScoreMedal1 >= 10 && ScoreMedal1 < 50) rank_medal1 = 3;
            else if (ScoreMedal1 >= 50 && ScoreMedal1 < 100) rank_medal1 = 4;

            // Gets current rank of medal 2
            int rank_medal2 = 0;
            if (ScoreMedal2 == 1) rank_medal2 = 1;
            else if (ScoreMedal2 > 1 && ScoreMedal2 < 10) rank_medal2 = 2;
            else if (ScoreMedal2 >= 10 && ScoreMedal2 < 50) rank_medal2 = 3;
            else if (ScoreMedal2 >= 50 && ScoreMedal2 < 100) rank_medal2 = 4;

            // Get current rank of medal 3
            //

            string connectionString = "Server=127.0.0.1;Port=3306;Database=db_ternium;Uid=root;password=Tijuana13!;";
            MySqlConnection conexion = new MySqlConnection(connectionString);
            conexion.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conexion;

            cmd.CommandText = @"SELECT id_user, id_medal, user_medal.rank FROM user_medal WHERE id_user = @id_user";
            cmd.Parameters.AddWithValue("@id_user", HttpContext.Session.GetInt32("local_id"));

            List<Medals> ListMedals = new List<Medals>();
            Medals temp;
            
            using(var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    temp = new Medals
                    {
                        Id_user = (int)reader["id_user"],
                        Id_medal = (int)reader["id_medal"],
                        Rank = (int)reader["rank"]
                    };
                    ListMedals.Add(temp);
                }
            }

            conexion.Close();

            if(ListMedals.Count > 0)
            {
                foreach(var medal in ListMedals)
                {
                    if(medal.Id_medal == 1)
                    {
                        if (rank_medal1 > medal.Rank)
                        {
                            conexion.Open();
                            var query = @"UPDATE user_medal SET user_medal.rank=@rank WHERE id_user=@id_user AND id_medal=@id_medal";
                            MySqlCommand update = new MySqlCommand(query, conexion);
                            update.Parameters.AddWithValue("@rank", rank_medal1);
                            update.Parameters.AddWithValue("@id_user", HttpContext.Session.GetInt32("local_id"));
                            update.Parameters.AddWithValue("@id_medal", 1);
                            update.ExecuteNonQuery();
                            conexion.Close();

                            Medal1 = rank_medal1;
                        }
                        else
                        {
                            Medal1 = medal.Rank;
                        }
                    }
                    else if (medal.Id_medal == 2)
                    {
                        if (rank_medal2 > medal.Rank)
                        {
                            conexion.Open();
                            var query = @"UPDATE user_medal SET user_medal.rank=@rank WHERE id_user=@id_user AND id_medal=@id_medal";
                            MySqlCommand update = new MySqlCommand(query, conexion);
                            update.Parameters.AddWithValue("@rank", rank_medal2);
                            update.Parameters.AddWithValue("@id_user", HttpContext.Session.GetInt32("local_id"));
                            update.Parameters.AddWithValue("@id_medal", 2);
                            update.ExecuteNonQuery();
                            conexion.Close();

                            Medal2 = rank_medal2;
                        }
                        else
                        {
                            Medal2 = medal.Rank;
                        }
                    }
                    else if (medal.Id_medal == 3)
                    {

                    }
                }
            }
            else
            {
                if(rank_medal1 > 0)
                {
                    conexion.Open();
                    var query = "INSERT INTO user_medal(id_user, id_medal, user_medal.rank) VALUES(@id_user, @id_medal, @rank)";
                    cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@id_user", HttpContext.Session.GetInt32("local_id"));
                    cmd.Parameters.AddWithValue("@id_medal", 1);
                    cmd.Parameters.AddWithValue("@rank", rank_medal1);
                    cmd.ExecuteNonQuery();
                    conexion.Close();

                    Medal1 = rank_medal1;
                }

                if (rank_medal2 > 0)
                {
                    conexion.Open();
                    var query = "INSERT INTO user_medal(id_user, id_medal, user_medal.rank) VALUES(@id_user, @id_medal, @rank)";
                    cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@id_user", HttpContext.Session.GetInt32("local_id"));
                    cmd.Parameters.AddWithValue("@id_medal", 2);
                    cmd.Parameters.AddWithValue("@rank", rank_medal2);
                    cmd.ExecuteNonQuery();
                    conexion.Close();

                    Medal2 = rank_medal1;
                }
            }

            conexion.Dispose();
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

            // Abre la conexi�n a la base de datos del sistema 
            string connectionString = "Server=127.0.0.1;Port=3306;Database=db_ternium;Uid=root;password=Tijuana13!;";
            MySqlConnection conexion = new MySqlConnection(connectionString);
            conexion.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conexion;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CheckTopTen";
            cmd.Parameters.AddWithValue("@Username", "temp");

            // Itera el top 10 actual, llamando un stored procedure para cada uno
            foreach (var user in top10)
            {
                cmd.Parameters["@Username"].Value = user.Username;
                cmd.ExecuteNonQuery();
            }

            // Se deshace de la conexi�n existente
            conexion.Dispose();
        }
    }
}
