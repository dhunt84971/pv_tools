using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace pv_tools
{
    public static class cXMLFunctions
    {
        /// <summary>
        /// Deletes the specified node from the xml document.
        /// If node is null or not found no action is taken.
        /// </summary>
        /// <param name="removeNode"></param>
        public static void DeleteNode(XmlNode removeNode)
        {
            if (removeNode != null  && removeNode.ParentNode != null)
            {
                removeNode.ParentNode.RemoveChild(removeNode);
            }
        }

        public static void DeleteNode(XmlDocument docXML, string xpath)
        {
            if (docXML == null || docXML.DocumentElement == null) throw new ArgumentNullException(
                "XML document is null.", nameof(docXML)
            );

            XmlElement delElement = docXML.DocumentElement;

            if (delElement.SelectSingleNode(xpath) != null){
                XmlNode removeNode = delElement.SelectSingleNode(xpath);
                DeleteNode(removeNode);
            }
        }

        public static void DeleteNode(string filenamePLCProject, string xpath)
        {
            // Load the PLC project into an XML document.
            XmlDocument xmlPLCProject = new XmlDocument();
            xmlPLCProject.Load(filenamePLCProject);
            
            DeleteNode(xmlPLCProject, xpath);

            // Save the modified project.
            SaveFormattedXML(filenamePLCProject, xmlPLCProject);
        }

        public static XmlNode CreateNode(XmlDocument xmlPLCProject, string parentNode, string newNode)
        {
            // Load the XML PLC Project.
            XmlElement rootPLCProject = xmlPLCProject.DocumentElement;

            // Find the parent node where the new one is to be added.
            XmlNode xparent = rootPLCProject.SelectSingleNode(parentNode);

            // Create the new node, append it to the parent and return it.
            XmlElement xnewNode = xmlPLCProject.CreateElement(newNode);
            xparent.AppendChild(xnewNode);

            return xnewNode;
        }

        public static XmlNode CreateNodeAfter(XmlDocument xmlPLCProject, string parentNode, string prevPeerNode, string newNode)
        {
            // Load the XML PLC Project.
            XmlElement rootPLCProject = xmlPLCProject.DocumentElement;

            // Find the parent node where the new one is to be added.
            XmlNode xparent = rootPLCProject.SelectSingleNode(parentNode);
            XmlNode xprev = xparent.SelectSingleNode(prevPeerNode);

            // Create the new node for insertion.
            XmlElement xnewNode = xmlPLCProject.CreateElement(newNode);
            xparent.InsertAfter(xnewNode, xprev);

            return xnewNode;
        }

        public static XmlNode CreateNode(string filenamePLCProject, string parentNode, string newNode)
        {
            // Load the XML PLC Project.
            XmlDocument xmlPLCProject = new XmlDocument();
            xmlPLCProject.Load(filenamePLCProject);

            CreateNode(xmlPLCProject, parentNode, newNode);

            // Save the document to a file and auto-indent the output.
            SaveFormattedXML(filenamePLCProject, xmlPLCProject);
            
            return null;
        }

        public static XmlNode CreateNodeAfter(string filenamePLCProject, string parentNode, string prevPeerNode, string newNode)
        {
            // Load the XML PLC Project.
            XmlDocument xmlPLCProject = new XmlDocument();
            xmlPLCProject.Load(filenamePLCProject);

            CreateNodeAfter(xmlPLCProject, parentNode, prevPeerNode, newNode);

            // Save the document to a file and auto-indent the output.
            SaveFormattedXML(filenamePLCProject, xmlPLCProject);
            
            return null;
        }

        public static XmlNode CloneAttributes (XmlNode xSource, XmlNode xDestination)
        {
            XmlElement xeDest = (XmlElement) xDestination;
            
            foreach (XmlAttribute attr in xSource.Attributes){
                xeDest.SetAttribute(attr.Name, attr.Value);
            }
            return xDestination;
        }

        public static string GetSubString(string source, string startPattern, string endPattern)
        {
            string retValue;
            int startPos = source.IndexOf(startPattern);
            if (startPos == -1) return "";
            startPos += startPattern.Length;
            int endPos = source.IndexOf(endPattern, startPos);
            if (endPos == -1) return "";
            if (endPos <= startPos) return "";
            int len = endPos - startPos;
            if (len <= 0) return "";
            retValue = source.Substring(startPos, len);

            return retValue;
        }

        public static XmlNode GetXMLNode(XmlDocument xmlPLCProject, string xpath)
        {

            XmlElement rootPLCProject = xmlPLCProject.DocumentElement;
            XmlNode node;

            // Execute the search.
            node = rootPLCProject.SelectSingleNode(xpath);

            return node;

        }

        public static XmlNode GetXMLNode(string xmlFilename, string xpath)
        {

            // Load the XML document.
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilename);


            return GetXMLNode(xmlDoc, xpath);

        }

        public static XmlNodeList GetXMLNodes(XmlDocument xmlPLCProject, string xpath)
        {

            XmlElement rootPLCProject = xmlPLCProject.DocumentElement;
            XmlNodeList nodes;

            // Execute the search.
            nodes = rootPLCProject.SelectNodes(xpath);

            return nodes;

        }

        public static XmlNodeList GetXMLNodes(string xmlFilename, string xpath)
        {

            // Load the XML document.
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilename);

            return GetXMLNodes(xmlDoc, xpath);

        }

        public static bool NodeExists(XmlDocument xDoc, string xpath)
        {
            XmlNode retNode = GetXMLNode(xDoc, xpath);

            return (retNode != null);
        }

                /// <summary>
        /// Merge the child nodes from one XML doc to another.
        /// </summary>
        /// <param name="xmlTarg">The target XML document being updated.</param>
        /// <param name="xmlMerg">The XML document being merged into the target.</param>
        /// <param name="parentPath">The XQuery path to the parent node.</param>
        /// <param name="childNode">The name of the children nodes being merged.</param>
        /// <param name="uniqueAttr">The name of the attribute that will be used to determine if a child already exists in the target.
        /// Duplicates found within the merge XML document will not be added to the target XML document.</param>
        public static void MergeNodes(XmlDocument xmlTarg, XmlDocument xmlMerg, string parentPath, string childNode, string uniqueAttr, bool replace)
        {
            // Get the node in the target project.
            XmlNode targNode = cXMLFunctions.GetXMLNode(xmlTarg, parentPath);

            // Get the node in the merge project.
            XmlNodeList mergNodes = cXMLFunctions.GetXMLNodes(xmlMerg, parentPath + @"/" + childNode);
            // Loop through these and add the ones that do not already exist in the target.
            foreach (XmlNode mergNode in mergNodes)
            {
                if (!cXMLFunctions.NodeExists(xmlTarg, parentPath + @"/" + childNode + @"[@" + uniqueAttr + @"='" + mergNode.Attributes[uniqueAttr].Value + @"']"))
                {
                    targNode.InnerXml += mergNode.OuterXml;
                }
                else{
                    if (replace){
                        XmlNode childxmlNode = GetXMLNode(xmlTarg, parentPath + @"/" + childNode + @"[@" + uniqueAttr + @"='" + mergNode.Attributes[uniqueAttr].Value + @"']");
                        Console.WriteLine("Replacing {0}", mergNode.Attributes[uniqueAttr].Value);
                        targNode.RemoveChild(childxmlNode);
                        targNode.InnerXml += mergNode.OuterXml;
                    }
                }
            }
        }

        /// <summary>
        /// Merge the child nodes from one XML doc to another.
        /// </summary>
        /// <param name="xmlTarg">The target XML document being updated.</param>
        /// <param name="xmlMerg">The XML document being merged into the target.</param>
        /// <param name="parentPath">The XQuery path to the parent node.</param>
        /// <param name="childNode">The name of the children nodes being merged.</param>
        /// <param name="uniqueAttr">The name of the attribute that will be used to determine if a child already exists in the target.
        /// Duplicates found within the merge XML document will not be added to the target XML document.</param>
        public static void MergeNodes(XmlDocument xmlTarg, XmlDocument xmlMerg, string parentPath, string childNode, string uniqueAttr)
        {
            MergeNodes(xmlTarg, xmlMerg, parentPath, childNode, uniqueAttr, false);
        }

        /// <summary>
        /// Merge the child nodes from one XML doc to another.
        /// </summary>
        /// <param name="xmlTarg">The fully qualified target XML filename  being updated.</param>
        /// <param name="xmlMerg">The fully qualified XML filename being merged into the target.</param>
        /// <param name="parentPath">The XQuery path to the parent node.</param>
        /// <param name="childNode">The name of the children nodes being merged.</param>
        /// <param name="uniqueAttr">The name of the attribute that will be used to determine if a child already exists in the target.
        /// Duplicates found within the merge XML document will not be added to the target XML document.
        /// Changes will be saved in the target XML document.</param>
        public static void MergeNodes(string xmlTargfname, string xmlMergfname, string parentPath, string childNode, string uniqueAttr, bool replace)
        {
            // Merge the Tasks and the Scheduledprograms in the project XML file.
            // Load the target file into an XML document.
            XmlDocument xmlTarg = new XmlDocument();
            xmlTarg.Load(xmlTargfname);
            // Load the merge PLC project into an XML document.
            XmlDocument xmlMerg = new XmlDocument();
            xmlMerg.Load(xmlMergfname);

            MergeNodes(xmlTarg, xmlMerg, parentPath, childNode, uniqueAttr, replace);
            
            // Save the modified project.
            SaveFormattedXML(xmlTargfname, xmlTarg);
        }

        /// <summary>
        /// Merge the child nodes from one XML doc to another.
        /// </summary>
        /// <param name="xmlTarg">The fully qualified target XML filename  being updated.</param>
        /// <param name="xmlMerg">The fully qualified XML filename being merged into the target.</param>
        /// <param name="parentPath">The XQuery path to the parent node.</param>
        /// <param name="childNode">The name of the children nodes being merged.</param>
        /// <param name="uniqueAttr">The name of the attribute that will be used to determine if a child already exists in the target.
        /// Duplicates found within the merge XML document will not be added to the target XML document.
        /// Changes will be saved in the target XML document.</param>
        public static void MergeNodes(string xmlTargfname, string xmlMergfname, string parentPath, string childNode, string uniqueAttr)
        {
            MergeNodes(xmlTargfname, xmlMergfname, parentPath, childNode, uniqueAttr, false);
        }

        public static void SetXMLNodeAttr(string xmlTargfname, string parentPath, string attrName, string attrValue)
        {
            // Load the target file into an XML document.
            XmlDocument xmlTarg = new XmlDocument();
            xmlTarg.Load(xmlTargfname);

            SetXMLNodeAttr(xmlTarg, parentPath, attrName, attrValue);
            
            // Save the modified project.
            SaveFormattedXML(xmlTargfname, xmlTarg);
        }

        public static bool SetXMLNodeAttr(XmlDocument xmlTarg, string parentPath, string attrName, string attrValue)
        {            
            try{
                GetXMLNode(xmlTarg, parentPath).Attributes[attrName].Value = attrValue;
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
                return true;
            }
            return false;
        }

        public static string GetXMLAttribute(XmlNode node, string xpath, string attrName)
        {
            string retValue = "";
            try{
                retValue = node.SelectSingleNode(xpath).Attributes[attrName].Value.ToString();
            }
            catch{
                //Console.WriteLine(xpath, );
            }
            return retValue;
        }

        public static string GetXMLAttribute(string xmlTargfname, string xpath, string attrName)
        {
            string retValue = "";
            try{
                XmlDocument xmlTarg = new XmlDocument();
                xmlTarg.Load(xmlTargfname);
                retValue = xmlTarg.SelectSingleNode(xpath).Attributes[attrName].Value.ToString();
            }
            catch{
                //Console.WriteLine(xpath, );
                // Return an empty string if there is an error.
            }
            return retValue;
        }

        public static bool SaveFormattedXML(string plcFileName, XmlDocument xmlPLCProject)
        {
            try{
                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.NewLineChars = "\r\n";
                writerSettings.Indent = true;
                XmlWriter writer = XmlWriter.Create(plcFileName, writerSettings);
                xmlPLCProject.Save(writer);
                writer.Close();
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
                return true;
            }
            return false;
        }

        public static void ReplaceXmlNode(XmlDocument xmlPLCProject, XmlDocument xmlSource, string xpath)
        {
            XmlNode srcNode = cXMLFunctions.GetXMLNode(xmlSource, xpath);
            XmlNode tgtNode = cXMLFunctions.GetXMLNode(xmlPLCProject, xpath);
            tgtNode.InnerXml = srcNode.InnerXml;
        }

        public static void ReplaceXmlNode(XmlDocument xmlPLCProject, string xmlSourceFName, string xpath)
        {
            XmlDocument xmlSource = new XmlDocument();
            xmlSource.Load(xmlSourceFName);
            ReplaceXmlNode(xmlPLCProject, xmlSource, xpath);
        }

        public static void ReplaceXmlNode(string xmlPLCProjectFName, string xmlSourceFName, string xpath)
        {
            XmlDocument xmlPLCProject = new XmlDocument();
            xmlPLCProject.Load(xmlPLCProjectFName);
            
            XmlDocument xmlSource = new XmlDocument();
            xmlSource.Load(xmlSourceFName);
            ReplaceXmlNode(xmlPLCProject, xmlSource, xpath);

            // Because a filename was passed for the PLC project the updated XML doc will be saved.
            SaveFormattedXML(xmlPLCProjectFName, xmlPLCProject);
        }


    }
}
