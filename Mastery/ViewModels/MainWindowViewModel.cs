using Mastery.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Mastery.Views;
using System.IO;

namespace Mastery.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ProjectModel CurrentProject
        {
            get { return _projectModel; }
            set
            {
                _projectModel = value;
                OnPropertyChanged("TargetHours");
                OnPropertyChanged("TaskTitle");
                OnPropertyChanged("CurrentHour");
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
        public double ProgressBarCurrentValue 
        {
            get { return m_progressBarCurrentValue; }
            set
            {
                m_progressBarCurrentValue = value;
                OnPropertyChanged("ProgressBarCurrentValue");
            }
        }
        public String ButtonText
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
        public String DisplayedPercentage
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
        private double ElapsedTime
        {
            get { return CurrentProject.ElapsedTime; }
            set
            {
                CurrentProject.ElapsedTime = value;
            }
        }

        #region Backing Fields
        private ProjectModel _projectModel = new ProjectModel();
        private string m_buttonText = "Start";
        private string m_displayedPercentage = "0.000%"; 
        private double m_progressBarCurrentValue = 0;
        #endregion

        private bool m_isTimerRunning = false;
        private Stopwatch m_dtStopwatch = new Stopwatch();
        private Timer m_intervalTimer = new Timer();
        private Timer m_backupTimer = new Timer();

        #region Methods
        public MainWindowViewModel()
        {
            Initialize();
        }

        public ICommand Save
        {
            get { return new RelayCommand(x => DoSave()); }
        }
        private void DoSave()
        {
            SaveSystem.Save(CurrentProject);
        }

        public ICommand Load
        {
            get { return new RelayCommand(x => DoLoad()); }
        }
        private void DoLoad()
        {
            CurrentProject = SaveSystem.Load();
            UpdateView();
        }

        public ICommand Shutdown
        {
            get { return new RelayCommand(x => DoShutdown()); }
        }
        private void DoShutdown()
        {
            SaveSystem.Save(CurrentProject);
            Application.Current.Shutdown();
        }

        public ICommand NewProject
        {
            get { return new RelayCommand(x => CreateNewProject()); }
        }
        private void CreateNewProject()
        {
            NewProject newProjectWindow = new NewProject(this);
            newProjectWindow.Show();
        }

        public ICommand Clear
        {
            get { return new RelayCommand(x => DoClear()); }
        }
        private void DoClear()
        {
            if (m_isTimerRunning) { ToggleButton(); }
            ButtonText = "Start";
            DisplayedPercentage = "0.000%";
            CurrentProject = new ProjectModel();
            Properties.Settings.Default.Reset();
            OnPropertyChanged("TargetHours");
        }

        public ICommand ProcessButton
        {
            get { return new RelayCommand(x => ToggleButton()); }
        }
        private void ToggleButton()
        {
            if (m_isTimerRunning)
            {
                ButtonText = "Continue";
                m_intervalTimer.Enabled = false;
                m_dtStopwatch.Stop();
                m_isTimerRunning = false;
            }
            else
            {
                ButtonText = "Pause";
                m_intervalTimer.Enabled = true;
                m_dtStopwatch.Start();
                m_isTimerRunning = true;
            }
        }

        private void Tick(Object source, System.Timers.ElapsedEventArgs e)
        {
            CurrentProject.ElapsedTime += m_dtStopwatch.ElapsedMilliseconds;
            m_dtStopwatch.Restart();
            double target = CurrentProject.TargetHours * 3600000.0;
            ProgressBarCurrentValue = (CurrentProject.ElapsedTime / target) * 100;
            DisplayedPercentage = ((ProgressBarCurrentValue >= 100) ? 100 : ProgressBarCurrentValue).ToString("F4") + "%";
            OnPropertyChanged("CurrentHour");
        }

        private void TickBackUp(Object source, System.Timers.ElapsedEventArgs e)
        {
            Properties.Settings.Default.Save();
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
                    MessageBox.Show("Previously save .MPF file could not be found please load it again!");
                }
            }
            UpdateView();
            OnPropertyChanged("TargetHours");

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

        private void UpdateView()
        {
            double target = CurrentProject.TargetHours * 3600000.0;
            ProgressBarCurrentValue = (CurrentProject.ElapsedTime / target) * 100;
            DisplayedPercentage = ((ProgressBarCurrentValue >= 100) ? 100 : ProgressBarCurrentValue).ToString("F4") + "%";
        }
    } 
        #endregion
}
