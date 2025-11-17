using System.Diagnostics;
using System.Text.Json;

namespace SkillSnap.Client.Services
{
    /// <summary>
    /// Performance measurement and verification service
    /// Provides tools for measuring request durations, load times, and cache performance
    /// </summary>
    public class PerformanceMonitorService
    {
        private readonly UserSessionService _sessionService;
        private readonly List<LoadTimeMetric> _loadTimeMetrics = new();
        private readonly Dictionary<string, CacheMetric> _cacheMetrics = new();
        
        public PerformanceMonitorService(UserSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public event EventHandler<PerformanceMetricEventArgs>? MetricRecorded;

        #region Request Duration Measurement

        /// <summary>
        /// Create a stopwatch for measuring operation duration
        /// </summary>
        public PerformanceStopwatch StartMeasurement(string operationName)
        {
            return new PerformanceStopwatch(operationName, this);
        }

        /// <summary>
        /// Measure the duration of an async operation
        /// </summary>
        public async Task<T> MeasureAsync<T>(string operationName, Func<Task<T>> operation, bool cacheHit = false)
        {
            using var stopwatch = StartMeasurement(operationName);
            var result = await operation();
            stopwatch.SetCacheHit(cacheHit);
            return result;
        }

        /// <summary>
        /// Measure the duration of a synchronous operation
        /// </summary>
        public T Measure<T>(string operationName, Func<T> operation, bool cacheHit = false)
        {
            using var stopwatch = StartMeasurement(operationName);
            var result = operation();
            stopwatch.SetCacheHit(cacheHit);
            return result;
        }

        /// <summary>
        /// Record a completed performance measurement
        /// </summary>
        internal void RecordMeasurement(string operationName, TimeSpan duration, bool cacheHit)
        {
            // Record in session service
            _sessionService.RecordPerformanceMetric(operationName, duration, cacheHit);
            
            // Update cache metrics
            UpdateCacheMetric(operationName, duration, cacheHit);
            
            // Raise event
            MetricRecorded?.Invoke(this, new PerformanceMetricEventArgs
            {
                OperationName = operationName,
                Duration = duration,
                CacheHit = cacheHit,
                Timestamp = DateTime.UtcNow
            });

            // Log to console in development
            var status = cacheHit ? "CACHE HIT" : "DATABASE";
            Console.WriteLine($"[PERF] {operationName}: {duration.TotalMilliseconds:F2}ms ({status})");
        }

        #endregion

        #region Load Time Testing

        /// <summary>
        /// Start measuring page load time
        /// </summary>
        public LoadTimeTracker StartLoadTimeTracking(string pageName)
        {
            return new LoadTimeTracker(pageName, this);
        }

        /// <summary>
        /// Record load time metric
        /// </summary>
        internal void RecordLoadTime(string pageName, TimeSpan totalLoadTime, TimeSpan? apiCallTime = null, int? apiCallCount = null)
        {
            var metric = new LoadTimeMetric
            {
                PageName = pageName,
                TotalLoadTime = totalLoadTime,
                ApiCallTime = apiCallTime,
                ApiCallCount = apiCallCount ?? 0,
                Timestamp = DateTime.UtcNow,
                UserId = _sessionService.UserId
            };

            _loadTimeMetrics.Add(metric);
            
            // Keep only last 50 load time metrics
            if (_loadTimeMetrics.Count > 50)
            {
                _loadTimeMetrics.RemoveAt(0);
            }

            Console.WriteLine($"[LOAD] {pageName}: {totalLoadTime.TotalMilliseconds:F2}ms total" +
                            (apiCallTime.HasValue ? $", {apiCallTime.Value.TotalMilliseconds:F2}ms API" : ""));
        }

        /// <summary>
        /// Get load time metrics for analysis
        /// </summary>
        public List<LoadTimeMetric> GetLoadTimeMetrics()
        {
            return new List<LoadTimeMetric>(_loadTimeMetrics);
        }

        /// <summary>
        /// Get average load times by page
        /// </summary>
        public Dictionary<string, double> GetAverageLoadTimes()
        {
            return _loadTimeMetrics
                .GroupBy(m => m.PageName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(m => m.TotalLoadTime.TotalMilliseconds)
                );
        }

        #endregion

        #region Cache Performance Analysis

        private void UpdateCacheMetric(string operation, TimeSpan duration, bool cacheHit)
        {
            if (!_cacheMetrics.ContainsKey(operation))
            {
                _cacheMetrics[operation] = new CacheMetric { Operation = operation };
            }

            var metric = _cacheMetrics[operation];
            metric.TotalRequests++;
            metric.TotalDuration += duration;

            if (cacheHit)
            {
                metric.CacheHits++;
                metric.CacheHitDuration += duration;
            }
            else
            {
                metric.CacheMisses++;
                metric.CacheMissDuration += duration;
            }

            metric.LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Get cache performance metrics
        /// </summary>
        public Dictionary<string, CacheMetric> GetCacheMetrics()
        {
            return new Dictionary<string, CacheMetric>(_cacheMetrics);
        }

        /// <summary>
        /// Get cache hit ratio for all operations
        /// </summary>
        public double GetOverallCacheHitRatio()
        {
            var totalRequests = _cacheMetrics.Values.Sum(m => m.TotalRequests);
            var totalHits = _cacheMetrics.Values.Sum(m => m.CacheHits);
            
            return totalRequests > 0 ? (double)totalHits / totalRequests : 0.0;
        }

        /// <summary>
        /// Generate performance report
        /// </summary>
        public PerformanceReport GenerateReport()
        {
            var sessionMetrics = _sessionService.GetPerformanceMetrics();
            
            return new PerformanceReport
            {
                ReportTimestamp = DateTime.UtcNow,
                SessionDuration = _sessionService.SessionDuration,
                TotalOperations = sessionMetrics.Count,
                AverageResponseTime = sessionMetrics.Any() ? 
                    TimeSpan.FromMilliseconds(sessionMetrics.Average(m => m.DurationMs)) : TimeSpan.Zero,
                CacheHitRatio = GetOverallCacheHitRatio(),
                LoadTimeMetrics = GetLoadTimeMetrics(),
                CacheMetrics = GetCacheMetrics(),
                TopSlowOperations = sessionMetrics
                    .OrderByDescending(m => m.Duration)
                    .Take(10)
                    .ToList()
            };
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Clear all performance data
        /// </summary>
        public void ClearAllMetrics()
        {
            _loadTimeMetrics.Clear();
            _cacheMetrics.Clear();
            _sessionService.ClearPerformanceMetrics();
        }

        /// <summary>
        /// Export performance data as JSON
        /// </summary>
        public string ExportMetricsAsJson()
        {
            var exportData = new
            {
                SessionDuration = _sessionService.SessionDuration,
                LoadTimeMetrics = GetLoadTimeMetrics(),
                CacheMetrics = GetCacheMetrics(),
                PerformanceMetrics = _sessionService.GetPerformanceMetrics()
            };

            return JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Disposable stopwatch for measuring performance with automatic recording
    /// </summary>
    public class PerformanceStopwatch : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _operationName;
        private readonly PerformanceMonitorService _monitorService;
        private bool _cacheHit = false;
        private bool _disposed = false;

        internal PerformanceStopwatch(string operationName, PerformanceMonitorService monitorService)
        {
            _operationName = operationName;
            _monitorService = monitorService;
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Set whether this operation was a cache hit
        /// </summary>
        public void SetCacheHit(bool cacheHit)
        {
            _cacheHit = cacheHit;
        }

        /// <summary>
        /// Get current elapsed time
        /// </summary>
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _stopwatch.Stop();
                _monitorService.RecordMeasurement(_operationName, _stopwatch.Elapsed, _cacheHit);
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Helper for tracking page load times
    /// </summary>
    public class LoadTimeTracker : IDisposable
    {
        private readonly Stopwatch _totalStopwatch;
        private readonly Stopwatch _apiStopwatch;
        private readonly string _pageName;
        private readonly PerformanceMonitorService _monitorService;
        private TimeSpan? _apiCallTime;
        private int _apiCallCount = 0;
        private bool _disposed = false;

        internal LoadTimeTracker(string pageName, PerformanceMonitorService monitorService)
        {
            _pageName = pageName;
            _monitorService = monitorService;
            _totalStopwatch = Stopwatch.StartNew();
            _apiStopwatch = new Stopwatch();
        }

        /// <summary>
        /// Start measuring API call time
        /// </summary>
        public void StartApiCall()
        {
            _apiStopwatch.Start();
        }

        /// <summary>
        /// Stop measuring API call time
        /// </summary>
        public void StopApiCall()
        {
            _apiStopwatch.Stop();
            _apiCallCount++;
            _apiCallTime = _apiStopwatch.Elapsed;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _totalStopwatch.Stop();
                _monitorService.RecordLoadTime(_pageName, _totalStopwatch.Elapsed, _apiCallTime, _apiCallCount);
                _disposed = true;
            }
        }
    }

    #endregion

    #region Data Models

    /// <summary>
    /// Load time measurement data
    /// </summary>
    public class LoadTimeMetric
    {
        public string PageName { get; set; } = string.Empty;
        public TimeSpan TotalLoadTime { get; set; }
        public TimeSpan? ApiCallTime { get; set; }
        public int ApiCallCount { get; set; }
        public DateTime Timestamp { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Cache performance metrics
    /// </summary>
    public class CacheMetric
    {
        public string Operation { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int CacheHits { get; set; }
        public int CacheMisses { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan CacheHitDuration { get; set; }
        public TimeSpan CacheMissDuration { get; set; }
        public DateTime LastUpdated { get; set; }

        public double HitRatio => TotalRequests > 0 ? (double)CacheHits / TotalRequests : 0.0;
        public double AverageHitTime => CacheHits > 0 ? CacheHitDuration.TotalMilliseconds / CacheHits : 0.0;
        public double AverageMissTime => CacheMisses > 0 ? CacheMissDuration.TotalMilliseconds / CacheMisses : 0.0;
    }

    /// <summary>
    /// Comprehensive performance report
    /// </summary>
    public class PerformanceReport
    {
        public DateTime ReportTimestamp { get; set; }
        public TimeSpan SessionDuration { get; set; }
        public int TotalOperations { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public double CacheHitRatio { get; set; }
        public List<LoadTimeMetric> LoadTimeMetrics { get; set; } = new();
        public Dictionary<string, CacheMetric> CacheMetrics { get; set; } = new();
        public List<PerformanceMetric> TopSlowOperations { get; set; } = new();
    }

    /// <summary>
    /// Event arguments for performance metrics
    /// </summary>
    public class PerformanceMetricEventArgs : EventArgs
    {
        public string OperationName { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public bool CacheHit { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion
}