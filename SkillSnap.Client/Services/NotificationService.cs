namespace SkillSnap.Client.Services
{
    /// <summary>
    /// Service for displaying notifications to users with different severity levels
    /// Inspired by Microsoft Copilot best practices for user feedback
    /// </summary>
    public class NotificationService
    {
        public event Action<NotificationMessage>? OnNotificationAdded;
        public event Action<int>? OnNotificationRemoved;

        private readonly List<NotificationMessage> _notifications = new();
        private int _nextId = 1;

        /// <summary>
        /// Show a success notification
        /// </summary>
        /// <param name="message">Success message</param>
        /// <param name="autoHide">Whether to auto-hide after 5 seconds</param>
        public void ShowSuccess(string message, bool autoHide = true)
        {
            AddNotification(message, NotificationType.Success, autoHide);
        }

        /// <summary>
        /// Show an error notification
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="autoHide">Whether to auto-hide after 10 seconds</param>
        public void ShowError(string message, bool autoHide = false)
        {
            AddNotification(message, NotificationType.Error, autoHide);
        }

        /// <summary>
        /// Show an info notification
        /// </summary>
        /// <param name="message">Info message</param>
        /// <param name="autoHide">Whether to auto-hide after 7 seconds</param>
        public void ShowInfo(string message, bool autoHide = true)
        {
            AddNotification(message, NotificationType.Info, autoHide);
        }

        /// <summary>
        /// Show a warning notification
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="autoHide">Whether to auto-hide after 8 seconds</param>
        public void ShowWarning(string message, bool autoHide = true)
        {
            AddNotification(message, NotificationType.Warning, autoHide);
        }

        /// <summary>
        /// Add a notification with specified type
        /// </summary>
        private void AddNotification(string message, NotificationType type, bool autoHide)
        {
            var notification = new NotificationMessage
            {
                Id = _nextId++,
                Message = message,
                Type = type,
                Timestamp = DateTime.Now,
                AutoHide = autoHide
            };

            _notifications.Add(notification);
            OnNotificationAdded?.Invoke(notification);

            // Auto-hide logic
            if (autoHide)
            {
                var hideDelay = type switch
                {
                    NotificationType.Success => 5000,
                    NotificationType.Info => 7000,
                    NotificationType.Warning => 8000,
                    NotificationType.Error => 10000,
                    _ => 5000
                };

                _ = Task.Delay(hideDelay).ContinueWith(_ => RemoveNotification(notification.Id));
            }
        }

        /// <summary>
        /// Remove a notification by ID
        /// </summary>
        /// <param name="id">Notification ID</param>
        public void RemoveNotification(int id)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == id);
            if (notification != null)
            {
                _notifications.Remove(notification);
                OnNotificationRemoved?.Invoke(id);
            }
        }

        /// <summary>
        /// Get all current notifications
        /// </summary>
        /// <returns>List of active notifications</returns>
        public List<NotificationMessage> GetNotifications()
        {
            return _notifications.ToList();
        }

        /// <summary>
        /// Clear all notifications
        /// </summary>
        public void ClearAll()
        {
            var ids = _notifications.Select(n => n.Id).ToList();
            _notifications.Clear();
            
            foreach (var id in ids)
            {
                OnNotificationRemoved?.Invoke(id);
            }
        }
    }

    /// <summary>
    /// Represents a notification message
    /// </summary>
    public class NotificationMessage
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public bool AutoHide { get; set; }
    }

    /// <summary>
    /// Types of notifications
    /// </summary>
    public enum NotificationType
    {
        Success,
        Error,
        Info,
        Warning
    }
}