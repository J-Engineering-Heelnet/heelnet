using System.Diagnostics;

namespace HeelnetTests.Utils
{
    /// <summary>
    /// Legt vast wélke versie van de software een testrun heeft getest: de git-branch en commit
    /// van de repository. Twee runs met dezelfde commit (en geen lokale wijzigingen) testten dezelfde code.
    /// </summary>
    public sealed class AppVersionInfo
    {
        public string? Branch { get; private init; }
        public string? Commit { get; private init; }
        public bool HasLocalChanges { get; private init; }

        private static readonly Lazy<AppVersionInfo> current = new(Detect);

        public static AppVersionInfo Current => current.Value;

        public override string ToString() =>
            Commit == null
                ? "git-versie onbekend"
                : $"{Branch} @ {Commit[..Math.Min(8, Commit.Length)]}{(HasLocalChanges ? " (+ lokale wijzigingen)" : "")}";

        private static AppVersionInfo Detect() => new()
        {
            Branch = Git("rev-parse --abbrev-ref HEAD"),
            Commit = Git("rev-parse HEAD"),
            HasLocalChanges = !string.IsNullOrEmpty(Git("status --porcelain -- Client Server"))
        };

        private static string? Git(string arguments)
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo("git", arguments)
                {
                    WorkingDirectory = TestConfig.RepositoryRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                if (process == null) return null;
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(5000);
                return process.ExitCode == 0 ? output : null;
            }
            catch
            {
                return null; // git niet geïnstalleerd of geen repository
            }
        }
    }
}
