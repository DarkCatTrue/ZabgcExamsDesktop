using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using ZabgcExamsDesktop.API.Models;

namespace ZabgcExamsDesktop.API
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7182/api/";

        public ApiService()
        {
            _httpClient = new HttpClient();
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            _httpClient = new HttpClient(handler);

            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> CreateExamAsync(CreateExamDto exam)
        {
            try
            {
                var json = JsonSerializer.Serialize(exam);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Sending POST to exams: {json}");

                var response = await _httpClient.PostAsync("exams", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Exam created successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create exam error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ExamDisplayDto>> SearchExamsAsync(
        int? departmentId = null,
        int? groupId = null,
        int? teacherId = null,
        int? audienceId = null)
        {
            try
            {
                var parameters = new List<string>();

                if (departmentId.HasValue)
                    parameters.Add($"departmentId={departmentId.Value}");

                if (groupId.HasValue)
                    parameters.Add($"groupId={groupId.Value}");

                if (teacherId.HasValue)
                    parameters.Add($"teacherId={teacherId.Value}");

                if (audienceId.HasValue)
                    parameters.Add($"audienceId={audienceId.Value}");

                var queryString = parameters.Any() ? "?" + string.Join("&", parameters) : "";
                var endpoint = $"exams/search{queryString}";

                Console.WriteLine($"Searching with URL: {endpoint}");

                return await GetAsync<List<ExamDisplayDto>>(endpoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search error: {ex.Message}");
                return new List<ExamDisplayDto>();
            }
        }

        public async Task<List<ExamDisplayDto>> GetExamsDisplayAsync()
        {
            return await GetAsync<List<ExamDisplayDto>>("exams/display");
        }
        // === CRUD для Departments ===
        public async Task<List<DepartmentDto>> GetDepartmentsAsync()
        {
            return await GetAsync<List<DepartmentDto>>("departments");
        }

        public async Task<DepartmentDto> GetDepartmentAsync(int id)
        {
            return await GetAsync<DepartmentDto>($"departments/{id}");
        }

        public async Task<bool> CreateDepartmentAsync(DepartmentDto department)
        {
            return await PostAsync("departments", department);
        }

        public async Task<bool> UpdateDepartmentAsync(DepartmentDto department)
        {
            return await PutAsync($"departments/{department.IdDepartment}", department);
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            return await DeleteAsync($"departments/{id}");
        }

        // === CRUD для Groups ===
        public async Task<List<GroupDto>> GetGroupsAsync()
        {
            return await GetAsync<List<GroupDto>>("groups");
        }

        public async Task<GroupDto> GetGroupAsync(int id)
        {
            return await GetAsync<GroupDto>($"groups/{id}");
        }

        public async Task<bool> CreateGroupAsync(GroupDto group)
        {
            return await PostAsync("groups", group);
        }

        public async Task<bool> UpdateGroupAsync(GroupDto group)
        {
            return await PutAsync($"groups/{group.IdGroup}", group);
        }

        public async Task<bool> DeleteGroupAsync(int id)
        {
            return await DeleteAsync($"groups/{id}");
        }

        // === CRUD для Teachers ===
        public async Task<List<TeacherDto>> GetTeachersAsync()
        {
            return await GetAsync<List<TeacherDto>>("teacher");
        }

        public async Task<TeacherDto> GetTeacherAsync(int id)
        {
            return await GetAsync<TeacherDto>($"teacher/{id}");
        }

        public async Task<bool> CreateTeacherAsync(TeacherDto teacher)
        {
            return await PostAsync("teacher", teacher);
        }

        public async Task<bool> UpdateTeacherAsync(TeacherDto teacher)
        {
            return await PutAsync($"teacher/{teacher.IdTeacher}", teacher);
        }

        public async Task<bool> DeleteTeacherAsync(int id)
        {
            return await DeleteAsync($"teacher/{id}");
        }

        // === CRUD для Audiences ===
        public async Task<List<AudienceDto>> GetAudiencesAsync()
        {
            return await GetAsync<List<AudienceDto>>("audiences");
        }

        public async Task<AudienceDto> GetAudienceAsync(int id)
        {
            return await GetAsync<AudienceDto>($"audiences/{id}");
        }

        public async Task<bool> CreateAudienceAsync(AudienceDto audience)
        {
            return await PostAsync("audiences", audience);
        }

        public async Task<bool> UpdateAudienceAsync(AudienceDto audience)
        {
            return await PutAsync($"audiences/{audience.IdAudience}", audience);
        }

        public async Task<bool> DeleteAudienceAsync(int id)
        {
            return await DeleteAsync($"audiences/{id}");
        }

        // === Методы CRUD для Типов занятий
        public async Task<List<TypeOfLessonDto>> GetTypeOfLessonsAsync()
        {
            return await GetAsync<List<TypeOfLessonDto>>("TypeOfLesson");
        }
        // === Методы CRUD для Типов экзамена
        public async Task<List<TypeOfExamDto>> GetTypeOfExamsAsync()
        {
            return await GetAsync<List<TypeOfExamDto>>("TypeOfExam");
        }
        // === Методы CRUD для Дисциплин
        public async Task<List<DisciplineDto>> GetDisciplinesAsync()
        {
            return await GetAsync<List<DisciplineDto>>("discipline");
        }
        // === Методы CRUD для Менеджеров
        public async Task<List<ManagerDto>> GetManagersAsync()
        {
            return await GetAsync<List<ManagerDto>>("manager");
        }
        // === Методы CRUD для Зав.Отделений
        public async Task<List<DepartmentOwnerDto>> GetDepartmentOwnersAsync()
        {
            return await GetAsync<List<DepartmentOwnerDto>>("departmentOwner");
        }
        // === Общие методы для HTTP запросов ===
        private async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    // Проверяем, что json не пустой
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return default(T);
                    }

                    var result = JsonSerializer.Deserialize<T>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result ?? default(T);
                }
                else
                {
                    // Логируем ошибку HTTP
                    MessageBox.Show($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API Request Error: {ex.Message}");
                return default(T);
            }
        }
        public async Task<bool> DeleteExamAsync(int id)
        {
            return await DeleteAsync($"exams/{id}");
        }

        private async Task<bool> PostAsync<T>(string endpoint, T data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }

        private async Task<bool> PutAsync<T>(string endpoint, T data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }

        private async Task<bool> DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
    }
}
