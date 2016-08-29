using System;

namespace DachauTemp.Windows.Models
{
    public class TempHumidityEvent
    {
        public DateTime DateTime { get; set; }
        public double Temperature { get; set; }
        public Double Humidity { get; set; }
    }
}
