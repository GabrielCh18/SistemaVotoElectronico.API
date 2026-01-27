using System.Net.Http.Json;
using SistemaVoto.Modelos; // <--- Asegúrate que coincida con tu namespace de modelos

namespace SistemaVotoElectronico.ApiConsumer
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        // ⚠️ ¡CONFIRMA QUE ESTE SEA TU PUERTO CUANDO DAS PLAY A LA API! ⚠️
        private readonly string _baseUrl = "https://localhost:7090/api/";

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // OPCIÓN A: TRAER UN SOLO OBJETO (Ej: El Resumen de Resultados)
        public async Task<ApiResult<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                // Tu API devuelve el objeto directo, así que lo leemos así:
                var datos = await _httpClient.GetFromJsonAsync<T>($"{_baseUrl}{endpoint}");

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

        // OPCIÓN B: TRAER LISTAS (Ej: Lista de Candidatos)
        public async Task<ApiResult<List<T>>> GetListAsync<T>(string endpoint)
        {
            try
            {
                var datos = await _httpClient.GetFromJsonAsync<List<T>>($"{_baseUrl}{endpoint}");
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
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}{endpoint}", data);

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
        // ---------------------------------------------------------
        // METODO PARA BORRAR (DELETE)
        // ---------------------------------------------------------
        public async Task<ApiResult<string>> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}{endpoint}");

                if (response.IsSuccessStatusCode)
                {
                    // Si salió bien (200 OK), devolvemos éxito
                    return new ApiResult<string>
                    {
                        Success = true,
                        Message = "Eliminado correctamente"
                    };
                }
                else
                {
                    // Si falló, leemos qué pasó
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
        // MÉTODO NUEVO: Envía datos y ESPERA UNA RESPUESTA con datos (TResponse)
        public async Task<ApiResult<TResponse>> PostWithResponseAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}{endpoint}", data);

                if (response.IsSuccessStatusCode)
                {
                    // Aquí SÍ leemos lo que devuelve la API (el JSON con código y nombre)
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