using Mastery.Utilities;
using Mastery.ViewModels;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Mastery.Views
{
    /// <summary>
    /// Interaction logic for NewProject.xaml
    /// </summary>
    public partial class NewProject : Window, INotifyPropertyChanged
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

        public bool CanCreate 
        {
            get { return m_canCreate; } 
            set
            {
                m_canCreate = value;
                OnPropertyChanged("CanCreate");
            }
        }
        public string TaskText 
        {
            get { return m_taskText; }
            set
            {
                m_taskText = value;
                OnPropertyChanged("TaskText");
            }
        }
        public string HoursValue 
        {
            get { return m_hoursValue; }
            set
            {
                m_hoursValue = value;
                OnPropertyChanged("HoursValue");
            }
        }
        public string InitialHoursValue 
        {
            get { return m_intialHoursValue; }
            set
            {
                m_intialHoursValue = value;
                OnPropertyChanged("InitialHoursValue");
            }
        }

        private MainWindowViewModel mainVM;
        private string m_taskText = "";
        private string m_hoursValue = "";
        private string m_intialHoursValue = "";
        private bool m_canCreate;
        
        public NewProject(MainWindowViewModel mainWindow)
        {
            InitializeComponent();
            DataContext = this;
            mainVM = mainWindow;
            StartCreateButtonMonitor();
        }

        private async void StartCreateButtonMonitor ()
        {
            while(true)
            {
                if (string.IsNullOrEmpty(TaskText) || string.IsNullOrWhiteSpace(HoursValue)) { CanCreate = false; }
                else { CanCreate = true; }
                await Task.Delay(100);
            }
        }

        private void MainMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PreviewHoursInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TestForNumeric(e.Text);
        }

        private void PreviewInitialHoursInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TestForNumeric(e.Text);
        }

        private static bool TestForNumeric(string text)
        {
            Regex regex = new Regex(@"[^\d]"); //regex that matches allowed text
            return regex.IsMatch(text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProjectModel project = new ProjectModel();
            project.Task = TaskText;
            HoursValue = Regex.Replace(HoursValue, @"\s+", "");

            int hoursValue = 0;
            if (int.TryParse(HoursValue, out hoursValue))
            {
                project.TargetHours = hoursValue;
            }

            int initialHoursValue = 0;
            if(int.TryParse(InitialHoursValue, out initialHoursValue))
            {
                project.ElapsedTime = initialHoursValue * 3600000;
            }

            if (SaveSystem.Save(project, true))
            {
                mainVM.CurrentProject = project;
            }
            Close();
        }

    }
}
