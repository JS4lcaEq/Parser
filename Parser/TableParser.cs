using System;
using System.Collections.Generic;
using System.Data;
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

        private DataTable createReportDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "nn", Caption = "#" });
            dt.Columns.Add(new DataColumn() { ColumnName = "phone_num"      , Caption = "телефон"       });
            dt.Columns.Add(new DataColumn() { ColumnName = "day_from"       , Caption = "dd-mm-yyyy"    });
            dt.Columns.Add(new DataColumn() { ColumnName = "day_to"         , Caption = "dd-mm-yyyy"    });
            dt.Columns.Add(new DataColumn() { ColumnName = "tel_serve"      , Caption = "абон.плата"    });
            dt.Columns.Add(new DataColumn() { ColumnName = "call_serve"     , Caption = "стоимость звонков"         });
            dt.Columns.Add(new DataColumn() { ColumnName = "others_serve"   , Caption = "стоимость остальных услуг" });
            dt.Columns.Add(new DataColumn() { ColumnName = "ndc"            , Caption = "НДС, вычислять пока как с неким коэффициентом"     });
            dt.Columns.Add(new DataColumn() { ColumnName = "itogo"          , Caption = "полная стоимость"          });
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

        public DataTable Parse(  XmlDocument doc                // xml документ
                                    , string rowXPath           // путь к строкам
                                    , string phoneXPath         // путь к телефону 
                                    , string telServeXPhath     // путь к абон плата
                                    , string callServeXPath     // путь к стоимость звонков
                                    , string othersServeXPath   // путь к стоимость остальных услуг
                                    , string summXPath          // путь к сумме
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
                dr["tel_serve"] = getValue(item, telServeXPhath);
                dr["call_serve"] = getValue(item, callServeXPath);
                dt.Rows.Add(dr);
            }

            DateTime? minDate = null;
            DateTime? maxDate = null;

            /* 
            
            if (false)
            {
                if (onErrorParseInpFormat != null)
                {
                    App.Current.Dispatcher.BeginInvoke(onErrorParseInpFormat, String.Format("Invalid input format"));
                }
                return null;
            }            
            
            Match match = Regex.Match(row[1].ToString(), @"Абонент: (\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
             dr["phone_num"] = match.Groups[1];
             DataRow dr = dt.NewRow();
             dt.Rows.Add(dr);
                    if (onFindMemeber != null)
                    {
                        App.Current.Dispatcher.BeginInvoke(onFindMemeber, dt.Rows.Count, String.Format("{0}", match.Groups[1]));
                    }             
            */


            return dt;
        }
    }
}
