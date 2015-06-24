using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CustomXMLDiff.DiffManager.DiffResult
{
    public class BaseDiffResultObject
    {
        public string XPath { get; set; }
        public string ExtendedXPath { get; set; }
        public XmlNode OriginalNode { get; set; }
        public string PatchSorceOriginalNode 
        { 
            get
            {
                if
                  (OriginalNode != null && OriginalNode.NodeType==XmlNodeType.Element&& OriginalNode.Attributes["patch:source"] != null)
                {
                    return OriginalNode.Attributes["patch:source"].Value;
                }
                else
                    return "unassigned";
        
        } }
        public string PatchSorceChangedNode
        {
            get
            {
                if
                  (OriginalNode != null && OriginalNode.NodeType == XmlNodeType.Element && ChangedNode.Attributes["patch:source"] != null)
                {
                    return OriginalNode.Attributes["patch:source"].Value;
                }
                else
                    return "unassigned";

            }
        }


        public XmlNode ChangedNode { get; set; }
        public DiffState State { get; set; }
    }
}
