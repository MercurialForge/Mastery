using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Mastery.Views
{
    /// <summary>
    /// Interaction logic for MonitorWindow.xaml
    /// </summary>
    public partial class MonitorWindow : Window
    {
        public BindingList<string> OutputLog { get; private set; }
        public Dictionary<string, int> applications = new Dictionary<string, int>();

        public MonitorWindow()
        {
            InitializeComponent();
            OutputLog = new BindingList<string>();

            // move to async
            System.Diagnostics.Process[] procArray;
            procArray = System.Diagnostics.Process.GetProcesses();
            for (int i = 0; i < procArray.Length; i++)
            {
                if (procArray[i].MainWindowTitle.Length > 0)
                {
                    if (!applications.ContainsKey(procArray[i].MainWindowTitle))
                    {
                        applications.Add(procArray[i].MainWindowTitle, procArray[i].Id);
                    }
                }
            }
            foreach (KeyValuePair<string, int> app in applications)
            {
                comboBox.Items.Add(app.Key);
            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string name = "";
            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
            {
                if (p.Id == applications[(string)comboBox.SelectedItem])
                    name = p.ProcessName;
            }
            Console.WriteLine(name);
        }

        private void OutputText_Updated(object sender, SizeChangedEventArgs e)
        {
            this.m_scrollViewer.UpdateLayout();
            this.m_scrollViewer.ScrollToVerticalOffset(this.m_outputLog.ActualHeight);
        }
    }
}
