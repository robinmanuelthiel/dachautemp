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

        /// <summary>
        /// Sends a new combination of temperature, humidity and a time stamp to the Event Hub
        /// </summary>
        /// <param name="temperature">Current temperature</param>
        /// <param name="humidity">Current humidity</param>
        /// <returns></returns>
        public async Task SendTempAndHumidityAsync(double temperature, double humidity)
        {
            var tempHumidityEvent = new TempHumidityEvent { DateTime = DateTime.Now.ToUniversalTime(), Temperature = temperature, Humidity = humidity };
            await SendEventAsync(tempHumidityEvent);
        }

        /// <summary>
        /// Serializes the TempHumidityEvent to JSON and sends it to the Event Hub's REST API
        /// </summary>
        /// <param name="tempHumidityEvent"></param>
        /// <returns></returns>
        private async Task SendEventAsync(TempHumidityEvent tempHumidityEvent)
        {
            // Convert TempHumidityEvent to JSON
            var json = JsonConvert.SerializeObject(tempHumidityEvent);
            var content = new StringContent(json);

            // Send event
            var response = await httpClient.PostAsync(baseAddress + "/dachautemp/messages", content);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Maybe token expired available. Renew and try again
                RefreshAccessToken();

                // Try again
                content = new StringContent(json);
                response = await httpClient.PostAsync(baseAddress + "/dachautemp/messages", content);
            }

            // Check if everything went fine
            if (!response.IsSuccessStatusCode)
                throw new UnauthorizedAccessException("Could not connect to Azure Event Hub. Access denied.");
        }

        /// <summary>
        /// Refreshed the Event Hub authentication token
        /// </summary>
        public void RefreshAccessToken()
        {
            TimeSpan fromEpochStart = DateTime.UtcNow - new DateTime(1970, 1, 1);
            string expiry = Convert.ToString((int)fromEpochStart.TotalSeconds + 86400); // 86400s = 24h
            string stringToSign = WebUtility.UrlEncode(baseAddress) + "\n" + expiry;

            // Create hash
            MacAlgorithmProvider provider = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
            var hash = provider.CreateHash(CryptographicBuffer.ConvertStringToBinary(sasKeyValue, BinaryStringEncoding.Utf8));
            hash.Append(CryptographicBuffer.ConvertStringToBinary(stringToSign, BinaryStringEncoding.Utf8));

            // Generate token
            var signature = CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset());
            string token = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}",
                            WebUtility.UrlEncode(baseAddress), WebUtility.UrlEncode(signature), expiry, sasKeyName);

            // Set HTTP Access token
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.ExpectContinue = false;
            httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
        }
    }
}
