using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;

namespace Mastery.Utilities
{
    public class SaveSystem
    {
        #region Saving
        public static bool Save(ProjectModel project, bool SetStartDate = false)
        {
            string fileName = "";
            if (SaveFile(out fileName))
            {
                SaveProjectModel(project, fileName, SetStartDate);
                Properties.Settings.Default.HasLoadPath = true;
                Properties.Settings.Default.LastLoadPath = fileName;
                return true;
            }
            return false;
        }

        public static void SaveNoPrompt(ProjectModel project)
        {
            SaveProjectModel(project, Properties.Settings.Default.LastLoadPath);
        }

        private static void SaveProjectModel(ProjectModel project, string fileName, bool SetStartDate = false)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writer.Write(Encoding.UTF8.GetBytes("MPF0")); // magic
                writer.Write(project.Task.Count());
                writer.Write(Encoding.UTF8.GetBytes(project.Task));
                if (SetStartDate)
                {
                    writer.Write(DateTime.Now.Month);
                    writer.Write(DateTime.Now.Day);
                    writer.Write(DateTime.Now.Year);
                    writer.Write(DateTime.Now.Hour);
                    writer.Write(DateTime.Now.Minute);
                }
                else
                {
                    writer.Write(project.StartDate.Month);
                    writer.Write(project.StartDate.Day);
                    writer.Write(project.StartDate.Year);
                    writer.Write(project.StartDate.Hour);
                    writer.Write(project.StartDate.Minute);
                }
                writer.Write(project.TargetHours);
                writer.Write(project.ElapsedTime);

                // added in V1.1 should automaticlly be added by the default project and updated next save. Typically 5 seconds after load.
                writer.Write(project.IsMonitoring);
                writer.Write(project.Applications.Count);
                for(int i = 0; i < project.Applications.Count; i++)
                {
                    writer.Write(project.Applications[i].Count());
                    writer.Write(Encoding.UTF8.GetBytes(project.Applications[i]));
                }
            }
        }
        #endregion

        #region Loading
        public static ProjectModel Load()
        {
            ProjectModel project = new ProjectModel();
            string fileName = "";

            if (OpenFile(out fileName))
            {
                project = LoadProjectModel(fileName);
                Properties.Settings.Default.HasLoadPath = true;
                Properties.Settings.Default.LastLoadPath = fileName;
                return project;
            }
            return null;

        }

        public static ProjectModel Load(string loadPath)
        {
            ProjectModel project = LoadProjectModel(loadPath);
            Properties.Settings.Default.HasLoadPath = true;
            Properties.Settings.Default.LastLoadPath = loadPath;
            return project;
        }

        private static ProjectModel LoadProjectModel(string loadPath)
        {
            ProjectModel project = new ProjectModel();

            using (BinaryReader reader = new BinaryReader(File.Open(loadPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                // Skip Magic
                reader.BaseStream.Seek(4, 0);

                // Read Task Name
                int taskTitleLength = reader.ReadInt32();
                project.Task = new string(reader.ReadChars(taskTitleLength));

                // Get DateTime of Project Beginning
                int month = reader.ReadInt32();
                int day = reader.ReadInt32();
                int year = reader.ReadInt32();
                int hour = reader.ReadInt32();
                int minute = reader.ReadInt32();
                DateTime dateTime = new DateTime(year, month, day, hour, minute, 0, 0);
                project.StartDate = dateTime;

                // Get Target Hours
                project.TargetHours = reader.ReadDouble();

                // Get Total Elapsed Time
                project.ElapsedTime = reader.ReadDouble();

                bool isEndOfStream = reader.BaseStream.Position == reader.BaseStream.Length;

                if (!isEndOfStream)
                {
                    // added in V1.1 should automaticlly be added by the default project and updated next save. Typically 5 seconds after load.
                    project.IsMonitoring = reader.ReadBoolean();

                    int applicationCount = reader.ReadInt32();
                    for (int i = 0; i < applicationCount; i++)
                    {
                        int charLength = reader.ReadInt32();
                        string application = new string(reader.ReadChars(charLength));
                        if(!project.Applications.Contains(application))
                        {
                            project.Applications.Add(application);
                        }
                    }
                }
            }
            return project;
        }
        #endregion

        #region Utilities
        private static bool OpenFile(out string outPath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Mastery Project File (*.MPF)|*.MPF";

            if (openFileDialog.ShowDialog() == true)
            {
                outPath = openFileDialog.FileName;
                return true;
            }

            outPath = "";
            return false;
        }

        private static bool SaveFile(out string outPath)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Mastery Project File (*.MPF)|*.MPF";

            if (saveFileDialog.ShowDialog() == true)
            {
                outPath = saveFileDialog.FileName;
                return true;
            }

            outPath = "";
            return false;
        }
        #endregion
    }
}
