using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http.Headers; // Required for MediaTypeHeaderValue
using Microsoft.AspNetCore.Http; // Required for IFormFile

namespace RaceDay.MVC.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = new HttpClient();

            // MUST MATCH YOUR API PORT
            _httpClient.BaseAddress = new Uri("https://localhost:7177/");
        }

        private void AttachAuthCookie()
        {
            _httpClient.DefaultRequestHeaders.Clear();
            var cookie = _httpContextAccessor.HttpContext?.Session.GetString("AuthCookie");

            if (!string.IsNullOrEmpty(cookie))
            {
                // Find the specific Identity cookie
                var match = Regex.Match(cookie, @"(\.AspNetCore\.Identity\.[^=]+)=([^;]+)");
                if (match.Success)
                {
                    _httpClient.DefaultRequestHeaders.Add("Cookie", $"{match.Groups[1].Value}={match.Groups[2].Value}");
                }
                else
                {
                    // Fallback
                    var firstPart = cookie.Split(';')[0];
                    if (firstPart.Contains("="))
                    {
                        _httpClient.DefaultRequestHeaders.Add("Cookie", firstPart);
                    }
                }
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            AttachAuthCookie();
            var response = await _httpClient.GetAsync(endpoint);

            // Debugging: See what the API returns in the Output window
            Console.WriteLine($"GET {endpoint} -> {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return default;
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
        {
            AttachAuthCookie();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode && response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var authCookie = cookies.FirstOrDefault();
                if (!string.IsNullOrEmpty(authCookie))
                {
                    _httpContextAccessor.HttpContext?.Session.SetString("AuthCookie", authCookie);
                }
            }

            return response;
        }

        // POST Request (For File Uploads)
        public async Task<HttpResponseMessage> PostFileAsync(string endpoint, IFormFile file)
        {
            AttachAuthCookie();

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            var fileContent = new StreamContent(fileStream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            return await _httpClient.PostAsync(endpoint, content);
        }

        // PUT Request
        public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
        {
            AttachAuthCookie();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await _httpClient.PutAsync(endpoint, content);
        }
    }
}