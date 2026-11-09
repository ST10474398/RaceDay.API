using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
using System.Text.Json;
using System.Linq;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiService _apiService;

        public OrganiserController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // ==========================================
        // 1. GET: Organiser/Dashboard
        // ==========================================
        public async Task<IActionResult> Dashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser" && role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var events = await _apiService.GetAsync<List<EventViewModel>>("api/Events");

            if (events != null)
            {
                foreach (var ev in events)
                {
                    var enrolments = await _apiService.GetAsync<List<object>>($"api/Events/{ev.EventID}/enrolments");
                    ev.TotalEnrolments = enrolments?.Count ?? 0;
                }
            }

            return View(events ?? new List<EventViewModel>());
        }

        // ==========================================
        // 2. GET: Organiser/ViewEnrolments/5
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ViewEnrolments(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser" && role != "Admin") return RedirectToAction("Login", "Account");

            var allEvents = await _apiService.GetAsync<List<EventViewModel>>("api/Events");
            var eventItem = allEvents?.FirstOrDefault(e => e.EventID == id);

            if (eventItem == null) return NotFound("Event not found.");

            var enrolments = await _apiService.GetAsync<List<JsonElement>>($"api/Events/{id}/enrolments");
            var participantList = new List<EnrolmentViewModel>();

            if (enrolments != null)
            {
                foreach (var en in enrolments)
                {
                    var userElement = en.GetProperty("user");
                    var participantName = userElement.ValueKind != JsonValueKind.Null
                        ? userElement.GetProperty("fullName").GetString()
                        : "Unknown User";

                    var participantEmail = userElement.ValueKind != JsonValueKind.Null
                        ? userElement.GetProperty("email").GetString()
                        : "Unknown Email";

                    participantList.Add(new EnrolmentViewModel
                    {
                        EnrolmentID = en.GetProperty("enrolmentID").GetInt32(),
                        Status = en.GetProperty("status").GetString() ?? "Pending",
                        ParticipantName = participantName ?? "Unknown",
                        ParticipantEmail = participantEmail ?? "Unknown",
                        EnrolledAt = en.GetProperty("enrolledAt").GetDateTime()
                    });
                }
            }

            ViewBag.EventName = eventItem.Name;
            ViewBag.EventID = eventItem.EventID;

            return View(participantList);
        }

        // ==========================================
        // 3. POST: Organiser/UpdateEnrolmentStatus
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> UpdateEnrolmentStatus(int enrolmentId, int eventId, string status)
        {
            var response = await _apiService.PutAsync($"api/Enrolments/{enrolmentId}/status", new { Status = status });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Enrolment {status} successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update status.";
            }

            return RedirectToAction("ViewEnrolments", new { id = eventId });
        }

        // ==========================================
        // 4. GET: Organiser/EnterResults/5
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> EnterResults(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser" && role != "Admin") return RedirectToAction("Login", "Account");

            var allEvents = await _apiService.GetAsync<List<EventViewModel>>("api/Events");
            var eventItem = allEvents?.FirstOrDefault(e => e.EventID == id);
            if (eventItem == null) return NotFound("Event not found.");

            var enrolments = await _apiService.GetAsync<List<JsonElement>>($"api/Events/{id}/enrolments");
            var participantList = new List<EnrolmentViewModel>();

            if (enrolments != null)
            {
                foreach (var en in enrolments)
                {
                    // Only include Approved participants
                    if (en.GetProperty("status").GetString() != "Approved") continue;

                    var userElement = en.GetProperty("user");
                    participantList.Add(new EnrolmentViewModel
                    {
                        EnrolmentID = en.GetProperty("enrolmentID").GetInt32(),
                        ParticipantName = userElement.ValueKind != JsonValueKind.Null ? userElement.GetProperty("fullName").GetString() ?? "Unknown" : "Unknown",
                        Status = "Approved"
                    });
                }
            }

            ViewBag.EventName = eventItem.Name;
            ViewBag.EventID = eventItem.EventID;
            return View(participantList);
        }

        // ==========================================
        // 5. POST: Organiser/EnterResults
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> EnterResults(int enrolmentId, int eventId, string finishTime, int position)
        {
            try
            {
                var timeSpan = TimeSpan.Parse(finishTime);
                var resultData = new
                {
                    EnrolmentID = enrolmentId,
                    FinishTime = timeSpan,
                    Position = position,
                    IsVerified = true
                };

                var response = await _apiService.PostAsync("api/Results", resultData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Result recorded successfully!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Failed to record result: {error}";
                }
            }
            catch (FormatException)
            {
                TempData["ErrorMessage"] = "Invalid time format. Please use HH:MM:SS (e.g., 01:30:00).";
            }

            return RedirectToAction("EnterResults", new { id = eventId });
        }
    }
}