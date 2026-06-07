using System.Net;
using System.Net.Http.Json;

namespace SCM3.Web.Services;

// Shared helper for the HTTP-client-backed service implementations in this folder —
// each wraps an endpoint group in SCM3.Api that returns 404 for "not found" rather than
// an empty 200, so a plain GetFromJsonAsync would throw instead of yielding null.
internal static class ApiHttpClientExtensions
{
    public static async Task<T?> GetOrDefaultAsync<T>(this HttpClient http, string requestUri)
    {
        var response = await http.GetAsync(requestUri);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}
