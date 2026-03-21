using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Infrastructure;

internal sealed class WhatsAppSender : IChannelDispatcher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MessagingDbContext _context;
    private readonly ITokenEncryptor _tokenEncryptor;
    private readonly ILogger<WhatsAppSender> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public WhatsAppSender(
        IHttpClientFactory httpClientFactory,
        MessagingDbContext context,
        ITokenEncryptor tokenEncryptor,
        ILogger<WhatsAppSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
        _tokenEncryptor = tokenEncryptor;
        _logger = logger;
    }

    public async Task<Result> SendAsync(ChannelMessage message, CancellationToken ct = default)
    {
        var waba = await _context.WhatsAppBusinessAccounts
            .FirstOrDefaultAsync(w => w.ClinicId == message.ClinicId, ct);

        if (waba is null)
            return Result.NotFound("WhatsApp Business Account not configured for this clinic");

        string accessToken;
        try
        {
            accessToken = _tokenEncryptor.Decrypt(waba.EncryptedAccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt WhatsApp access token for clinic {ClinicId}", message.ClinicId);
            return Result.Error("DECRYPTION_FAILED:Failed to decrypt access token");
        }

        var client = _httpClientFactory.CreateClient("WhatsApp");

        var url = $"https://graph.facebook.com/v21.0/{waba.PhoneNumberId}/messages";

        var components = BuildTemplateComponents(message.Parameters);

        var payload = new WhatsAppMessagePayload
        {
            MessagingProduct = "whatsapp",
            To = message.RecipientPhone,
            Type = "template",
            Template = new WhatsAppTemplate
            {
                Name = message.TemplateName,
                Language = new WhatsAppLanguage { Code = "en" },
                Components = components.Count > 0 ? components : null
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(payload, options: JsonOptions);

        try
        {
            var response = await client.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError(
                    "WhatsApp API returned {StatusCode} for clinic {ClinicId}: {Body}",
                    response.StatusCode, message.ClinicId, errorBody);
                return Result.Error($"WHATSAPP_API_ERROR:{response.StatusCode}:{errorBody}");
            }

            _logger.LogInformation(
                "WhatsApp template '{Template}' sent to {Phone} for clinic {ClinicId}",
                message.TemplateName, message.RecipientPhone, message.ClinicId);

            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error sending WhatsApp message for clinic {ClinicId}", message.ClinicId);
            return Result.Error($"WHATSAPP_HTTP_ERROR:{ex.Message}");
        }
    }

    private static List<WhatsAppComponent> BuildTemplateComponents(Dictionary<string, string> parameters)
    {
        if (parameters.Count == 0)
            return [];

        var bodyParams = parameters
            .Select(kvp => new WhatsAppParameter { Type = "text", Text = kvp.Value })
            .ToList();

        return
        [
            new WhatsAppComponent
            {
                Type = "body",
                Parameters = bodyParams
            }
        ];
    }

    // --- Internal DTOs for WhatsApp Graph API ---

    private sealed class WhatsAppMessagePayload
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = "whatsapp";
        public string To { get; set; } = string.Empty;
        public string Type { get; set; } = "template";
        public WhatsAppTemplate Template { get; set; } = new();
    }

    private sealed class WhatsAppTemplate
    {
        public string Name { get; set; } = string.Empty;
        public WhatsAppLanguage Language { get; set; } = new();
        public List<WhatsAppComponent>? Components { get; set; }
    }

    private sealed class WhatsAppLanguage
    {
        public string Code { get; set; } = "en";
    }

    private sealed class WhatsAppComponent
    {
        public string Type { get; set; } = string.Empty;
        public List<WhatsAppParameter> Parameters { get; set; } = [];
    }

    private sealed class WhatsAppParameter
    {
        public string Type { get; set; } = "text";
        public string? Text { get; set; }
    }
}
