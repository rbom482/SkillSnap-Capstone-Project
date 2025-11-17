using System.Text;
using System.Text.Json;

namespace SkillSnap.Client.Services
{
    public class SkillService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public SkillService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        /// <summary>
        /// Get all skills from the API
        /// </summary>
        /// <returns>List of skills</returns>
        public async Task<List<Skill>> GetSkillAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/skills");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var skills = JsonSerializer.Deserialize<List<Skill>>(json, _jsonOptions);
                    return skills ?? new List<Skill>();
                }
                else
                {
                    Console.WriteLine($"Error fetching skills: {response.StatusCode}");
                    return new List<Skill>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetSkillAsync: {ex.Message}");
                return new List<Skill>();
            }
        }

        /// <summary>
        /// Get a specific skill by ID
        /// </summary>
        /// <param name="id">Skill ID</param>
        /// <returns>Skill details or null</returns>
        public async Task<Skill?> GetSkillByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/skills/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Skill>(json, _jsonOptions);
                }
                else
                {
                    Console.WriteLine($"Error fetching skill {id}: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetSkillByIdAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Add a new skill
        /// </summary>
        /// <param name="newSkill">Skill to add</param>
        /// <returns>Created skill or null if failed</returns>
        public async Task<Skill?> AddSkillAsync(Skill newSkill)
        {
            try
            {
                var json = JsonSerializer.Serialize(newSkill, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/skills", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Skill>(responseJson, _jsonOptions);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error creating skill: {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AddSkillAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Update an existing skill
        /// </summary>
        /// <param name="skill">Skill to update</param>
        /// <returns>Updated skill or null if failed</returns>
        public async Task<Skill?> UpdateSkillAsync(Skill skill)
        {
            try
            {
                var json = JsonSerializer.Serialize(skill, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"api/skills/{skill.Id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Skill>(responseJson, _jsonOptions);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error updating skill: {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateSkillAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Delete a skill
        /// </summary>
        /// <param name="id">Skill ID</param>
        /// <returns>True if successful</returns>
        public async Task<bool> DeleteSkillAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/skills/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DeleteSkillAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get skills for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's skills</returns>
        public async Task<List<Skill>> GetSkillsByUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/skills/user/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var skills = JsonSerializer.Deserialize<List<Skill>>(json, _jsonOptions);
                    return skills ?? new List<Skill>();
                }
                else
                {
                    Console.WriteLine($"Error fetching user skills: {response.StatusCode}");
                    return new List<Skill>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetSkillsByUserAsync: {ex.Message}");
                return new List<Skill>();
            }
        }

        /// <summary>
        /// Get skills grouped by level for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Skills grouped by level</returns>
        public async Task<List<SkillGroup>> GetSkillsByUserGroupedAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/skills/user/{userId}/by-level");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var skillGroups = JsonSerializer.Deserialize<List<SkillGroup>>(json, _jsonOptions);
                    return skillGroups ?? new List<SkillGroup>();
                }
                else
                {
                    Console.WriteLine($"Error fetching grouped skills: {response.StatusCode}");
                    return new List<SkillGroup>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetSkillsByUserGroupedAsync: {ex.Message}");
                return new List<SkillGroup>();
            }
        }
    }

    // Skill model for client-side use
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public int PortfolioUserId { get; set; }
        public PortfolioUser? PortfolioUser { get; set; }
    }

    // SkillGroup model for grouped skills
    public class SkillGroup
    {
        public string Level { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<Skill> Skills { get; set; } = new();
    }
}