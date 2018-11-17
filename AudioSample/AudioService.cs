using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace AudioSample
{
    public class AudioService : IHostedService
    {
        private readonly IHttpClientFactory _httpClient;

        public AudioService(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var status = SaveAudioFileAsync();
            return Task.CompletedTask;
        }

        private async Task<bool> SaveAudioFileAsync()
        {
            string host = "https://westus.tts.speech.microsoft.com";
            // The TTS service requires an access token. See X for details.
            string subscriptionKey = "YOUR SUBSCRIPTION KEY HERE";
            string route = "/cognitiveservices/v1";

            string accessToken;
            Console.WriteLine("Attempting token exchange. Please wait...\n");
            Authentication auth = new Authentication(subscriptionKey);

            try
            {
                accessToken = auth.GetAccessToken();
                Console.WriteLine("Successfully obtained an access token. \n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to obtain an access token.");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.Message);
                return false;
            }

            // SSML + XML body for the request
            string body = @"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
                              <voice name='Microsoft Server Speech Text to Speech Voice (en-US, Jessa24kRUS)'>
                               We hope you enjoy using Text-to-Speech, a Microsoft Speech Services feature.
                              </voice></speak>";

            // Instantiate the client
            using (var request = new HttpRequestMessage())
            {
                // Set the HTTP method
                request.Method = HttpMethod.Post;

                // Construct the URI
                request.RequestUri = new Uri(host + route);

                // Set the content type header
                request.Content = new StringContent(body, Encoding.UTF8, "application/ssml+xml");

                // Set additional header, such as Authorization and User-Agent
                request.Headers.Add("Authorization", "Bearer " + accessToken);
                request.Headers.Add("Connection", "Keep-Alive");
                request.Headers.Add("User-Agent", "erhopf-speech-test");
                request.Headers.Add("X-Microsoft-OutputFormat", "riff-24khz-16bit-mono-pcm");
                request.Headers.Add("Connection", "Keep-Alive");
                try
                {
                    var client = _httpClient.CreateClient();
                    // Create a request                   
                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();

                        Console.WriteLine($"Status code: {response.StatusCode}");

                        // Asynchronously read the response
                        using (var dataStream = await response.Content.ReadAsStreamAsync())
                        {
                            /* Write the response to a file. In this sample,
                             * it's an audio file. Then close the stream. */
                            using (var fileStream = new FileStream(@"sample.wav", FileMode.Create, FileAccess.Write, FileShare.Write))
                            {
                                await dataStream.CopyToAsync(fileStream);
                                fileStream.Close();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (new FileInfo("sample.wav").Length == 0)
            {
                Console.WriteLine("The response is empty. Please check your request. Press any key to exit.");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Your speech file is ready for playback. Press any key to exit.");
                Console.ReadLine();
            }

            return true;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
    public class Authentication
    {
        public static readonly string FetchTokenUri = "https://westus.api.cognitive.microsoft.com/sts/v1.0/issueToken";
        private string subscriptionKey;
        private string token;

        public Authentication(string subscriptionKey)
        {
            this.subscriptionKey = subscriptionKey;
            this.token = FetchTokenAsync(FetchTokenUri, subscriptionKey).Result;
        }
        private async Task<string> FetchTokenAsync(string fetchUri, string subscriptionKey)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                UriBuilder uriBuilder = new UriBuilder(fetchUri);

                var result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null);
                return await result.Content.ReadAsStringAsync();
            }
        }
        public string GetAccessToken()
        {
            return this.token;
        }
    }
}