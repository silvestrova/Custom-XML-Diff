using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CustomXMLDiff.DiffManager.DiffResult
{
   public class BaseDiffResultObjectList
    {
       private List<BaseDiffResultObject> _items = new List<BaseDiffResultObject>();
        public List<BaseDiffResultObject> Items
       {
           get
           {
               return _items;

           }
       }
        public virtual void OnAdd(XmlNode newElement, string xPath, string fullxPath)
       {
           Items.Add(new BaseDiffResultObject{XPath = xPath, ChangedNode = newElement, State=DiffState.Added, ExtendedXPath=fullxPath});
       }
        public virtual void OnChange(XmlNode oldElement, XmlNode newElement, string xPath, string fullxPath)
        {
            Items.Add(new BaseDiffResultObject { XPath = xPath, ChangedNode = newElement, OriginalNode = oldElement, State = DiffState.Changed, ExtendedXPath = fullxPath});
        }
        public virtual void OnRemove(XmlNode removedElement, string xPath, string fullxPath)
        {
            Items.Add(new BaseDiffResultObject { XPath = xPath, OriginalNode = removedElement, State = DiffState.Removed, ExtendedXPath = fullxPath });
        }
    }
}
