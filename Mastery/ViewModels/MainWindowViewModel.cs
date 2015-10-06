using Mastery.Utilities;
using Mastery;
using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Mastery.Views;
using System.IO;
using System.Runtime.InteropServices;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls.Primitives;

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
        public bool IsMonitoringActive
        {
            get { return m_isMonitoringActive; }
            set { m_isMonitoringActive = value; }
        }

        private ProjectModel _projectModel = new ProjectModel();
        private string m_buttonText = "Start";
        private string m_displayedPercentage = "0.000%";
        private double m_progressBarCurrentValue = 0;

        private bool m_isMasteryActive = false;
        private Stopwatch m_dtStopwatch = new Stopwatch();
        private Timer m_intervalTimer = new Timer();
        private Timer m_backupTimer = new Timer();
        private Timer m_mouseQueryTimer = new Timer();
        private DateTime m_beginning;
        private MainWindow m_mainWindow;
        private Point previousPoint;
        private WeightedBool m_isUserInactive = new WeightedBool();
        private bool m_isMonitoringActive;
        private int m_previousHour;
        #endregion

        public MainWindowViewModel(Window mainWindow)
        {
            Initialize();
            m_mainWindow = (MainWindow)mainWindow;
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
                m_previousHour = CurrentProject.CurrentHour;
                UpdateView();
                if (m_isMasteryActive)
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
        public void ShowAbout()
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

        //// Reset everything
        //public ICommand Clear
        //{
        //    get { return new RelayCommand(x => DoClear()); }
        //}
        //private void DoClear()
        //{
        //    // Stop timer
        //    if (m_isTimerRunning) { ToggleButton(); }

        //    // Reset visuals to default
        //    ButtonText = "Start";
        //    DisplayedPercentage = "0.0000%";
        //    ProgressBarCurrentValue = 0;

        //    // Load default project
        //    CurrentProject = new ProjectModel();
        //    Properties.Settings.Default.Reset();

        //    // Clear dirty values
        //    UpdateView();
        //}

        // Process the button input
        public ICommand ProcessButton
        {
            get { return new RelayCommand(x => ToggleButton()); }
        }
        private void ToggleButton()
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

        public ICommand ToggleMonitoring
        {
            get { return new RelayCommand(c => DoToggleMonitoring()); }
        }
        private void DoToggleMonitoring()
        {
            if(IsMonitoringActive)
            {
                if(m_isMasteryActive)
                {
                    return;
                }
                else
                {
                    ToggleButton();
                    return;
                }
            }
            else
            {
                if (m_isMasteryActive)
                {
                    ToggleButton();
                    return;
                }
                else
                {
                    return;
                }
            }
        }
        #endregion

        #region P/Invoke
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        // The GetForegroundWindow function returns a handle to the foreground window
        // (the window  with which the user is currently working).
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // The GetWindowThreadProcessId function retrieves the identifier of the thread
        // that created the specified window and, optionally, the identifier of the
        // process that created the window.
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // Returns the name of the process owning the foreground window.
        private string GetForegroundProcessName()
        {
            IntPtr hwnd = GetForegroundWindow();

            // The foreground window can be NULL in certain circumstances, 
            // such as when a window is losing activation.
            if (hwnd == null)
                return "Unknown";

            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);

            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
            {
                if (p.Id == pid)
                    return p.ProcessName;
            }

            return "Unknown";
        }
        #endregion

        private void TickMouseQuery(Object source, System.Timers.ElapsedEventArgs e)
        {
            string activeProcess = GetForegroundProcessName();
            Console.WriteLine(activeProcess);

            if (activeProcess == "Photoshop" || activeProcess == "CLIPStudioPaint")
            {
                Point p = GetMousePosition();
                if (p == previousPoint)
                {
                    m_isUserInactive.AddWeight(true);
                    return;
                }
                m_isUserInactive.SetFalse();
                previousPoint = p;
                return;
            }
            else
            {
                m_isUserInactive.SetTrue();
                return;
            }
        }

        private void Tick(Object source, System.Timers.ElapsedEventArgs e)
        {
            if(!IsMonitoringActive)
            {
                if (m_isMasteryActive)
                {
                    ProcessTick();
                }
                m_beginning = DateTime.Now;
                return;
            }

            if (m_isMasteryActive)
            {
                if (!m_isUserInactive.State)
                {
                    ProcessTick();
                    return;
                }
                m_beginning = DateTime.Now;
            }
            m_beginning = DateTime.Now;
        }

        private void ProcessTick()
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
            if (m_previousHour != CurrentProject.CurrentHour)
            {
                m_previousHour = CurrentProject.CurrentHour;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {

                    ShowHourPlusPopUp();

                });
            }
        }

        private void UpdateView()
        {
            double target = CurrentProject.TargetHours * 3600000.0;
            ProgressBarCurrentValue = (CurrentProject.ElapsedTime / target) * 100;
            DisplayedPercentage = ((ProgressBarCurrentValue >= 100) ? 100 : ProgressBarCurrentValue).ToString("F4") + "%";
            OnPropertyChanged("CurrentHour");
            OnPropertyChanged("TargetHours");
        }

        private void ShowHourPlusPopUp()
        {
            SystemTrayPopup balloon = new SystemTrayPopup();
            balloon.PopupText = "+1 Hour! " + "You can do one more, right?";
            m_mainWindow.ShowTaskbarPopup(balloon);
        }

        private void Initialize()
        {
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

            m_intervalTimer.Interval = 1;
            m_intervalTimer.AutoReset = true;
            m_intervalTimer.Elapsed += Tick;
            m_beginning = DateTime.Now;
            m_intervalTimer.Start();

            m_backupTimer.Interval = 5000; // 5 seconds
            m_backupTimer.AutoReset = true;
            m_backupTimer.Elapsed += TickBackUp;
            m_backupTimer.Start();

            m_mouseQueryTimer.Interval = 1000; // 1 second
            m_mouseQueryTimer.AutoReset = true;
            m_mouseQueryTimer.Elapsed += TickMouseQuery;
            m_mouseQueryTimer.Start();
        }
    }
}
