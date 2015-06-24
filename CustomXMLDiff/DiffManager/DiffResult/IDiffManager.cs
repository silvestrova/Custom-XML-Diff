using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CustomXMLDiff.DiffManager.DiffResult
{
    public interface IDiffManager
    {
        void ApplyDiff(XmlNode diffgramParent, XmlNode sourceParent, ref BaseDiffResultObjectList results);
        XmlDocument ApplyChanges(BaseDiffResultObjectList results, XmlNode originalDocument);
    }
}
