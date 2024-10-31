namespace Faluf.Trading.Core.Abstractions.Models;

public abstract class BaseFilter
{
	public int Page { get; set; } = 1;

	public int PageSize { get; set; } = 10;

	public string? SearchString { get; set; }
}