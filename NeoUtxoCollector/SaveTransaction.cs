﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NeoUtxoCollector
{
    internal class SaveTransaction : SaveBase
    {
        private SaveUtxo saveUtxo;

        public SaveTransaction()
            : base()
        {
            InitDataTable(TableType.Transaction);

            saveUtxo = new SaveUtxo();
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Transaction, name);
            return true;
        }

        internal string GetTranSqlText(JToken jObject, uint height, uint blockTime)
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
            slist.Add(height.ToString());
           

            if (jObject["tx"] != null)
            {
                sql += saveUtxo.GetUtxoSqlText(jObject, height);               
            }

            sql += MysqlConn.InsertSqlBuilder(DataTableName, slist);

            if (jObject["type"].ToString() == "InvocationTransaction")
            {
                sql += notify.GetNotifySqlText(jObject, blockHeight, blockTime, contractState.ContractDict);
            }

            return sql;
        }

        internal void ListClear()
        {
            throw new NotImplementedException();
        }
    }
}