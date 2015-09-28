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
        public static bool Save(ProjectModel project)
        {
            string fileName = "";
            if (SaveFile(out fileName))
            {
                SaveProjectModel(project, fileName);
                Properties.Settings.Default.HasLoadPath = true;
                Properties.Settings.Default.LastLoadPath = fileName;
                return true;
            }
            return false;
        }

        private static void SaveProjectModel(ProjectModel project, string fileName)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writer.Write(Encoding.UTF8.GetBytes("MPF0")); // magic
                writer.Write(project.Task.Count());
                writer.Write(Encoding.UTF8.GetBytes(project.Task));
                writer.Write(DateTime.Now.Month);
                writer.Write(DateTime.Now.Day);
                writer.Write(DateTime.Now.Year);
                writer.Write(DateTime.Now.Hour);
                writer.Write(DateTime.Now.Minute);
                writer.Write(project.TargetHours);
                writer.Write(project.ElapsedTime);
            }
        }

        public static void SaveNoPrompt(ProjectModel project)
        {
            SaveProjectModel(project, Properties.Settings.Default.LastLoadPath);
        }

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
            }
            return project;
        }

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
    }
}
