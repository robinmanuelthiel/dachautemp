using DachauTemp.Windows.Models;
using DachauTemp.Windows.Services;
using System;
using System.Threading.Tasks;
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

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            eventHubService = new EventHubService();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Connect with GHI shield
                // Uncomment one of the following two lines to either init with the GHI FEZ Hat or GHI FEZ Cream
                shield = new GHIFezHatShield();
                //shield = new GHIFezCreamShield(4);

                // Init shield
                await shield.InitiatizeAsync();

                // Collect and send data once
                await ProcessTempAndHumidAsync();

                // Setup timer to collect and send data repetitively
                this.timer = new DispatcherTimer();
                this.timer.Interval = TimeSpan.FromMinutes(30);
                this.timer.Tick += Timer_Tick;
                this.timer.Start();
            }
            catch (ArgumentOutOfRangeException)
            {
                // No FEZHat found
                UpdateValue.Text = "No shield found.";
            }
        }

        private async void Timer_Tick(object sender, object e)
        {
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
