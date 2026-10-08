using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using PharmaSaaS.Application.Contracts.AI;
using PharmaSaaS.Application.Interfaces.Services;

namespace PharmaSaaS.Infrastructure.Services;

public sealed class AIService(IHttpClientFactory httpClientFactory, IUnitOfWork unitOfWork, IOptions<OpenRouterSettings> options) : IAIService
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private const string _systemPromptImage =
        "You are a medical assistant that reads handwritten prescriptions. " +
        "Extract only the drug/medication names from the image. " +
        "Return a JSON array of strings only, no extra text, no markdown. Example: [\"Amoxicillin\",\"Paracetamol\"]";

    private const string _systemPromptText =
        "You are a pharmacist assistant. Given a symptom description or a medication need in Arabic or English, " +
        "suggest the most appropriate over-the-counter or commonly prescribed medications. " +
        "Return a JSON array of drug/product names only, no extra text, no markdown. Example: [\"Tobramycin Eye Drops\",\"Gentamicin Ointment\"]";

    public async Task<Result<AIResponse>> ParsePrescriptionImageAsync(string pharmacyId, byte[] imageBytes, string contentType, CancellationToken cancellationToken = default)
    {
        var apiKeyResult = await ResolveApiKeyAsync(pharmacyId, cancellationToken);

        if (apiKeyResult.IsFailure)
            return Result.Failure<AIResponse>(apiKeyResult.Error);

        var base64 = Convert.ToBase64String(imageBytes);
        var dataUrl = $"data:{contentType};base64,{base64}";

        var messages = new[]
        {
            new { role = "system", content = (object)_systemPromptImage },
            new
            {
                role = "user",
                content = (object)new object[]
                {
                    new { type = "image_url", image_url = new { url = dataUrl } },
                    new { type = "text", text = "Extract the drug names from this prescription." }
                }
            }
        };

        var callResult = await CallOpenRouterAsync(messages, apiKeyResult.Value.ApiKey, cancellationToken);

        if (callResult.IsFailure)
            return Result.Failure<AIResponse>(callResult.Error);

        if (apiKeyResult.Value.IsUsingPlatformKey)
        {
            await unitOfWork.AIUsageLogs.AddAsync(AIUsageLog.Create(pharmacyId), cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success(new AIResponse(callResult.Value));
    }

    public async Task<Result<AIResponse>> SuggestMedicationsAsync(string pharmacyId, string description, CancellationToken cancellationToken = default)
    {
        var apiKeyResult = await ResolveApiKeyAsync(pharmacyId, cancellationToken);

        if (apiKeyResult.IsFailure)
            return Result.Failure<AIResponse>(apiKeyResult.Error);

        var messages = new[]
        {
            new { role = "system", content = (object)_systemPromptText },
            new { role = "user", content = (object)description }
        };

        var callResult = await CallOpenRouterAsync(messages, apiKeyResult.Value.ApiKey, cancellationToken);
        if (callResult.IsFailure)
            return Result.Failure<AIResponse>(callResult.Error);

        if (apiKeyResult.Value.IsUsingPlatformKey)
        {
            await unitOfWork.AIUsageLogs.AddAsync(AIUsageLog.Create(pharmacyId), cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success(new AIResponse(callResult.Value));
    }

    private async Task<Result<(string ApiKey, bool IsUsingPlatformKey)>> ResolveApiKeyAsync(string pharmacyId, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var usageThisMonth = await unitOfWork.AIUsageLogs.CountAsync(
            l => l.PharmacyId == pharmacyId && l.UsedAt >= monthStart, cancellationToken);

        if (usageThisMonth < settings.MonthlyLimitPerPharmacy)
            return Result.Success<(string, bool)>((settings.ApiKey, true));

        var pharmacy = await unitOfWork.Pharmacies.FindByTenantIdAsync(pharmacyId, cancellationToken);

        if (pharmacy is not { CustomAiApiKey: { Length: > 0 } customKey })
            return Result.Failure<(string, bool)>(AIErrors.MonthlyLimitReached);

        return Result.Success<(string, bool)>((customKey, false));
    }

    private async Task<Result<IReadOnlyList<string>>> CallOpenRouterAsync(object messages, string apiKey, CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient("OpenRouter");

            var requestBody = new { model = options.Value.Model, messages };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = content;

            using var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return Result.Failure<IReadOnlyList<string>>(AIErrors.ServiceUnavailable);

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var parsed = JsonSerializer.Deserialize<OpenRouterResponse>(responseJson, _jsonOptions);
            var rawContent = parsed?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

            if (string.IsNullOrEmpty(rawContent))
                return Result.Failure<IReadOnlyList<string>>(AIErrors.ParseFailed);

            var drugs = JsonSerializer.Deserialize<List<string>>(rawContent, _jsonOptions);

            if (drugs is null || drugs.Count == 0)
                return Result.Failure<IReadOnlyList<string>>(AIErrors.ParseFailed);

            return Result.Success<IReadOnlyList<string>>(drugs);
        }
        catch
        {
            return Result.Failure<IReadOnlyList<string>>(AIErrors.ServiceUnavailable);
        }
    }

    private sealed class OpenRouterResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }

        public sealed class Choice
        {
            [JsonPropertyName("message")]
            public Message? Message { get; set; }
        }

        public sealed class Message
        {
            [JsonPropertyName("content")]
            public string? Content { get; set; }
        }
    }
}
