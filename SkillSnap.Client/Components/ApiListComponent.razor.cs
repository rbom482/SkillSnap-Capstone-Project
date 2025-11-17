using Microsoft.AspNetCore.Components;

namespace SkillSnap.Client.Components
{
    /// <summary>
    /// Reusable component for displaying lists of items from APIs with built-in loading, error handling, and empty states
    /// Inspired by Microsoft Copilot best practices for Blazor components
    /// </summary>
    /// <typeparam name="T">The type of items to display</typeparam>
    public partial class ApiListComponent<T> : ComponentBase where T : class
    {
        [Parameter] public List<T>? Items { get; set; }
        [Parameter] public bool IsLoading { get; set; }
        [Parameter] public string? ErrorMessage { get; set; }
        [Parameter] public RenderFragment<T>? ItemTemplate { get; set; }
        [Parameter] public RenderFragment? LoadingTemplate { get; set; }
        [Parameter] public RenderFragment? ErrorTemplate { get; set; }
        [Parameter] public RenderFragment? EmptyTemplate { get; set; }
        [Parameter] public string CssClass { get; set; } = "";
        [Parameter] public string LoadingText { get; set; } = "Loading...";
        [Parameter] public string EmptyText { get; set; } = "No items found.";
        [Parameter] public EventCallback OnRetry { get; set; }
        [Parameter] public bool ShowRetryButton { get; set; } = true;

        /// <summary>
        /// Determines if the component should show the loading state
        /// </summary>
        private bool ShouldShowLoading => IsLoading && Items == null;

        /// <summary>
        /// Determines if the component should show the error state
        /// </summary>
        private bool ShouldShowError => !string.IsNullOrEmpty(ErrorMessage) && !IsLoading;

        /// <summary>
        /// Determines if the component should show the empty state
        /// </summary>
        private bool ShouldShowEmpty => Items != null && Items.Count == 0 && !IsLoading && string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Determines if the component should show the items
        /// </summary>
        private bool ShouldShowItems => Items != null && Items.Count > 0 && !IsLoading && string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Handles retry button click
        /// </summary>
        private async Task HandleRetryAsync()
        {
            if (OnRetry.HasDelegate)
            {
                await OnRetry.InvokeAsync();
            }
        }
    }
}