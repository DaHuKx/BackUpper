using Newtonsoft.Json;
using System.Text;

namespace Backup
{
    enum Role
    {
        Target,
        Source
    }

    internal class Folder
    {
        public string FolderPath { get; set; }
        public Role Role { get; set; }
        public int Ferquency { get; set; }
        private int _remainingTime;
        public string RemainingTimeToBackUp
        {
            get { return $"{_remainingTime / 3600:D2}:{_remainingTime % 3600 / 60:D2}:{_remainingTime % 3600 % 60:D2}"; }
        }

        [JsonIgnore]
        public string Name { get { return Path.GetFileName(FolderPath); } }
        [JsonIgnore]
        public string[] Files { get { return Directory.GetFiles(FolderPath); } }
        [JsonIgnore]
        public string[] Directories { get { return Directory.GetDirectories(FolderPath); } }

        public Folder(string folderPath, Role role, int ferquency = 1)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            FolderPath = Path.GetFullPath(folderPath);
            Role = role;
            Ferquency = ferquency;
            _remainingTime = ferquency;
        }

        [JsonIgnore]
        public string Info
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine($"Path: {FolderPath}");
                stringBuilder.AppendLine($"Role: {Role}");

                if (Role == Role.Source)
                {
                    stringBuilder.AppendLine($"Ferquency: {Ferquency / 3600:D2}:{(Ferquency % 3600) / 60:D2}:{Ferquency % 3600 % 60:D2}");
                }

                stringBuilder.AppendLine($"Files: ");

                if (Files.Length < 1)
                {
                    stringBuilder.AppendLine(" - Empty");
                }
                else
                {
                    foreach (string file in Files)
                    {
                        stringBuilder.AppendLine($" - {Path.GetFileName(file)}");
                    }
                }

                stringBuilder.AppendLine($"Directories: ");
                if (Directories.Length < 1)
                {
                    stringBuilder.AppendLine(" - Empty");
                }
                else
                {
                    foreach (string directory in Directories)
                    {
                        stringBuilder.AppendLine($" - {Path.GetFileName(directory)}");
                    }
                }

                return stringBuilder.ToString();
            }
        }

        public int GetRemainingTime()
        {
            return _remainingTime;
        }

        public void MakeTick()
        {
            _remainingTime--;
        }

        public void UpdateRemainingTime()
        {
            _remainingTime = Ferquency;
        }
    }
}
