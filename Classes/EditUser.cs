using System;
using System.ComponentModel;

namespace Uchet.Classes
{
    internal class EditUser : INotifyPropertyChanged
    {
        private int _mainId, _userId, _num;
        private string _name, _surname, _middleName, _statusName, _rankName, _position;

        public int mainId
        {
            get { return _mainId; }
            set
            {
                if (_mainId != value)
                {
                    _mainId = value;
                    OnPropertyChanged("mainId");
                }
            }
        }

        public int userId
        {
            get { return _userId; }
            set
            {
                if (_userId != value)
                {
                    _userId = value;
                    OnPropertyChanged("userId");
                }
            }
        }

        public int num
        {
            get { return _num; }
            set
            {
                if (_num != value)
                {
                    _num = value;
                    OnPropertyChanged("num");
                }
            }
        }

        public String name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("name");
                }
            }
        }

        public String surname
        {
            get { return _surname; }
            set
            {
                if (_surname != value)
                {
                    _surname = value;
                    OnPropertyChanged("surname");
                }
            }
        }

        public String middleName
        {
            get { return _middleName; }
            set
            {
                if (_middleName != value)
                {
                    _middleName = value;
                    OnPropertyChanged("middleName");
                }
            }
        }

        public String statusName
        {
            get { return _statusName; }
            set
            {
                if (_statusName != value)
                {
                    _statusName = value;
                    OnPropertyChanged("statusName");
                }
            }
        }

        public String rankName
        {
            get { return _rankName; }
            set
            {
                if (_rankName != value)
                {
                    _rankName = value;
                    OnPropertyChanged("rankName");
                }
            }
        }

        public String position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged("position");
                }
            }
        }

        public EditUser() { }
        public EditUser(int _mainId, int _userId, int _num, string _name, string _surname, string _middleName, string _statusName, string _rankName, string _position)
        {
            this._mainId = _mainId;
            this._userId = _userId;
            this._num = _num;
            this._name = _name;
            this._surname = _surname;
            this._middleName = _middleName;
            this._statusName = _statusName;
            this._rankName = _rankName;
            this._position = _position;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
