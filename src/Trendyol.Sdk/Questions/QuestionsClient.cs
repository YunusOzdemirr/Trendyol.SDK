using System.Text.Json;
using Trendyol.Sdk.Internal;
using Trendyol.Sdk.Internal.Http;

namespace Trendyol.Sdk.Questions;

internal sealed class QuestionsClient : IQuestionsClient
{
    private readonly TrendyolClient _client;
    internal QuestionsClient(TrendyolClient client) => _client = client;

    public async Task<QuestionPage> GetQuestionsAsync(QuestionFilter? filter = null, CancellationToken cancellationToken = default)
    {
        filter ??= new QuestionFilter();
        if (filter.Page < 0 || filter.Size is <= 0 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(filter));
        }

        var uri = new TrendyolQuery(SellerRoute("questions/filter"))
            .AddEpochMilliseconds("startDate", filter.StartDate).AddEpochMilliseconds("endDate", filter.EndDate)
            .Add("status", filter.Status).Add("barcode", filter.Barcode).Add("page", filter.Page).Add("size", filter.Size)
            .Add("orderByField", filter.OrderByField).Add("orderByDirection", filter.OrderByDirection).ToString();
        return await RequiredAsync<QuestionPage>("Questions.GetQuestions", HttpMethod.Get, uri,
            "integration/qna/sellers/{sellerId}/questions/filter", null, cancellationToken).ConfigureAwait(false);
    }

    public async Task<CustomerQuestion> GetQuestionAsync(long questionId, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.Positive(questionId, nameof(questionId));
        return await RequiredAsync<CustomerQuestion>("Questions.GetQuestion", HttpMethod.Get,
            SellerRoute($"questions/{questionId}"), "integration/qna/sellers/{sellerId}/questions/{id}", null, cancellationToken).ConfigureAwait(false);
    }

    public Task AnswerQuestionAsync(long questionId, AnswerQuestionRequest request, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.Positive(questionId, nameof(questionId));
        TrendyolGuard.NotNull(request, nameof(request));
        TrendyolGuard.NotEmpty(request.Text, nameof(request.Text));
        return _client.SendAsync("Questions.AnswerQuestion", HttpMethod.Post, SellerRoute($"questions/{questionId}/answers"),
            "integration/qna/sellers/{sellerId}/questions/{id}/answers", request, cancellationToken);
    }

    private string SellerRoute(string suffix) => $"integration/qna/sellers/{_client.SellerId}/{suffix}";
    private async Task<T> RequiredAsync<T>(string operation, HttpMethod method, string uri, string template, object? request, CancellationToken token) =>
        await _client.SendAsync<T>(operation, method, uri, template, request, token).ConfigureAwait(false)
        ?? throw new JsonException($"Trendyol returned an empty {operation} response.");
}
