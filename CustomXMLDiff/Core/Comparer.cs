using CustomXMLDiff.Core.Diagnostics;
using CustomXMLDiff.DiffManager.DiffResult;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.XmlDiffPatch;
using System.Xml;
using CustomXMLDiff.DiffManager;

namespace CustomXMLDiff
{
    public class Comparer
    {
        
        private static List<BaseDiffResultObject> _results;
        XmlDiff _diff;
        XmlDiffOptions _diffOptions;
        string _tempFilePath;
        
        public IDiffManager Manager { get; set; } 

        public Comparer()
        {
            _results = new List<BaseDiffResultObject>();
            _diff = new XmlDiff();
            _diffOptions = new XmlDiffOptions();
            _tempFilePath = Path.GetTempFileName();
            Manager = new BaseDiffManager();

        }
        public Comparer(IDiffManager manager)
        {
            _results = new List<BaseDiffResultObject>();
            _diff = new XmlDiff();
            _diffOptions = new XmlDiffOptions();
            _tempFilePath = Path.GetTempFileName();
            Manager = manager;

        }
        ~Comparer()
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }

        public BaseDiffResultObjectList DoCompare(string originalDocumentPath, string changedDocumentPath)
        {
            return  DoCompare(originalDocumentPath, changedDocumentPath, _tempFilePath, XmlDiffOptions.None);
        }  
        public BaseDiffResultObjectList DoCompare(string originalDocumentPath, string changedDocumentPath, string outputFilePath)
        {
            return DoCompare(originalDocumentPath, changedDocumentPath, outputFilePath, XmlDiffOptions.None);
        }

        public BaseDiffResultObjectList DoCompare(string originalDocumentPath, string changedDocumentPath, XmlDiffOptions options)
        {
            return DoCompare(originalDocumentPath, changedDocumentPath, _tempFilePath, options);
        }
        public BaseDiffResultObjectList DoCompare(string originalDocumentPath, string changedDocumentPath, string outputFilePath, XmlDiffOptions options)
        {
            Assert.StringIsNullOrEmpty(originalDocumentPath, "First File path is empty");
            Assert.StringIsNullOrEmpty(changedDocumentPath, "Second File path is empty");
            Assert.StringIsNullOrEmpty(outputFilePath, "Output File Path is empty");

            XmlTextReader originalDocumentReader = new XmlTextReader(new StreamReader(originalDocumentPath));
            XmlTextReader changedDocumentReader = new XmlTextReader(new StreamReader(changedDocumentPath));


            XmlDocument originalDocument = new XmlDocument();
            
            originalDocument.Load(originalDocumentReader);
            originalDocumentReader.Close();

            XmlDocument changedDocument = new XmlDocument();
            changedDocument.Load(changedDocumentReader);
            return DoCompare(originalDocument.DocumentElement, changedDocument.DocumentElement, outputFilePath, options);
        }

        public BaseDiffResultObjectList DoCompare(XmlDocument originalDocument, XmlDocument changedDocument, string outputFilePath, XmlDiffOptions options)
        {
            return DoCompare(originalDocument.DocumentElement, changedDocument.DocumentElement, outputFilePath, options);
        }
        public BaseDiffResultObjectList DoCompare(XmlDocument originalDocument, XmlDocument changedDocument, XmlDiffOptions options)
        {
            return DoCompare(originalDocument.DocumentElement, changedDocument.DocumentElement, this._tempFilePath, options);
        }
        public BaseDiffResultObjectList DoCompare(XmlDocument originalDocument, XmlDocument changedDocument)
        {
            return DoCompare(originalDocument.DocumentElement, changedDocument.DocumentElement, this._tempFilePath,  XmlDiffOptions.None);
        }
        public BaseDiffResultObjectList DoCompare(XmlNode originalDocument, XmlNode changedDocument, XmlDiffOptions options)
        {
            return DoCompare(originalDocument, changedDocument, _tempFilePath, options);
        }
        public BaseDiffResultObjectList DoCompare(XmlNode originalDocument, XmlNode changedDocument, string outputFilePath, XmlDiffOptions options)
        {
            Assert.StringIsNullOrEmpty(outputFilePath, "Output File Path is empty");
            BaseDiffResultObjectList results = new BaseDiffResultObjectList();
            StreamWriter stream = new StreamWriter(outputFilePath, false);
            XmlTextWriter tw = new XmlTextWriter(stream);
            
            tw.Formatting = Formatting.Indented;
            SetDiffOptions(options);
            bool isEqual = false;
  
            try
            {
                isEqual = _diff.Compare(originalDocument, changedDocument, tw);
            }
            finally
            {
                tw.Close();
                stream.Close();
            }

            if (isEqual)
            {

                return results;
            }

            XmlTextReader diffGram = new XmlTextReader(outputFilePath);
            XmlDocument diffgramDoc = new XmlDocument();
            diffgramDoc.Load(diffGram);
            Manager.ApplyDiff(diffgramDoc.DocumentElement.FirstChild, originalDocument, ref results);
            return results;
        }
        public XmlDocument GetResultFile(BaseDiffResultObjectList changes, XmlNode originalNode)
        {
            Assert.ArgumentNotNull(originalNode, "Original Node can't be null");
            return Manager.ApplyChanges(changes, originalNode);
        }
        public XmlDocument GetResultFile(BaseDiffResultObjectList changes, string originalNodePath)
        {
            Assert.StringIsNullOrEmpty(originalNodePath, "Output File Path is empty");

            XmlTextReader originalDocumentReader = new XmlTextReader(new StreamReader(originalNodePath));
          
            XmlDocument originalDocument = new XmlDocument();

            originalDocument.Load(originalDocumentReader);
            originalDocumentReader.Close();

            return GetResultFile(changes, originalDocument);
        }
        public XmlDocument GetResultFile(BaseDiffResultObjectList changes, XmlDocument originalNode)
        {
            Assert.ArgumentNotNull(originalNode, "Original document can't be null");
            return GetResultFile(changes, originalNode.DocumentElement);
        }
        private void SetDiffOptions(XmlDiffOptions options)
        {
            _diffOptions = options;
        }

    }
}
