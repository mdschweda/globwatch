using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Cli.Utils;

namespace GlobWatch {

    /// <summary>
    /// Runs commands when file system changes occur.
    /// </summary>
    public class CommandRunner {

        IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRunner"/> class.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        public CommandRunner(IConfiguration config) {
            if (config == null || String.IsNullOrEmpty(config.Command))
                throw new ArgumentException();

            _config = config;
        }

        /// <summary>
        /// Starts watching the file system.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token that should be used to cancel the watching task.
        /// </param>
        /// <returns>A <see cref="Task"/> that represent the watching process.</returns>
        public async Task Run(CancellationToken cancellationToken) {
            var watcher = new GlobWatcher(_config);
            watcher.Changed += RunCommand;
            await watcher.Watch(cancellationToken);
        }

        // Run the command after change
        void RunCommand(object sender, WaitForChangedResult e) {
            var workDir = _config.WorkingDirectory;
            if (String.IsNullOrEmpty(workDir)) {
                var dir = _config.Directory;
                if (String.IsNullOrEmpty(dir))
                    dir = Directory.GetCurrentDirectory();
                workDir = Path.GetDirectoryName(Path.Combine(dir, e.Name));
            }

            var tokens = _config.Command
                .Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            var cmd = tokens[0];
            var args = tokens.Skip(1)
                .FirstOrDefault()
                ?.Replace("%pathBefore", e.OldName)
                .Replace("%path", e.Name);

            var process = new Process() {
                StartInfo = new ProcessStartInfo {
                    FileName = cmd,
                    Arguments = args,
                    WorkingDirectory = workDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.OutputDataReceived += OutputReceived;
            process.ErrorDataReceived += ErrorReceived;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        void ErrorReceived(object sender, DataReceivedEventArgs e) =>
            AnsiConsole.GetOutput().WriteLine(e.Data.Red());

        void OutputReceived(object sender, DataReceivedEventArgs e) =>
            Console.Write(e.Data);

    }

}
