using SkillSnap.Client.Services;

namespace SkillSnap.Client.Services
{
    /// <summary>
    /// Comprehensive state management service for Blazor components
    /// Manages user session data, editing states, and cross-component data persistence
    /// </summary>
    public class UserSessionService
    {
        private readonly AuthService _authService;
        private SessionState _currentSession = new();

        public UserSessionService(AuthService authService)
        {
            _authService = authService;
            
            // Subscribe to authentication state changes
            _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        public event EventHandler<SessionStateChangedEventArgs>? SessionStateChanged;

        #region User Information Properties
        
        /// <summary>
        /// Current authenticated user ID
        /// </summary>
        public string? UserId => _authService.CurrentUser?.Id;

        /// <summary>
        /// Current user's roles
        /// </summary>
        public List<string> UserRoles => _authService.CurrentUser?.Roles ?? new List<string>();

        /// <summary>
        /// Check if current user has admin role
        /// </summary>
        public bool IsAdmin => _authService.CurrentUser?.IsAdmin ?? false;

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        public bool IsAuthenticated => _authService.IsAuthenticated;

        /// <summary>
        /// Current user information
        /// </summary>
        public UserInfo? CurrentUser => _authService.CurrentUser;

        /// <summary>
        /// Current session state
        /// </summary>
        public SessionState CurrentSession => _currentSession;

        #endregion

        #region Project Editing State

        /// <summary>
        /// Set current project being edited
        /// </summary>
        public void SetEditingProject(int? projectId, string? projectTitle = null)
        {
            var previousProjectId = _currentSession.EditingProjectId;
            
            _currentSession.EditingProjectId = projectId;
            _currentSession.EditingProjectTitle = projectTitle;
            _currentSession.LastEditedProject = projectId.HasValue ? DateTime.UtcNow : null;
            
            OnSessionStateChanged("EditingProject", previousProjectId, projectId);
        }

        /// <summary>
        /// Clear project editing state
        /// </summary>
        public void ClearEditingProject()
        {
            SetEditingProject(null);
        }

        /// <summary>
        /// Check if currently editing a project
        /// </summary>
        public bool IsEditingProject => _currentSession.EditingProjectId.HasValue;

        /// <summary>
        /// Get current editing project ID
        /// </summary>
        public int? EditingProjectId => _currentSession.EditingProjectId;

        #endregion

        #region Skill Editing State

        /// <summary>
        /// Set current skill being edited
        /// </summary>
        public void SetEditingSkill(int? skillId, string? skillName = null)
        {
            var previousSkillId = _currentSession.EditingSkillId;
            
            _currentSession.EditingSkillId = skillId;
            _currentSession.EditingSkillName = skillName;
            _currentSession.LastEditedSkill = skillId.HasValue ? DateTime.UtcNow : null;
            
            OnSessionStateChanged("EditingSkill", previousSkillId, skillId);
        }

        /// <summary>
        /// Clear skill editing state
        /// </summary>
        public void ClearEditingSkill()
        {
            SetEditingSkill(null);
        }

        /// <summary>
        /// Check if currently editing a skill
        /// </summary>
        public bool IsEditingSkill => _currentSession.EditingSkillId.HasValue;

        /// <summary>
        /// Get current editing skill ID
        /// </summary>
        public int? EditingSkillId => _currentSession.EditingSkillId;

        #endregion

        #region Navigation State

        /// <summary>
        /// Set current page/view state
        /// </summary>
        public void SetCurrentView(string viewName, Dictionary<string, object>? parameters = null)
        {
            var oldView = _currentSession.CurrentView;
            
            _currentSession.CurrentView = viewName;
            _currentSession.ViewParameters = parameters ?? new Dictionary<string, object>();
            _currentSession.LastViewChange = DateTime.UtcNow;
            _navigationCount++;
            
            OnSessionStateChanged("CurrentView", oldView, viewName);
        }

        /// <summary>
        /// Get current view name
        /// </summary>
        public string CurrentView => _currentSession.CurrentView;

        /// <summary>
        /// Get current view parameters
        /// </summary>
        public Dictionary<string, object> ViewParameters => _currentSession.ViewParameters;

        /// <summary>
        /// Get current page/view name for display
        /// </summary>
        public string? CurrentPage => _currentSession.CurrentView;

        /// <summary>
        /// Get navigation count (view changes)
        /// </summary>
        public int NavigationCount => _navigationCount;
        private int _navigationCount = 0;

        /// <summary>
        /// Get editing start time
        /// </summary>
        public DateTime? EditingStartTime => _currentSession.EditingProjectId.HasValue ? 
            _currentSession.LastEditedProject : 
            _currentSession.EditingSkillId.HasValue ? _currentSession.LastEditedSkill : null;

        /// <summary>
        /// Get session start time
        /// </summary>
        public DateTime SessionStartTime => _currentSession.SessionStart;

        #endregion

        #region Performance Tracking

        /// <summary>
        /// Record performance metrics for monitoring
        /// </summary>
        public void RecordPerformanceMetric(string operation, TimeSpan duration, bool cacheHit = false)
        {
            var metric = new PerformanceMetric
            {
                Operation = operation,
                Duration = duration,
                Timestamp = DateTime.UtcNow,
                CacheHit = cacheHit,
                UserId = UserId
            };

            _currentSession.PerformanceMetrics.Add(metric);
            
            // Keep only last 100 metrics to prevent memory bloat
            if (_currentSession.PerformanceMetrics.Count > 100)
            {
                _currentSession.PerformanceMetrics.RemoveAt(0);
            }

            OnSessionStateChanged("PerformanceMetric", null, metric);
        }

        /// <summary>
        /// Get performance metrics for analysis
        /// </summary>
        public List<PerformanceMetric> GetPerformanceMetrics()
        {
            return new List<PerformanceMetric>(_currentSession.PerformanceMetrics);
        }

        /// <summary>
        /// Clear performance metrics
        /// </summary>
        public void ClearPerformanceMetrics()
        {
            _currentSession.PerformanceMetrics.Clear();
            OnSessionStateChanged("PerformanceMetricsCleared", null, null);
        }

        #endregion

        #region Session Management

        /// <summary>
        /// Initialize session state
        /// </summary>
        public async Task InitializeSessionAsync()
        {
            // Initialize authentication state first
            await _authService.InitializeAsync();
            
            // Reset session state on initialization
            _currentSession = new SessionState
            {
                SessionStart = DateTime.UtcNow,
                CurrentView = "Home"
            };
            
            OnSessionStateChanged("SessionInitialized", null, _currentSession);
        }

        /// <summary>
        /// Clear all session state
        /// </summary>
        public void ClearSession()
        {
            var oldSession = _currentSession;
            _currentSession = new SessionState();
            
            OnSessionStateChanged("SessionCleared", oldSession, _currentSession);
        }

        /// <summary>
        /// Get session duration
        /// </summary>
        public TimeSpan SessionDuration => DateTime.UtcNow - _currentSession.SessionStart;

        #endregion

        #region Event Handlers

        private void OnAuthenticationStateChanged(object? sender, AuthenticationStateChangedEventArgs e)
        {
            // Clear editing states when authentication changes
            if (!e.IsAuthenticated)
            {
                ClearEditingProject();
                ClearEditingSkill();
                ClearPerformanceMetrics();
            }
            
            OnSessionStateChanged("AuthenticationChanged", null, e.IsAuthenticated);
        }

        private void OnSessionStateChanged(string changeType, object? oldValue, object? newValue)
        {
            SessionStateChanged?.Invoke(this, new SessionStateChangedEventArgs
            {
                ChangeType = changeType,
                OldValue = oldValue,
                NewValue = newValue,
                Timestamp = DateTime.UtcNow
            });
        }

        #endregion
    }

