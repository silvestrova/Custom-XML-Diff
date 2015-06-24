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

        public BaseDiffResultObjectList DoCompare(string file1Path, string file2Path)
        {
            return  DoCompare(file1Path, file2Path, _tempFilePath, XmlDiffOptions.None);
        }  
        public BaseDiffResultObjectList DoCompare(string file1Path, string file2Path, string outputFilePath)
        {
            return DoCompare(file1Path, file2Path, outputFilePath, XmlDiffOptions.None);
        }

        public BaseDiffResultObjectList DoCompare(string file1Path, string file2Path, XmlDiffOptions options)
        {
            return DoCompare(file1Path, file2Path, _tempFilePath, options);
        }
        public BaseDiffResultObjectList DoCompare(string file1Path, string file2Path, string outputFilePath, XmlDiffOptions options)
        {
            Assert.StringIsNullOrEmpty(file1Path, "First File path is empty");
            Assert.StringIsNullOrEmpty(file2Path, "Second File path is empty");
            Assert.StringIsNullOrEmpty(outputFilePath, "Output File Path is empty");

            XmlTextReader file1Reader = new XmlTextReader(new StreamReader(file1Path));
            XmlTextReader file2Reader = new XmlTextReader(new StreamReader(file2Path));


            XmlDocument file1 = new XmlDocument();
            
            file1.Load(file1Reader);
            file1Reader.Close();

            XmlDocument file2 = new XmlDocument();
            file2.Load(file2Reader);
            return DoCompare(file1.DocumentElement, file2.DocumentElement, outputFilePath, options);
        }

        public BaseDiffResultObjectList DoCompare(XmlNode file1, XmlNode file2, XmlDiffOptions options)
        {
            return DoCompare(file1, file2, _tempFilePath, options);
        }
        public BaseDiffResultObjectList DoCompare(XmlNode file1, XmlNode file2, string outputFilePath, XmlDiffOptions options)
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
                isEqual = _diff.Compare(file1, file2, tw);
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
            Manager.ApplyDiff(diffgramDoc.DocumentElement.FirstChild, file1, ref results);
            return results;
        }

        private void SetDiffOptions(XmlDiffOptions options)
        {
            _diffOptions = options;
        }

    }
}
