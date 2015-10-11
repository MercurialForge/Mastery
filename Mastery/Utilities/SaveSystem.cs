using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;
using System.Windows;

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

            string[] rawName = new FileInfo(fileName).Name.Split('.');
            string rawDirectory = new FileInfo(fileName).DirectoryName;
            string pathTmp = Path.Combine(rawDirectory, rawName[0] + ".tmp");

            // clean
            File.Delete(pathTmp);

            // write temp
            using (BinaryWriter writer = new BinaryWriter(File.Open(pathTmp, FileMode.Create)))
            {
                writer.Write(Encoding.UTF8.GetBytes("MPF0")); // magic
                writer.Write((UInt16)0); // save space for length
                writer.Write((byte)2); // Version Number
                writer.Write((byte)project.Task.Count());
                writer.Write(Encoding.UTF8.GetBytes(project.Task));
                if (SetStartDate)
                {
                    writer.Write((byte)DateTime.Now.Month);
                    writer.Write((byte)DateTime.Now.Day);
                    writer.Write(DateTime.Now.Year);
                    writer.Write((byte)DateTime.Now.Hour);
                    writer.Write((byte)DateTime.Now.Minute);
                }
                else
                {
                    writer.Write((byte)project.StartDate.Month);
                    writer.Write((byte)project.StartDate.Day);
                    writer.Write(project.StartDate.Year);
                    writer.Write((byte)project.StartDate.Hour);
                    writer.Write((byte)project.StartDate.Minute);
                }
                writer.Write((uint)project.TargetHours);
                writer.Write(project.ElapsedTime);

                writer.Write(project.IsMonitoring);
                writer.Write((byte)project.Applications.Count);

                for (int i = 0; i < project.Applications.Count; i++)
                {
                    writer.Write((byte)project.Applications[i].Count());
                    writer.Write(Encoding.UTF8.GetBytes(project.Applications[i]));
                }
                writer.Seek(4, 0);
                writer.Write((UInt16)writer.BaseStream.Length);
            }

            // overwrite save
            File.Copy(pathTmp, fileName, true);

            // it's safe, delete the temp now
            File.Delete(pathTmp);
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
            Properties.Settings.Default.Reset();
            ProjectModel project = new ProjectModel();

            using (BinaryReader reader = new BinaryReader(File.Open(loadPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                // Skip Magic
                reader.BaseStream.Seek(4, 0);

                // Validate
                bool isValid = (reader.ReadUInt16() == reader.BaseStream.Length);
                if (!isValid) { ShowCorruptionMessage(); return new ProjectModel(); }

                bool correctVersion = (reader.ReadByte() == 2);
                if (!isValid) { ShowIncompatibleVersionMessage(); return new ProjectModel(); }

                // Read Task Name
                int taskTitleLength = reader.ReadByte();
                project.Task = new string(reader.ReadChars(taskTitleLength));

                // Get DateTime of Project Beginning
                int month = reader.ReadByte();
                int day = reader.ReadByte();
                int year = reader.ReadInt32();
                int hour = reader.ReadByte();
                int minute = reader.ReadByte();
                DateTime dateTime = new DateTime(year, month, day, hour, minute, 0, 0);
                project.StartDate = dateTime;

                // Get Target Hours
                project.TargetHours = reader.ReadUInt32();

                // Get Total Elapsed Time
                project.ElapsedTime = reader.ReadDouble();

                project.IsMonitoring = reader.ReadBoolean();

                int applicationCount = reader.ReadByte();

                if (applicationCount != 0)
                {
                    for (int i = 0; i < applicationCount; i++)
                    {
                        int charLength = reader.ReadByte();
                        string application = new string(reader.ReadChars(charLength));
                        if (!project.Applications.Contains(application))
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
        private static void ShowIncompatibleVersionMessage()
        {
            MessageBoxResult result = MessageBox.Show("Incompatible Version. Please Initialize a NEW file", "Wrong Version", MessageBoxButton.OK);
        }

        private static void ShowCorruptionMessage()
        {
            MessageBoxResult result = MessageBox.Show("The save appears corrupted. If a \".tmp\" file is present, rename it's extention to \".MPF\" to recover your data", "Corrupted File", MessageBoxButton.OK);
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
        #endregion
    }
}
