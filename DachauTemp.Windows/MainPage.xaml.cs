using DachauTemp.Windows.Services;
using GHIElectronics.UWP.Shields;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DachauTemp.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private EventHubService eventHubService;
        private FEZHAT hat;
        private DispatcherTimer timer;

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
                // Connect with GHI FEZ Hat
                hat = await FEZHAT.CreateAsync();

                // Collect and send data once
                await ProcessTempAndHumidAsync();

                // Setup timer to collect and send data repetitively
                this.timer = new DispatcherTimer();
                this.timer.Interval = TimeSpan.FromMilliseconds(60000);
                this.timer.Tick += Timer_Tick;
                this.timer.Start();
            }
            catch (ArgumentOutOfRangeException)
            {
                // No FEZHat found
                UpdateValue.Text = "No FEZHat found.";
            }
        }

        private async void Timer_Tick(object sender, object e)
        {
            await ProcessTempAndHumidAsync();
        }

        private async Task ProcessTempAndHumidAsync()
        {
            // Collect data
            var temp = hat.GetTemperature();
            var humidity = 0.0;

            // Update UI
            TempValue.Text = Math.Round(temp, 2) + " C";
            HumidityValue.Text = Math.Round(humidity, 2) + "%";
            UpdateValue.Text = DateTime.Now.ToString();

            // Send data to event hub
            await eventHubService.SendTempAndHumidityAsync(temp, humidity);
        }
    }
}
