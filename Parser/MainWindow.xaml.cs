using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

        private bool isDebugMode;

        public void Log(string msg)
        {
            var tt = ((InterfaceData)Resources["tb"]);
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () { tt.Log += String.Format(@"
{0:HH:mm:ss} {1}", DateTime.Now, msg); });
        }

        public void LogClear()
        {
            var tt = ((InterfaceData)Resources["tb"]);
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () { tt.Log = "Отчет анализа данных"; });
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



            DataTable dtClients = tp.ParseToClients(    doc
                                                        , Properties.Settings.Default.clientsRowXPath
                                                        , "CLNT_ID"
                                                        , Properties.Settings.Default.clientNameXPath
                                                        , Properties.Settings.Default.clientStartXPath
                                                        , Properties.Settings.Default.clientEndXPath    );
            Dictionary<string, string> startDate = new Dictionary<string, string>();
            Dictionary<string, string> endDate = new Dictionary<string, string>();

            foreach (DataRow row in dtClients.Rows)
            {
                if (!startDate.ContainsKey(row["client_id"].ToString()))
                {
                    startDate.Add(row["client_id"].ToString(), row["day_from"].ToString());
                }
                if (!endDate.ContainsKey(row["client_id"].ToString()))
                {
                    endDate.Add(row["client_id"].ToString(), row["day_to"].ToString());
                }
            }

            if (isDebugMode)
            {
                StringBuilder sbClients = TableToCsv.GetString(dtClients);
                System.IO.File.WriteAllText(_cFileName + String.Format(".clients_{0:yyyy-MM-dd_HH-mm-ss}.csv", DateTime.Now), sbClients.ToString(), Encoding.GetEncoding("windows-1251"));
                DataTable dtDataType = tp.ParseToDataType(doc
                                                        , Properties.Settings.Default.dataRowXPath
                                                        , Properties.Settings.Default.groupXPath
                                                        , Properties.Settings.Default.typeXPath );

                StringBuilder sbTypes = TableToCsv.GetString(dtDataType);

                System.IO.File.WriteAllText(_cFileName + String.Format(".types_{0:yyyy-MM-dd_HH-mm-ss}.csv", DateTime.Now), sbTypes.ToString(), Encoding.GetEncoding("windows-1251"));

            }




            DataTable dt = tp.ParseToLongReport(    doc
                                                    , Properties.Settings.Default.rowXPath
                                                    , Properties.Settings.Default.phoneXPath
                                                    , Properties.Settings.Default.telServeXPhath
                                                    , Properties.Settings.Default.callServeXPath
                                                    , null
                                                    , Properties.Settings.Default.summXPath
                                                    , Properties.Settings.Default.clientIdXPath
                                                    , startDate
                                                    , endDate 
                                                    , isDebugMode);

            StringBuilder sb = TableToCsv.GetString(dt);

            System.IO.File.WriteAllText(_cFileName + String.Format(".report_{0:yyyy-MM-dd_HH-mm-ss}.csv", DateTime.Now), sb.ToString(), Encoding.GetEncoding("windows-1251"));


            Log(String.Format("Анализ завершен, обнаружено телефонов: {0}, клиентов: {1}", dt.Rows.Count, dtClients.Rows.Count));

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () { tt.IsInProgress = false; });
        }

        private void onFindMemeber(int membersCount, string member)
        {
            var tt = ((InterfaceData)Resources["tb"]);
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () { tt.Progress = membersCount; });
        }

        private void onErrorParseInpFormat(string obj)
        {
            Log(String.Format("Ошибка формата данных {0}", obj));
        }

        private void onSuccessXmlOpen(string obj)
        {
            Log(String.Format("Xml документ открыт, обнаружен корневой элемент '{0}'", obj));
        }

        private void onErrorXmlOpen(string obj)
        {
            Log(String.Format("Xml документ содержит ошибку: {0}", obj));
        }

        private void onSuccessFileOpen(long obj)
        {
            Log(String.Format("Файл успешно прочитан {0} байт", obj));
        }

        private void onErrorFileOpen(Exception e)
        {
            Log(String.Format("Файл '{0}' открыт с ошибкой: {1}", _cFileName, e.Message));
        }

        private void Fr_onStart(string obj)
        {
            Log(String.Format("Открыт на чтение файл '{0}' ", obj));
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

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            isDebugMode = true;
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isDebugMode = false;
        }
    }
}
