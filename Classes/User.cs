using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uchet.Classes
{
    internal class User
    {
        public int id { get; set; }

        public int rankId;

        public string surname, name, middleName, position;

        public int RankId
        {
            get { return rankId; }
            set { rankId = value; }
        }
        public string Surname
        {
            get { return surname; }
            set { surname = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string MiddleName
        {
            get { return middleName; }
            set { middleName = value; }
        }
        public string Position
        {
            get { return position; }
            set { position = value; }
        }

        public User() { }

        public User(int rankId, string surname, string name, string middleName, string position)
        {
            this.rankId = rankId;
            this.surname = surname;
            this.name = name;
            this.middleName = middleName;
            this.position = position;
        }

        public User(int rankId)
        {
            this.rankId = rankId;            
        }
    }
}
