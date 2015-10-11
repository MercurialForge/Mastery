using Mastery.Utilities;
using Mastery.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Mastery.Views
{
    /// <summary>
    /// Interaction logic for MonitorWindow.xaml
    /// </summary>
    public partial class MonitorWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        public MainWindowViewModel MainWindow { get; set; }
        public ObservableCollection<string> MonitoredApps { get; set; }
        public ObservableCollection<string> ActiveApps { get; set; }
        public string CurrentlySelected { get; set; }
        public string CurrentlySelectedApp { get; set; }

        private Timer m_pullAppsTimer;

        #region Commands
        public ICommand AddSelected
        {
            get { return new RelayCommand(x => DoAddSelected(), x => CheckIfCanAdd()); }
        }
        private bool CheckIfCanAdd()
        {
            return (!string.IsNullOrEmpty(CurrentlySelectedApp) && !MonitoredApps.Contains(CurrentlySelectedApp));
        }
        private void DoAddSelected()
        {
            if (!MonitoredApps.Contains(CurrentlySelectedApp))
            {
                MonitoredApps.Add(CurrentlySelectedApp);
                MainWindow.CurrentProject.Applications = MonitoredApps.ToList();
            }
        }

        public ICommand RemoveSelected
        {
            get { return new RelayCommand(x => DoRemoveSelected(), x => (!string.IsNullOrEmpty(CurrentlySelected))); }
        }
        private void DoRemoveSelected()
        {
            MonitoredApps.Remove(CurrentlySelected);
            MainWindow.CurrentProject.Applications = MonitoredApps.ToList();
        } 
        #endregion

        public MonitorWindow(MainWindowViewModel parentViewModel)
        {
            InitializeComponent();
            MainWindow = parentViewModel;
            ActiveApps = new ObservableCollection<string>();
            MonitoredApps = new ObservableCollection<string>(MainWindow.CurrentProject.Applications);
            m_pullAppsTimer = new Timer(new TimerCallback(this.UpdateAppsList), null, 0, 500);
            comboBox.SelectedIndex = 0;
            DataContext = this;
        }

        private void UpdateAppsList(object unused)
        {
            List<string> applications = new List<string>();
            applications.Clear();

            Process[] AllProcesses = Process.GetProcesses();
            Process self = Process.GetCurrentProcess();
            for (int i = 0; i < AllProcesses.Length; i++)
            {
                string processName = AllProcesses[i].ProcessName;
                if (!string.IsNullOrEmpty(AllProcesses[i].MainWindowTitle))
                {
                    if (!applications.Contains(processName))
                    {
                        applications.Add(processName);
                    }
                }
            }

            foreach (string app in applications)
            {
                if (ActiveApps.Contains(app)) { continue; }
                else
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate { ActiveApps.Add(app); });
                }
            }

            List<string> ClosedApps = new List<string>();
            foreach (string s in ActiveApps)
            {
                if (applications.Contains(s)) { continue; }
                else
                {
                    ClosedApps.Add(s);
                }
            }

            foreach (string s in ClosedApps)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate { ActiveApps.Remove(s); });
            }
        }
    }
}
