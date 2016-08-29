using GHIElectronics.UWP.Gadgeteer.Mainboards;
using GHIElectronics.UWP.Gadgeteer.Modules;
using GHIElectronics.UWP.GadgeteerCore;
using GHIElectronics.UWP.Shields;
using System.Threading.Tasks;

namespace DachauTemp.Windows.Models
{
    public interface IGHIShield
    {
        Task InitiatizeAsync();
        double GetTemperature();
        double GetHumidity();
    }

    public class GHIFezHatShield : IGHIShield
    {
        private FEZHAT hat;

        public async Task InitiatizeAsync()
        {
            hat = await FEZHAT.CreateAsync();
        }

        public double GetHumidity()
        {
            return 0.0;
        }

        public double GetTemperature()
        {
            return hat.GetTemperature();
        }
    }

    public class GHIFezCreamShield : IGHIShield
    {
        private FEZCream mainboard;
        private TempHumidSI70 tempHumid;
        private int tempHumidSocket;

        public GHIFezCreamShield(int tempHumidSocket)
        {
            this.tempHumidSocket = tempHumidSocket;
        }

        public async Task InitiatizeAsync()
        {
            mainboard = await Module.CreateAsync<FEZCream>();
            tempHumid = await Module.CreateAsync<TempHumidSI70>(mainboard.GetProvidedSocket(tempHumidSocket));
        }

        public double GetHumidity()
        {
            var measurement = tempHumid.TakeMeasurement();
            return measurement.RelativeHumidity;
        }

        public double GetTemperature()
        {
            var measurement = tempHumid.TakeMeasurement();
            return measurement.Temperature;
        }
    }
}
