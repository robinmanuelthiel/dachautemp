using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DachauTemp.Windows.Models
{
    public class TempHumidityEvent
    {
        public DateTime DateTime { get; set; }
        public double Temperature { get; set; }
        public Double Humidity { get; set; }
    }
}
