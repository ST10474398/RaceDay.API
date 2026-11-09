using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiService _apiService;

        public ParticipantController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Participant/MyEvents
        public async Task<IActionResult> MyEvents()
        {
            if (HttpContext.Session.GetString("AuthCookie") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch raw JSON to handle the nested Event object
            var json = await _apiService.GetAsync<List<JsonElement>>("api/Enrolments/my");

            var myEnrolments = new List<MyEnrolmentViewModel>();

            if (json != null)
            {
                foreach (var item in json)
                {
                    var enrolment = new MyEnrolmentViewModel
                    {
                        EnrolmentID = item.GetProperty("enrolmentID").GetInt32(),
                        Status = item.GetProperty("status").GetString() ?? "Pending",
                        EnrolledAt = item.GetProperty("enrolledAt").GetDateTime(),
                    };

                    // Extract Event details from the nested object
                    if (item.TryGetProperty("event", out var eventElement) && eventElement.ValueKind != JsonValueKind.Null)
                    {
                        enrolment.EventID = eventElement.GetProperty("eventID").GetInt32();
                        enrolment.EventName = eventElement.GetProperty("name").GetString() ?? "";
                        enrolment.EventLocation = eventElement.GetProperty("location").GetString() ?? "";
                        enrolment.EventDate = eventElement.GetProperty("eventDate").GetDateTime();
                    }

                    myEnrolments.Add(enrolment);
                }
            }

            return View(myEnrolments);
        }
    
            // GET: Participant/MyResults
        public async Task<IActionResult> MyResults()
        {
            if (HttpContext.Session.GetString("AuthCookie") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch the results for this user
            var results = await _apiService.GetAsync<List<JsonElement>>("api/Results/me");

            var myResults = new List<MyResultViewModel>();

            if (results != null)
            {
                foreach (var item in results)
                {
                    var enrolmentElement = item.GetProperty("enrolment");
                    var eventElement = enrolmentElement.GetProperty("event");

                    myResults.Add(new MyResultViewModel
                    {
                        EventName = eventElement.GetProperty("name").GetString() ?? "Unknown Event",
                        EventDate = eventElement.GetProperty("eventDate").GetDateTime(),
                        FinishTime = item.GetProperty("finishTime").GetString() ?? "00:00:00",
                        Position = item.GetProperty("position").GetInt32(),
                        IsVerified = item.GetProperty("isVerified").GetBoolean()
                    });
                }
            }

            return View(myResults);
        }
    }
}