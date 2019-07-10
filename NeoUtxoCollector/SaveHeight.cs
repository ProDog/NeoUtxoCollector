using System;
using System.Data;

namespace NeoUtxoCollector
{
    internal class SaveHeight : SaveBase
    {
        public SaveHeight() : base()
        {
            InitDataTable(TableType.Height);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Height, name);
            return true;
        }

        internal uint getHeight()
        {
            string sql = $"select height from {DataTableName}";
            DataTable dt = MysqlConn.ExecuteDataSet(sql).Tables[0];
            if (dt.Rows.Count == 0)
            {
                return 0;
            }
            else
            {
                return uint.Parse(dt.Rows[0]["height"].ToString()) + 1;
            }
        }

        public string GetUpdateHeightSql(uint height)
        {
            string sql = "";

            if (height == 0)
            {
                sql = $"insert into {DataTableName} values ({height})";
            }
            else
            {
                sql = $"update {DataTableName} set height = {height}";
            }

            return sql;
        }
    }
}