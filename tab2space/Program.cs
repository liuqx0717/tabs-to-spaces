using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace tab2space {

    [Serializable]
    [XmlRoot]
    public class _ProgramData {
        [XmlElement] public int MainWindowWidth;
        [XmlElement] public int MainWindowHeight;
        [XmlElement] public int DefaultTabWidth;
        [XmlElement] public bool MainWindowMaximized;
        [XmlElement] public bool Wordwrap;
        [XmlElement] public string FontFamilyName;
        [XmlElement] public float FontSize;
        [XmlElement] public int ForeColor;
        [XmlElement] [XmlIgnore] public bool ForeColorSpecified;
        [XmlElement] public int BackColor;
        [XmlElement] [XmlIgnore] public bool BackColorSpecified;

        public _ProgramData()
        {
            MainWindowWidth = 0;
            MainWindowHeight = 0;
            DefaultTabWidth = 0;
            MainWindowMaximized = false;
            Wordwrap = false;
            FontFamilyName = null;
            FontSize = 0;
            ForeColor = 0;
            ForeColorSpecified = false;
            BackColor = 0;
            BackColorSpecified = false;
        }
    }






    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 


        private static string ProgramDataFile;
        public static _ProgramData ProgramData;
        private static XmlSerializer serializer;
        public const string Version = "1.0.3";

        [STAThread]
        static void Main()
        {
            ProgramDataFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tag2Space.xml");
            ProgramData = new _ProgramData();
            serializer = new XmlSerializer(typeof(_ProgramData));

            ReadProgramData();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());

            SaveProgramData();
        }



        public static bool SaveProgramData()
        {
            bool ret = true;
            FileStream stream = null;

            try {
                stream = new FileStream(ProgramDataFile, FileMode.Create);
                serializer.Serialize(stream, ProgramData);

            }
            catch(Exception ex) {
                ret = false;
            }
            finally {
                if (stream != null) stream.Close();

            }

            return ret;
        }

        public static bool ReadProgramData()
        {
            bool ret = true;
            FileStream stream = null;

            try {
                stream = new FileStream(ProgramDataFile, FileMode.Open);
                ProgramData = (_ProgramData)serializer.Deserialize(stream);

            }
            catch (Exception ex) {
                ret = false;
            }
            finally {
                if (stream != null) stream.Close();

            }

            return ret;
        }






    }




}
