using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GlobWatch.Internal;
using Microsoft.DotNet.Cli.Utils;

namespace GlobWatch {

    class Program {

        static int Main(string[] args) {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            App.Current.OnExecute(async () => 
                await Watch(CommandArgsConfiguration.Parse(App.Current, args))
            );

            return App.Current.Execute(args);
        }

        static async Task<int> Watch(IConfiguration config) {
            try {
                var runner = new CommandRunner(config);
                Preamble(config);
                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) => cts.Cancel();
                await runner.Run(cts.Token);
                return 0;
            } catch (ArgumentException) {
                App.Current.ShowHelp();
                return 1;
            } catch (Exception e) {
                AnsiConsole
                    .GetOutput()
                    .WriteLine($"{"Unexpected error".Red().Bold()}: {e.Message}");
                return 2;
            }
        }

        // Print argument summary
        static void Preamble(IConfiguration config) {
            var dir = !String.IsNullOrEmpty(config.Directory) ?
                config.Directory : Directory.GetCurrentDirectory();

            var workDir = !String.IsNullOrEmpty(config.WorkingDirectory) ?
                config.WorkingDirectory : dir;

            var includes = (config.IncludePatterns?.Count ?? 0) > 0 ?
                config.IncludePatterns : new[] { "**/*.*" };

            var @out = AnsiConsole.GetOutput();
            
            @out.WriteLine(
                "  ,——._.,——,—.\n" +
                " ( @ ) ( @ )  \\  [GlobWatch]\n" +
                "  `—´   `—´\n"
            );

            @out.WriteLine("- Watching:");
            foreach (var i in includes)
                @out.WriteLine($"  - {i.Green().Bold()}");

            if ((config.ExcludePatterns?.Count ?? 0) > 0) {
                @out.WriteLine("- Except:");
                foreach (var i in config.ExcludePatterns)
                    @out.WriteLine($"  - {i.Red().Bold()}");
            }

            @out.WriteLine($"- Inside: {dir.Bold()}");

            if (config.IgnoreCase)
                @out.WriteLine("- Ignore Case");
            
            if (config.WatchedEvents != 0 && config.WatchedEvents != WatcherChangeTypes.All) {
                string events = null;
                if (config.WatchedEvents.HasFlag(WatcherChangeTypes.Created))
                    events = "Created";
                if (config.WatchedEvents.HasFlag(WatcherChangeTypes.Changed))
                    events = events == null ? "Modified" : $"{events}, modified";
                if (config.WatchedEvents.HasFlag(WatcherChangeTypes.Deleted))
                    events = events == null ? "Deleted" : $"{events}, deleted";
                if (config.WatchedEvents.HasFlag(WatcherChangeTypes.Renamed))
                    events = events == null ? "Renamed" : $"{events}, renamed";
                @out.WriteLine($"- For events of type: {events.Bold()}");
            }

            @out.WriteLine($"- Using command: {config.Command.Bold()}\n");

            if (workDir != dir)
                @out.WriteLine($"- In working directory: {workDir.Bold()}");
        }

    }

}
