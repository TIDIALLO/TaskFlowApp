using System.Net.Http.Json;
using TaskFlow.Shared.Contracts.Tasks;

namespace TaskFlow.Client.Services;

/// <summary>
/// Service client pour les opérations CRUD sur les tâches.
/// Encapsule les appels HTTP vers l'API backend.
/// </summary>
public class TaskService
{
    private readonly HttpClient _httpClient;

    public TaskService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<TaskItemResponse>> GetMyTasksAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<TaskItemResponse>>("api/tasks");
        return result ?? [];
    }

    public async Task<TaskItemResponse?> GetByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<TaskItemResponse>($"api/tasks/{id}");
    }

    public async Task<ServiceResult> CreateAsync(CreateTaskRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/tasks", request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>();
            return ServiceResult.Fail(error?.Detail ?? "Failed to create task.");
        }
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(Guid id, UpdateTaskRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/tasks/{id}", request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>();
            return ServiceResult.Fail(error?.Detail ?? "Failed to update task.");
        }
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ChangeStatusAsync(Guid id, string newStatus)
    {
        var request = new ChangeStatusRequest(newStatus);
        var response = await _httpClient.PatchAsJsonAsync($"api/tasks/{id}/status", request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>();
            return ServiceResult.Fail(error?.Detail ?? "Failed to change status.");
        }
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/tasks/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>();
            return ServiceResult.Fail(error?.Detail ?? "Failed to delete task.");
        }
        return ServiceResult.Ok();
    }
}
