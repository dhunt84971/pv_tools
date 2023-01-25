# pv_tools

Command line tool for interrogating the exported displays from a PanelView application.

## Typical Usage:
```dotnet run <command> [<additional arguments>]```

## Commands:
    getTouchCells <display folder> [<output file>]
    getNavigation <display folder> [<display type> = all] [<output file>]
    convertSLCTags <display folder>

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
    dotnet run getNavigation "/home/dave/AppendOC/Jobs/2022/ES2232 - ACI - B38 Process Glycol/HMI" all navigation.csv
NOTE: The display type must be specified if an output file argument is to be included.

## CONVERTSLCTAGS -
This command converts the SLC tag names found in the display XML files to matching Logix PLC tag names.  This is useful for transitioning an existing Panelview or FTView SE application after the target SLC has been upgraded to a Logix PLC using the converter.  The tags are converted in-place, so be sure to make a backup of the exported display xml files before running this command on the target folder.  All files in the target folder will be converted.

### Usage:
    dotnet run convertSLCTags <display folder>

### Examples:
    dotnet run convertSLCTags "/home/dave/AppendOC/Jobs/2022/ES2232 - ACI - B38 Process Glycol/HMI"


