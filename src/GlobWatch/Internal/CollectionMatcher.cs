using System;
using System.Collections.Generic;
using System.Linq;
using Minimatch;

namespace GlobWatch.Internal {

    /// <summary>
    /// Matches relative paths against glob paterns.
    /// </summary>
    internal class PatternMatcher {

        IEnumerable<Minimatcher> _includeMatchers, _excludeMatchers;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatternMatcher"/> class.
        /// </summary>
        /// <param name="includes">The collection of glob patterns for paths to include.</param>
        /// <param name="excludes">The collection of glob patterns for exceptions to <paramref name="includes"/>.</param>
        /// <param name="ignoreCase"><c>True</c> to ignore case during path tests; otherwise <c>false</c>.</param>
        internal PatternMatcher(IEnumerable<string> includes, IEnumerable<string> excludes, bool ignoreCase) {
            if (!(includes?.Any() ?? false))
                includes = new[] { "**/*.*" };

            if (excludes == null)
                excludes = Enumerable.Empty<string>();

            _includeMatchers = includes
                .Select(i => new Minimatcher(i, new Options { IgnoreCase = ignoreCase }));

            _excludeMatchers = excludes
                .Select(e => new Minimatcher(e, new Options { IgnoreCase = ignoreCase }));
        }

        /// <summary>
        /// Tests a relative path against the provided glob patterns.
        /// </summary>
        /// <param name="relativePath">A relative path to test.</param>
        /// <returns><c>True</c> if the provided path matches the patterns; otherwise <c>false</c>.</returns>
        internal bool Test(string relativePath) {
            if (String.IsNullOrEmpty(relativePath))
                return false;

            relativePath = relativePath.Replace('\\', '/');

            return !_excludeMatchers.Any(m => m.IsMatch(relativePath)) &&
            _includeMatchers.Any(m => m.IsMatch(relativePath));
        }

    }

}