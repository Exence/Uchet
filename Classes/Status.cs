using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uchet.Classes
{
    public class Status
    {
        public int id { get; set; }
        public string statusName { get; set; }

        public Status() { }

        public Status(string statusName)
        {
            this.statusName = statusName;
        }
    }
}
