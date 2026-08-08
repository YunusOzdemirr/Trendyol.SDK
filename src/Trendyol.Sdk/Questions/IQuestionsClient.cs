#pragma warning disable CS1591

namespace Trendyol.Sdk.Questions;

public interface IQuestionsClient
{
    public Task<QuestionPage> GetQuestionsAsync(QuestionFilter? filter = null, CancellationToken cancellationToken = default);
    public Task<CustomerQuestion> GetQuestionAsync(long questionId, CancellationToken cancellationToken = default);
    public Task AnswerQuestionAsync(long questionId, AnswerQuestionRequest request, CancellationToken cancellationToken = default);
}
