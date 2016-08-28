# DachauTemp
This is a sample demo application that I use to collect the current temperature and humidity data in my apartment in Dachau (Germany) to collect them in the cloud.

## Setup
I use a Raspberry Pi 3 running [Windows 10 IoT Core](https://developer.microsoft.com/en-us/windows/iot) connected with the [GHI Electronics FEZ Hat](https://www.ghielectronics.com/catalog/product/500) which comes with an integrated tempertature sensor that can be accessed super easy with the according library that is available via [NuGet](https://www.nuget.org/packages/GHIElectronics.UWP.Shields.FEZHAT/).

On the device runs a simple Universal Windows Application that uses the FEZ Hat driver to measure the temperature data (humidity is not implemented yed) and sends them together with the current time stamp to an [Azure Event Hub](https://azure.microsoft.com/services/event-hubs/) that collects the data.

Once the data rached the Event Hub, an [Azure Stream Analytics](https://azure.microsoft.com/services/stream-analytics/) job sends the values to a [Power BI](https://powerbi.microsoft.com) dashboard where I can monitor the values.

## How to get started
To get this demo running you need to follow these steps:

#### 1.  Create an Azure Event Hub
Due Event Hubs are currently only available in the [old Azure portal](https://manage.windowsazure.com), login there, go to the *Service Bus* section and create a new Event Hub.

Once this is done, select your Event Hub inside the portal, click on *Configure* and create two shared access policies: One for your client (send permission only) and one for your Stream Analytics job (manage, send and listen permissions).

Take a look at the shared access key generator below and copy replace the primary key with the placeholder in the [EventHubService.cs](https://github.com/robinmanuelthiel/DachauTemp/blob/master/DachauTemp.Windows/Services/EventHubService.cs#L20) file.
