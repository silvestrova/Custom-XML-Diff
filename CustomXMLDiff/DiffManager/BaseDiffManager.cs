using CustomXMLDiff.DiffManager.DiffResult;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace CustomXMLDiff.DiffManager
{
    public class BaseDiffManager : IDiffManager
    {
        public void ApplyDiff(XmlNode diffgramParent, XmlNode sourceParent, ref BaseDiffResultObjectList results)
        {
            IEnumerator enumerator = diffgramParent.ChildNodes.GetEnumerator();
            XmlNode matchNode = sourceParent;
            while (enumerator.MoveNext())
            {
                XmlNode current = (XmlNode)enumerator.Current;
                if (current.NodeType != XmlNodeType.Comment)
                {
                    XmlElement element = enumerator.Current as XmlElement;
                    if (element == null)
                    {
                        continue;
                    }
                    if (element.NamespaceURI != "http://schemas.microsoft.com/xmltools/2002/xmldiff")
                    {
                        throw new Exception("Invalid element in diffgram.");
                    }
                    string attribute = element.GetAttribute("match");
                    if (attribute != string.Empty)
                    {
                        int index = 0;

                        if (int.TryParse(attribute, out index))
                        {
                            matchNode = sourceParent.ChildNodes[index - 1];
                        }
                    }


                    string localName = element.LocalName;
                    if (localName != null)
                    {
                        localName = string.IsInterned(localName);
                        if (localName == "node")
                        {
                            if (element.ChildNodes.Count > 0)
                            {
                                ApplyDiff(element, matchNode, ref results);
                            }
                        }
                        else
                        {
                            if (localName == "add")
                            {
                                results.OnAdd(element.FirstChild, FindXPath(matchNode), FindXPath(matchNode, true));

                                continue;
                            }
                            if (localName == "remove")
                            {
                                results.OnRemove(matchNode, FindXPath(matchNode), FindXPath(matchNode, true));
                                continue;
                            }
                            if (localName == "change")
                            {
                                results.OnChange(matchNode, element.FirstChild, FindXPath(matchNode), FindXPath(matchNode, true));
                                if (element.ChildNodes.Count > 0)
                                {
                                    ApplyDiff(element, matchNode, ref results);
                                }
                            }
                        }
                    }
                }
            }

        }
        static string FindXPath(XmlNode node, bool isFull=false)
        {
            StringBuilder builder = new StringBuilder();
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Attribute:
                        builder.Insert(0, "/@" + node.Name);
                        node = ((XmlAttribute)node).OwnerElement;
                        break;
                    case XmlNodeType.Element:
                        int index = FindElementIndex((XmlElement)node);
                        if (isFull)
                        {
                            string attribute = node.Attributes["name"]==null?null: node.Attributes["name"].Value;
                            attribute = string.IsNullOrEmpty(attribute) && node.Attributes["type"] != null?node.Attributes["type"].Value:attribute;
                            string insert =string.IsNullOrEmpty(attribute)? "/" + node.Name + "[" + index + "]" : "/" + node.Name + "[" + index + "|" + attribute + "]";
                            builder.Insert(0, insert);
                        }
                        else
                        {
                            builder.Insert(0, "/" + node.Name + "[" + index + "]");
                        }
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Document:
                        return builder.ToString();
                    case XmlNodeType.Comment:
                        builder.Insert(0, "/comment");
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Text:
                        builder.Insert(0, "/value: " + node.Value);
                        node = node.ParentNode;
                        break;
                    default:
                        throw new ArgumentException("Only elements and attributes are supported");
                }
            }
            throw new ArgumentException("Node was not in a document");
        }
        static int FindElementIndex(XmlElement element)
        {
            XmlNode parentNode = element.ParentNode;
            if (parentNode is XmlDocument)
            {
                return 1;
            }
            XmlElement parent = (XmlElement)parentNode;
            int index = 1;
            foreach (XmlNode candidate in parent.ChildNodes)
            {
                if (candidate is XmlElement && candidate.Name == element.Name)
                {
                    if (candidate == element)
                    {
                        return index;
                    }
                    index++;
                }
            }
            throw new ArgumentException("Couldn't find element within parent");
        }



        public XmlDocument ApplyChanges(BaseDiffResultObjectList results, XmlNode originalDocument)
        {
            XmlDocument resultDocument = new XmlDocument();
            XPathNavigator navigator = originalDocument.CreateNavigator();
            foreach (var item in results.Items)
            {
                GetCommonNode(item, ref navigator);
            }
            resultDocument.LoadXml(navigator.OuterXml);
            return resultDocument;
        }

        private void GetCommonNode(BaseDiffResultObject item, ref XPathNavigator navigator)
        {
            
            switch(item.State)
            {
                case DiffState.Added:
                    {
                       var nav = navigator.SelectSingleNode(item.XPath);
                       if (nav != null)
                       {
                           XmlElement el = (XmlElement)item.ChangedNode;
                           el.SetAttribute("patchState", "Added");
                           if (item.XPath.Substring(item.XPath.LastIndexOf("/")).Contains(item.ChangedNode.Name))
                               nav.InsertAfter(el.OuterXml);
                           else
                           nav.AppendChild(el.OuterXml);
                       }
                        break;
                    }
                case DiffState.Changed:
                    {
                       var nav =  navigator.SelectSingleNode(item.XPath);
                       if (nav != null)
                       {
                           XmlElement el = (XmlElement)item.OriginalNode;
                           el.SetAttribute("patchState", "Changed");
                           el.SetAttribute("changedValue", item.ChangedNode.OuterXml);
                           nav.ReplaceSelf(el.OuterXml);
                       }
                        break;
                    }
                case DiffState.Removed:
                    {
                        var nav = navigator.SelectSingleNode(item.XPath);
                        if (nav != null)
                        {
                            XmlElement el = (XmlElement)item.OriginalNode;
                            el.SetAttribute("patchState", "Removed");
                            nav.ReplaceSelf(el.OuterXml);
                        }
                        break;
                    }
        }
        }
    }
}
