using DachauTemp.Windows.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace DachauTemp.Windows.Services
{
    public class EventHubService
    {
        private const string sasKeyName = "DachauTempSender";
        private const string sasKeyValue = "YOUR_KEY_HERE";
        private const string baseAddress = "https://dachautemp-ns.servicebus.windows.net";
        private HttpClient httpClient = new HttpClient();

        public async Task SendTempAndHumidityAsync(double temperature, double humidity)
        {
            var tempHumidityEvent = new TempHumidityEvent { DateTime = DateTime.Now, Temperature = temperature, Humidity = humidity };
            await SendEventAsync(tempHumidityEvent);
        }

        private async Task SendEventAsync(TempHumidityEvent tempHumidityEvent)
        {
            // Get token
            var token = CreateToken();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);

            // Convert TempHumidityEvent to JSON
            var json = JsonConvert.SerializeObject(tempHumidityEvent);
            var content = new StringContent(json);

            // Send event
            var response = await httpClient.PostAsync(baseAddress + "/dachautemp/messages", content);
        }

        public string CreateToken()
        {
            TimeSpan fromEpochStart = DateTime.UtcNow - new DateTime(1970, 1, 1);
            string expiry = Convert.ToString((int)fromEpochStart.TotalSeconds + 3600);
            string stringToSign = WebUtility.UrlEncode(baseAddress) + "\n" + expiry;

            // Create hash
            MacAlgorithmProvider provider = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
            var hash = provider.CreateHash(CryptographicBuffer.ConvertStringToBinary(sasKeyValue, BinaryStringEncoding.Utf8));
            hash.Append(CryptographicBuffer.ConvertStringToBinary(stringToSign, BinaryStringEncoding.Utf8));

            // Generate token
            var signature = CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset());
            string token = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}",
                            WebUtility.UrlEncode(baseAddress), WebUtility.UrlEncode(signature), expiry, sasKeyName);

            return token;
        }
    }
}
