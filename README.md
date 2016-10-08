# DachauTemp
This is a sample demo application that I use to measure the current temperature and humidity data in my apartment in Dachau (Germany) to collect them in the cloud.

[![screenshot]](https://msit.powerbi.com/view?r=eyJrIjoiMDQ2MmI4ZTctZjQxZS00N2Y4LWEwNzYtODRjZDE2ZWJmZDAwIiwidCI6IjcyZjk4OGJmLTg2ZjEtNDFhZi05MWFiLTJkN2NkMDExZGI0NyIsImMiOjV9)
[screenshot]: https://raw.githubusercontent.com/robinmanuelthiel/DachauTemp/master/Misc/PowerBiScreenshot.png "Power BI Dashboard"


## Setup
I use a Raspberry Pi 3 running [Windows 10 IoT Core](https://developer.microsoft.com/en-us/windows/iot) connected with the [GHI FEZ Cream](https://www.ghielectronics.com/catalog/product/541) connected to a tempertature and humiditysensor. The shield can be accessed super easy with the according library that is available via NuGet.

On the device runs a simple Universal Windows Application that uses the FEZ driver to measure the temperature data and humidity and sends them together with the current time stamp to an [Azure IoT Hub](https://azure.microsoft.com/services/iot-hub/) that collects the data.

Once the data reached the Event Hub, an [Azure Stream Analytics](https://azure.microsoft.com/services/stream-analytics/) job sends the values to a [Power BI](https://powerbi.microsoft.com) dashboard where I can monitor the values.

## How to get started
To get this demo running you need to follow these steps:

#### 1.  Create an Azure Event Hub
Due Event Hubs are currently only available in the [old Azure portal](https://manage.windowsazure.com), login there, go to the *Service Bus* section and create a new Event Hub.

Once this is done, select your Event Hub inside the portal, click on *Configure* and create two shared access policies: One for your client (send permission only) and one for your Stream Analytics job (manage, send and listen permissions).

Take a look at the shared access key generator below and copy replace the primary key with the placeholder in the [EventHubService.cs](https://github.com/robinmanuelthiel/DachauTemp/blob/master/DachauTemp.Windows/Services/EventHubService.cs#L20) file.
```
private const string sasKeyValue = "YOUR_KEY_HERE";
```
Next, replace my Event Hub address with yours.
```
private const string baseAddress = "https://dachautemp-ns.servicebus.windows.net";
```

#### 2. Send measurements to the event hub
This is mainly what the code does. Just deploy it on a Raspberry Pi with Windows 10 IoT Core and FEZ Hat or FEZ Cream connected. Head over to the [MainPage.xaml.cs](https://github.com/robinmanuelthiel/DachauTemp/blob/master/DachauTemp.Windows/MainPage.xaml.cs#L32#L33) file and uncomment one of these lines to choose between the FEZ Hat or FEZ Cream. When using the FEZ Cream, you have to provide the port your temperature-humidity-sensor is connected to.

```
shield = new GHIFezHatShield();
shield = new GHIFezCreamShield(4);
```
Now simply deploy and run the code. The application will login to your EventHub and send it an updated temperature every 30 minutes.

#### 3. Stream the EventHub data to a Power BI dashboard
To view the data in a Power BI dashboard, we need to connect the Event Hub and Power BI together. This connection is the Stream Analytics job. Simply head over to the Azure portal and create a one. You will be asked for an input and output. Choose the Event Hub as the input source for your job and select the second access policy with manage, send and listen permissions. The output source should be an Power BI account that you can connect with Azure.

Make sure that your query collects all the data from your Event Hub and simply reaches it into PowerBi. The code should look as simple as this:
```
SELECT * INTO [PowerBiOutput] FROM [EventHubInput]
```
The values inside the square brackets may differ of course. Just make sure it the the alias you chose for your input and output.

