using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;


namespace Client
{
    public class Program
    {
        private static async Task Main()
        {
            // discovers endpoints from metadata
            var client = new HttpClient();

            var discovery = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
            if (discovery.IsError)
            {
                Console.WriteLine(discovery.Error);
                return;
            }

            // request access token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discovery.TokenEndpoint,
                ClientId = "aqts",
                ClientSecret = "aqts-secret",
                Scope = "acquisition provisioning publish"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine("Retrieved token to IdentityServer for AQTS APIs");
            Console.WriteLine(tokenResponse.Json);

            // call api with access token
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var response = await apiClient.GetAsync("https://localhost:6001/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                Console.WriteLine(JsonSerializer.Serialize(content, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }
}