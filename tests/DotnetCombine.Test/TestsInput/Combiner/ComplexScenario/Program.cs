// Using usings in top level statements

using System.Net.Http;

using var client = new HttpClient();
using var request = new HttpRequestMessage();

request.Content = new StringContent("");
await client.SendAsync(request);
