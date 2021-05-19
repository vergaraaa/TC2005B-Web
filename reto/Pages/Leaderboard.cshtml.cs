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

        public class Helper
        {
            public string Username { get; set; }
            public int Score { get; set; }
        }
        [BindProperty]
        public IList<Helper> Leaderboard { get ; set; }

        public string Username { get; set; }

        public async Task OnGet()
        {
            Username = HttpContext.Session.GetString("username");

            Uri baseURL = new Uri("https://chatarrap-api.herokuapp.com/attempts/scores");
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("auth_key", HttpContext.Session.GetString("token"));

            var response = await client.GetAsync(baseURL);
            var json = await response.Content.ReadAsStringAsync();

            Leaderboard = JsonConvert.DeserializeObject<List<Helper>>(json);
        }

        [BindProperty]
        public int OptionTime { get; set; }
        /*public async Task OnPost()
        {
            //string url = "";

            if (OptionTime == 0)
            {
                Uri baseURL = new Uri("https://chatarrap-api.herokuapp.com/attempts/scoresWeek");
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Add("auth_key", HttpContext.Session.GetString("token"));

                var response = await client.GetAsync(baseURL);
                var json = await response.Content.ReadAsStringAsync();

                Leaderboard = JsonConvert.DeserializeObject<List<Helper>>(json);
            }
            else if(OptionTime == 1)
            {
                Uri baseURL = new Uri("https://chatarrap-api.herokuapp.com/attempts/scores");
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Add("auth_key", HttpContext.Session.GetString("token"));

                var response = await client.GetAsync(baseURL);
                var json = await response.Content.ReadAsStringAsync();

                Leaderboard = JsonConvert.DeserializeObject<List<Helper>>(json);
            }

            /*HttpClient client = new HttpClient();
            Uri baseURL = new Uri(url);

            client.DefaultRequestHeaders.Add("auth_key", HttpContext.Session.GetString("token"));

            var response = await client.GetAsync(baseURL);
            var json = await response.Content.ReadAsStringAsync();

            Leaderboard = JsonConvert.DeserializeObject<List<Helper>>(json);
        }*/
    }
}
