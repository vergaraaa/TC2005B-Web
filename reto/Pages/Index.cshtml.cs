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
                    // Definimos las variables que usaremos en este método
                    int id_user;
                    string query;

                    // Conectamos la base de datos
                    string db_string = @"server=127.0.0.1;uid=root;password=Tijuana13!;database=db_ternium";
                    MySqlConnection conexion = new MySqlConnection(db_string);
                    conexion.Open();

                    query = "SELECT id_user FROM user WHERE username = @username";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@username", User.Username);
                    var reader = cmd.ExecuteReader();

                    // Read regresa falso cuando no se obtiene ningún valor de la query
                    if (!reader.Read())
                    { 
                        // Cerramos la conexión anterior para permitir futuras queries
                        conexion.Close();

                        // Crea un nuevo usuario y lo inserta en la tabla de usuarios
                        conexion.Open();
                        query = "INSERT INTO user(username) VALUES (@username)";
                        cmd = new MySqlCommand(query, conexion);
                        cmd.Parameters.AddWithValue("@username", User.Username);
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                        conexion.Close();
                    }
                    else
                    {
                        // Cerramos la conexión para permitir futuras queries
                        conexion.Close();
                    }

                    // Ya sea un nuevo usuario o ya registrado, insertamos la sesión en la base de datos
                    // Obtenemos el id del usuario
                    conexion.Open();
                    query = "SELECT id_user FROM user WHERE username = @username";
                    cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@username", User.Username);
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    id_user = Convert.ToInt32(reader["id_user"]);
                    conexion.Close();

                    // Insertamos la sesión en la base de datos
                    conexion.Open();
                    query = "INSERT INTO session(id_user, timestamp) VALUES (@id_user, @timestamp)";
                    cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@id_user", id_user);
                    cmd.Parameters.AddWithValue("@timestamp", DateTime.Now);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    // Cerramos la conexión a la base de datos
                    conexion.Close();

                    // Establecer variables de sesión
                    var parsedObj = JObject.Parse(responseContent);
                    HttpContext.Session.SetString("id", parsedObj["user"].ToString());
                    HttpContext.Session.SetString("username", User.Username);
                    HttpContext.Session.SetString("token", parsedObj["token"].ToString());
                    HttpContext.Session.SetInt32("local_id", id_user);

                    // Código para guardar las variables de sesión
                    client.DefaultRequestHeaders.Add("auth_key", parsedObj["token"].ToString());

                    response = await client.GetAsync("https://chatarrap-api.herokuapp.com/users/" + parsedObj["user"].ToString());
                    var json = await response.Content.ReadAsStringAsync();
                    string departments = JObject.Parse(json)["usertype"].ToString();
                    HttpContext.Session.SetString("departments", departments);
                    
                    return new RedirectToPageResult("/Profile");
                }
                else
                {
                    // Si hay error regresamos al login
                    Error = "Credenciales incorrectas";
                    return Page();
                }
            }
            else
            {
                // Si hay error regresamos al login
                Error = "Credenciales incorrectas";
                return Page();
            }
        }
    }
}