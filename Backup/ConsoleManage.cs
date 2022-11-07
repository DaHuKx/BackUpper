using System.Text;

namespace Backup
{
    internal static class ConsoleManage
    {
        private static StringBuilder stringBuilder;

        /// <summary>
        /// Write string in console with selected color.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="color"></param>
        public static void Write(string str, ConsoleColor color = ConsoleColor.White)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            if (color != ConsoleColor.White)
            {
                Console.ForegroundColor = color;
                Console.Write(str);
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.Write(str);
            }
        }

        /// <summary>
        /// Writing menu of the programm.
        /// </summary>
        internal static void WriteMenu()
        {
            Console.Clear();

            WriteLine("File backup by DaHuKx\n", ConsoleColor.Yellow);

            WriteLine("1. Start BackUp", ConsoleColor.DarkGreen);
            WriteLine("2. Add source folder", ConsoleColor.DarkGreen);
            WriteLine("3. Change target folder", ConsoleColor.DarkGreen);
            WriteLine("4. Change source folder", ConsoleColor.DarkGreen);
            WriteLine("5. Remove source folder", ConsoleColor.DarkGreen);

            WriteLine("\n\nFolders info:", ConsoleColor.DarkYellow);

            foreach (Folder folder in FolderSettings.Folders)
            {
                WriteLine(folder.Info);
            }
        }

        /// <summary>
        /// Write the menu of BackUp mode.
        /// </summary>
        /// <param name="folderName">Name of BackUp folder (DateTime.Now)</param>
        internal static void WriteBackUpMenu(string folderName)
        {
            Console.Clear();

            stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("BackUp Menu\n");
            stringBuilder.AppendLine($"Folder: {folderName}\n");

            foreach (Folder folder in FolderSettings.Folders)
            {
                if (folder.Role != Role.Target)
                {
                    if (folder.GetRemainingTime() == 1)
                    {
                        stringBuilder.AppendLine($"{folder.Name} - In process.");
                    }
                    else
                    {
                        stringBuilder.AppendLine($"{folder.Name} - Back up after: {folder.RemainingTimeToBackUp}");
                    }
                }
            }

            stringBuilder.Append("\nPress '1' to stop Back upping and open menu.");

            Console.Write(stringBuilder);
        }

        /// <summary>
        /// Writes string in console with selected color and changes the line.
        /// </summary>
        /// <param name="str">string</param>
        /// <param name="color">color of the string</param>
        public static void WriteLine(string str, ConsoleColor color = ConsoleColor.White)
        {
            Write(str, color);
            Console.WriteLine();
        }

        /// <summary>
        /// Waits until one of the incoming keys is pressed.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static char WaitOneOfKeyPressed(char[] keys)
        {
            if (keys.Length == 0)
            {
                throw new ArgumentException("The array must contain values.");
            }

            ConsoleKeyInfo consoleKey;
            do
            {
                consoleKey = Console.ReadKey(true);
            }
            while (!keys.Contains(consoleKey.KeyChar));

            return consoleKey.KeyChar;
        }

        /// <summary>
        /// Asks the user a question with the answer Yes/No.
        /// Key 1 - Yes.
        /// Key 2 - No.
        /// </summary>
        /// <param name="question">Question</param>
        /// <returns>User answer.</returns>
        public static bool YesOrNoQuestion(string question)
        {
            WriteLine(question);
            WriteLine("1. Yes\n" +
                      "2. No");

            if (WaitOneOfKeyPressed(new char[] { '1', '2' }) == '1')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Writes Done in console and wait for any key pressed.
        /// </summary>
        public static void DoneMessage()
        {
            WriteLine("\nDone.", ConsoleColor.Green);
            WriteLine("Press any key to continue.");
            Console.ReadKey();
        }
    }
}
