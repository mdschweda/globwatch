# Globwatch

```
 ,——._.,——,—.
( @ ) ( @ )  \  [globwatch]
 `—´   `—´
```

Globwatch is a .NET Core file system watcher that runs commands on changes.

## Command line help

```
Usage: globwatch [arguments] [options]

Arguments:
  command  The command to run.

Options:
  -i|--include      The glob pattern or glob patterns, separated by '|', for paths to watch. The default value is '**/*.*'
  -x|--exclude      The glob pattern or glob patterns, separated by '|', for paths to exclude from watching.
  -e|--events       The events to watch: c - created, m - modified, d - deleted, r - renamed. The default value is 'cmdr'.
  -d|--dir          The base directory to watch. The default default is the current directory.
  -w|--workdir      The work directory of the executed command. The default default is the directory where the changed occured.
  -ic|--ignorecase  Specifies that the search is case insensitive.

Placeholders %path and %pathBefore are available inside your command argument.

Return values:
  0 - Execution aborted by user
  1 - Invalid arguments
  2 - Unexpected error

Examples:
  globwatch "cmd.exe /c echo %path"
  globwatch "minify %path" -i **/*.css|**/*.js -e cm -ic
```