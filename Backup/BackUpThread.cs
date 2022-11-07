using System.Text;

namespace Backup
{
    internal static class BackUpThread
    {
        static private readonly List<Folder> _folders = FolderSettings.Folders;
        static private StringBuilder _stringBuilder;
        static private bool _work;

        /// <summary>
        /// Starting BackUp Async
        /// </summary>
        /// <param name="timeNow">Time of BackUp folder created</param>
        public static async void StartAsync(object timeNow)
        {
            _work = true;
            string time = (string)timeNow;

            await Task.Run(() =>
            {
                while (_work)
                {
                    foreach (Folder folder in _folders)
                    {
                        if (folder.Role == Role.Target)
                        {
                            continue;
                        }

                        folder.MakeTick();

                        if (folder.GetRemainingTime() == 0)
                        {
                            string path = Path.Combine(_folders[0].FolderPath, time, folder.Name);
                            _stringBuilder = new StringBuilder();

                            string errors = CopyFiles(folder.FolderPath, path);

                            if (!string.IsNullOrWhiteSpace(errors))
                            {
                                path = Path.Combine(_folders[0].FolderPath, time, $"Journal_{time}");

                                WriteErrors(Path.Combine(path, $"{folder.Name}_{DateTime.Now.TimeOfDay:HHmmss}.txt"), errors);
                            }

                            folder.UpdateRemainingTime();
                        }
                    }

                    ConsoleManage.WriteBackUpMenu(time);
                    Thread.Sleep(1000);
                }
            });
        }

        /// <summary>
        /// Writing Exceptions in Journal.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="errors">string of exceptions</param>
        private static void WriteErrors(string path, string errors)
        {
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    byte[] buffer = Encoding.Default.GetBytes(errors);
                    fileStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception exc)
            {
                ConsoleManage.Write(exc.Message);
                Stop();
            }
        }

        /// <summary>
        /// Сopies files and directories from one folder to another
        /// </summary>
        /// <param name="fromDir">Source directory</param>
        /// <param name="toDir">Target directory</param>
        /// <returns>String of errors. Can be null.</returns>
        private static string CopyFiles(string fromDir, string toDir)
        {
            void AddError(Type exType, string msg, string fromDir, string toDir, string file = null)
            {
                _stringBuilder.Append($"{exType}:\n" +
                                     $"{msg}\n");

                if (file != null)
                {
                    _stringBuilder.Append($"file: {file}\n");
                }

                _stringBuilder.Append($"from: {fromDir}\n" +
                                      $"to: {toDir}\n\n");
            }

            foreach (string file in Directory.GetFiles(fromDir))
            {
                try
                {
                    File.Copy(file, toDir + '\\' + Path.GetFileName(file), true);
                }
                catch (UnauthorizedAccessException unauthorizedEx)
                {
                    AddError(typeof(UnauthorizedAccessException), unauthorizedEx.Message, fromDir, toDir, file);
                }
                catch (ArgumentException argumentEx)
                {
                    AddError(typeof(ArgumentException), argumentEx.Message, fromDir, toDir, file);
                }
                catch (PathTooLongException pathTooLongEx)
                {
                    AddError(typeof(PathTooLongException), pathTooLongEx.Message, fromDir, toDir, file);
                }
                catch (DirectoryNotFoundException dirNotFoundEx)
                {
                    AddError(typeof(DirectoryNotFoundException), dirNotFoundEx.Message, fromDir, toDir, file);
                }
                catch (FileNotFoundException fileNotFoundEx)
                {
                    AddError(typeof(FileNotFoundException), fileNotFoundEx.Message, fromDir, toDir, file);
                }
                catch (IOException iOEx)
                {
                    AddError(typeof(IOException), iOEx.Message, fromDir, toDir, file);
                }
                catch (NotSupportedException notSupportedEx)
                {
                    AddError(typeof(NotSupportedException), notSupportedEx.Message, fromDir, toDir, file);
                }
            }

            foreach (string directory in Directory.GetDirectories(fromDir))
            {
                try
                {
                    Directory.CreateDirectory(toDir + '\\' + Path.GetFileName(directory));
                    CopyFiles(directory, toDir + '\\' + Path.GetFileName(directory));
                }
                catch (IOException iOEx)
                {
                    AddError(typeof(IOException), iOEx.Message, fromDir, toDir);
                }
                catch (UnauthorizedAccessException unauthorAccessEx)
                {
                    AddError(typeof(UnauthorizedAccessException), unauthorAccessEx.Message, fromDir, toDir);
                }
                catch (ArgumentException argNullEx)
                {
                    AddError(typeof(ArgumentException), argNullEx.Message, fromDir, toDir);
                }
                catch (NotSupportedException notSupEx)
                {
                    AddError(typeof(NotSupportedException), notSupEx.Message, fromDir, toDir);
                }
            }

            return _stringBuilder.ToString();
        }

        /// <summary>
        /// Stops the BackUp.
        /// </summary>
        public static void Stop()
        {
            _work = false;
        }
    }
}
