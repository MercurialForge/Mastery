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
        static public void Save(ProjectModel project)
        {
            string fileName = "";
            if (OpenMainFile(out fileName))
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
        }

        static public ProjectModel Load(string loadPath)
        {
            ProjectModel project = new ProjectModel();

            using (BinaryReader reader = new BinaryReader(File.Open(loadPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                reader.BaseStream.Seek(4, 0); // magic

                int taskTitleLength = reader.ReadInt32();
                project.Task = new string(reader.ReadChars(taskTitleLength));

                int month = reader.ReadInt32();
                int day = reader.ReadInt32();
                int year = reader.ReadInt32();
                int hour = reader.ReadInt32();
                int minute = reader.ReadInt32();
                DateTime dateTime = new DateTime(year, month, day, hour, minute, 0, 0);
                project.StartDate = dateTime;

                project.TargetHours = reader.ReadDouble();
                project.ElapsedTime = reader.ReadDouble();
            }
            return project;
        }

        static public ProjectModel Load()
        {
            ProjectModel project = new ProjectModel();

            string fileName = "";
            if (OpenMainFile(out fileName))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    reader.BaseStream.Seek(4, 0); // magic

                    int taskTitleLength = reader.ReadInt32();
                    project.Task = new string(reader.ReadChars(taskTitleLength));

                    int month = reader.ReadInt32();
                    int day = reader.ReadInt32();
                    int year = reader.ReadInt32();
                    int hour = reader.ReadInt32();
                    int minute = reader.ReadInt32();
                    DateTime dateTime = new DateTime(year, month, day, hour, minute, 0, 0);
                    project.StartDate = dateTime;

                    project.TargetHours = reader.ReadDouble();
                    project.ElapsedTime = reader.ReadDouble();
                }
            }
            return project;
        }

        static private bool OpenMainFile(out string outPath)
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
    }
}
