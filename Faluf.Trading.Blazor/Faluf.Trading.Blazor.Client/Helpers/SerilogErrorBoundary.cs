using Microsoft.AspNetCore.Components.Web;

namespace Faluf.Trading.Blazor.Client.Helpers;

public sealed class SerilogErrorBoundary(ILogger<SerilogErrorBoundary> logger) : ErrorBoundary
{
	protected override async Task OnErrorAsync(Exception exception)
	{
		logger.LogError(exception, "An error occurred in the error boundary");

		await Task.CompletedTask;
	}
}