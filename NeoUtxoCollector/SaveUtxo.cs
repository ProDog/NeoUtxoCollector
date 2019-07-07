using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoUtxoCollector
{
    class SaveUtxo : SaveBase
    {
        public SaveUtxo() : base()
        {
            InitDataTable(TableType.UTXO);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.UTXO, name);
            return true;
        }

        public string GetUtxoSqlText(JToken jObject, uint height)
        {
            string sql = "";

            List<string> slist = null;
            sql += MysqlConn.InsertSqlBuilder(DataTableName, slist);

            return sql;

        }

    }

    class UTXO
    {
        public UTXO()
        {
            addr = string.Empty;
            txid = string.Empty;
            n = -1;
            asset = string.Empty;
            value = 0;
            createHeight = -1;
            used = string.Empty;
            useHeight = -1;
            claimed = string.Empty;
        }

        public string addr { get; set; }
        public string txid { get; set; }
        public int n { get; set; }
        public string asset { get; set; }
        public decimal value { get; set; }
        public int createHeight { get; set; }
        public string used { get; set; }
        public int useHeight { get; set; }
        public string claimed { get; set; }
    }
}
