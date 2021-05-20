using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

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
        public int OptionTime { get; set; }
        public async Task OnPost()
        {
            Username = HttpContext.Session.GetString("username");
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

            var respuesta = await client.GetAsync(baseURL);
            var json = await respuesta.Content.ReadAsStringAsync();
            Leaderboard = JsonConvert.DeserializeObject<List<Helper>>(json);
        }
    }
}
