using DachauTemp.Windows.Models;
using DachauTemp.Windows.Services;
using System;
using System.Diagnostics;
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

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            eventHubService = new EventHubService();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            gpio = GpioController.GetDefault();

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

                // Collect and send data once
                await ProcessTempAndHumidAsync();

                // Setup timer to collect and send data repetitively
                this.timer = new DispatcherTimer();
                this.timer.Interval = TimeSpan.FromSeconds(1);
                this.timer.Tick += Timer_Tick;
                this.timer.Start();
            }
            catch (Exception ex)
            {
                // Right hat not found
                UpdateValue.Text = "The desired shield could not be found.";
                Debug.WriteLine(ex);
            }
        }

        private async void Timer_Tick(object sender, object e)
        {
            // Switch off status LEDs
            redStatus?.Write(GpioPinValue.Low);
            greenStatus?.Write(GpioPinValue.Low);

            // Update humid and temp every 30 minutes
            var minute = DateTime.Now.Minute;
            if (minute == 0 || minute == 30)
                await ProcessTempAndHumidAsync();
        }

        private async Task ProcessTempAndHumidAsync()
        {
            // Collect data
            var temp = shield.GetTemperature();
            var humidity = shield.GetHumidity();

            // Update UI
            TempValue.Text = Math.Round(temp, 2) + " \u00B0C";
            HumidityValue.Text = Math.Round(humidity, 2) + "%";
            UpdateValue.Text = DateTime.Now.ToString();

            // Send data to event hub
            await eventHubService.SendTempAndHumidityAsync(temp, humidity);
        }
    }
}
