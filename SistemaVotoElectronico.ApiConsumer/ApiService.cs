using System.Net.Http.Json;
using SistemaVoto.Modelos;

namespace SistemaVotoElectronico.ApiConsumer
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        // CAMBIO CRUCIAL: Borramos la variable _baseUrl.
        // Ahora confiamos ciegamente en la URL que configuramos en Program.cs (Render).

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // OPCIÓN A: TRAER UN SOLO OBJETO
        public async Task<ApiResult<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                // Usamos directamente "endpoint". El HttpClient ya sabe la base (Render).
                var datos = await _httpClient.GetFromJsonAsync<T>(endpoint);

                return new ApiResult<T>
                {
                    Success = true,
                    Data = datos
                };
            }
            catch (Exception ex)
            {
                return new ApiResult<T> { Success = false, Message = ex.Message };
            }
        }

        // OPCIÓN B: TRAER LISTAS
        public async Task<ApiResult<List<T>>> GetListAsync<T>(string endpoint)
        {
            try
            {
                var datos = await _httpClient.GetFromJsonAsync<List<T>>(endpoint);
                return new ApiResult<List<T>> { Success = true, Data = datos };
            }
            catch (Exception ex)
            {
                return new ApiResult<List<T>> { Success = false, Message = ex.Message };
            }
        }

        // OPCIÓN C: ENVIAR DATOS (POST)
        public async Task<ApiResult<string>> PostAsync<T>(string endpoint, T data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResult<string> { Success = true, Message = "Éxito" };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new ApiResult<string> { Success = false, Message = error };
                }
            }
            catch (Exception ex)
            {
                return new ApiResult<string> { Success = false, Message = ex.Message };
            }
        }

        // METODO PARA BORRAR (DELETE)
        public async Task<ApiResult<string>> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResult<string>
                    {
                        Success = true,
                        Message = "Eliminado correctamente"
                    };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new ApiResult<string>
                    {
                        Success = false,
                        Message = error
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResult<string>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        // MÉTODO POST CON RESPUESTA DE DATOS
        public async Task<ApiResult<TResponse>> PostWithResponseAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<TResponse>();
                    return new ApiResult<TResponse>
                    {
                        Success = true,
                        Data = resultado
                    };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new ApiResult<TResponse> { Success = false, Message = error };
                }
            }
            catch (Exception ex)
            {
                return new ApiResult<TResponse> { Success = false, Message = ex.Message };
            }
        }
    }
}