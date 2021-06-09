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
using reto.Models;

namespace reto.Pages
{
    public class ProfileModel : PageModel
    {

        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(ILogger<ProfileModel> logger)
        {
            _logger = logger;
        }
        
        // Variable para guardar el nombre del usuario
        public string Username { get; set; }

        // Lista de tipo string para guardar los departamentos del usuario
        public IList<string> Departments { get; set; }

        // Variable para guardar el token del usuario
        public string Token { get; set; }

        // Variable para guardar el Id del usuario
        public string Id { get; set; }

        // Array de tipo int para guardar los ranks de las medallas
        public int[] Medals { get; set; } = new int[3];

        // Array de tipo int para guardar la puntuación de las medallas
        public int[] ScoreMedals { get; set; } = new int[3];

        // Variable para guardar la calificación más baja del usuario
        public int Min_grade { get; set; }

        // Variable para guardar la calificación más alta del usuario
        public int Max_grade { get; set; }

        // Lista de tipo Attempts para guardar los intentos de la API
        [BindProperty]
        public IList<Attempts> ListAttempts { get; set; }


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
                Departments = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("departments"));
                
                // Actualizamos el top diez
                await ActualizaTopDiez();

                // Realiza la conexión al API de Ternium para calcular las medallas al hacer un request a los attempts
                string url = "https://chatarrap-api.herokuapp.com/attempts/";
                Uri baseURL = new Uri(url);
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("auth_key", HttpContext.Session.GetString("token"));
                var response = await client.GetAsync(baseURL);
                var json = await response.Content.ReadAsStringAsync();

                ListAttempts = JsonConvert.DeserializeObject<List<Attempts>>(json);

                int cont = 0; int max_grade = 0; int min_grade = 100;

                // Ciclo para calcular el score de las medallas 1 y 2 del usuario
                foreach (var attempt in ListAttempts)
                {
                    if (attempt.Username == "alberto")
                    {
                        ScoreMedals[0]++;
                        if (attempt.Score == 100 && attempt.Attempt == 1)
                        {
                            ScoreMedals[1]++;
                        }
                        if (attempt.Score > max_grade) { max_grade = attempt.Score; }
                        if (attempt.Score < min_grade) { min_grade = attempt.Score; }
                        cont++;
                    }
                }

                // Condición para calcular la calificación más baja y la máxima
                if (cont == 0) { Min_grade = 0; }
                else { Min_grade = min_grade; }
                Max_grade = max_grade;


                // Abre la conexión a la base de datos del sistema 
                string connectionString = "Server=127.0.0.1;Port=3306;Database=db_ternium;Uid=root;password=Tijuana13!;";
                MySqlConnection conexion = new MySqlConnection(connectionString);

                // Ejecuta un stored procedure para obtener el dato de la medalla mantenerse en el top 10
                conexion.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "DaysInTopTen";
                cmd.Parameters.AddWithValue("@Username", Username);
                var reader1 = cmd.ExecuteReader();
                if (!reader1.Read()) ScoreMedals[2] = (int)reader1["Days"];
                conexion.Close();

                // Obtener el rank actual de la medalla 1
                int rank_medal1 = 0;
                if (ScoreMedals[0] >= 1 && ScoreMedals[0] < 5) rank_medal1 = 1;
                else if (ScoreMedals[0] >= 5 && ScoreMedals[0] < 10) rank_medal1 = 2;
                else if (ScoreMedals[0] >= 10 && ScoreMedals[0] < 50) rank_medal1 = 3;
                else if (ScoreMedals[0] >= 50 && ScoreMedals[0] < 100) rank_medal1 = 4;

                // Obtener el rank actual de la medalla 2
                int rank_medal2 = 0;
                if (ScoreMedals[1] == 1) rank_medal2 = 1;
                else if (ScoreMedals[1] > 1 && ScoreMedals[1] < 10) rank_medal2 = 2;
                else if (ScoreMedals[1] >= 10 && ScoreMedals[1] < 50) rank_medal2 = 3;
                else if (ScoreMedals[1] >= 50 && ScoreMedals[1] < 100) rank_medal2 = 4;

                // Obtener el rank actual de la medalla 3
                int rank_medal3 = 0;
                if (ScoreMedals[2] == 1) rank_medal3 = 1;
                else if (ScoreMedals[2] > 1 && ScoreMedals[2] < 30) rank_medal3 = 2;
                else if (ScoreMedals[2] >= 30 && ScoreMedals[2] < 180) rank_medal3 = 3;
                else if (ScoreMedals[2] >= 180 && ScoreMedals[2] < 365) rank_medal3 = 4;

                // Retoma la conexión usada en la parte previa del método
                conexion.Open();
                cmd = new MySqlCommand();
                cmd.Connection = conexion;

                // Query para verificar si el usuario tiene medallas
                cmd.CommandText = @"SELECT id_user, id_medal, user_medal.rank FROM user_medal WHERE id_user = @id_user";
                cmd.Parameters.AddWithValue("@id_user", HttpContext.Session.GetInt32("local_id"));

                // Inicializamos una lista de medallas
                List<Medal> ListMedals = new List<Medal>();
                
                // Medalla temporal para añadir a la lista
                Medal temp;

