using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Kori.Contracts;
using Kori.Models;

namespace Kori.Services;

public sealed class GatewayHandler :  IGatewayHandler
{
    private readonly HttpClient _httpClient;

    public GatewayHandler(HttpClient  httpClient)
    {
        _httpClient = httpClient;
    }
    
    private static string _cleanupBaseUrl(string baseUrl) => $"{baseUrl.TrimEnd('/').Trim()}/api";

    public async Task<bool> HandleTestGatewayConnection(string address)
    {
        var endpoint = $"{_cleanupBaseUrl(address)}/health";

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out _))
        {
            return false;
        }

        var response = await _httpClient.GetAsync(endpoint);
        return response.IsSuccessStatusCode;
    }

    public async Task<Gateway?> HandleRegisterDeviceToGateway(string address, string code)
    {
        var endpoint = $"{_cleanupBaseUrl(address)}/device/register";
        var payload = new Dictionary<string, string>
        {
            { "code", code },
        };

        var response = await _httpClient.PostAsJsonAsync(endpoint, payload);
        if (!response.IsSuccessStatusCode) return null;
        
        var serializedResponse = await response.Content.ReadFromJsonAsync<RegisterDeviceResponse>();
        if (serializedResponse is null) return null;
        
        if (string.IsNullOrEmpty(serializedResponse.Token)) return null;

        return new Gateway
        {
            Address = address,
            Token = serializedResponse.Token,
        };
    }

    public async Task<string> HandleCreatePairingInvite(Gateway gateway)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/device/paring/invites";
        
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return string.Empty;

        var content = await response.Content.ReadFromJsonAsync<CreateDevicePairingResponse>();
        return content is null ? string.Empty : content.Code;
    }

    public async Task<List<Device>> HandleRegisteredDevices(Gateway gateway)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/device/paring/devices";
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return [];

        return await response.Content.ReadFromJsonAsync<List<Device>>() ?? [];
    }

    public async Task<bool> HandlePromoteDeviceToOwner(Gateway gateway, Device device)
    {
        var endpoint = $"device/pairing/devices/{device.Id}/promote";
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<Profile>?> HandleGetAllProfiles(Gateway gateway)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/profiles";
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        
        var response = await _httpClient.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<List<Profile>>();
    }

    public async Task<Profile?> HandleCreateProfile(Gateway gateway, string name)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/profiles";

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);

        var payload = new Dictionary<string, string>
        {
            { "name", name },
        };
        request.Content = JsonContent.Create(payload);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<Profile>();
    }

    public async Task<bool> HandleUpdateProfile(Gateway gateway, Profile profile)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/profiles";
        
        var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);

        var payload = new Dictionary<string, string>
        {
            { "id", profile.Id },
            { "name", profile.Name },
        };
        request.Content = JsonContent.Create(payload);
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> HandleDeleteProfile(Gateway gateway, Profile profile)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/profiles{profile.Id}";
        
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<Provider?> HandleAddProvider(Gateway gateway, Provider provider, string profileId, string? displayName=null)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/providers";

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);

        var payload = new Dictionary<string, string>
        {
            { "name", provider.Name },
            { "api_key", provider.ApiKey }
        };

        if (!string.IsNullOrEmpty(provider.EndpointUrl))
        {
            payload.Add("endpoint", provider.EndpointUrl);
        }

        if (!string.IsNullOrEmpty(displayName))
        {
            payload.Add("display_name", displayName);
        }

        request.Content = JsonContent.Create(payload);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var created = await response.Content.ReadFromJsonAsync<Provider>();
        return created ?? provider;
    }

    public async Task<bool> HandleDeleteProvider(Gateway gateway, Provider provider, string profileId)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/providers/{provider.Id}";
        
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);
        
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<Provider>> HandleGetAllProviders(Gateway gateway, string profileId)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/providers";
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);
        
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return [];

        return await response.Content.ReadFromJsonAsync<List<Provider>>() ?? [];
    }

    public async Task<List<string>> HandleGetProviderModels(Gateway gateway, Provider provider, string profileId)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/providers/{provider.Id}/models";
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);
        
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return [];

        return await response.Content.ReadFromJsonAsync<List<string>>() ?? [];
    }

    public async Task<bool> HandleAssignModel(Gateway gateway, string providerId, string modelId, ModelAssignmentType assignment, string profileId)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/model-assignments/{assignment.ToString().ToLower()}";
        
        var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);

        var payload = new Dictionary<string, string>
        {
            { "provider_id", providerId },
            { "model_id", modelId }
        };
        
        request.Content = JsonContent.Create(payload);
        
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> HandleDeleteModelAssignment(Gateway gateway, ModelAssignmentType assignment, string profileId)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/model-assignments/{assignment.ToString().ToLower()}";
        
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);
        
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ModelAssignment>> HandleGetAllModelAssignments(Gateway gateway, string profileId)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/model-assignments";
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);
        
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return [];

        return await response.Content.ReadFromJsonAsync<List<ModelAssignment>>() ?? [];
    }

    public async Task<Chat?> HandleChat(Gateway gateway, string profileId, Chat chat)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/chat/message";
        
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);

        if (string.IsNullOrWhiteSpace(chat.Message)) return null;

        var payload = new Dictionary<string, string>
        {
            { "message", chat.Message }
        };

        if (!string.IsNullOrWhiteSpace(chat.ConversationId))
        {
            payload.Add("conversation_id", chat.ConversationId);
        }
        
        request.Content = JsonContent.Create(payload);
        
        var response = await _httpClient.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<Chat>();
    }

    public async Task<List<Conversation>> HandleGetAllChats(Gateway gateway, string profileId)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/chat/conversations";

        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadFromJsonAsync<ConversationsResponse>();
        if (content is null) return [];
        
        return content.Conversations.Select(c => new Conversation{Id = c.Id, Messages = c.Messages}).ToList();
    }

    public async Task<List<Connector>> HandleGetAllConnectors(Gateway gateway, string profileId)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/connectors";
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);
        
        var response = await _httpClient.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<List<Connector>>() ?? [];
    }

    public async Task<bool> HandleDeleteConnector(Gateway gateway, Connector connector, string profileId)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/connectors?provider={connector.Provider}&category={connector.Category}";
        
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);
        
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<string> HandleAddConnector(Gateway gateway, Connector connector, string profileId)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/connectors/register";
        
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);

        var payload = new Dictionary<string, string>
        {
            { "provider", connector.Provider },
            { "category", connector.Category }
        };
        
        request.Content = JsonContent.Create(payload);
        
        var response = await _httpClient.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<string>() ?? string.Empty;
    }

    public async Task<List<Goal>> HandleGetAllGoals(Gateway gateway, string profileId)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/goals";
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);
        
        var response = await _httpClient.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<List<Goal>>() ?? [];
    }

    public async Task<bool> HandleDeleteGoal(Gateway gateway, Goal goal, string profileId)
    {
        var endpoint = $"{_cleanupBaseUrl(gateway.Address)}/goals/{goal.Id}";
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gateway.Token);
        request.Headers.Add("X-Profile-Id", profileId);
        
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }


    // ======= Records ======= //
    private sealed record RegisterDeviceResponse([property: JsonPropertyName("token")] string Token);
    private sealed record CreateDevicePairingResponse([property: JsonPropertyName("code")] string Code);
    public sealed record ConversationsResponse(
        [property: JsonPropertyName("conversations")]
        List<Conversation> Conversations);
}