using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uchet.Classes
{
    public class Rank
    {
        public int id { get; set; }
        public string rankName { get; set; }
        

        public Rank() { }

        public Rank(string rankName)
        {
            this.rankName = rankName;
        }
    }
}
