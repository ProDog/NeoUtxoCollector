using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NeoUtxoCollector
{
    internal class SaveTransaction : SaveBase
    {
        private SaveUtxo saveUtxo;
        private SaveNotify saveNotify;
        private SaveAddressTransaction address_tx;

        public SaveTransaction()
            : base()
        {
            InitDataTable(TableType.Transaction);

            saveUtxo = new SaveUtxo();
            saveNotify = new SaveNotify();
            address_tx = new SaveAddressTransaction();
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Transaction, name);
            return true;
        }

        internal string GetTranSqlText(JToken jObject, uint blockHeight, uint blockTime)
        {
            string sql = "";
            List<string> slist = new List<string>();
            slist.Add(jObject["txid"].ToString());
            slist.Add(jObject["size"].ToString());
            slist.Add(jObject["type"].ToString());
            slist.Add(jObject["version"].ToString());
            slist.Add(jObject["attributes"].ToString());
            slist.Add(jObject["sys_fee"].ToString());
            slist.Add(jObject["net_fee"].ToString());
            slist.Add(blockHeight.ToString());

            sql += MysqlConn.InsertSqlBuilder(DataTableName, slist);


            sql += saveUtxo.GetUtxoSqlText(jObject, blockHeight);
            

            if ((string)jObject["type"] == "InvocationTransaction" && jObject["script"] != null)
            {
                sql += saveNotify.GetNotifySqlText(jObject, blockHeight, blockTime);
            }

            return sql;
        }

        internal void ListClear()
        {
            throw new NotImplementedException();
        }
    }
}