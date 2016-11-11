using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace GlobWatch.Internal {

    /// <summary>
    /// Provides configuration information from command line arguments.
    /// </summary>
    internal class CommandArgsConfiguration : IConfiguration {

        #region IConfiguration

        /// <inheritdoc/>
        public string Command {
            get;
            private set;
        }

        /// <inheritdoc/>
        public string WorkingDirectory {
            get;
            private set;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<string> IncludePatterns {
            get;
            private set;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<string> ExcludePatterns {
            get;
            private set;
        }

        /// <inheritdoc/>
        public WatcherChangeTypes WatchedEvents {
            get;
            private set;
        }

        /// <inheritdoc/>
        public string Directory {
            get;
            private set;
        }

        /// <inheritdoc/>
        public bool IgnoreCase {
            get;
            private set;
        }

        #endregion

        private CommandArgsConfiguration() { }

        /// <summary>
        /// Creates a <see cref="IConfiguration"/> object from command line arguments.
        /// </summary>
        /// <param name="app">The console app.</param>
        /// <param name="args">The command line arguments to parse.</param>
        /// <returns>
        /// The parsed configuration or <c>null</c> if the provided arguments are invalid or incomplete.
        /// </returns>
        internal static IConfiguration Parse(CommandLineApplication app, params string[] args) {
            if (!(args?.Any() ?? false))
                return null;

            var config = new CommandArgsConfiguration();

            var options = app.GetOptions();
            var command = app.Arguments.Single(a => a.Name == "command");
            var include = options.Single(o => o.LongName == "include");
            var exclude = options.Single(o => o.LongName == "exclude");
            var events = options.Single(o => o.LongName == "events");
            var dir = options.Single(o => o.LongName == "dir");
            var iname = options.Single(o => o.LongName == "ignorecase");
            var workdir = options.Single(o => o.LongName == "workdir");

            config.Command = Sanitize(command.Values);

            if (config.Command == null)
                return null;

            config.IncludePatterns = SanitizeCollection(include.Values);
            config.ExcludePatterns = SanitizeCollection(exclude.Values);
            config.WatchedEvents = ParseEvents(events.Values);
            config.Directory = Sanitize(dir.Values);
            config.IgnoreCase = iname.HasValue();

            return config;
        }

        #region Helper functions

        static string Sanitize(IEnumerable<string> tokens) =>
            tokens != null ?
                Sanitize(String.Join(" ", tokens)) : null;

        static string Sanitize(string arg) {
            if (arg != null) {
                if (arg.StartsWith("\""))
                    arg = arg.Substring(1);

                if (arg.EndsWith("\""))
                    arg = arg.Substring(0, arg.Length - 1);

                if (String.IsNullOrWhiteSpace(arg))
                    arg = null;
            }

            return arg;
        }

        static IReadOnlyCollection<string> SanitizeCollection(IEnumerable<string> tokens) {
            var arg = Sanitize(tokens);
            string[] patterns;
            if (arg != null)
                patterns = arg.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            else
                patterns = new string[0];

            return new ReadOnlyCollection<string>(patterns);
        }

        static WatcherChangeTypes ParseEvents(IEnumerable<string> tokens) {
            var arg = Sanitize(tokens);
            if (arg == null)
                return WatcherChangeTypes.All;

            var flags = 0;

            foreach (var c in arg) {
                if (Char.ToUpper(c) == 'C')
                    flags |= (int)WatcherChangeTypes.Created;
                else if (Char.ToUpper(c) == 'M')
                    flags |= (int)WatcherChangeTypes.Changed;
                else if (Char.ToUpper(c) == 'D')
                    flags |= (int)WatcherChangeTypes.Deleted;
                else if (Char.ToUpper(c) == 'R')
                    flags |= (int)WatcherChangeTypes.Renamed;
            }

            if (flags == 0)
                flags = (int)WatcherChangeTypes.All;

            return (WatcherChangeTypes)flags;
        }

        #endregion

    }

}
