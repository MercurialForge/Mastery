using Mastery.Utilities;
using Mastery.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Mastery.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties
        public ProjectModel CurrentProject
        {
            get { return m_projectModel; }
            set
            {
                m_projectModel = value;
                m_previousHour = m_projectModel.CurrentHour;
                IsMonitoringActive = m_projectModel.IsMonitoring;
                if (IsMonitoringActive) { DoProcessButton(); }
                OnPropertyChanged("TargetHours");
                OnPropertyChanged("TaskTitle");
                OnPropertyChanged("CurrentHour");
                UpdateView();
            }
        }

        public int CurrentHour
        {
            get { return CurrentProject.CurrentHour; }
        }
        public bool IsMonitoringActive
        {
            get { return m_isMonitoringActive; }
            set
            {
                m_isMonitoringActive = value;
                CurrentProject.IsMonitoring = m_isMonitoringActive;
                OnPropertyChanged("IsMonitoringActive");
            }
        }
        public string TaskTitle
        {
            get { return "Task: " + CurrentProject.Task; }
        }
        public string ButtonText
        {
            get
            {
                return m_buttonText;
            }
            set
            {
                m_buttonText = value;
                OnPropertyChanged("ButtonText");
            }
        }
        public string DisplayedPercentage
        {
            get { return m_displayedPercentage; }
            set
            {
                m_displayedPercentage = value;
                OnPropertyChanged("DisplayedPercentage");
            }
        }
        public double TargetHours
        {
            get { return CurrentProject.TargetHours; }
        }
        public double ProgressBarCurrentValue
        {
            get { return m_progressBarCurrentValue; }
            set
            {
                m_progressBarCurrentValue = value;
                OnPropertyChanged("ProgressBarCurrentValue");
            }
        }
        #endregion

        #region Fields
        private string m_buttonText = "Start";
        private string m_displayedPercentage = "0.000%";
        private double ElapsedTime
        {
            get { return CurrentProject.ElapsedTime; }
            set
            {
                CurrentProject.ElapsedTime = value;
            }
        }
        private double m_progressBarCurrentValue = 0;
        private bool m_isMasteryActive;
        private bool m_isMonitoringActive;
        private Timer m_tickTimer;
        private Timer m_backupTickTimer;
        private Stopwatch m_dtStopwatch = new Stopwatch();
        private ProjectModel m_projectModel = new ProjectModel();
        private MainWindow m_mainWindow;
        private DateTime m_previousDeltaQuery;
        private int m_previousHour;
        private UserActivityTimer m_activityTimer;
        #endregion

        public MainWindowViewModel(Window mainWindow)
        {
            Initialize();
            m_mainWindow = (MainWindow)mainWindow;
        }

        #region Commands
        public ICommand Save
        {
            get { return new RelayCommand(x => DoSave()); }
        }
        private void DoSave()
        {
            if (Properties.Settings.Default.HasLoadPath)
            {
                if (File.Exists(Properties.Settings.Default.LastLoadPath))
                {
                    SaveSystem.SaveNoPrompt(CurrentProject);
                    return;
                }
                else
                {
                    SaveSystem.Save(CurrentProject);
                    return;
                }
            }
            SaveSystem.Save(CurrentProject);
        }

        public ICommand SaveAs
        {
            get { return new RelayCommand(x => DoSaveAs()); }
        }
        private void DoSaveAs()
        {
            SaveSystem.Save(CurrentProject);
        }

        public ICommand Load
        {
            get { return new RelayCommand(x => DoLoad()); }
        }
        private void DoLoad()
        {
            ProjectModel project = SaveSystem.Load();
            if (project != null)
            {
                CurrentProject = project;
                if (m_isMasteryActive)
                {
                    DoProcessButton();
                }
            }
        }

        public ICommand Minimize
        {
            get { return new RelayCommand(x => DoMinimize()); }
        }
        private void DoMinimize()
        {
            m_mainWindow.WindowState = WindowState.Minimized;
            m_mainWindow.Hide();
        }

        public ICommand RestoreWindow
        {
            get { return new RelayCommand(x => DoRestoreWindow()); }
        }
        private void DoRestoreWindow()
        {
            m_mainWindow.Show();
            m_mainWindow.WindowState = WindowState.Normal;
        }

        public ICommand Details
        {
            get { return new RelayCommand(x => ShowDetails()); }
        }
        private void ShowDetails()
        {
            DetailsPanel details = new DetailsPanel(CurrentProject);
            details.Show();
        }

        public ICommand About
        {
            get { return new RelayCommand(x => ShowAbout()); }
        }
        public void ShowAbout()
        {
            AboutWindow about = new AboutWindow();
            about.Show();
        }

        public ICommand Shutdown
        {
            get { return new RelayCommand(x => DoShutdown()); }
        }
        private void DoShutdown()
        {
            // Auto save if file location is still valid
            if (Properties.Settings.Default.HasLoadPath)
            {
                if (File.Exists(Properties.Settings.Default.LastLoadPath))
                {
                    SaveSystem.SaveNoPrompt(CurrentProject);
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show(
                        "The file Mastery was writing to has been Moved or Deleted. Please Save!"
                        + Environment.NewLine
                        + Environment.NewLine
                        + "OK : to Save" + Environment.NewLine
                        + "Cancel : to Ignore", "FILE NOT FOUND", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK) { SaveSystem.Save(CurrentProject); }
                    else { Properties.Settings.Default.HasLoadPath = false; }
                }
            }
            // Request to save unsaved default if time has elapsed on it.
            else
            {
                if (ElapsedTime > 100)
                {
                    SaveSystem.Save(CurrentProject);
                }
            }
            // Shutdown
            Application.Current.Shutdown();
        }

        public ICommand NewProject
        {
            get { return new RelayCommand(x => ShowNewProject()); }
        }
        private void ShowNewProject()
        {
            NewProject newProjectWindow = new NewProject(this);
            newProjectWindow.Show();
        }

        public ICommand ShowMonitorControl
        {
            get { return new RelayCommand(x => DoShowmMonitorControl()); }
        }
        private void DoShowmMonitorControl()
        {
            MonitorWindow monitor = new MonitorWindow(this);
            monitor.Show();
        }

        public ICommand ToggleMonitoring
        {
            get { return new RelayCommand(c => DoToggleMonitoring()); }
        }
        private void DoToggleMonitoring()
        {
            if (IsMonitoringActive)
            {
                if (m_isMasteryActive)
                {
                    return;
                }
                else
                {
                    DoProcessButton();
                    return;
                }
            }
            else
            {
                if (m_isMasteryActive)
                {
                    DoProcessButton();
                    return;
                }
                else
                {
                    return;
                }
            }
        }

        public ICommand ProcessButton
        {
            get { return new RelayCommand(x => DoProcessButton()); }
        }
        private void DoProcessButton()
        {
            if (m_isMasteryActive)
            {
                ButtonText = "Resume";
                m_isMasteryActive = false;
            }
            else
            {
                ButtonText = "Pause";
                m_isMasteryActive = true;
            }
        }
        #endregion

        #region Private Methods
        private void Tick(object userState)
        {
            if (!m_isMasteryActive) { return; }

            if (IsMonitoringActive)
            {
                if (UserIsActive())
                {
                    ProcessTick();
                }
                m_previousDeltaQuery = DateTime.Now;
            }
            else
            {
                ProcessTick();
                m_previousDeltaQuery = DateTime.Now;
            }
        }

        private bool UserIsActive()
        {
            if (m_activityTimer.UserActiveState == UserActivityState.Inactive || m_activityTimer.UserActiveState == UserActivityState.Unknown)
            {
                return false;
            }

            string activeProcess = Win32Helpers.GetForegroundProcessName();
            foreach (string application in CurrentProject.Applications)
            {
                if (application == activeProcess)
                {
                    return true;
                }
            }
            return false;
        }

        private void ProcessTick()
        {
            double deltaTime = (DateTime.Now - m_previousDeltaQuery).TotalMilliseconds;
            CurrentProject.ElapsedTime += deltaTime;
            m_previousDeltaQuery = DateTime.Now;

            // Set progress bar value
            double target = CurrentProject.TargetHours * 3600000.0;
            ProgressBarCurrentValue = (CurrentProject.ElapsedTime / target) * 100;

            UpdateView();
            ProcessHourChangePopup();
        }

        private void UpdateView()
        {
            double target = CurrentProject.TargetHours * 3600000.0;
            ProgressBarCurrentValue = (CurrentProject.ElapsedTime / target) * 100;
            DisplayedPercentage = ((ProgressBarCurrentValue >= 100) ? 100 : ProgressBarCurrentValue).ToString("F4") + "%";
            OnPropertyChanged("CurrentHour");
            OnPropertyChanged("TargetHours");
        }

        private void ProcessHourChangePopup()
        {
            if (m_previousHour != CurrentProject.CurrentHour)
            {
                m_previousHour = CurrentProject.CurrentHour;

                // Invoked delegate on main thread for UI calls
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ShowHourPlusPopUp("Replace me with funny dialog!");
                });
            }
        }

        private void ShowHourPlusPopUp(string message)
        {
            if (Properties.Settings.Default.ShowPopups)
            {
                SystemTrayPopup balloon = new SystemTrayPopup();
                balloon.PopupText = "+1 Hour! " + CleverDialog.Next();
                m_mainWindow.ShowTaskbarPopup(balloon);
            }
        }

        private void TickAutoSave(object userState)
        {
            Properties.Settings.Default.Save();
        }

        private void Initialize()
        {
            LoadLastActiveMPF();

            m_activityTimer = new UserActivityTimer(10000);
            m_activityTimer.Enable();

            // Set default tick timer
            m_tickTimer = new Timer(new TimerCallback(this.Tick), null, 0, 16);
            m_previousDeltaQuery = DateTime.Now;

            // Set auto save timer
            m_backupTickTimer = new Timer(new TimerCallback(this.TickAutoSave), null, 0, 5000);
        }

        private void LoadLastActiveMPF()
        {
            // Load previous project if expected; warn if missing.
            if (Properties.Settings.Default.HasLoadPath)
            {
                if (File.Exists(Properties.Settings.Default.LastLoadPath))
                {
                    CurrentProject = SaveSystem.Load(Properties.Settings.Default.LastLoadPath);
                    m_previousHour = CurrentProject.CurrentHour;
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show(
                        "The previously loaded .MPF (project file) could not be found. If you moved it please click \"OK\" to relocate it. \"Cancel\" to ignore.", "FILE NOT FOUND", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK) { DoLoad(); }
                    else { Properties.Settings.Default.HasLoadPath = false; }
                }
            }
            UpdateView();
        }
        #endregion
    }
}
