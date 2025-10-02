using DBfirst.Models;
using Front_end.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Front_end.Controllers
{
    public class TeacherController : Controller
    {
        HttpClient _httpClient;
        private const string base_url = "http://localhost:5224";

        public TeacherController(HttpClient client)
        {
            _httpClient = client;
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IActionResult> Index()
        {
            var odataResponse = await _httpClient.GetFromJsonAsync<OdataResponse<List<Teacher>>>("http://localhost:5224/odata/Teachers?$expand=Subject");
            List<Teacher> teachers = odataResponse?.Value ?? new List<Teacher>();
            return View(teachers);
        }

        public async Task<IActionResult> Create()
        {
            var response = await _httpClient.GetStringAsync("http://localhost:5224/odata/Subjects");
            List<Subject> subjects = JsonConvert.DeserializeObject<List<Subject>>(response); 
            return View(subjects);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var teacherResponse = await _httpClient.GetFromJsonAsync<OdataResponse<List<Teacher>>>($"http://localhost:5224/odata/Teachers?$expand=Subject&filter=TeacherId eq {id}");
            var response = await _httpClient.GetStringAsync("http://localhost:5224/odata/Subjects");

            if (teacherResponse.Value == null)
            {
                return NotFound();
            }

            List<Subject> subjects = JsonConvert.DeserializeObject<List<Subject>>(response);

            var viewModel = new TeacherViewModel
            {
                Teacher = teacherResponse.Value[0],
                Subjects = subjects
            };
            
            return View(viewModel);
        }
    }

    public class TeacherViewModel
    {
        public Teacher Teacher { get; set; }
        public List<Subject> Subjects { get; set; }
    }
}
