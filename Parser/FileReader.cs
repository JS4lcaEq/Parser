using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;

namespace Parser
{
    class FileReader
    {

        public event Action<Exception> onErrorFileOpen;
        public event Action<string> onErrorXmlOpen;
        public event Action<int, int> onSuccessExtractTable;
        public event Action<long> onSuccessFileOpen;
        public event Action<string> onSuccessXmlOpen;
        public event Action<string> onStart;
        //public delegate void OneArg(String arg);


        public XmlDocument Read(string filePath)
        {
            XmlDocument doc = new XmlDocument();

            if (onStart != null)
            {
                App.Current.Dispatcher.BeginInvoke(onStart, filePath);
            }

            FileStream stream = null;

            try
            {
                stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                if (onSuccessFileOpen != null)
                {
                    App.Current.Dispatcher.BeginInvoke(onSuccessFileOpen, stream.Length);
                }
            }
            catch(Exception e)
            {
                if (onErrorFileOpen != null)
                {
                    App.Current.Dispatcher.BeginInvoke(onErrorFileOpen, e);
                }
                return null;                
            }

            try
            {
                doc.Load(stream);
                if (onSuccessXmlOpen != null)
                {
                    XmlNode root = doc.DocumentElement;
                    App.Current.Dispatcher.BeginInvoke(onSuccessXmlOpen, root.Name);
                }
            }
            catch (Exception e)
            {
                if (onErrorXmlOpen != null)
                {
                    App.Current.Dispatcher.BeginInvoke(onErrorXmlOpen, e);
                }
                return null;
            }


            return doc;
            
        }
    }
}
