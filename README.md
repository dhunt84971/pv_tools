# pv_tools

Command line tool for interrogating the exported displays from a PanelView application.

## Typical Usage:
```dotnet run <command> [<additional arguments>]```

## Commands:
    getTouchCells <display folder> [<output file>]
    getNavigation <display folder> [<display type> = all] [<output file>]

## GETTOUCHCELLS -
This command returns a list of touchcell usage for each display.

### Usage:
    dotnet run getTouchCells <display folder> [<output file>]

### Examples:
    dotnet run getTouchCells "/home/dave/AppendOC/Jobs/2022/ES2232 - ACI - B38 Process Glycol/HMI"
    dotnet run getTouchCells "/home/dave/AppendOC/Jobs/2022/ES2232 - ACI - B38 Process Glycol/HMI" outfile.csv

## GETNAVIGATION -
This command returns the navigation matrix for the exported displays.

### Usage:
    dotnet run getNavigation <display folder> [<display type> = all] [<output file>]

### Examples:
    dotnet run getNavigation "/home/dave/AppendOC/Jobs/2022/ES2232 - ACI - B38 Process Glycol/HMI"
    dotnet run getNavigation "/home/dave/AppendOC/Jobs/2022/ES2232 - ACI - B38 Process Glycol/HMI" screens
    dotnet run getNavigation "/home/dave/AppendOC/Jobs/2022/ES2232 - ACI - B38 Process Glycol/HMI" popups
    dotnet run getNavigation "/home/dave/AppendOC/Jobs/2022/ES2232 - ACI - B38 Process Glycol/HMI" all navigation.csvnavigation.csv
NOTE: The display type must be specified if an output file argument is to be included.

