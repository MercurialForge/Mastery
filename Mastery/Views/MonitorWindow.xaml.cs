using Mastery.Utilities;
using Mastery.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

        public MonitorWindow(MainWindowViewModel parentViewModel)
        {
            DataContext = this;
            MainWindow = parentViewModel;
            MonitoredApps = new ObservableCollection<string>(MainWindow.CurrentProject.Applications);
            ActiveApps = new ObservableCollection<string>();
            InitializeComponent();
            UpdateAppsList();
            comboBox.SelectedIndex = 0;
        }

        private async void UpdateAppsList()
        {
            List<string> applications = new List<string>();
            while (true)
            {
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
                        ActiveApps.Add(app);
                    }
                }

                List<string> ClosedApps = new List<string>();
                foreach(string s in ActiveApps)
                {
                    if (applications.Contains(s)) { continue; }
                    else
                    {
                        ClosedApps.Add(s);
                    }
                }

                foreach(string s in ClosedApps)
                {
                    ActiveApps.Remove(s);
                }

                await Task.Delay(500);
            }
        }
    }
}
