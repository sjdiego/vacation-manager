using System.Diagnostics;

namespace VacationManager.Api.Services
{
    public interface IGitVersionService
    {
        string GetCommitHash();
    }

    public class GitVersionService : IGitVersionService
    {
        private readonly string _commitHash;

        public GitVersionService()
        {
            _commitHash = RetrieveCommitHash();
        }

        public string GetCommitHash()
        {
            return _commitHash;
        }

        private static string RetrieveCommitHash()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "git";
                    process.StartInfo.Arguments = "rev-parse --short HEAD";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();

                    return !string.IsNullOrEmpty(output) ? output : "unknown";
                }
            }
            catch
            {
                return "unknown";
            }
        }
    }
}
