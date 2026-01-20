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
    }
}