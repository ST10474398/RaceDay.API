using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
using System.Linq;
using Microsoft.AspNetCore.Http; // Required for IFormFile

namespace RaceDay.MVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApiService _apiService;

        public EventsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Events/Create
        [HttpGet]
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser" && role != "Admin")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        public async Task<IActionResult> Create(EventViewModel model, IFormFile imageFile)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Upload the image to the API (which saves it locally)
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadResponse = await _apiService.PostFileAsync("api/Events/upload-image", imageFile);

                if (uploadResponse.IsSuccessStatusCode)
                {
                    // Read the returned URL from the API
                    var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<UploadResult>();
                    model.ImageUrl = uploadResult?.ImageUrl;
                }
                else
                {
                    ModelState.AddModelError("", "Failed to upload image. Please try again.");
                    return View(model);
                }
            }

            // 2. Create the event with the ImageUrl
            var response = await _apiService.PostAsync("api/Events", model);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Failed to create event. Check the API is running and CategoryID is valid.");
            return View(model);
        }

        // GET: Events/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var allEvents = await _apiService.GetAsync<List<EventViewModel>>("api/Events");
            var eventItem = allEvents?.FirstOrDefault(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound($"Event with ID {id} was not found.");
            }

            return View(eventItem);
        }

        // POST: Events/Enrol
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId)
        {
            if (HttpContext.Session.GetString("AuthCookie") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var response = await _apiService.PostAsync($"api/Events/{eventId}/enrol", new { });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully enrolled! Awaiting approval.";
                return RedirectToAction("Details", new { id = eventId });
            }

            // Read the error body from the API
            var errorContent = await response.Content.ReadAsStringAsync();

            // Pass the exact error to the view
            TempData["ErrorMessage"] = $"API Error ({(int)response.StatusCode}): {errorContent}";

            return RedirectToAction("Details", new { id = eventId });
        }
    }

    // --- Helper Class for Upload Response ---
    public class UploadResult
    {
        public string? ImageUrl { get; set; }
    }
}