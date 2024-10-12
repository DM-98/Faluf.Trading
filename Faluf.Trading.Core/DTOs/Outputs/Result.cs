using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Faluf.Trading.Core.DTOs.Outputs;

public class Result<T>
{
	public Result(bool isSuccess, string? errorMessage = null, Exception? exception = null, T? content = default, HttpStatusCode statusCode = HttpStatusCode.OK, int recordCount = 0)
	{
		IsSuccess = isSuccess;
		ErrorMessage = errorMessage;
		ExceptionMessage = exception?.Message;
		InnerExceptionMessage = exception?.InnerException?.Message;
		StackTrace = exception?.StackTrace;
		Content = content;
		StatusCode = statusCode;
		RecordCount = recordCount;
	}

	public Result() { }

	[MemberNotNullWhen(true, nameof(Content))]
	public bool IsSuccess { get; set; }

	public int RecordCount { get; set; }

	public T? Content { get; set; }

	public string? ErrorMessage { get; set; }

	public string? ExceptionMessage { get; set; }

	public string? InnerExceptionMessage { get; set; }

	public string? StackTrace { get; set; }

	public HttpStatusCode StatusCode { get; set; }

    public static Result<TValue> Ok<TValue>(TValue content, int recordCount = 0) where TValue : T => new(true, content: content, recordCount: recordCount, statusCode: HttpStatusCode.OK);

    public static Result<TValue> Created<TValue>(TValue content, int recordCount = 0) where TValue : T => new(true, content: content, recordCount: recordCount, statusCode: HttpStatusCode.Created);

    public static Result<TValue> Unauthorized<TValue>(string errorMessage) where TValue : T => new(false, errorMessage: errorMessage, statusCode: HttpStatusCode.Unauthorized);

    public static Result<TValue> Locked<TValue>(string errorMessage) where TValue : T => new(false, errorMessage: errorMessage, statusCode: HttpStatusCode.Locked);

	//public static Result<TValue> NotFound<TValue>(string errorMessage) where TValue : T => new(false, errorMessage: errorMessage, statusCode: HttpStatusCode.NotFound);

	public static Result<TValue> BadRequest<TValue>(string errorMessage) where TValue : T => new(false, errorMessage: errorMessage, statusCode: HttpStatusCode.BadRequest);

	public static Result<TValue> BadRequest<TValue>(IEnumerable<string> errorMessages) where TValue : T => new(false, errorMessage: string.Join(Environment.NewLine, errorMessages), statusCode: HttpStatusCode.BadRequest);

	public static Result<TValue> InternalServerError<TValue>(string errorMessage, Exception ex) where TValue : T => new(false, errorMessage: errorMessage, exception: ex, statusCode: HttpStatusCode.InternalServerError);
}

public sealed class Result(
	bool isSuccess,
	string? errorMessage = null,
	Exception? exception = null,
	object? content = default,
	HttpStatusCode statusCode = HttpStatusCode.OK,
	int recordCount = 0)
	: Result<object>(isSuccess, errorMessage, exception, content, statusCode, recordCount)
{
	public static Result Ok() => new(true, statusCode: HttpStatusCode.OK);

    public static Result NotFound(string errorMessage) => new(false, errorMessage: errorMessage, statusCode: HttpStatusCode.NotFound);

    public static Result Unauthorized(string errorMessage) => new(false, errorMessage: errorMessage, statusCode: HttpStatusCode.Unauthorized);

    public static Result InternalServerError(string errorMessage, Exception ex) => new(false, errorMessage: errorMessage, exception: ex, statusCode: HttpStatusCode.InternalServerError);
}