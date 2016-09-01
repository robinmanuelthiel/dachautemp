using DachauTemp.Windows.Models;
using DachauTemp.Windows.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DachauTemp.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private EventHubService eventHubService;
        private DispatcherTimer timer;
        private IGHIShield shield;
        private GpioController gpio;
        private GpioPin redStatus;
        private GpioPin greenStatus;
        private List<double> tempMeasurements;
        private List<double> humidMeasurements;

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;

            eventHubService = new EventHubService();
            gpio = GpioController.GetDefault();
            tempMeasurements = new List<double>();
            humidMeasurements = new List<double>();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Register status LEDs
                redStatus = gpio.OpenPin(35);
                redStatus.SetDriveMode(GpioPinDriveMode.Output);
                greenStatus = gpio.OpenPin(47);
                greenStatus.SetDriveMode(GpioPinDriveMode.Output);
            }
            catch (COMException)
            {
                // Some Raspberry Pi Models (like model 3) can't control power and status LEDs via pins...
            }

            try
            {
                // Connect with GHI shield
                // Uncomment one of the following two lines to either init with the GHI FEZ Hat or GHI FEZ Cream
                //shield = new GHIFezHatShield();
                shield = new GHIFezCreamShield(4);

                // Init shield
                await shield.InitiatizeAsync();

                // Setup timer to collect and send data repetitively
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            catch (Exception)
            {
                // Right hat not found
                UpdateValue.Text = "The desired shield could not be found.";
            }
        }

        private async void Timer_Tick(object sender, object e)
        {
            // Switch off status LEDs
            redStatus?.Write(GpioPinValue.Low);
            greenStatus?.Write(GpioPinValue.Low);

            // Collect temperature and humidity
            var temp = shield.GetTemperature();
            var humid = shield.GetHumidity();
            tempMeasurements.Add(temp);
            humidMeasurements.Add(humid);

            // Update UI
            TempValue.Text = Math.Round(temp, 2) + " \u00B0C";
            HumidityValue.Text = Math.Round(humid, 2) + "%";
            UpdateValue.Text = DateTime.Now.ToString();

            // Send average of collected humid and temp every full 30 minutes
            var minute = DateTime.Now.Minute;
            if (minute == 0 || minute == 30)
                await SendAvgTempAndHumidAsync();
        }

        private async Task SendAvgTempAndHumidAsync()
        {
            // Calculate average
            var avgTemp = tempMeasurements.Average();
            var avgHumid = humidMeasurements.Average();

            // Clear lists
            tempMeasurements.Clear();
            humidMeasurements.Clear();

            // Send data to event hub
            await eventHubService.SendTempAndHumidityAsync(avgTemp, avgHumid);
        }
    }
}
