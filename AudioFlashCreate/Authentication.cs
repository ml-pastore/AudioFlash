using System;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;

public class Authentication
{

    
    public string tokenFetchUri {set; get;}
    public string subscriptionKey {set; get;}
    public string resourceName {set; get;}
    public string hostName {set; get;}

 
    public void AssertAuthentication()
    {
        if (string.IsNullOrWhiteSpace(tokenFetchUri))
        {
            throw new ArgumentNullException(nameof(tokenFetchUri));
        }   
        if (string.IsNullOrWhiteSpace(subscriptionKey))
        {
            throw new ArgumentNullException(nameof(subscriptionKey));
        }
        this.tokenFetchUri = tokenFetchUri;
        this.subscriptionKey = subscriptionKey;
    }

    public async Task<string> FetchTokenAsync()
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.subscriptionKey);
            UriBuilder uriBuilder = new UriBuilder(this.tokenFetchUri);

            var result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null).ConfigureAwait(false);
            return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}