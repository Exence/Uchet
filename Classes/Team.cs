using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Uchet.Classes
{
    internal class Team : INotifyPropertyChanged
    { 

        private int onList, onFace, onService, absent, ch10, ch15, ch20, noArrived, shouldCome;

        public int id { get; set; }

        public string teamName;

        public int OnList
        {
            get { return onList; }
            set {
                if (onList != value)
                {
                    onList = value;
                    OnPropertyChanged("onList");
                }
            }
        }
        public int OnFace
        {
            get { return onFace; }
            set {
                if (onFace != value)
                {
                    onFace = value;
                    OnPropertyChanged("onFace");
                }
            }
        }
        public int OnService
        {
            get { return onService; }
            set {
                if (onService != value)
                {
                    onService = value;
                    OnPropertyChanged("onService");
                }
             }
        }
        public int Absent
        {
            get { return absent; }
            set {
                if (absent != value)
                {
                    absent = value;
                    OnPropertyChanged("absent");
                }
             }
        }
        public int Ch10
        {
            get { return ch10; }
            set {
                if (ch10 != value)
                {
                    ch10 = value;
                    OnPropertyChanged("ch10");
                }
            }
        }
        public int Ch15
        {
            get { return ch15; }
            set {
                if (ch15 != value)
                {
                    ch15 = value;
                    OnPropertyChanged("ch15");
                }
            }
        }
        public int Ch20
        {
            get { return ch20; }
            set {
                if (ch20 != value)
                {
                    ch20 = value;
                    OnPropertyChanged("ch20");
                }
            }
        }
        public int NoArrived
        {
            get { return noArrived; }
            set {
                if (noArrived != value)
                {
                    noArrived = value;
                    OnPropertyChanged("noArrived");
                }
            }
        }
        public int ShouldCome
        {
            get { return shouldCome; }
            set {
                if (shouldCome != value)
                {
                    shouldCome = value;
                    OnPropertyChanged("shouldCome");
                }
            }
        }
        public string TeamName
        {
            get { return teamName; }
            set {
                if (teamName != value)
                {
                    teamName = value;
                    OnPropertyChanged("teamName");
                }
            }
        }

        public Team() { }
        public Team(string teamName)
        {
            this.teamName = teamName;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = "Teams")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
