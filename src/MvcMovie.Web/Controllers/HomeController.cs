using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MvcMovie.Web.Models;

using Serilog;

using System.Diagnostics;

namespace MvcMovie.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDiagnosticContext _diagnosticContext;

        public HomeController(ILogger<HomeController> logger,
            IDiagnosticContext diagnosticContext)
        {
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public IActionResult Index()
        {
            _diagnosticContext.Set("Home", nameof(Index));
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
