namespace Backup
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!FolderSettings.SettingsFileExist())
            {
                FolderSettings.MissedFile();
            }

            FolderSettings.Initialize();

            Menu.Open();
        }
    }
}