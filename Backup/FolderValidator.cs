using FluentValidation;

namespace Backup
{
    internal class FolderValidator : AbstractValidator<Folder>
    {
        public FolderValidator(List<Folder> folders)
        {
            RuleFor(folder => folder.FolderPath).NotEmpty()
                                                .Must(BeExistOrCreateble)
                                                .WithMessage(folder => $"Can't create folder with this path: {folder.FolderPath}");

            RuleFor(folder => folder.FolderPath).Must(path => (folders == null) || !(folders.FindAll(folder => folder.FolderPath == path).Count > 0))
                                                .WithMessage(folder => $"Folder can't have equal paths: {folder.FolderPath}");

            RuleFor(folder => folder.FolderPath).Must(BeControled)
                                                .WithMessage(folder => $"Didn't have permission to read folder: {folder.FolderPath}");

            RuleFor(folder => folder.Ferquency).NotEmpty().GreaterThanOrEqualTo(1);
        }

        private bool BeExistOrCreateble(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                    Directory.Delete(path);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private bool BeControled(string path)
        {
            if (!Directory.Exists(path))
            {
                return true;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            try
            {
                directoryInfo.GetAccessControl();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
