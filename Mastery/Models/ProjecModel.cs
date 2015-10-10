using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mastery.Utilities
{
    public class ProjectModel
    {
        // Saved and Loaded Properties
        public string Task { get; set; }
        public DateTime StartDate { get; set; }
        public double TargetHours { get; set; }
        public double ElapsedTime { get; set; }
        public bool IsMonitoring { get; set; }
        public List<string> Applications { get; set; }

        // Query Properties
        public int CurrentHour
        {
            get
            {
                TimeSpan timespan = TimeSpan.FromMilliseconds(ElapsedTime);
                return (int)timespan.TotalHours;
            }
        }

        // Constructor
        public ProjectModel()
        {
            Applications = new List<string>();
            Task = "Drawing";
            StartDate = DateTime.Now;
            TargetHours = 5000;
            ElapsedTime = 0.0;
            Applications.Add("CLIPStudioPaint");
            Applications.Add("Photoshop");
        }
    }
}
