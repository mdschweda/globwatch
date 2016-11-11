using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GlobWatch.Internal;

namespace GlobWatch {

    /// <summary>
    /// Watches the file system for changes using glob patterns.
    /// </summary>
    public class GlobWatcher {

        internal event EventHandler<WaitForChangedResult> Changed;

        WatcherChangeTypes _events;
        PatternMatcher _matcher;
        string _dir;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobWatcher"/> class.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        public GlobWatcher(IConfiguration config) {
            _matcher = new PatternMatcher(
                config.IncludePatterns,
                config.ExcludePatterns,
                config.IgnoreCase
            );

            _dir = !String.IsNullOrEmpty(config.Directory) ?
                config.Directory : Directory.GetCurrentDirectory();
            _dir = Path.GetFullPath(_dir);

            _events = config.WatchedEvents != 0 ?
                config.WatchedEvents : WatcherChangeTypes.All;
        }

        /// <summary>
        /// Starts watching the file system.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token that should be used to cancel the watching task.
        /// </param>
        /// <returns>A <see cref="Task"/> that represent the watching process.</returns>
        public async Task Watch(CancellationToken cancellationToken) {
            try {
                var watcher = new FileSystemWatcher(_dir) {
                    IncludeSubdirectories = true
                };

                cancellationToken.Register(() => watcher?.Dispose());

                if (_events.HasFlag(WatcherChangeTypes.Created))
                    watcher.Created += OnWatcherFileSystemEvent;

                if (_events.HasFlag(WatcherChangeTypes.Changed))
                    watcher.Changed += OnWatcherFileSystemEvent;

                if (_events.HasFlag(WatcherChangeTypes.Deleted))
                    watcher.Deleted += OnWatcherFileSystemEvent;

                if (_events.HasFlag(WatcherChangeTypes.Renamed))
                    watcher.Renamed += OnWatcherRenameEvent;

                watcher.EnableRaisingEvents = true;

                await Task.Delay(Timeout.Infinite, cancellationToken);
            } catch (TaskCanceledException) {
                Console.WriteLine("Stopping by request...");
            }
        }

        private void OnWatcherRenameEvent(object sender, RenamedEventArgs e) {
            if (_matcher.Test(e.Name))
                Changed?.Invoke(
                    this,
                    new WaitForChangedResult {
                        ChangeType = e.ChangeType,
                        Name = e.Name,
                        OldName = e.OldName
                    }
                );
        }

        private void OnWatcherFileSystemEvent(object sender, FileSystemEventArgs e) {
            if (_matcher.Test(e.Name))
                Changed?.Invoke(
                    this,
                    new WaitForChangedResult {
                        ChangeType = e.ChangeType,
                        Name = e.Name
                    }
                );
        }

    }



}
