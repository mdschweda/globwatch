using System.Collections.Generic;
using System.IO;

namespace GlobWatch {

    /// <summary>
    /// Provides an interface that represents the configuration of the application.
    /// </summary>
    public interface IConfiguration {

        /// <summary>
        /// Gets the command to run when file system events occur.
        /// </summary>
        string Command { get; }

        /// <summary>
        /// Gets the working directory of the executed command.
        /// </summary>
        string WorkingDirectory { get; }

        /// <summary>
        /// Gets the glob patterns that describe the paths to watch.
        /// </summary>
        IReadOnlyCollection<string> IncludePatterns { get; }

        /// <summary>
        /// Gets the glob patterns that describe exceptions to <see cref="IncludePatterns"/>.
        /// </summary>
        IReadOnlyCollection<string> ExcludePatterns { get; }

        /// <summary>
        /// Gets the file system events to watch.
        /// </summary>
        WatcherChangeTypes WatchedEvents { get; }

        /// <summary>
        /// Gets the base directory to watch.
        /// </summary>
        string Directory { get; }

        /// <summary>
        /// Gets a value that indicates if <see cref="IncludePatterns"/> are case insensitive.
        /// </summary>
        bool IgnoreCase { get; }

    }

}
