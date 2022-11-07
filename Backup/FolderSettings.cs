using FluentValidation.Results;
using Newtonsoft.Json;
using System.Text;

namespace Backup
{
    internal static class FolderSettings
    {
        public static string FilePath { get { return "Settings.json"; } }
        private static List<Folder> _folders;
        public static List<Folder> Folders
        {
            get { return _folders; }
            set
            {
                if (_folders != value)
                {
                    _folders = value;
                }
            }
        }

        /// <summary>
        /// Initialize Folders List from Settings.json
        /// </summary>
        internal static void Initialize()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(FilePath))
                    Folders = JsonConvert.DeserializeObject<List<Folder>>(streamReader.ReadToEnd());

                if (!FoldersIsCorrect())
                {
                    throw new Exception("Incorrected data.");
                }
            }
            catch (Exception exception)
            {
                ConsoleManage.WriteLine("Settings file is corrupted.\n" + exception.Message, ConsoleColor.Red);
                if (ConsoleManage.YesOrNoQuestion("Create new Settings file?"))
                {
                    CreateNewSettingsFile();
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }

        /// <summary>
        /// Checks if Folders is correct.
        /// </summary>
        /// <returns>True if correct else false</returns>
        private static bool FoldersIsCorrect()
        {
            return !((_folders.Count < 2)
                    || (_folders.ToList().FindAll(folder => folder.Role == Role.Target).Count != 1)
                    || (_folders[0].Role != Role.Target))
                    || (_folders.All(folder => Directory.Exists(folder.FolderPath)));
        }

        /// <summary>
        /// Check if file Settings.json is exist
        /// </summary>
        /// <returns></returns>.
        internal static bool SettingsFileExist()
        {
            return File.Exists("Settings.json");
        }

        /// <summary>
        /// Starting BackUp mode.
        /// </summary>
        internal static void StartBackUp()
        {
            string time = DateTime.Now.ToString("ddMMyyyy_HHmmss");

            Directory.CreateDirectory(Path.Combine(_folders[0].FolderPath, time));

            for (int index = 1; index < _folders.Count; index++)
            {
                Directory.CreateDirectory(Path.Combine(_folders[0].FolderPath, time, _folders[index].Name));
            }

            Directory.CreateDirectory(Path.Combine(_folders[0].FolderPath, time, "Journal_" + time));

            BackUpThread.StartAsync(time);
            ConsoleManage.WaitOneOfKeyPressed(new char[] { '1' });
            BackUpThread.Stop();

            Console.Clear();

            Menu.Open();
        }

        /// <summary>
        /// Updating file Settings.json with Folders list.
        /// </summary>
        private static async void UpdateSettingsFile()
        {
            await Task.Run(() =>
            {
                using (FileStream fileStream = File.Open("Settings.json", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fileStream.SetLength(0);

                    byte[] buffer = Encoding.Default.GetBytes(JsonConvert.SerializeObject(_folders));
                    fileStream.WriteAsync(buffer, 0, buffer.Length);
                }
            });
        }

        /// <summary>
        /// Gives index of folder that user want to remove.
        /// </summary>
        /// <returns>int Index</returns>
        private static int GetRemovedFolderIndex()
        {
            int removedFolderIndex;

            do
            {
                Console.Clear();
                ConsoleManage.WriteLine("Getting removed folder index...\n", ConsoleColor.Yellow);
                for (int index = 1; index < _folders.Count; index++)
                {
                    ConsoleManage.WriteLine($"{index}. {_folders[index].FolderPath}");
                }

                ConsoleManage.Write("Index of folder: ");

                try
                {
                    removedFolderIndex = Convert.ToInt32(Console.ReadLine());

                    if (removedFolderIndex < 1 || removedFolderIndex > _folders.Count - 1)
                    {
                        throw new Exception($"Index must be between 1 and {_folders.Count - 1}");
                    }

                    break;
                }
                catch (Exception exception)
                {
                    ConsoleManage.WriteLine(exception.Message, ConsoleColor.Red);

                    if (ConsoleManage.YesOrNoQuestion("Rerty?"))
                    {
                        continue;
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                }
            }
            while (true);

            return removedFolderIndex;
        }

        /// <summary>
        /// Adds source folder to BackUp
        /// </summary>.
        internal static void AddSourceFolder()
        {
            _folders.Add(InitializeFolder(Role.Source));

            UpdateSettingsFile();

            ConsoleManage.DoneMessage();
            Menu.Open();
        }

        /// <summary>
        /// Changes Target Folder.
        /// </summary>
        internal static void ChangeTargetFolder()
        {
            do
            {
                Console.Clear();
                ConsoleManage.WriteLine("Target folder changing...\n", ConsoleColor.Yellow);
                ConsoleManage.WriteLine($"Old path: {Folders[0].FolderPath}");
                ConsoleManage.Write($"New path: ");

                string str;
                try
                {
                    str = Console.ReadLine();

                    Folder folder = new Folder(str, Role.Target);

                    FolderValidator validation = new FolderValidator(Folders);
                    ValidationResult result = validation.Validate(folder);

                    if (!result.IsValid)
                    {
                        throw new ArgumentException(result.ToString());
                    }
                    else
                    {
                        _folders[0].FolderPath = str;

                        UpdateSettingsFile();
                        break;
                    }
                }
                catch (ArgumentException exception)
                {
                    ConsoleManage.WriteLine(exception.Message, ConsoleColor.Red);

                    if (ConsoleManage.YesOrNoQuestion("Retry?"))
                    {
                        continue;
                    }
                    else
                    {
                        Environment.Exit(0);
                        break;
                    }
                }
            }
            while (true);

            ConsoleManage.DoneMessage();
            Menu.Open();
        }

        /// <summary>
        /// Changes Source folder.
        /// </summary>
        internal static void ChangeSourceFolder()
        {
            int removedFolderIndex = GetRemovedFolderIndex();

            Folder newFolder = InitializeFolder(Role.Source);

            if (newFolder != null)
            {
                _folders[removedFolderIndex] = newFolder;
            }

            UpdateSettingsFile();

            ConsoleManage.DoneMessage();
            Menu.Open();
        }

        /// <summary>
        /// Removes Source Folder.
        /// </summary>
        internal static void RemoveSourceFolder()
        {
            if (_folders.Count == 2)
            {
                Console.Clear();
                ConsoleManage.WriteLine("Removing source folder...\n", ConsoleColor.Yellow);
                ConsoleManage.WriteLine("Count of source folders cannot be less than 1.\n", ConsoleColor.Red);

                ConsoleManage.Write("Press any key to open menu.");
                Console.ReadKey();
                Menu.Open();
                return;
            }

            _folders.Remove(_folders[GetRemovedFolderIndex()]);

            UpdateSettingsFile();

            ConsoleManage.DoneMessage();
            Menu.Open();
        }

        /// <summary>
        /// Creates new settings file.
        /// </summary>
        internal static void CreateNewSettingsFile()
        {
            _folders = new List<Folder>();

            _folders.Add(InitializeFolder(Role.Target));
            _folders.Add(InitializeFolder(Role.Source));

            UpdateSettingsFile();

            ConsoleManage.DoneMessage();
            Menu.Open();
        }

        /// <summary>
        /// Gives Folders list from Settings.json
        /// </summary>
        /// <returns></returns>
        internal static List<Folder> GetFolders()
        {
            using (StreamReader streamReader = new StreamReader("Settings.json"))
            {
                try
                {
                    List<Folder> folders = JsonConvert.DeserializeObject<List<Folder>>(streamReader.ReadToEnd());

                    if (!FoldersIsCorrect())
                    {
                        throw new Exception("File is not correct.");
                    }

                    return _folders;
                }
                catch (Exception exception)
                {
                    streamReader.Close();

                    ConsoleManage.WriteLine(exception.Message + '\n', ConsoleColor.Red);

                    if (ConsoleManage.YesOrNoQuestion("Recreate Settings folder?"))
                    {
                        CreateNewSettingsFile();
                        return GetFolders();
                    }
                    else
                    {
                        Environment.Exit(0);
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Notice user that file Settings.json is missed.
        /// </summary>
        internal static void MissedFile()
        {
            ConsoleManage.WriteLine($"Missed file {Path.GetFullPath("Settings.json")}", ConsoleColor.Red);
            if (ConsoleManage.YesOrNoQuestion("Create new settings file?"))
            {
                CreateNewSettingsFile();
            }
            else
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Initialize new folder with input Role: Target/Source.
        /// </summary>
        /// <param name="role">Role of folder</param>
        /// <returns>Folder</returns>
        internal static Folder InitializeFolder(Role role)
        {
            Folder newFolder;

            do
            {
                try
                {
                    Console.Clear();
                    ConsoleManage.WriteLine($"Initializing {role} folder...\n", ConsoleColor.Yellow);
                    ConsoleManage.Write($"{role} folder path: ");
                    string path = Console.ReadLine();

                    if (role == Role.Source)
                    {
                        ConsoleManage.Write("Backup frequency (seconds): ");
                        int frequency = Convert.ToInt32(Console.ReadLine());
                        newFolder = new Folder(path, role, frequency);
                    }
                    else
                    {
                        newFolder = new Folder(path, role);
                    }

                    FolderValidator validation = new FolderValidator(Folders);
                    ValidationResult result = validation.Validate(newFolder);

                    if (!result.IsValid)
                    {
                        throw new Exception(result.ToString());
                    }

                    if (newFolder.Ferquency < 60 && role == Role.Source)
                    {
                        if (!ConsoleManage.YesOrNoQuestion($"\n{newFolder.Ferquency} is too fast. Can be problems with programm. Are you sure?"))
                        {
                            throw new Exception("Better write Frequency greater than 60 sec.");
                        }
                    }

                    break;
                }
                catch (Exception exception)
                {
                    ConsoleManage.WriteLine(exception.ToString() + '\n', ConsoleColor.Red);

                    if (ConsoleManage.YesOrNoQuestion("Retry?"))
                    {
                        continue;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            while (true);

            return newFolder;
        }
    }
}