    #region Data Models

    /// <summary>
    /// Comprehensive session state container
    /// </summary>
    public class SessionState
    {
        public DateTime SessionStart { get; set; } = DateTime.UtcNow;
        
        // Editing states
        public int? EditingProjectId { get; set; }
        public string? EditingProjectTitle { get; set; }
        public DateTime? LastEditedProject { get; set; }
        
        public int? EditingSkillId { get; set; }
        public string? EditingSkillName { get; set; }
        public DateTime? LastEditedSkill { get; set; }
        
        // Navigation state
        public string CurrentView { get; set; } = "Home";
        public Dictionary<string, object> ViewParameters { get; set; } = new();
        public DateTime? LastViewChange { get; set; }
        
        // Performance tracking
        public List<PerformanceMetric> PerformanceMetrics { get; set; } = new();
    }

    /// <summary>
    /// Performance metric for monitoring and optimization
    /// </summary>
    public class PerformanceMetric
    {
        public string Operation { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime Timestamp { get; set; }
        public bool CacheHit { get; set; }
        public string? UserId { get; set; }
        
        public double DurationMs => Duration.TotalMilliseconds;
    }

    /// <summary>
    /// Event arguments for session state changes
    /// </summary>
    public class SessionStateChangedEventArgs : EventArgs
    {
        public string ChangeType { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion
}