using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;

        public HomeController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index()
        {
            // Fetch ALL events from the API
            var events = await _apiService.GetAsync<List<EventViewModel>>("api/Events");

            return View(events ?? new List<EventViewModel>());
        }

        // GET: Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }
    }
}