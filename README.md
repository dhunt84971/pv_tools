# pv_tools

Command line tool for interrogating the exported displays from a PanelView application.

## Typical Usage:
```dotnet run <command> [<additional arguments>]```

## Commands:
    getTouchCells <display folder> [<output file>]
    getNavigation <display folder> [<display type> = all] [<output file>]
    convertSLCTags <display folder>
    fileSearchReplace <replace file> <folder> [verbose]

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

## FILESEARCHREPLACE -
This command reads a file that defines items to search and replace in a list of comma separated values called the **replace file** and performs the multiple search and replacements on all the files in the specified **folder**.  The strings are converted in-place, so be sure to make a backup of the file(s) before running this command on the target folder.  All files in the target folder will be converted.

### Usage:
    dotnet run fileSearchReplace <replace file> <display folder>

### Examples:
    dotnet run fileSearchReplace replace.csv "/home/dave/AppendOC/Jobs/2022/ES2232 - ACI - B38 Process Glycol/HMI"