                // Ejectuamos la query
                using (var reader = cmd.ExecuteReader())
                {
                    // Iteramos los resultados
                    while (reader.Read())
                    {
                        // Asignamos valores a la medalla temporal
                        temp = new Medal
                        {
                            Id_user = (int)reader["id_user"],
                            Id_medal = (int)reader["id_medal"],
                            Rank = (int)reader["rank"]
                        };
                        // Añadimos la medalla a la lista
                        ListMedals.Add(temp);
                    }
                }
                
                // Cerramos la conexión a la base de datos
                conexion.Close();

                // Condición que verifica si la lista está vacía o se agregó una medalla
                // Esto con el fin de ver si alguna de las medallas subió de rank y se tiene que actualizar
                if (ListMedals.Count > 0)
                {
                    // En caso de que sí haya medallas, iteramos cada una
                    foreach (var medal in ListMedals)
                    {
                        // Verificamos el tipo de medalla 
                        if (medal.Id_medal == 1)
                        {
                            // Condición que compara el rank actual calculado con el rank actual en la base de datos
                            if (rank_medal1 > medal.Rank)
                            {
                                // En caso de que sí, se actualiza la base de datos
                                conexion.Open();
                                var query = @"UPDATE user_medal SET user_medal.rank=@rank WHERE id_user=@id_user AND id_medal=@id_medal";
                                MySqlCommand update = new MySqlCommand(query, conexion);
                                update.Parameters.AddWithValue("@rank", rank_medal1);
                                update.Parameters.AddWithValue("@id_user", HttpContext.Session.GetInt32("local_id"));
                                update.Parameters.AddWithValue("@id_medal", 1);
                                update.ExecuteNonQuery();
                                conexion.Close();

                                // Se guarda el rank de la medalla
                                Medals[0] = rank_medal1;
                            }
                            else
                            {
                                // En caso de que el rank sea igual o menor
                                // Se guarda el rank de la medalla
                                Medals[0] = medal.Rank;
                            }
                        }

                        // Funcionamiento equivalente para la medalla 2
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

                                Medals[1] = rank_medal2;
                            }
                            else
                            {
                                Medals[1] = medal.Rank;
                            }
                        }

                        // Funcionamiento equivalente para la medalla 3
                        else if (medal.Id_medal == 3)
                        {
                            if (rank_medal3 > medal.Rank)
                            {
                                conexion.Open();
                                var query = @"UPDATE user_medal SET user_medal.rank=@rank WHERE id_user=@id_user AND id_medal=@id_medal";
                                MySqlCommand update = new MySqlCommand(query, conexion);
                                update.Parameters.AddWithValue("@rank", rank_medal3);
                                update.Parameters.AddWithValue("@id_user", HttpContext.Session.GetInt32("local_id"));
                                update.Parameters.AddWithValue("@id_medal", 3);
                                update.ExecuteNonQuery();
                                conexion.Close();

                                Medals[2] = rank_medal3;
                            }
                            else
                            {
                                Medals[2] = medal.Rank;
                            }
                        }
                    }
                }
                // Si no hay medallas en la base de datos
                else
                {
                    // Comparamos si el rank actual es mayor que 0 en cada una de las medallas para ver si 
                    // se puede añadir alguna a la base de datos

                    // Para medalla 1
                    if (rank_medal1 > 0)
                    {
                        conexion.Open();
                        var query = "INSERT INTO user_medal(id_user, id_medal, user_medal.rank) VALUES(@id_user, @id_medal, @rank)";
                        cmd = new MySqlCommand(query, conexion);
                        cmd.Parameters.AddWithValue("@id_user", HttpContext.Session.GetInt32("local_id"));
                        cmd.Parameters.AddWithValue("@id_medal", 1);
                        cmd.Parameters.AddWithValue("@rank", rank_medal1);
                        cmd.ExecuteNonQuery();
                        conexion.Close();

                        Medals[0] = rank_medal1;
                    }

                    // Para medalla 2
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

                        Medals[1] = rank_medal2;
                    }

                    // Para medalla 3
                    if (rank_medal3 > 0)
                    {
                        conexion.Open();
                        var query = "INSERT INTO user_medal(id_user, id_medal, user_medal.rank) VALUES(@id_user, @id_medal, @rank)";
                        cmd = new MySqlCommand(query, conexion);
                        cmd.Parameters.AddWithValue("@id_user", HttpContext.Session.GetInt32("local_id"));
                        cmd.Parameters.AddWithValue("@id_medal", 3);
                        cmd.Parameters.AddWithValue("@rank", rank_medal3);
                        cmd.ExecuteNonQuery();
                        conexion.Close();

                        Medals[2] = rank_medal3;
                    }
                }

                // Se deshace la conexión a la base de datos
                conexion.Dispose();
                return Page();
            }   
        }

        public async Task ActualizaTopDiez()
        {
            string url = "https://chatarrap-api.herokuapp.com/attempts/scores";

            Uri baseURL = new Uri(url);
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("auth_key", HttpContext.Session.GetString("token"));
            var respuesta = await client.GetAsync(baseURL);
            var json = await respuesta.Content.ReadAsStringAsync();

            // Obtiene los primeros 10 lugares al momento del leaderboard, o menos si hay menos
            var scores = JsonConvert.DeserializeObject<List<Helper>>(json);
            int n = Math.Min(scores.Count, 10);

            // Esta función depende en la condición actual donde los scores se devuelven de mayor a menor
            var top10 = scores.Take(n);

            // Abre la conexión a la base de datos del sistema 
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

            // Se deshace de la conexión existente
            conexion.Dispose();
        }
    }
}
