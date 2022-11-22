# pv_tools

Command line tool for interrogating the exported displays from a PanelView application.

## Typical Usage:
```dotnet run <command> [<additional arguments>]```

## Commands:
    ```getTouchCells <display folder> [<output file>]```

## GETTOUCHCELLS -
This command returns a list of touchcell usage for each display.

### Usage:
```dotnet run getTouchCells <display folder> [<output file>]```

### Examples:
```dotnet run getTouchCells "/home/dave/AppendOC/Jobs/2022/ES2232 - ACI - B38 Process Glycol/HMI"```
```dotnet run getTouchCells "/home/dave/AppendOC/Jobs/2022/ES2232 - ACI - B38 Process Glycol/HMI" outfile.csv```


