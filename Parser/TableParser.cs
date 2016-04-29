using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Parser
{
    class TableParser
    {
        public event Action<string> onErrorParseInpFormat;
        public event Action<int, string> onFindMemeber;

        private DateTime? getDate(DataRow row, int markerIndex, string markerMask, int valueIndex, string valueMask)
        {
            if (Regex.IsMatch(row[markerIndex].ToString(), markerMask, RegexOptions.IgnoreCase))
            {
                if (valueMask == null)
                {
                    return Convert.ToDateTime(row[valueIndex].ToString());
                }
                else
                {
                    Match match = Regex.Match(row[valueIndex].ToString(), valueMask);
                    if (match.Success)
                    {
                        return Convert.ToDateTime(match.Groups[match.Groups.Count - 1].Value);
                    }
                }
            }
            return null;
        }

        private DataTable createDataTypesDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "nn", Caption = "#" });
            dt.Columns.Add(new DataColumn() { ColumnName = "data_type", Caption = "тип" });
            dt.Columns.Add(new DataColumn() { ColumnName = "value_type", Caption = "тип значения" });
            return dt;
        }

        private DataTable createClientsDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "nn", Caption = "#" });
            dt.Columns.Add(new DataColumn() { ColumnName = "client_id", Caption = "идентификатор клиента" });
            dt.Columns.Add(new DataColumn() { ColumnName = "client_name", Caption = "наименование клиента" });
            dt.Columns.Add(new DataColumn() { ColumnName = "day_from", Caption = "dd-mm-yyyy" });
            dt.Columns.Add(new DataColumn() { ColumnName = "day_to", Caption = "dd-mm-yyyy" });
            return dt;
        }

        private DataTable createReportDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "nn", Caption = "#" });
            //dt.Columns.Add(new DataColumn() { ColumnName = "client_id"      , Caption = "идентификатор клиента" });
            dt.Columns.Add(new DataColumn() { ColumnName = "phone_num", Caption = "телефон" });
            dt.Columns.Add(new DataColumn() { ColumnName = "day_from", Caption = "dd-mm-yyyy" });
            dt.Columns.Add(new DataColumn() { ColumnName = "day_to", Caption = "dd-mm-yyyy" });
            dt.Columns.Add(new DataColumn() { ColumnName = "tel_serve", Caption = "абон.плата" });
            dt.Columns.Add(new DataColumn() { ColumnName = "call_serve", Caption = "стоимость звонков" });
            dt.Columns.Add(new DataColumn() { ColumnName = "others_serve", Caption = "стоимость остальных услуг" });
            dt.Columns.Add(new DataColumn() { ColumnName = "ndc", Caption = "НДС, вычислять пока как с неким коэффициентом" });
            dt.Columns.Add(new DataColumn() { ColumnName = "itogo", Caption = "полная стоимость" });
            return dt;
        }

        private string getValue(XmlNode node, string xPath)
        {
            string ret = null;

            XmlNode subNode = node.SelectSingleNode(xPath);
            if (subNode != null)
            {
                ret = subNode.InnerText;
            }
            return ret;
        }

        private float getNumberValue(XmlNode node, string xPath)
        {

            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ".";

            float ret = 0;

            XmlNode subNode = node.SelectSingleNode(xPath);
            if (subNode != null)
            {
                ret = float.Parse( subNode.InnerText, culture );
            }
            return ret;
        }

        private string summValue(XmlNode node, string xPath)
        {
            float ret = 0;
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ".";
            XmlNodeList rowsNodeList = node.SelectNodes(xPath);
            foreach (XmlNode item in rowsNodeList)
            {
                float value = float.Parse(item.InnerText, culture);
                ret += value;
            }

            return ret.ToString();
        }

        private string summValue(XmlNode node, string maskGroup, string maskType, bool isDebugMode)
        {
            string log = "   ";

            float ret = 0;
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ".";

            Regex rgxGroup = new Regex(maskGroup, RegexOptions.IgnoreCase);
            Regex rgxType = new Regex(maskType, RegexOptions.IgnoreCase);

            XmlNodeList rowsNodeList = node.SelectNodes(Properties.Settings.Default.dataRowXPath);
            foreach (XmlNode item in rowsNodeList)
            {
                string group = getValue(item, Properties.Settings.Default.groupXPath);
                string type = getValue(item, Properties.Settings.Default.typeXPath);

                log += " || " + group + "-" + type;

                if ((maskGroup == "null" && group == null) || group != null && rgxGroup.IsMatch(group))
                {
                    if ((maskType == "null" && type == null) || type != null && rgxType.IsMatch(type))
                    {
                        float value = float.Parse(getValue(item, Properties.Settings.Default.valueXPath), culture);
                        ret += value;
                        log += "+" + value;
                    }
                }
            }
            if (isDebugMode)
            {
                return ret.ToString() + log;
            }
            return ret.ToString();
        }

        public DataTable ParseToClients(XmlDocument doc       // xml документ
                                    , string clientsRowXPath  // путь к строкам
                                    , string clientIdXPath    // путь к id 
                                    , string clientNameXPath  // путь к наименованию
                                    , string clientStartXPath // путь к начало отчетного периода
                                    , string clientEndXPath   // путь к конец отчетного периода
            )
        {
            DataTable dt = createClientsDataTable();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            XmlNode root = doc.DocumentElement;
            XmlNodeList rowsNodeList = root.SelectNodes(clientsRowXPath);
            int nn = 0;
            foreach (XmlNode item in rowsNodeList)
            {
                string clientId = getValue(item, clientIdXPath);
                string clientName = getValue(item, clientNameXPath);
                if (!dic.ContainsKey(clientId))
                {
                    nn++;
                    DataRow dr = dt.NewRow();
                    dr["nn"] = nn;
                    dr["client_id"] = clientId;
                    dr["client_name"] = clientName;
                    dr["day_from"] = getValue(item, clientStartXPath);
                    dr["day_to"] = getValue(item, clientEndXPath);
                    dt.Rows.Add(dr);
                    dic.Add(clientId, clientName);
                }

            }
            return dt;
        }

        public DataTable ParseToLongReport(XmlDocument doc                // xml документ
                                    , string rowXPath           // путь к строкам
                                    , string phoneXPath         // путь к телефону 
                                    , string telServeXPhath     // путь к абон плата
                                    , string callServeXPath     // путь к стоимость звонков
                                    , string othersServeXPath   // путь к стоимость остальных услуг
                                    , string summXPath          // путь к сумме
                                    , string clientIdXPath      // путь к идентификатору клиента
                                    
                                    , Dictionary<string, string> startDate
                                    , Dictionary<string, string> endDate
                                    , bool isDebugMode
                                    )
        {
            DataTable dt = createReportDataTable();

            XmlNode root = doc.DocumentElement;
            XmlNodeList rowsNodeList = root.SelectNodes(rowXPath);
            int nn = 0;
            foreach (XmlNode item in rowsNodeList)
            {
                nn++;
                DataRow dr = dt.NewRow();
                dr["nn"] = nn;
                dr["phone_num"] = getValue(item, phoneXPath);
                dr["tel_serve"] = getNumberValue(item, telServeXPhath);
                dr["call_serve"] = summValue(item, "начисления", "ходящ", isDebugMode);
                dr["others_serve"] = summValue(item, "начисления", "(sms)|(mms)|(gprs)", isDebugMode);
                string clientId = getValue(item, clientIdXPath);
                dr["day_from"] = startDate[clientId];
                dr["day_to"] = endDate[clientId];
                dr["itogo"] = getNumberValue(item, summXPath);
                dt.Rows.Add(dr);
            }

            DateTime? minDate = null;
            DateTime? maxDate = null;


            return dt;
        }

        public DataTable ParseToDataType(XmlDocument doc                // xml документ
                                            , string dataRowXPath           // путь к строкам
                                            , string groupXPath // путь к группировке
                                            , string typeXPath  // путь к типу
                                            )
        {
            DataTable dt = createDataTypesDataTable();
            XmlNode root = doc.DocumentElement;
            XmlNodeList rowsNodeList = root.SelectNodes(dataRowXPath);
            int nn = 0;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (XmlNode item in rowsNodeList)
            {
                string type = String.Format("{0}/{1}", getValue(item, groupXPath), getValue(item, typeXPath));
                if (!dic.ContainsKey(type))
                {
                    nn++;
                    DataRow dr = dt.NewRow();
                    dr["nn"] = nn;
                    dr["data_type"] = type;
                    dr["value_type"] = "text";
                    dt.Rows.Add(dr);
                    dic.Add(type, "text");
                }
            }
            return dt;
        }
    }
}
