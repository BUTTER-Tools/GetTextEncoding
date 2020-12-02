using PluginContracts;
using System.Drawing;
using System.Windows.Forms;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using OutputHelperLib;
using System.Linq;
using Ude;

namespace GetTextEncoding
{
    public class GetTextEncoding : InputPlugin
    {

        public string[] InputType { get; } = {"N/A (Top-Level Plugin)"};
        public string OutputType { get; } = "OutputArray";

        public bool KeepStreamOpen { get; } = false;
        public StreamReader InputStream { get; set; }
        public string IncomingTextLocation { get; set; } = "";
        private bool ScanSubfolders = false;
        private string FileExtension = "*.txt";
        public string SelectedEncoding { get; set; } = "utf-8";
        public bool InheritHeader { get; } = false;
        public bool TopLevel { get; } = true;

        public Dictionary<int, string> OutputHeaderData { get; set; } = new Dictionary<int, string>(){
                                                                                            {0, "FullPath"},
                                                                                            {1, "Created"},
                                                                                            {2, "FileSizeKB" },
                                                                                            {3, "Encoding" },
                                                                                            {4, "EncodingConfidence" }
                                                                                        };
        public int TextCount { get; set; }

        #region IPlugin Details and Info

        public string PluginName { get; } = "Diagnostic: Get File Encodings";
        public string PluginType { get; } = "Load File(s)";
        public string PluginVersion { get; } = "1.0.3";
        public string PluginAuthor { get; } = "Ryan L. Boyd (ryan@ryanboyd.io)";
        public string PluginDescription { get; } = "This plugin will read files and try to determine their encoding (e.g., win-1252, UTF-8, etc.). This pluging should be at the top level of a tree in your Analysis Pipeline, and should be immediately followed by a CSV output writer. For example:" + Environment.NewLine + Environment.NewLine + Environment.NewLine +
            "\tDiagnostic: Get File Encodings" + Environment.NewLine +
            "\t |" + Environment.NewLine +
            "\t |-- Save Output to CSV";
        public string PluginTutorial { get; } = "https://youtu.be/2H4Dc6woJF8";


        public Icon GetPluginIcon
        {
            get
            {
                return Properties.Resources.icon;
            }
        }

        #endregion

        #region Settings and ChangeSettings() Method

        public void ChangeSettings()
        {



            using (var form = new SettingsForm_GetTextEncoding(IncomingTextLocation, ScanSubfolders, FileExtension))
            {


                form.Icon = Properties.Resources.icon;

                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    FileExtension = form.FileExtension;
                    IncomingTextLocation = form.TextFileDirectory;
                    ScanSubfolders = form.ScanSubfolders;
                }
            }



        }
        #endregion


        public Payload RunPlugin(Payload Incoming)
        {

            Payload pData = new Payload();

            string fileName = Incoming.ObjectList[0].ToString();
            pData.FileID = Path.GetFileName(fileName);
            pData.SegmentID = Incoming.SegmentID;

            try
            {

                FileInfo oFileInfo = new FileInfo(fileName);
                string FileEncodingDetected = null;
                float encodingConf = 0.0f;

                //old way using simple helpers
                //SimpleHelpers.FileEncoding.DetectFileEncoding(fileName).BodyName;

                using (FileStream fs = File.OpenRead(fileName))
                {
                    Ude.CharsetDetector cdet = new Ude.CharsetDetector();
                    cdet.Feed(fs);
                    cdet.DataEnd();
                    if (cdet.Charset != null)
                    {
                        FileEncodingDetected = cdet.Charset;
                        encodingConf = cdet.Confidence;
                        //Console.WriteLine("Charset: {0}, confidence: {1}",
                        //     cdet.Charset, cdet.Confidence);
                    }
                    
                }

                string DetectedEncodingString = "[UNKNOWN]";

                pData.SegmentNumber.Add(1);

                if (FileEncodingDetected != null)
                {
                    DetectedEncodingString = FileEncodingDetected;
                }
                pData.StringArrayList.Add(new string[5] { fileName,
                                                         oFileInfo.CreationTime.ToString(),
                                                         (oFileInfo.Length / 1024.0).ToString("#.##"),
                                                         DetectedEncodingString,
                                                         encodingConf.ToString()
                                                        }); ;
            }
            catch
            {
                pData.StringArrayList = new List<string[]>();
                pData.SegmentNumber.Add(1);
            }

            return (pData);


        }



        public IEnumerable TextEnumeration()
        {
            //for this plugin, all that we're really doing is setting the IEnumerable full of the text files
            SearchOption FolderDepth = new SearchOption();

            if (ScanSubfolders)
            {
                FolderDepth = SearchOption.AllDirectories;
            }
            else
            {
                FolderDepth = SearchOption.TopDirectoryOnly;
            }

            if (!string.IsNullOrEmpty(IncomingTextLocation))
            {
                return (Directory.EnumerateFiles(IncomingTextLocation, FileExtension, FolderDepth));
            }
            else
            {
                return (Enumerable.Empty<string>());
            }
            
        }

        public void Initialize()
        {
            TextCount = 0;

            SearchOption FolderDepth = new SearchOption();
            if (ScanSubfolders)
            {
                FolderDepth = SearchOption.AllDirectories;
            }
            else
            {
                FolderDepth = SearchOption.TopDirectoryOnly;
            }

            var files = Directory.EnumerateFiles(IncomingTextLocation, FileExtension, FolderDepth);



            foreach (string filecount in files)
            {
                TextCount++;
            }
        }


        public bool InspectSettings()
        {
            if (!String.IsNullOrEmpty(IncomingTextLocation) && !String.IsNullOrEmpty(FileExtension))
            { 
                return true;
            }
            else
            {
                return false;
            }
        }

        public Payload FinishUp(Payload Input)
        {
            return (Input);
        }


        #region Import/Export Settings
        public void ImportSettings(Dictionary<string, string> SettingsDict)
        {
            FileExtension = SettingsDict["FileExtension"];
            IncomingTextLocation = SettingsDict["IncomingTextLocation"];
            ScanSubfolders = Boolean.Parse(SettingsDict["ScanSubfolders"]);

        }

        public Dictionary<string, string> ExportSettings(bool suppressWarnings)
        {
            Dictionary<string, string> SettingsDict = new Dictionary<string, string>();
            SettingsDict.Add("FileExtension", FileExtension);
            SettingsDict.Add("IncomingTextLocation", IncomingTextLocation);
            SettingsDict.Add("ScanSubfolders", ScanSubfolders.ToString());
            return (SettingsDict);
        }
        #endregion

    }

}
