using Microsoft.Extensions.Configuration;
using Model.Dtos.Request;
using Model.Dtos.Response;
using Model.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public async Task<WorkflowResponse> RunWorkflowAsync(WorkflowRequest request)
    {
        //var flaskUrl = _config["Flask:Url"];
        //var flaskToken = _config["Flask:Token"];

        //var json = JsonSerializer.Serialize(request);
        //var httpRequest = new HttpRequestMessage(HttpMethod.Post, flaskUrl)
        //{
        //    Content = new StringContent(json, Encoding.UTF8, "application/json")
        //};
        //Console.WriteLine("Calling Flask API:");
        //Console.WriteLine($"URL: {flaskUrl}");
        //Console.WriteLine($"Headers: Authorization: Bearer {flaskToken}");
        //Console.WriteLine($"Body: {json}");
        //// Add security header (same as Flask API expects)
        //httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", flaskToken);

        //var response = await _httpClient.SendAsync(httpRequest);
        //var rawResponse = await response.Content.ReadAsStringAsync();

        return new WorkflowResponse
        {
            //StatusCode = (int)response.StatusCode,
            //RawResponse = rawResponse
            StatusCode = 200,
            RawResponse = "mmmmmmmodelllll ruuuuuuunnnnn"
        };
    }
}
