using System;
using System.Xml;
using System.IO;
using StringExtensionLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace pv_tools
{
    class Program
    {

        const bool DEBUG = true;

        static string appPath = "";
        static string displayFolderRoot = "";

        #region COMMAND LINE INTERFACE
        static void Main(string[] args)
        {
            // Get the path of the application.
            appPath = System.IO.Directory.GetCurrentDirectory().ToString();

            if (args[0].Contains(",")){
                args = args[0].Split(",");
            }

            if (args.Length > 0)
            {
                Console.WriteLine(args[0]);
                switch (args[0].ToLower())
                {
                    case "gettouchcells":
                        displayFolderRoot = args[1];
                        if (args.Length > 2)
                            getTouchCells(args[1], args[2]);
                        else
                            getTouchCells(args[1]);
                        break;
                    case "getnavigation":
                        if (args.Length > 3)
                            getNavigation(args[1], args[2], args[3]);
                        else if (args.Length > 2)
                            getNavigation(args[1], args[2]);
                        else
                            getNavigation(args[1]);
                        break;
                    case "convertslctags":
                        convertSLCTags(args[1]);
                        break;
                    case "getdisplaysecuritycodes":
                        if (args.Length > 2)
                            getDisplaySecurityCodes(args[1], args[2]);
                        else
                            getDisplaySecurityCodes(args[1]);
                        break;
                    case "filesearchreplace":
                        if (args.Length > 4)
                            fileSearchReplace(args[1], args[2], args[3]=="regex", args[4]=="verbose");
                        else if (args.Length > 3)
                            fileSearchReplace(args[1], args[2], args[3]=="regex");
                        else if (args.Length > 2)
                            fileSearchReplace(args[1], args[2]);
                        break;
                    case "fileprefix":
                        if (args.Length > 2)
                            filePrefix(args[1], args[2]);
                        break;
                    default:
                        showHelp(args);
                        break;
                }
            }
        }

        #endregion COMMAND LINE INTERFACE

        #region COMMAND LINE COMMANDS

        /// <summary>
        /// getTouchCells is a recursive function that reports on the touchcell usage for each exported
        /// display in the display folder.  TouchCells include any pushbuttons and numeric input objects.
        ///
        /// If an output file argument is specified the function will create the output file and save the
        /// touchcell usage information in CSV format to the file.  Otherwise output will be directed to the
        /// console. NOTE: The specified output file will be deleted if it already exists.
        /// 
        /// The recurs argument is internal to the function and should not be assigned by the calling
        /// function.
        /// </summary>
        /// <param name="displayFolder"></param>
        /// <param name="outfile"></param>
        /// <param name="recurs"></param>
        static void getTouchCells(string displayFolder, string outfile = "", bool recurs = false)
        {
            string outfilepath = "";
            string sourcef = getFullPath(displayFolder, appPath);
            bool outToFile = outfile != "";
            string outText = "";

            if (outToFile) outfilepath = getFullPath(outfile, appPath);
            
            // Delete the output file on the first pass if it already exists.
            if (!recurs && outToFile && File.Exists(outfilepath)){
                File.Delete(outfilepath);
            }

            if(File.Exists(sourcef))
            {
                // Only process xml files.
                if (!sourcef.EndsWith(".xml")) return;
                
                outText = String.Format("Touchcells for display {0}:", getNamefromPath(sourcef));
                appendFile(outfilepath, outText, outToFile);

                
                //string xpath = @"//gotoButton|//logoutButton|//loginButton|//momentaryButton|//maintainedButton|//numericInputCursorPoint|//numericInputEnable|//interlockedButton|//acknowledgeAllAlarmsButton";
                // string xpath = @"//*[ends-with(local-name(), 'Button')]"; // XPath 2.0 syntax
                string xpath = @"//*[substring(local-name(), string-length(local-name()) - string-length('Button') + 1) = 'Button'] | ";
                xpath += @"//*[substring(local-name(), string-length(local-name()) - string-length('Key') + 1) = 'Key']|//numericInputCursorPoint|//numericInputEnable";

                XmlNodeList nodes = cXMLFunctions.GetXMLNodes(sourcef, xpath);
                int cellNum = 0;
                appendFile(outfilepath, 
                    "Touch Cell,Function Description,Security Level,Function Type,Range (min),Range (max)", 
                    outToFile
                );
                foreach (XmlNode node in nodes){
                    cellNum++;
                    // Initialize all the fields:
                    string description = "[no caption]";
                    string functionType = "Momentary Pushbutton";
                    string security = "D";
                    string rmin = "N/A";
                    string rmax = "N/A";
                    bool buttonFound = false;

                    description = removeLineFeeds(cXMLFunctions.GetXMLAttribute(node, ".//caption", "caption")).Trim();
                    if (description == "") description = "[no caption]";
                    
                    // gotoButton specifics
                    if (node.Name == "gotoButton"){
                        functionType = "Goto Display " + node.Attributes["display"].Value.ToString().Trim();
                        if (description == "[no caption]"){
                            description = node.Attributes["parameterFile"].Value.ToString().Trim();
                        }
                        string display = node.Attributes["display"].Value.ToString().Trim();
                        security = getScreenSecurity(displayFolderRoot, display);
                        security = security == "*" ? "D" : "Class " + security;
                        if (description == "[no caption]"){
                            description = node.Attributes["parameterFile"].Value.ToString().Trim();
                        }
                        buttonFound = true;
                    }
                    // closeButton specifics
                    if (node.Name == "closeButton"){
                        description = "Close Display Button";
                        buttonFound = true;
                    }
                    // numericInputCursorPoint specifics
                    if (node.Name == "numericInputCursorPoint"){
                        functionType = "Numeric Input";
                        rmin = node.Attributes["minValue"].Value.ToString();
                        rmax = node.Attributes["maxValue"].Value.ToString();
                        if (description == "[no caption]"){
                            description = node.Attributes["name"].Value.ToString().Trim();
                        }
                        buttonFound = true;
                    }
                    
                    // numericInputEnable specifics
                    if (node.Name == "numericInputEnable"){
                        functionType = "Numeric Input";
                        rmin = node.Attributes["minValue"].Value.ToString();
                        rmax = node.Attributes["maxValue"].Value.ToString();
                        if (description == "[no caption]"){
                            description = node.Attributes["name"].Value.ToString().Trim();
                        }
                        buttonFound = true;
                    }
                    
                    // momentaryButton specifics
                    if (node.Name == "momentaryButton"){
                        functionType = "Momentary Pushbutton";
                        string buttonName = node.Attributes["name"].Value.ToString().Trim();
                        XmlNodeList captions = cXMLFunctions.GetXMLNodes(
                            sourcef, String.Format("//momentaryButton[@name='{0}']//states//state//caption", buttonName)
                        );
                        description = "";
                        string lastcaption = "";
                        foreach (XmlNode caption in captions){
                            string text = removeLineFeeds(caption.Attributes["caption"].Value.ToString().Trim());
                            if (text != "Error" && text != "" && text != lastcaption){
                                if (description.Length > 0)
                                    description += " / ";
                                description += text;
                                lastcaption = text;
                            }
                        }
                        buttonFound = true;
                    }
                    
                    // maintainedButton specifics
                    if (node.Name == "maintainedButton"){
                        functionType = "Maintained Pushbutton";
                        string buttonName = node.Attributes["name"].Value.ToString().Trim();
                        XmlNodeList captions = cXMLFunctions.GetXMLNodes(
                            sourcef, String.Format("//maintainedButton[@name='{0}']//states//state//caption", buttonName)
                        );
                        description = "";
                        bool firstPass = true;
                        foreach (XmlNode caption in captions){
                            if (!firstPass) description += " / ";
                            string text = removeLineFeeds(caption.Attributes["caption"].Value.ToString().Trim());
                            if (text != "Error" && text != ""){
                                description += text;
                                firstPass = false;
                            }
                        }
                        buttonFound = true;
                    }

                    // interlockedButton specifics
                    if (node.Name == "interlockedButton"){
                        functionType = "Interlocked Pushbutton";
                        string buttonName = node.Attributes["name"].Value.ToString().Trim();
                        XmlNodeList captions = cXMLFunctions.GetXMLNodes(
                            sourcef, String.Format("//interlockedButton[@name='{0}']//states//state//caption", buttonName)
                        );
                        description = "";
                        bool firstPass = true;
                        foreach (XmlNode caption in captions){
                            if (!firstPass) description += " / ";
                            string text = removeLineFeeds(caption.Attributes["caption"].Value.ToString().Trim());
                            if (text != "Error" && text != ""){
                                description += text;
                                firstPass = false;
                            }
                        }
                        buttonFound = true;
                    }

                    // acknowledgeAllAlarmsButton specifics
                    if (node.Name == "acknowledgeAllAlarmsButton"){
                        functionType = "Acknowledge All Alarms Button";
                        string buttonName = node.Attributes["name"].Value.ToString().Trim();
                        XmlNode caption = cXMLFunctions.GetXMLNode(
                            sourcef, String.Format("//acknowledgeAllAlarmsButton[@name='{0}']//caption", buttonName)
                        );
                        description = removeLineFeeds(caption.Attributes["caption"].Value.ToString().Trim());
                        buttonFound = true;
                    }

                    // Catch and convert all unfound button names to description text.
                    if (!buttonFound){
                        functionType = convertToSpacedString(node.Name);
                        description = convertToSpacedString(node.Name);
                        string buttonName = node.Attributes["name"].Value.ToString().Trim();
                        
                        XmlNode caption = cXMLFunctions.GetXMLNode(
                            sourcef, String.Format("//{0}[@name='{1}']//caption", node.Name, buttonName)
                        );
                        if (caption != null) {
                            string captionVal = caption.Attributes["caption"].Value.ToString().Trim();
                            if (captionVal.Length > 0)
                            description = removeLineFeeds(captionVal);
                        }
                                                
                        buttonFound = true;
                    }

                    // check for security visibility
                    string visibility = cXMLFunctions.GetXMLAttribute(node, ".//animateVisibility", "expression");
                    if (visibility != ""){
                        visibility.Replace(",", "-");
                        visibility.Replace(";", "-");
                        security = visibility;
                    }
                    
                    outText = String.Format("{0},{1},{2},{3},{4},{5}", 
                        cellNum, description, security, functionType, rmin, rmax
                    );
                    appendFile(outfilepath, outText, outToFile);
                }
                
                appendFile(outfilepath, "", outToFile);
            }
            else if(Directory.Exists(sourcef))
            {
                string [] fileEntries = Directory.GetFiles(sourcef);
                Array.Sort(fileEntries);
                foreach(string fileName in fileEntries)
                    getTouchCells(fileName, outfile, true);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", sourcef);
            }
        }

        static void getDisplaySecurityCodes(string displayFolder, string outfile = ""){
            string sourcef = getFullPath(displayFolder, appPath);
            string securityCode = "*";
            string displayName = "";

            string outfilepath = "";
            bool outToFile = outfile != "";
            string outText = "";

            if (outToFile){
                outfilepath = getFullPath(outfile, appPath);
                // Delete the output file on the first pass if it already exists.
                if (File.Exists(outfilepath)){
                    File.Delete(outfilepath);
                }
            }
            
            outText = String.Format("Security codes required for displays.");
            appendFile(outfilepath, outText, outToFile);
            appendFile(outfilepath, "Display Name, Security Code", outToFile);
            if(Directory.Exists(sourcef)){
                string [] fileEntries = Directory.GetFiles(sourcef);
                foreach(string fileName in fileEntries){
                    securityCode = cXMLFunctions.GetXMLAttribute(fileName, "//displaySettings", "securityCode");
                    displayName = getNamefromPath(fileName).RemoveSuffix(".xml");
                    outText = String.Format("{0}, {1}", displayName, securityCode);
                    appendFile(outfilepath, outText, outToFile);
                } 
            }
            else{
                Console.WriteLine("{0} is not a valid file or directory.", sourcef);
            }
        }

        static string getScreenSecurity(string displayFolder, string displayName)
        {
            // This function is terribly inefficient, but whatever.  Fix it if you don't like it.
            string sourcef = getFullPath(displayFolder, appPath);
            string securityCode = "*";
            IList<string> displays = new List<string>();
            if (Directory.Exists(sourcef))
            {
                // Get the list of displays.
                IList<string> fileEntries = new List<string>(Directory.GetFiles(sourcef, "*.xml"));
                
                foreach (string fileName in fileEntries)
                {
                    securityCode = cXMLFunctions.GetXMLAttribute(fileName, "//displaySettings", "securityCode");
                    if (getNamefromPath(fileName).RemoveSuffix(".xml") == displayName){
                        return securityCode;
                    }
                }
                return securityCode;
            }
            return securityCode;
        }

        /// <summary>
        /// getNavigation generates a display navigation matrix in CSV format from the exported displays
        /// found in the specified display folder.  Output is directed to the console by default.  Specify
        /// an output file argument to save the data to a CSV file.
        /// </summary>
        /// <param name="displayFolder"></param>
        /// <param name="outfile"></param>
        static void getNavigation(string displayFolder, string displayType = "all", string outfile = "")
        {
            string outfilepath = "";
            string sourcef = getFullPath(displayFolder, appPath);
            bool outToFile = outfile != "";
            string outText = "";
            IList<string> displays = new List<string>();

            if (outToFile) outfilepath = getFullPath(outfile, appPath);

            // Delete the output file on the first pass if it already exists.
            if (outToFile && File.Exists(outfilepath)){
                File.Delete(outfilepath);
            }

            if (Directory.Exists(sourcef))
            {
                // Get the list of displays.
                //IList<string> fileEntries = new List<string>(Directory.GetFiles(sourcef, "*.xml").OrderBy(x => x));
                List<string> fileEntries = new List<string>(Directory.GetFiles(sourcef, "*.xml"));
                fileEntries = numericSortList(fileEntries.ToList());
                
                // Generate two separate lists of screens and popups.
                List<string> fileEntries_Screens = new List<string>();
                List<string> fileEntries_Popups = new List<string>();
                
                foreach (string fileName in fileEntries)
                {
                    bool isPopup = cXMLFunctions.GetXMLAttribute(fileName, "//displaySettings", "displayType") == "onTop";
                    if (isPopup)
                        fileEntries_Popups.Add(fileName);
                    else
                        fileEntries_Screens.Add(fileName);
                }
                if (displayType.ToLower() == "all")
                    fileEntries = fileEntries_Screens.Concat(fileEntries_Popups).ToList();
                else if (displayType.ToLower() == "screens")
                    fileEntries = fileEntries_Screens.ToList();
                else if (displayType.ToLower() == "popups")
                    fileEntries = fileEntries_Popups.ToList();
                else
                    throw new ArgumentOutOfRangeException(
                        "Invalid displayType.  Must be one of - (all, screens, popups)."
                    );
                
                foreach(string fileName in fileEntries){
                    displays.Add(getNamefromPath(fileName).RemoveSuffix(".xml").Replace(",", "-"));
                }
                int numDisplays = displays.Count;
                // Output the column headers.
                outText = " ,Navigate To Screen\nCurrent Screen,";
                int displayIndex = 0;
                foreach (string fileName in fileEntries)
                {
                    bool isPopup = cXMLFunctions.GetXMLAttribute(fileName, "//displaySettings", "displayType") == "onTop";
                    // If this is a popup, copy it to the end of the list.
                    outText += (isPopup)? displays[displayIndex] + " (PU) ," : displays[displayIndex] + ",";
                    displayIndex++;
                }
                appendFile(outfilepath, outText, outToFile);

                // Fill in the rest of the table.
                displayIndex = 0;
                foreach (string fileName in fileEntries)
                {
                    bool isPopup = cXMLFunctions.GetXMLAttribute(fileName, "//displaySettings", "displayType") == "onTop";
                    outText = (isPopup)? displays[displayIndex] + " (PU) ," : displays[displayIndex] + ",";
                    foreach (string display in displays)
                    {
                        //string xpath = String.Format("//gotoButton[lower-case(@display)='{0}']", display.ToLower());  // XPATH 2.0 Syntax
                        string xpath = String.Format("//gotoButton[translate(@display, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='{0}']", display.ToLower());
                        XmlNodeList nodes = cXMLFunctions.GetXMLNodes(fileName, xpath);
                        outText += (nodes.Count > 0) ? "X," : " ,";
                    }
                    appendFile(outfilepath, outText, outToFile);
                    displayIndex++;
                }
            }
            else
            {
                Console.WriteLine("{0} is not a valid directory.", sourcef);
            }
        }

        static void convertSLCTags(string displayFolder){
            string sourcef = getFullPath(displayFolder, appPath);
            
            if(File.Exists(sourcef))
            {
                string fileContents = File.ReadAllText(sourcef);
                string pattern, replace, newFileContents;
                
                // Find the ::[PLC] that sometimes appears in the address and replace with just [PLC].
                pattern = @"::\[";
                replace = "[";
                newFileContents = Regex.Replace(fileContents, pattern, replace);

                // Find N10:12/2 and replace with N10[12].2.
                pattern = @"([A-Za-z])(\d+):(\d+)/(\d+)";
                replace = @"$1$2[$3].$4";
                newFileContents = Regex.Replace(newFileContents, pattern, replace);

                // Find N10:12 and replace with N10[12].
                pattern = @"([A-Za-z])(\d+):(\d+)";
                replace = @"$1$2[$3]";
                newFileContents = Regex.Replace(newFileContents, pattern, replace);

                // Find B3/# and replace with B3:[TRUNC(#/16)].((#-TRUNC(#/16))*16).
                pattern = @"([A-Za-z])(\d+)/(\d+)";
                replace = @"$1$2[$3]";
                newFileContents = Regex.Replace(newFileContents, pattern, 
                    m => string.Format(
                        "{0}{1}[{2}].{3}",
                        m.Groups[1].Value,
                        m.Groups[2].Value,
                        Math.Truncate(m.Groups[3].Value.ToInt32()/16.0),
                        ((m.Groups[3].Value.ToInt32()/16.0) - 
                            Math.Truncate(m.Groups[3].Value.ToInt32()/16.0))*16
                    ));
                File.WriteAllText(sourcef, newFileContents);
            }
            else if(Directory.Exists(sourcef))
            {
                string [] fileEntries = Directory.GetFiles(sourcef);
                foreach(string fileName in fileEntries)
                    convertSLCTags(fileName);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", sourcef);
            }
        }

        static void fileSearchReplace(string srFilename, string folderFile, bool useRegex = false, bool verbose = false)
        {
            string sourcef = getFullPath(folderFile, appPath);
            string replacef = getFullPath(srFilename, appPath);

            if (!File.Exists(replacef))
            {
                Console.WriteLine("{0} is not a valid file or directory.", replacef);
                return;
            }

            if (File.Exists(sourcef))
            {
                string fileContents = File.ReadAllText(sourcef);
                string[] replaceContents = File.ReadAllLines(replacef);
                string newFileContents = fileContents;

                foreach (string replaceLine in replaceContents)
                {
                    string find = replaceLine.Split(",")[0].Trim();
                    string replace = replaceLine.Split(",")[1].Trim();

                    if (verbose)
                    {
                        Console.WriteLine("Replacing {0} with {1} in file {2}.", find, replace, sourcef);
                    }

                    if (find.Length > 0) // && replace.Length > 0) - If the replace string is empty the find strings will simply be removed.
                    {
                        if (useRegex)
                        {
                            Regex regex = new Regex(find);
                            newFileContents = regex.Replace(newFileContents, replace);
                        }
                        else
                        {
                            newFileContents = newFileContents.Replace(find, replace);
                        }
                    }
                }

                File.WriteAllText(sourcef, newFileContents);
            }
            else if (Directory.Exists(sourcef))
            {
                if (verbose)
                {
                    if (useRegex)
                    {
                        Console.WriteLine("Using regular expressions.");
                    }
                    else
                    {
                        Console.WriteLine("Using standard text search and replace.");
                    }
                }
                string[] fileEntries = Directory.GetFiles(sourcef);
                foreach (string fileName in fileEntries)
                    fileSearchReplace(srFilename, fileName, useRegex, verbose);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", sourcef);
            }
        }

        static void filePrefix(string folderName, string prefix)
        {
            string sourcefolder = getFullPath(folderName, appPath);

            if (!Directory.Exists(sourcefolder))
            {
                Console.WriteLine("{0} is not a valid directory.", sourcefolder);
                return;
            }
            else
            {
                string[] fileEntries = Directory.GetFiles(sourcefolder);
                    foreach (string fileName in fileEntries)
                    {
                        string newName = getPathfromName(fileName) + prefix + getNamefromPath(fileName);
                        Console.WriteLine("Renaming {0} to {1}",fileName, newName);
                        FileInfo f = new FileInfo(fileName);
                        if (f.Exists)
                        {
                            f.MoveTo(newName);
                        }
                    }
            }
        }
        static void showHelp(string[] args)
        {
            string cliResponse = File.ReadAllText(string.Format("{0}/{1}", appPath, "CLIHelp.txt"));
            Console.WriteLine(cliResponse);
        }

        #endregion COMMAND LINE COMMANDS

        #region HELPER FUNCTIONS

        static List<string> numericSortList(List<string> source)
        {
            // Prep the list of filenames.
            // Get the path from the filenames.
            string path = getPathfromName(source[0]);
            // Create a new list that has the paths removed.
            var nameList = source.Select(x => x.Replace(path,"")).ToList();

            var numericList = nameList.Where(s => char.IsDigit(s[0]))
                                    .OrderBy(s => Convert.ToInt32(s.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)[0]))
                                    .ToList();
            
            //numericList.ForEach(x => Console.WriteLine(x));

            var alphaList = nameList.Where(s => !char.IsDigit(s[0]))
                                .OrderBy(s => s)
                                .ToList();
            //alphaList.ForEach(x => Console.WriteLine(x));

            
            List<string> sortedList = numericList.Concat(alphaList).ToList();
            sortedList = sortedList.Select(x => string.Concat(path, x)).ToList();

            return sortedList;
        }

        static string removeLineFeeds(string text)
        {
            text = text.Replace("\n", " ");
            return text;
        }
        static string getFullPath(string path, string appPath)
        {
            string fullPath = path;
            // Check for full path in filename.
            if ((path.Left(1) != "/") && (path.Substring(1).Left(1) != ":")) 
            {
                fullPath = appPath + "/" + path;
            }
            
            // Remove the trailing /.
            fullPath.TrimEnd('/');

            return fullPath;
        }

        static string getNamefromPath(string fName){
            string[] pathNames = fName.Split("/");
            return pathNames[pathNames.Length-1];
        }

        static string getPathfromName(string fName){
            string[] pathNames = fName.Split("/");
            pathNames = pathNames[..^1];
            return String.Join("/", pathNames) + "/";
        }

        static void appendFile(string fName, string text, bool outToFile = true){
            if (outToFile){
                // Create the output file if it does not exist.
                if (!File.Exists(fName)){
                    using (StreamWriter sw = File.CreateText(fName))
                    {
                        sw.WriteLine(text);
                    }
                }
                else {
                    using (StreamWriter sw = File.AppendText(fName))
                    {
                        sw.WriteLine(text);
                    }
                }
            }
            else {
                Console.WriteLine(text);
            }
            return;
        }

        static IList<string> ConcatIList(IList<string> list1, IList<string> list2){
            IList<string> retList = new List<string>();
            foreach(string l1 in list1)
                retList.Add(l1);
            foreach(string l2 in list2)
                retList.Add(l2);
            return retList;
        }

        public static string convertToSpacedString(string input) 
        // Developed by chatGpt using the prompt:
        // Please provide a C# function that takes a string input like this "silenceAlarmsButton" and converts it to this "Silence Alarms Button".
        {
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder result = new StringBuilder();
            result.Append(Char.ToUpper(input[0]));

            for (int i = 1; i < input.Length; i++)
            {
                if (Char.IsUpper(input[i]))
                {
                    // Insert a space before the uppercase letter if the previous character is not a space
                    // and it's not uppercase (to handle acronyms properly)
                    if (input[i - 1] != ' ' && !Char.IsUpper(input[i - 1]))
                    {
                        result.Append(' ');
                    }
                }
                result.Append(input[i]);
            }

            // Capitalize the first character of each word
            string finalResult = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result.ToString().ToLower());

            return finalResult;
        }


        #endregion HELPER FUNCTIONS
    }
}

