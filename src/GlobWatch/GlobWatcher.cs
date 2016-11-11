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
                while (true) {
                    var t1 = WaitForChange(cancellationToken);
                    var t2 = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

                    await Task.WhenAny(t1, t2);

                    if (cancellationToken.IsCancellationRequested)
                        throw new TaskCanceledException();

                    if (t1.Status == TaskStatus.RanToCompletion)
                        if (_matcher.Test(t1.Result.Name))
                            Changed?.Invoke(this, t1.Result);
                }
            } catch (TaskCanceledException) {
                Console.WriteLine("Stopping by request...");
            }
        }

        // Wait for a single change
        Task<WaitForChangedResult> WaitForChange(CancellationToken cancellationToken) {
            var tcs = new TaskCompletionSource<WaitForChangedResult>();


            var watcher = new FileSystemWatcher(_dir) {
                IncludeSubdirectories = true
            };

            cancellationToken.Register(() => {
                watcher?.Dispose();
                tcs.TrySetCanceled();
            });

            if (_events.HasFlag(WatcherChangeTypes.Created))
                watcher.Created += (s, e) => {
                    tcs.TrySetResult(new WaitForChangedResult {
                        ChangeType = e.ChangeType,
                        Name = e.Name
                    });

                    watcher.Dispose();
                };

            if (_events.HasFlag(WatcherChangeTypes.Changed))
                watcher.Changed += (s, e) =>
                    tcs.TrySetResult(new WaitForChangedResult {
                        ChangeType = e.ChangeType,
                        Name = e.Name
                    });

            if (_events.HasFlag(WatcherChangeTypes.Renamed))
                watcher.Renamed += (s, e) => {
                    tcs.TrySetResult(new WaitForChangedResult {
                        ChangeType = e.ChangeType,
                        Name = e.Name,
                        OldName = e.OldName
                    });

                    watcher.Dispose();
                };

            if (_events.HasFlag(WatcherChangeTypes.Deleted))
                watcher.Deleted += (s, e) => {
                    tcs.TrySetResult(new WaitForChangedResult {
                        ChangeType = e.ChangeType,
                        Name = e.Name
                    });

                    watcher.Dispose();
                };

            watcher.EnableRaisingEvents = true;
            return tcs.Task;
        }

    }

}
