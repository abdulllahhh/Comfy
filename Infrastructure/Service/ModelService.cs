using Microsoft.Extensions.Configuration;
using Model.Dtos.Request;
using Model.Dtos.Response;
using Model.Interface;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Service;

public class ModelService : IModelService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    public ModelService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }
    public async Task<WorkflowResponse> RunWorkflowAsync(string Id,WorkflowRequest request)
    {

        var flaskUrl = _config["Flask:Url"];
        var flaskToken = _config["Flask:RequestSecret"];
        var payload = new Dictionary<string, object>
        {
            ["user_id"] = Id,
            ["prompt"] = request.Prompt,
            ["seed"] = request.Seed,
            ["steps"] = request.Steps,
            ["cfg"] = request.Cfg
        };
        Console.WriteLine("runweorflow async user id\n \n \n" + Id);
        var json = JsonSerializer.Serialize(payload);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, flaskUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        Console.WriteLine("Calling Flask API:");
        Console.WriteLine($"URL: {flaskUrl}");
       // Console.WriteLine($"Headers: Authorization: Bearer {flaskToken}");
        Console.WriteLine($"Body: {json}");
        // Add security header (same as Flask API expects)
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "my-secret-token");

        try
        {
            var response = await _httpClient.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                // Check content type to determine if it's an image or JSON error
                var contentType = response.Content.Headers.ContentType?.MediaType;

                if (contentType == "image/png" || contentType == "image/jpeg")
                {
                    // Handle binary image response
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    var base64Image = Convert.ToBase64String(imageBytes);

                    return new WorkflowResponse
                    {
                        Success = true,
                        ImageData = base64Image,
                        ImageFormat = contentType == "image/png" ? "png" : "jpeg",
                        StatusCode = (int)response.StatusCode
                    };
                }
                else
                {
                    // Handle JSON response (likely an error)
                    var rawResponse = await response.Content.ReadAsStringAsync();

                    // Try to parse as JSON to extract error
                    try
                    {
                        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(rawResponse);

                        if (jsonResponse.TryGetProperty("error", out var errorElement))
                        {
                            return new WorkflowResponse
                            {
                                Success = false,
                                Error = errorElement.GetString(),
                                StatusCode = (int)response.StatusCode,
                                RawResponse = rawResponse
                            };
                        }
                    }
                    catch
                    {
                        // If JSON parsing fails, return raw response
                    }

                    return new WorkflowResponse
                    {
                        Success = false,
                        Error = "Unexpected response format",
                        StatusCode = (int)response.StatusCode,
                        RawResponse = rawResponse
                    };
                }
            }
            else
            {
                var rawResponse = await response.Content.ReadAsStringAsync();
                return new WorkflowResponse
                {
                    Success = false,
                    Error = $"Flask API returned {response.StatusCode}",
                    StatusCode = (int)response.StatusCode,
                    RawResponse = rawResponse
                };
            }
        }
        catch (Exception ex)
        {
            return new WorkflowResponse
            {
                Success = false,
                Error = $"Request failed: {ex.Message}",
                StatusCode = 500
            };
        }
    }
}
