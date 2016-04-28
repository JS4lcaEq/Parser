using Microsoft.Win32;
using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Xml;

namespace Parser
{

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public void Log(string msg)
        {
            var tt = ((InterfaceData)Resources["tb"]);
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () { tt.Log += String.Format(@"
{0:HH:mm:ss} {1}", DateTime.Now, msg); });
        }

        public void LogClear()
        {
            var tt = ((InterfaceData)Resources["tb"]);
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () { tt.Log = "Log"; });
        }

        private void OpenXml()
        {
            var tt = ((InterfaceData)Resources["tb"]);

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () { tt.IsInProgress = true; });

            FileReader fr = new FileReader();

            fr.onErrorFileOpen += onErrorFileOpen;
            fr.onErrorXmlOpen += onErrorXmlOpen;
            fr.onSuccessFileOpen += onSuccessFileOpen;
            fr.onSuccessXmlOpen += onSuccessXmlOpen;

            fr.onStart += Fr_onStart;
            XmlDocument doc = fr.Read(_cFileName);

            TableParser tp = new TableParser();

            DataTable dt = tp.Parse(
                  doc
                , Properties.Settings.Default.rowXPath
                , Properties.Settings.Default.phoneXPath
                , Properties.Settings.Default.telServeXPhath
                , Properties.Settings.Default.callServeXPath
                , null
                , null  );

            StringBuilder sb = TableToCsv.GetString(dt);

            System.IO.File.WriteAllText(_cFileName + String.Format(".report_{0:yyyy-mm-dd_HH-mm-ss}.csv", DateTime.Now), sb.ToString());

            Log("file read, rows found: " + dt.Rows.Count);

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () { tt.IsInProgress = false; });
        }

        private void onFindMemeber(int membersCount, string member)
        {
            var tt = ((InterfaceData)Resources["tb"]);
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () { tt.Progress = membersCount; });
        }

        private void onErrorParseInpFormat(string obj)
        {
            Log(String.Format("Error!!! {0}", obj));
        }

        private void onSuccessXmlOpen(string obj)
        {
            Log(String.Format("Xml success opened, find: root element '{0}'", obj));
        }

        private void onErrorXmlOpen(string obj)
        {
            Log(String.Format("Xml opened with error: {0}", obj));
        }

        private void onSuccessFileOpen(long obj)
        {
            Log(String.Format("success read {0} bites from file", obj));
        }

        private void onErrorFileOpen(Exception e)
        {
            Log(String.Format("File '{0}' opened with error: {1}", _cFileName, e.Message));
        }

        private void Fr_onStart(string obj)
        {
            Log(String.Format("start read '{0}' file", obj));
        }

        private string _cFileName;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            LogClear();
            OpenFileDialog myDialog = new OpenFileDialog();
            myDialog.Filter = "XML(*.xml)|*.xml" + "|Все файлы (*.*)|*.*";
            myDialog.CheckFileExists = true;
            myDialog.Multiselect = false;
            if (myDialog.ShowDialog() == true)
            {
                _cFileName = myDialog.FileName;
                Thread thread = new Thread(OpenXml);
                thread.Start();
            }
        }
    }
}
