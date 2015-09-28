using Mastery.Utilities;
using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Mastery.Views;
using System.IO;

namespace Mastery.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields & Properties
        public ProjectModel CurrentProject
        {
            get { return _projectModel; }
            set
            {
                _projectModel = value;
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
        public double TargetHours
        {
            get { return CurrentProject.TargetHours; }
        }
        private double ElapsedTime
        {
            get { return CurrentProject.ElapsedTime; }
            set
            {
                CurrentProject.ElapsedTime = value;
            }
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
        public string TaskTitle
        {
            get { return "Task: " + CurrentProject.Task; }
        }

        private ProjectModel _projectModel = new ProjectModel();
        private string m_buttonText = "Start";
        private string m_displayedPercentage = "0.000%";
        private double m_progressBarCurrentValue = 0;

        private bool m_isTimerRunning = false;
        private Stopwatch m_dtStopwatch = new Stopwatch();
        private Timer m_intervalTimer = new Timer();
        private Timer m_backupTimer = new Timer();
        private DateTime m_beginning;
        private Window m_mainWindow;
        #endregion

        public MainWindowViewModel(Window mainWindow)
        {
            Initialize();
            m_mainWindow = mainWindow;
        }

        #region Commands
        // Save the project to a file
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
                }
                return;
            }
            SaveSystem.Save(CurrentProject);
        }

        // Save the project to a new file
        public ICommand SaveAs
        {
            get { return new RelayCommand(x => DoSaveAs()); }
        }
        private void DoSaveAs()
        {
            SaveSystem.Save(CurrentProject);
        }

        // Load project from a file
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
                UpdateView();
                if (m_isTimerRunning)
                {
                    ToggleButton();
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
        public void ShowAbout ()
        {
            AboutWindow about = new AboutWindow();
            about.Show();
        }

        // Shutdown the app and do so safely with saving
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

        // Create a new project
        public ICommand NewProject
        {
            get { return new RelayCommand(x => CreateNewProject()); }
        }
        private void CreateNewProject()
        {
            NewProject newProjectWindow = new NewProject(this);
            newProjectWindow.Show();
        }

        // Reset everything
        public ICommand Clear
        {
            get { return new RelayCommand(x => DoClear()); }
        }
        private void DoClear()
        {
            // Stop timer
            if (m_isTimerRunning) { ToggleButton(); }

            // Reset visuals to default
            ButtonText = "Start";
            DisplayedPercentage = "0.0000%";
            ProgressBarCurrentValue = 0;

            // Load default project
            CurrentProject = new ProjectModel();
            Properties.Settings.Default.Reset();

            // Clear dirty values
            UpdateView();
        }

        // Process the button input
        public ICommand ProcessButton
        {
            get { return new RelayCommand(x => ToggleButton()); }
        }
        private void ToggleButton()
        {
            if (m_isTimerRunning)
            {
                ButtonText = "Continue";
                m_dtStopwatch.Stop();
                m_intervalTimer.Enabled = false;
                m_isTimerRunning = false;
            }
            else
            {
                ButtonText = "Pause";
                m_dtStopwatch.Start();
                m_intervalTimer.Enabled = true;
                m_isTimerRunning = true;
                m_beginning = DateTime.Now;
            }
        }
        #endregion

        private void Tick(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Delta Time
            CurrentProject.ElapsedTime += (DateTime.Now - m_beginning).TotalMilliseconds;
            m_beginning = DateTime.Now;

            // Get Elapsed Time 
            double target = CurrentProject.TargetHours * 3600000.0;
            ProgressBarCurrentValue = (CurrentProject.ElapsedTime / target) * 100;

            // Update
            UpdateView();
        }

        private void TickBackUp(Object source, System.Timers.ElapsedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void UpdateView()
        {
            double target = CurrentProject.TargetHours * 3600000.0;
            ProgressBarCurrentValue = (CurrentProject.ElapsedTime / target) * 100;
            DisplayedPercentage = ((ProgressBarCurrentValue >= 100) ? 100 : ProgressBarCurrentValue).ToString("F4") + "%";
            OnPropertyChanged("CurrentHour");
            OnPropertyChanged("TargetHours");
        }

        private void Initialize()
        {
            if (Properties.Settings.Default.HasLoadPath)
            {
                if (File.Exists(Properties.Settings.Default.LastLoadPath))
                {
                    CurrentProject = SaveSystem.Load(Properties.Settings.Default.LastLoadPath);
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("The previously loaded .MPF (project file) could not be found. If you moved it please click \"OK\" to relocate it. \"Cancel\" to ignore.", "FILE NOT FOUND", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK) { SaveSystem.Load(); }
                    else { Properties.Settings.Default.HasLoadPath = false; }
                }
            }
            UpdateView();

            m_intervalTimer.Interval = 1;
            m_intervalTimer.AutoReset = true;
            m_intervalTimer.Elapsed += Tick;

            m_backupTimer.Interval = 5000; // 5 seconds
            m_backupTimer.AutoReset = true;
            m_backupTimer.Elapsed += TickBackUp;
            m_backupTimer.Start();

            GC.KeepAlive(m_intervalTimer);
            GC.KeepAlive(m_backupTimer);
            GC.KeepAlive(m_dtStopwatch);
        }
    }
}
