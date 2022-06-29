using System.ComponentModel;

namespace Uchet.Classes
{
    internal class ArriveUser : INotifyPropertyChanged
    {
        private bool _isArrive;
        public int num { get; set; }
        public string rank { get; set; }
        public string fName { get; set; }

        public bool isArrive
        {
            get { return _isArrive; }
            set
            {
                if (_isArrive != value)
                {
                    _isArrive = value;
                    OnPropertyChanged("isArrive");
                }
            }
        }


        public ArriveUser() { }
        public ArriveUser(int num, string rank, string fName, bool isArrive)
        {
            this.num = num;
            this.rank = rank;
            this.fName = fName;
            this._isArrive = isArrive;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
