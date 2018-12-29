using System;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AudioFlash
{
    class Program
    {

        public enum RetCodes {Success, InvalidParm, ConfigFileNotFound}

        static async Task Main(string[] args)
        {

            string configName = "_bin/config.json";
            
            Console.WriteLine($"{configName}");
            
            if(! File.Exists(configName))
            {
                Console.WriteLine($"File does not exist: {configName}");
                Environment.Exit((int) RetCodes.ConfigFileNotFound);
            }

            IConfig c = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configName));
            
            c.Authentication = JsonConvert.DeserializeObject<Authentication>
                (File.ReadAllText(c.FileInPut.authFile));

            
            TestToken t = new TestToken(c);
            t.TextIn = "Howdy, stranger";
            t.FileOut = string.Format(@"{0}\sample.wav",c.FileOutPut.SoundFolder);

            await t.testToken(c.Authentication);

            Console.WriteLine($"Done");
            Environment.Exit((int) RetCodes.Success);
        
        }

    }

    class TestToken
    {

        public string TextIn {set; get;}
        public string FileOut {set; get;}
     
        IConfig _config {set; get;}

        public TestToken(IConfig c)
        {
            _config = c;
        }

        public async Task testToken(Authentication Auth)
        {

            // Gets an access token
          
            string myURI = Auth.tokenFetchUri;
            string myKey = Auth.subscriptionKey;
            string resourceName = Auth.resourceName;
            string host = Auth.hostName;

            string accessToken;
            Console.WriteLine("Attempting token exchange. Please wait...\n");

            try
            {
                accessToken = await Auth.FetchTokenAsync().ConfigureAwait(false);
                Console.WriteLine("Successfully obtained an access token. \n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to obtain an access token.");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine(accessToken);

            string body = String.Format(@"<speak {0} "+
                        "<voice name='{1}'> " +
                        "{2} </voice></speak>"
                        , _config.SpeakVersion, _config.Voice, TextIn);

                        
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(host);
                    request.Content = new StringContent(body, Encoding.UTF8, "application/ssml+xml");
                    request.Headers.Add("Authorization", "Bearer " + accessToken);
                    request.Headers.Add("Connection", "Keep-Alive");
                    request.Headers.Add("User-Agent", resourceName);
                    request.Headers.Add("X-Microsoft-OutputFormat", _config.OutputFormat);
                    
                    Console.WriteLine("Calling the TTS service. Please wait... \n");
                    
                    using (var response = await client.SendAsync(request).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var dataStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            Console.WriteLine("Your speech file is being written to file {0}...", FileOut);
                            using (var fileStream = new FileStream(FileOut
                                , FileMode.Create, FileAccess.Write, FileShare.Write))
                            {
                                await dataStream.CopyToAsync(fileStream).ConfigureAwait(false);
                                fileStream.Close();
                            }
                            Console.WriteLine("\nYour file is ready. ");
                            
                        }
                    }
                }
            }






        } // testToken()


    }
}
