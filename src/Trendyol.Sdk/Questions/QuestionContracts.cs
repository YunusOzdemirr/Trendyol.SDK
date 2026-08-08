#pragma warning disable CS1591

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Trendyol.Sdk.Questions;

public sealed class QuestionFilter
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? Status { get; set; }
    public string? Barcode { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
    public string? OrderByField { get; set; }
    public string? OrderByDirection { get; set; }
}

public sealed class QuestionPage
{
    public long TotalElements { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public List<CustomerQuestion> Content { get; set; } = [];
}

public sealed class CustomerQuestion
{
    public long Id { get; set; }
    public string? Text { get; set; }
    public string? Status { get; set; }
    public string? Barcode { get; set; }
    public string? ProductName { get; set; }
    public long? CreationDate { get; set; }
    public QuestionAnswer? Answer { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class QuestionAnswer
{
    public string Text { get; set; } = string.Empty;
    public long? CreationDate { get; set; }
}

public sealed class AnswerQuestionRequest
{
    public string Text { get; set; } = string.Empty;
}
