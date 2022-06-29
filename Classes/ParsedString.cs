using System;

namespace Uchet.Classes
{
    internal class ParsedString
    {
        public DateTime arriveTime { get; set; }
        public DateTime timeAfterSignal { get; set; }
        public string passType { get; set; }
        public string eventType { get; set; }
        public string surname { get; set; }
        public string name { get; set; }
        public string middleName { get; set; }

        public ParsedString() { }


    }
}
