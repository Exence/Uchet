using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uchet.Classes
{
    public static class Flags
    {
        public static bool isStarted { get; set; }
        public static DateTime ConvertedTime { get; set; }
        public static int selectedIndex { get; set; }
        public static string hh { get; set; } /// Время подачи сигнала (часы)
        public static string mm { get; set; } /// Время подачи сигнала (минуты)

    }
}
