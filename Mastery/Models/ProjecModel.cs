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
        public string Task { get; set; }
        public DateTime StartDate {get; set;}
        public double TargetHours { get; set; }
        public double ElapsedTime { get; set; }
        public TimeSpan RunTime
        {
            get
            {
                TimeSpan timespan = TimeSpan.FromMilliseconds(ElapsedTime);
                return timespan;
            }
        }
        public int CurrentHour 
        { 
            get 
            {
                TimeSpan timespan = TimeSpan.FromMilliseconds(ElapsedTime);
                return (int)timespan.TotalHours;
            } 
        }

        public ProjectModel ()
        {
            Task = "Drawing";
            StartDate = DateTime.Now;
            TargetHours = 5000.0;
            ElapsedTime = 0.0;
        }
    }
}
