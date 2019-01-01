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

            IConfig c = new Config();
            string aa = "bb";

            try{
                c = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configName));
            }
            catch(Exception e)
            {
                aa = "cc";
            }

            c.Authentication = JsonConvert.DeserializeObject<Authentication>
                (File.ReadAllText(c.FileInPut.authFile));

            ///// SAMPLE //////
            TestToken t = new TestToken();

            ISound sndOut = new SoundOutput(c.SoundDefault);
            t.Sound = sndOut;
            t.TextIn = "Are you going to Scarborough fair?";
            t.FileOut = string.Format(@"{0}\sample.wav",c.FileOutPut.SoundFolder);

            await t.testToken(c.Authentication);

            ///// SAMPLE //////
            sndOut = new SoundOutput(c.SoundDefault);
            sndOut.Language = "de-DE";
            sndOut.Speaker = "Hedda";
            t.Sound = sndOut;
            t.TextIn = "Der, die, oder das?  Aktivität.";
            t.FileOut = string.Format(@"{0}\sample1.wav",c.FileOutPut.SoundFolder);

            await t.testToken(c.Authentication);

            ///// SAMPLE //////
            sndOut = new SoundOutput(c.SoundDefault);
            t.TextIn = "Are you STILL going to Scarborough fair?";
            t.Sound = sndOut;
            t.FileOut = string.Format(@"{0}\sample2.wav",c.FileOutPut.SoundFolder);

            await t.testToken(c.Authentication);

            Console.WriteLine($"Done");
            Environment.Exit((int) RetCodes.Success);
        
        }

    }

    class TestToken
    {

        public string TextIn {set; get;}
        public string FileOut {set; get;}
        public ISound Sound {set; get;}

        public async Task testToken(IAuthentication Auth)
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

            string speakVer = Sound.SpeakVersion.Replace("!DefaultLang!", Sound.Language);

            string body = String.Format(@"<speak {0} "+
                        "<voice name='Microsoft Server Speech Text to Speech Voice ({1})'> " +
                        "<prosody rate='{2}'> " +
                        "{3} </prosody></voice></speak>"
                        , speakVer
                        , String.Concat(Sound.Language,", ",Sound.Speaker)
                        , Sound.ProsodyRate
                        ,TextIn);

                        
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
                    request.Headers.Add("X-Microsoft-OutputFormat", Sound.OutputFormat);
                    
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
