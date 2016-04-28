using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Parser
{
    class InterfaceData : INotifyPropertyChanged
    {
        private string _text;
        private bool _isInProgress;
        private string _progressVisible;
        private string _log;
        private int? _progress;

        public string Text {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        public bool IsInProgress
        {
            get { return _isInProgress; }
            set
            {
                _isInProgress = value;
                ProgressVisible = value ? "Visible" : "Hidden";
                OnPropertyChanged("IsInProgress");
            }
        }

        public string ProgressVisible
        {
            get{ return _progressVisible; }
            set
            {
                _progressVisible = value;
                OnPropertyChanged("ProgressVisible");
            }
        }

        public string Log
        {
            get { return _log; }
            set
            {
                _log = value;
                OnPropertyChanged("Log");
            }
        }

        public int? Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
