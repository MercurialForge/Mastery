using Mastery.Utilities;
using Mastery.ViewModels;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Mastery.Views
{
    /// <summary>
    /// Interaction logic for NewProject.xaml
    /// </summary>
    public partial class NewProject : Window
    {
        MainWindowViewModel mainVM;

        public NewProject(MainWindowViewModel mainWindow)
        {
            InitializeComponent();
            mainVM = mainWindow;
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
            e.Handled = !TestForNumeric(e.Text);
        }

        private static bool TestForNumeric(string text)
        {
            Regex regex = new Regex(@"[^\d]"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TaskText.Text == "" || HoursValue.Text == "") { return; }
            ProjectModel project = new ProjectModel();
            project.Task = TaskText.Text;
            project.TargetHours = int.Parse(HoursValue.Text);
            if (SaveSystem.Save(project))
            {
                mainVM.CurrentProject = project;
            }
            Close();
        }

    }
}
