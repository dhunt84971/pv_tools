﻿using System;
using System.Xml;
using System.IO;
using StringExtensionLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace pv_tools
{
    class Program
    {

        const bool DEBUG = true;

        static string appPath = "";

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

                
                string xpath = @"//gotoButton|//logoutButton|//loginButton|//momentaryButton|//maintainedButton|//numericInputCursorPoint";
                XmlNodeList nodes = cXMLFunctions.GetXMLNodes(sourcef, xpath);
                int cellNum = 0;
                appendFile(outfilepath, 
                    "Touch Cell, Function Description, Security Level, Function Type, Range (min), Range (max)", 
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

                    description = removeLineFeeds(cXMLFunctions.GetXMLAttribute(node, ".//caption", "caption")).Trim();
                    if (description == "") description = "[no caption]";
                    
                    // gotoButton specifics
                    if (node.Name == "gotoButton"){
                        functionType = "Goto Display " + node.Attributes["display"].Value.ToString().Trim();
                        if (description == "[no caption]"){
                            description = node.Attributes["parameterFile"].Value.ToString().Trim();
                        }
                    }
                    // loginButton specifics
                    if (node.Name == "loginButton"){
                        description = "Login";
                    }
                    // logoutButton specifics
                    if (node.Name == "logoutButton"){
                        description = "Logout";
                    }
                    // numericInputCursorPoint specifics
                    if (node.Name == "numericInputCursorPoint"){
                        functionType = "Numeric Input";
                        rmin = node.Attributes["minValue"].Value.ToString();
                        rmax = node.Attributes["maxValue"].Value.ToString();
                        if (description == "[no caption]"){
                            description = node.Attributes["name"].Value.ToString().Trim();
                        }
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
                    }

                    // check for security visibility
                    string visibility = cXMLFunctions.GetXMLAttribute(node, ".//animateVisibility", "expression");
                    if (visibility != ""){
                        visibility.Replace(",", "-");
                        security = visibility;
                    }
                    
                    outText = String.Format("{0}, {1}, {2}, {3}, {4}, {5}", 
                        cellNum, description, security, functionType, rmin, rmax
                    );
                    appendFile(outfilepath, outText, outToFile);
                }
                
                appendFile(outfilepath, "", outToFile);
            }
            else if(Directory.Exists(sourcef))
            {
                string [] fileEntries = Directory.GetFiles(sourcef);
                foreach(string fileName in fileEntries)
                    getTouchCells(fileName, outfile, true);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", sourcef);
            }
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
                IList<string> fileEntries = new List<string>(Directory.GetFiles(sourcef, "*.xml"));

                // Generate two separate lists of screens and popups.
                IList<string> fileEntries_Screens = new List<string>();
                IList<string> fileEntries_Popups = new List<string>();
                
                foreach (string fileName in fileEntries)
                {
                    bool isPopup = cXMLFunctions.GetXMLAttribute(fileName, "//displaySettings", "displayType") == "onTop";
                    if (isPopup)
                        fileEntries_Popups.Add(fileName);
                    else
                        fileEntries_Screens.Add(fileName);
                }
                if (displayType.ToLower() == "all")
                    fileEntries = ConcatIList(fileEntries_Screens, fileEntries_Popups);
                else if (displayType.ToLower() == "screens")
                    fileEntries = fileEntries_Screens.ToList();
                else if (displayType.ToLower() == "popups")
                    fileEntries = fileEntries_Popups.ToList();
                else
                    throw new ArgumentOutOfRangeException(
                        "Invalid displayType.  Must be all, screens or popups."
                    );

                foreach(string fileName in fileEntries){
                    displays.Add(getNamefromPath(fileName).RemoveSuffix(".xml"));
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
                        string xpath = String.Format("//gotoButton[@display='{0}']", display);
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

        static void showHelp(string[] args)
        {
            string cliResponse = File.ReadAllText(string.Format("{0}/{1}", appPath, "CLIHelp.txt"));
            Console.WriteLine(cliResponse);
        }

        #endregion COMMAND LINE COMMANDS

        #region HELPER FUNCTIONS

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

        #endregion HELPER FUNCTIONS
    }
}

