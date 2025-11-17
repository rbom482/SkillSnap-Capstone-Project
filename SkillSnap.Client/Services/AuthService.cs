using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace SkillSnap.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private string? _currentToken;
        private UserInfo? _currentUser;

        public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public event EventHandler<AuthenticationStateChangedEventArgs>? AuthenticationStateChanged;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_currentToken);
        public UserInfo? CurrentUser => _currentUser;

        /// <summary>
        /// Initialize authentication state from stored token
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                var token = await GetTokenFromStorageAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    await SetTokenAsync(token);
                    await LoadCurrentUserAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing auth service: {ex.Message}");
                await ClearAuthenticationAsync();
            }
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                var loginData = new { email, password };
                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/auth/login", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (loginResponse?.Token != null)
                    {
                        await SetTokenAsync(loginResponse.Token);
                        _currentUser = loginResponse.User;
                        
                        // Store token in local storage
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", loginResponse.Token);
                        
                        NotifyAuthenticationStateChanged(true);
                        return AuthResult.Success("Login successful");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return AuthResult.Failure(errorResponse?.Message ?? "Login failed");
                }
            }
            catch (Exception ex)
            {
                return AuthResult.Failure($"Login error: {ex.Message}");
            }

            return AuthResult.Failure("Login failed");
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        public async Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName)
        {
            try
            {
                var registerData = new { email, password, firstName, lastName };
                var json = JsonSerializer.Serialize(registerData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/auth/register", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var registerResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (registerResponse?.Token != null)
                    {
                        await SetTokenAsync(registerResponse.Token);
                        _currentUser = registerResponse.User;
                        
                        // Store token in local storage
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", registerResponse.Token);
                        
                        NotifyAuthenticationStateChanged(true);
                        return AuthResult.Success("Registration successful");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return AuthResult.Failure(errorResponse?.Message ?? "Registration failed");
                }
            }
            catch (Exception ex)
            {
                return AuthResult.Failure($"Registration error: {ex.Message}");
            }

            return AuthResult.Failure("Registration failed");
        }

        /// <summary>
        /// Logout and clear authentication data
        /// </summary>
        public async Task LogoutAsync()
        {
            await ClearAuthenticationAsync();
            NotifyAuthenticationStateChanged(false);
        }

        /// <summary>
        /// Refresh the current token
        /// </summary>
        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("api/auth/refresh", null);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var refreshResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (refreshResponse?.Token != null)
                    {
                        await SetTokenAsync(refreshResponse.Token);
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", refreshResponse.Token);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token refresh failed: {ex.Message}");
            }

            await ClearAuthenticationAsync();
            return false;
        }

        private async Task SetTokenAsync(string token)
        {
            _currentToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task ClearAuthenticationAsync()
        {
            _currentToken = null;
            _currentUser = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing token from storage: {ex.Message}");
            }
        }

        private async Task<string?> GetTokenFromStorageAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting token from storage: {ex.Message}");
                return null;
            }
        }

        private async Task LoadCurrentUserAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/auth/me");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _currentUser = JsonSerializer.Deserialize<UserInfo>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    // Token might be expired or invalid
                    await ClearAuthenticationAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading current user: {ex.Message}");
                await ClearAuthenticationAsync();
            }
        }

        private void NotifyAuthenticationStateChanged(bool isAuthenticated)
        {
            AuthenticationStateChanged?.Invoke(this, new AuthenticationStateChangedEventArgs(isAuthenticated));
        }
    }

    // Data models
    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsAdmin => Roles.Contains("Admin");
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new();
    }

    public class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    public class AuthResult
    {
        public bool IsSuccess { get; }
        public string Message { get; }

        private AuthResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public static AuthResult Success(string message = "") => new(true, message);
        public static AuthResult Failure(string message) => new(false, message);
    }

    public class AuthenticationStateChangedEventArgs : EventArgs
    {
        public bool IsAuthenticated { get; }

        public AuthenticationStateChangedEventArgs(bool isAuthenticated)
        {
            IsAuthenticated = isAuthenticated;
        }
    }
}