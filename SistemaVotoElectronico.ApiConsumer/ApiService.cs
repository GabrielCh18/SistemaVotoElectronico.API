using System.Net.Http.Json;
using SistemaVotoElectronico.Modelos;

namespace SistemaVotoElectronico.ApiConsumer;

public class ApiService
{
    private readonly HttpClient _httpClient;
    // IMPORTANTE: Este puerto debe ser el mismo que sale al correr tu API
    private readonly string _baseUrl = "https://localhost:7124/api/";

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Método genérico para obtener listas (Candidatos, Elecciones, etc.)
    public async Task<ApiResult<List<T>>> GetListAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResult<List<T>>>($"{_baseUrl}{endpoint}");
            return response ?? new ApiResult<List<T>> { Success = false, Message = "Error al obtener datos" };
        }
        catch (Exception ex)
        {
            return new ApiResult<List<T>> { Success = false, Message = ex.Message };
        }
    }

    // Método para enviar datos (Login, Registro, Votar)
    public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}{endpoint}", data);
            var result = await response.Content.ReadFromJsonAsync<ApiResult<TResponse>>();
            return result ?? new ApiResult<TResponse> { Success = false, Message = "Respuesta vacía del servidor" };
        }
        catch (Exception ex)
        {
            return new ApiResult<TResponse> { Success = false, Message = ex.Message };
        }
    }
}