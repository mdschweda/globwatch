using Microsoft.Extensions.CommandLineUtils;

namespace GlobWatch.Internal {

    /// <summary>
    /// Encapsulates the console application.
    /// </summary>
    internal static class App {

        /// <summary>
        /// Gets the current console application.
        /// </summary>
        internal static CommandLineApplication Current {
            get;
            private set;
        }

        // Configure available command line arguments
        static App() {
            Current = new CommandLineApplication(false) {
                Name = "globwatch",
                FullName = "Glob pattern path watcher",
                Description = "Watches the file system using glob patterns and runs commands on changes.",
                ExtendedHelpText = @"
Placeholders %path, %pathBefore and %event are available inside your command argument.

Return values:
  0 - Execution aborted by user
  1 - Invalid arguments
  2 - Unexpected error

Examples:
  globwatch ""cmd.exe /c echo %path""
  globwatch ""minify %path"" -i **/*.css|**/*.js -e cm -ic",
                ShortVersionGetter = () => "1.0",
                LongVersionGetter = () => "1.0.1"
            };

            Current.Argument("command", "The command to run.", true);

            Current.Option(
                "-i|--include",
                "The glob pattern or glob patterns, separated by '|', for paths to watch. The default value is '**/*.*'",
                CommandOptionType.SingleValue
            );

            Current.Option(
                "-x|--exclude",
                "The glob pattern or glob patterns, separated by '|', for paths to exclude from watching.",
                CommandOptionType.SingleValue
            );

            Current.Option(
                "-e|--events",
                "The events to watch: c - created, m - modified, d - deleted, r - renamed. The default value is 'cmdr'.",
                CommandOptionType.SingleValue
            );

            Current.Option(
                "-d|--dir",
                "The base directory to watch. The default default is the current directory.",
                CommandOptionType.SingleValue
            );

            Current.Option(
                "-w|--workdir",
                "The work directory of the executed command. The default default is the directory where the changed occured.",
                CommandOptionType.SingleValue
            );

            Current.Option(
                "-ic|--ignorecase",
                "Specifies that the search is case insensitive.",
                CommandOptionType.NoValue
            );
        }

    }

}
