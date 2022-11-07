namespace Backup
{
    internal static class Menu
    {
        /// <summary>
        /// Opens menu in console to User.
        /// </summary>
        internal static void Open()
        {
            ConsoleManage.WriteMenu();

            char pressedKey = ConsoleManage.WaitOneOfKeyPressed(new char[] { '1', '2', '3', '4', '5' });

            switch (pressedKey)
            {
                case '1':
                    FolderSettings.StartBackUp();
                    break;

                case '2':
                    FolderSettings.AddSourceFolder();
                    break;

                case '3':
                    FolderSettings.ChangeTargetFolder();
                    break;

                case '4':
                    FolderSettings.ChangeSourceFolder();
                    break;

                case '5':
                    FolderSettings.RemoveSourceFolder();
                    break;
            }
        }
    }
}
