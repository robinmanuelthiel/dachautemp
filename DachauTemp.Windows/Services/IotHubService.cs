using DachauTemp.Windows.Models;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DachauTemp.Windows.Services
{
    public class IotHubService
    {
        private DeviceClient deviceClient;
        private string iotHubHostname = "{Azure IoT Hub Hostname}";
        private string deviceId = "{Device ID}";
        private string deviceKey = "{Device Primary Key}";

        public IotHubService()
        {
            // Initialize device client
            deviceClient = DeviceClient.Create(iotHubHostname, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));
        }

        public async Task SendTempAndHumidityAsync(double avgTemperature, double avgHumidity)
        {
            // Round to full seconds and milliseconds
            var now = DateTime.Now.ToUniversalTime();
            now = now.AddSeconds(now.Second * -1).AddMilliseconds(now.Millisecond * -1);

            // Create telemetry data to send
            var dataPoint = new
            {
                DateTime = now,
                Temperature = avgTemperature,
                Humidity = avgHumidity,
                DeviceId = deviceId,
                Project = "DachauTemp"
            };

            // Convert to JSON and create Message
            var json = JsonConvert.SerializeObject(dataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(json));

            // Send event
            await deviceClient.SendEventAsync(message);
        }
    }
}
