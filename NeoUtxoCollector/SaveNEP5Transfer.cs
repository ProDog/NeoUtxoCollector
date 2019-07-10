using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace NeoUtxoCollector
{
    class SaveNEP5Transfer : SaveBase
    {
        private SaveAddressTransaction address_tx;

        public SaveNEP5Transfer() : base()
        {
            InitDataTable(TableType.NEP5Transfer);

            address_tx = new SaveAddressTransaction();
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.NEP5Transfer, name);
            return true;
        }        

        public string GetNep5TransferSql(uint blockHeight, uint blockTime, string txid, JToken notifications)
        {
            string sql = "";
            foreach (JObject notify in notifications)
            {
                string contract = notify["contract"].ToString();

                //只监控 ZORO
                if (contract == Settings.Default.ZoroHash)
                {
                    JToken values = notify["state"]["value"];
                    string method = Encoding.UTF8.GetString(ThinNeo.Helper.HexString2Bytes(values[0]["value"].ToString()));
                    if (method == "transfer")
                    {
                        string from = values[1]["value"].ToString() == "" ? "" : ThinNeo.Helper_NEO.GetAddress_FromScriptHash(new ThinNeo.Hash160(values[1]["value"].ToString()));
                        string to = ThinNeo.Helper_NEO.GetAddress_FromScriptHash(new ThinNeo.Hash160(values[2]["value"].ToString()));

                        string value = values[3]["type"].ToString() == "ByteArray" ? new BigInteger(ThinNeo.Helper.HexString2Bytes(values[3]["value"].ToString())).ToString() : BigInteger.Parse(values[3]["value"].ToString(), NumberStyles.AllowHexSpecifier).ToString();

                        //构造保存Nep5Transfer的list
                        List<string> slist = new List<string>();
                        slist.Add(blockHeight.ToString());
                        slist.Add(txid);
                        slist.Add(contract);
                        slist.Add(from);
                        slist.Add(to);
                        slist.Add(value.ToString());
                        sql += MysqlConn.InsertSqlBuilder(DataTableName, slist);

                        //构造保存Address tx的list
                        List<string> flist = new List<string>();
                        flist.Add(from);
                        flist.Add(txid);
                        flist.Add(TransType.Send);
                        flist.Add(contract);
                        flist.Add(value);
                        flist.Add(blockHeight.ToString());
                        flist.Add(blockTime.ToString());

                        sql += address_tx.GetAddressTxSql(flist);

                        //构造保存Address tx的list
                        List<string> tlist = new List<string>();
                        tlist.Add(to);
                        tlist.Add(txid);
                        tlist.Add(TransType.Get);
                        tlist.Add(contract);
                        tlist.Add(value);
                        tlist.Add(blockHeight.ToString());
                        tlist.Add(blockTime.ToString());

                        sql += address_tx.GetAddressTxSql(tlist);
                    }

                }

            }

            return sql;
        }
    }
}