using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using reto.Models;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;


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

        [BindProperty]
        public new User User { get; set; }
        public string Error { get; set; }

        public async Task<IActionResult> OnPost()
        {
            // REST API code
            // Buscamos el recurso
            Uri baseURL = new Uri("https://chatarrap-api.herokuapp.com/users/login");

            // Creamos cliente para hacer petición
            HttpClient client = new HttpClient();

            // Armamos la petición
            JObject joPeticion = new JObject();
            joPeticion.Add("username", User.Username);
            joPeticion.Add("password", User.Password);

            var stringContent = new StringContent(joPeticion.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(baseURL.ToString(), stringContent);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                if (responseContent != "[]")
                {
                    // Definition of variables that will be reused through this method
                    int id_user;
                    string query;

                    // Connect to the database using our local credentials
                    string db_string = @"server=127.0.0.1;uid=root;password=Tijuana13!;database=db_ternium";
                    MySqlConnection conexion = new MySqlConnection(db_string);
                    conexion.Open();

                    // For better readability, use caps accordingly in sql commands
                    query = "SELECT id_user FROM user WHERE username = @username";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@username", User.Username);
                    var reader = cmd.ExecuteReader();

                    // Read returns false when no values were obtained from the query above
                    if (!reader.Read())
                    {
                        // Closes the previous connection read to allow further queries
                        conexion.Close();

                        // Creates a new user and inserts it to the users table
                        conexion.Open();
                        query = "INSERT INTO user(username, department) VALUES (@username, @department)";
                        cmd = new MySqlCommand(query, conexion);
                        cmd.Parameters.AddWithValue("@username", User.Username);
                        cmd.Parameters.AddWithValue("@department", "Chatarreria"); // Here Chatarreria is used as a default type
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                        conexion.Close();
                    }
                    else
                    {
                        // Closes the previous connection read to allow further queries
                        conexion.Close();
                    }

                    // With either an old user or one just created above, its login is recorded in the session table
                    // Part 1: The users id is recovered from the users table
                    conexion.Open();
                    query = "SELECT id_user FROM user WHERE username = @username";
                    cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@username", User.Username);
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    id_user = Convert.ToInt32(reader["id_user"]);
                    conexion.Close();

                    // Part 2: The user id and the time are logged to the session table
                    conexion.Open();
                    query = "INSERT INTO session(id_user, timestamp) VALUES (@id_user, @timestamp)";
                    cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@id_user", id_user);
                    cmd.Parameters.AddWithValue("@timestamp", DateTime.Now);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    // The connection to the database is closed and we can allow through the logged user
                    conexion.Close();

                    // Session variables
                    var parsedObj = JObject.Parse(responseContent);
                    HttpContext.Session.SetString("id", parsedObj["user"].ToString());
                    HttpContext.Session.SetString("username", User.Username);
                    HttpContext.Session.SetString("departments", @"['Chatarreria', 'Test']");
                    HttpContext.Session.SetString("token", parsedObj["token"].ToString());

                    return RedirectToPage("/Profile");
                }
                else
                {
                    Error = "Credenciales incorrectas";
                    return Page();
                }
            }
            else
            {
                Error = "Credenciales incorrectas";
                return Page();
            }
        }
    }
}