using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherModule.Core.Helpers
{
    public static class WeatherIconHelper
    {
        public static string GetIcon(int code)
        {
            return code switch
            {
                0 => "bi-sun-fill",
                1 or 2 => "bi-cloud-sun-fill",
                3 => "bi-cloud-fill",
                45 or 48 => "bi-cloud-fog-fill",
                51 or 53 or 55 => "bi-cloud-drizzle-fill",
                61 or 63 or 65 => "bi-cloud-rain-fill",
                71 or 73 or 75 => "bi-snow",
                80 or 81 or 82 => "bi-cloud-rain-heavy-fill",
                _ => "bi-question-circle"
            };
        }
    }
}
