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

namespace Mastery.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public double ProgressBarCurrentValue
        {
            get
            {
                return _progressValue;
            }
            set
            {
                _progressValue = value;
                OnPropertyChanged("ProgressBarCurrentValue");
            }
        }
        public double TargetHours
        {
            get { return _targetHours; }
            set
            {
                _targetHours = value;
                OnPropertyChanged("TargetHours");
            }
        }
        private double _progressValue = 0;
        private double _targetHours = 0.01f;

        public String ButtonText 
        { 
            get 
            { 
                return _buttonText; 
            } 
            set 
            { 
                _buttonText = value;
                OnPropertyChanged("ButtonText");
            } 
        }
        public String DisplayedPercentage
        {
            get { return m_percentage; }
            set
            {
                m_percentage = value;
                OnPropertyChanged("DisplayedPercentage");
            }
        }
        private string _buttonText = "Start";
        private string m_percentage = "0.000%";

        private double ElapsedTime
        {
            get { return m_elapsedTime; }
            set
            {
                m_elapsedTime = value;
                Properties.Settings.Default.ElapsedTime = m_elapsedTime;
            }
        }
        private double m_elapsedTime;

        private bool m_isTimerRunning = false;
        private Stopwatch m_dtStopwatch = new Stopwatch();
        private Timer m_intervalTimer = new Timer();
        private Timer m_backupTimer = new Timer();

        public MainWindowViewModel ()
        {
            Initialize();
        }

        public ICommand Shutdown
        {
            get { return new RelayCommand(x => DoShutdown()); }
        }
        private void DoShutdown()
        {
            Application.Current.Shutdown();
        }

        public ICommand Clear
        {
            get { return new RelayCommand(x => DoClear()); }
        }
        private void DoClear()
        {
            if (m_isTimerRunning) { ToggleButton(); }
            ButtonText = "Start";
            m_elapsedTime = 0;
            ProgressBarCurrentValue = 0;
            DisplayedPercentage = "0.000%";
        }

        public ICommand ProcessButton
        {
            get { return new RelayCommand(x => ToggleButton()); }
        }
        private void ToggleButton ()
        {
            if(m_isTimerRunning)
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
            ElapsedTime += m_dtStopwatch.ElapsedMilliseconds;
            m_dtStopwatch.Restart();
            double target = Properties.Settings.Default.TargetHours * 3600000.0;
            ProgressBarCurrentValue = (ElapsedTime / target) * 100;
            DisplayedPercentage = ((ProgressBarCurrentValue >= 100) ? 100 : ProgressBarCurrentValue).ToString("F4") + "%";
        }

        private void TickBackUp(Object source, System.Timers.ElapsedEventArgs e)
        {
            Properties.Settings.Default.Save();
            Console.WriteLine("Back Up Tick");
        }

        private void Initialize()
        {
            m_elapsedTime = Properties.Settings.Default.ElapsedTime;
            double target = Properties.Settings.Default.TargetHours * 3600000.0;
            ProgressBarCurrentValue = (ElapsedTime / target) * 100;
            DisplayedPercentage = ((ProgressBarCurrentValue >= 100) ? 100 : ProgressBarCurrentValue).ToString("F4") + "%";

            m_intervalTimer.Interval = 16; // 60 FPS roughly
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
