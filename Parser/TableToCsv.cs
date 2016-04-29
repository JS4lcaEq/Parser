using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    class TableToCsv
    {
        public static StringBuilder GetString(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DataColumn col in dt.Columns)
            {
                if (sb.Length > 0)
                {
                    sb.Append(";");
                }
                sb.AppendFormat(@" {0} ",col.ColumnName);
            }

            foreach (DataRow row in dt.Rows)
            {
                sb.Append(@"
");
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName != "nn")
                    {
                        sb.Append(";");
                    }
                    sb.AppendFormat(@" {0} ", row[col.ColumnName]);
                }
            }

            return sb;
        }

    }
}
