using System.Text;
using System.Text.Json;

namespace SkillSnap.Client.Services
{
    public class ProjectService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProjectService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        /// <summary>
        /// Get all projects from the API
        /// </summary>
        /// <returns>List of projects</returns>
        public async Task<List<Project>> GetProjectAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/projects");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var projects = JsonSerializer.Deserialize<List<Project>>(json, _jsonOptions);
                    return projects ?? new List<Project>();
                }
                else
                {
                    Console.WriteLine($"Error fetching projects: {response.StatusCode}");
                    return new List<Project>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetProjectAsync: {ex.Message}");
                return new List<Project>();
            }
        }

        /// <summary>
        /// Get a specific project by ID
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project details or null</returns>
        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/projects/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Project>(json, _jsonOptions);
                }
                else
                {
                    Console.WriteLine($"Error fetching project {id}: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetProjectByIdAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Add a new project with comprehensive validation and error handling
        /// </summary>
        /// <param name="newProject">Project to add</param>
        /// <returns>API result with success status and data</returns>
        public async Task<ApiResult<Project>> AddProjectAsync(Project newProject)
        {
            // Client-side validation
            var validationResult = ValidateProject(newProject);
            if (!validationResult.IsValid)
            {
                return ApiResult<Project>.Failure(validationResult.ErrorMessage!);
            }

            try
            {
                var json = JsonSerializer.Serialize(newProject, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/projects", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var project = JsonSerializer.Deserialize<Project>(responseJson, _jsonOptions);
                    return ApiResult<Project>.Success(project!);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var userFriendlyMessage = GetUserFriendlyErrorMessage(response.StatusCode, errorContent);
                    Console.WriteLine($"Error creating project: {response.StatusCode} - {errorContent}");
                    return ApiResult<Project>.Failure(userFriendlyMessage);
                }
            }
            catch (HttpRequestException httpEx)
            {
                var message = "Unable to connect to the server. Please check your internet connection and try again.";
                Console.WriteLine($"HTTP Exception in AddProjectAsync: {httpEx.Message}");
                return ApiResult<Project>.Failure(message);
            }
            catch (TaskCanceledException)
            {
                var message = "The request timed out. Please try again.";
                return ApiResult<Project>.Failure(message);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred. Please try again later.";
                Console.WriteLine($"Exception in AddProjectAsync: {ex.Message}");
                return ApiResult<Project>.Failure(message);
            }
        }

        /// <summary>
        /// Update an existing project
        /// </summary>
        /// <param name="project">Project to update</param>
        /// <returns>Updated project or null if failed</returns>
        public async Task<Project?> UpdateProjectAsync(Project project)
        {
            try
            {
                var json = JsonSerializer.Serialize(project, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"api/projects/{project.Id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Project>(responseJson, _jsonOptions);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error updating project: {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateProjectAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Delete a project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>True if successful</returns>
        public async Task<bool> DeleteProjectAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/projects/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DeleteProjectAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get projects for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's projects</returns>
        public async Task<List<Project>> GetProjectsByUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/projects/user/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var projects = JsonSerializer.Deserialize<List<Project>>(json, _jsonOptions);
                    return projects ?? new List<Project>();
                }
                else
                {
                    Console.WriteLine($"Error fetching user projects: {response.StatusCode}");
                    return new List<Project>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetProjectsByUserAsync: {ex.Message}");
                return new List<Project>();
            }
        }

        /// <summary>
        /// Validates a project before sending to API
        /// </summary>
        private ValidationResult ValidateProject(Project project)
        {
            if (string.IsNullOrWhiteSpace(project.Title))
            {
                return new ValidationResult(false, "Project title is required.");
            }

            if (project.Title.Length > 200)
            {
                return new ValidationResult(false, "Project title must be 200 characters or less.");
            }

            if (string.IsNullOrWhiteSpace(project.Description))
            {
                return new ValidationResult(false, "Project description is required.");
            }

            if (project.Description.Length > 1000)
            {
                return new ValidationResult(false, "Project description must be 1000 characters or less.");
            }

            if (!string.IsNullOrWhiteSpace(project.GitHubUrl) && !IsValidUrl(project.GitHubUrl))
            {
                return new ValidationResult(false, "GitHub URL must be a valid URL.");
            }

            if (!string.IsNullOrWhiteSpace(project.LiveDemoUrl) && !IsValidUrl(project.LiveDemoUrl))
            {
                return new ValidationResult(false, "Live demo URL must be a valid URL.");
            }

            return new ValidationResult(true);
        }

        /// <summary>
        /// Validates if a string is a valid URL
        /// </summary>
        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result) && 
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Converts technical error messages to user-friendly ones
        /// </summary>
        private string GetUserFriendlyErrorMessage(System.Net.HttpStatusCode statusCode, string errorContent)
        {
            return statusCode switch
            {
                System.Net.HttpStatusCode.BadRequest => "Invalid project data. Please check your input and try again.",
                System.Net.HttpStatusCode.Unauthorized => "You are not authorized to perform this action.",
                System.Net.HttpStatusCode.Forbidden => "You don't have permission to create projects.",
                System.Net.HttpStatusCode.NotFound => "The requested resource was not found.",
                System.Net.HttpStatusCode.Conflict => "A project with similar data already exists.",
                System.Net.HttpStatusCode.InternalServerError => "A server error occurred. Please try again later.",
                System.Net.HttpStatusCode.ServiceUnavailable => "The service is temporarily unavailable. Please try again later.",
                _ => "An error occurred while creating the project. Please try again."
            };
        }
    }

    // Project model for client-side use
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? GitHubUrl { get; set; }
        public string? LiveDemoUrl { get; set; }
        public string? Technologies { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int PortfolioUserId { get; set; }
        public PortfolioUser? PortfolioUser { get; set; }
    }

    // PortfolioUser model for client-side use
    public class PortfolioUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents the result of an API operation
    /// </summary>
    public class ApiResult<T>
    {
        public bool IsSuccess { get; init; }
        public T? Data { get; init; }
        public string? ErrorMessage { get; init; }

        private ApiResult(bool isSuccess, T? data, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
        }

        public static ApiResult<T> Success(T data) => new(true, data, null);
        public static ApiResult<T> Failure(string errorMessage) => new(false, default, errorMessage);
    }

    /// <summary>
    /// Represents a validation result
    /// </summary>
    public record ValidationResult(bool IsValid, string? ErrorMessage = null);
}