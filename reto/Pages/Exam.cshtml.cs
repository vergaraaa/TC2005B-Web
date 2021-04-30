using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace reto.Pages
{
    public class ExamModel : PageModel
    {

        private readonly ILogger<ExamModel> _logger;

        public ExamModel(ILogger<ExamModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}
