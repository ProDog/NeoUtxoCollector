using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
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

        public string GetUtxoSqlText(JToken tx, uint blockHeight)
        {
            string sql = "";
            string txid = (string)tx["txid"];
            JArray vinJA = (JArray)tx["vin"];
            JArray voutJA = (JArray)tx["vout"];

            //Utxo产生,Insert
            if (voutJA.Count > 0)
            {
                foreach (JObject vo in voutJA)
                {
                    List<string> slist = new List<string>();

                    slist.Add(vo["address"].ToString());
                    slist.Add(txid);
                    slist.Add(vo["n"].ToString());
                    slist.Add(vo["asset"].ToString());
                    slist.Add(vo["value"].ToString());
                    slist.Add(blockHeight.ToString());
                    slist.Add("");  //used
                    slist.Add("0");  //useHeight
                    slist.Add("");  //claimed

                    sql += MysqlConn.InsertSqlBuilder(DataTableName, slist);

                }
            }

            //Utxo使用,Update
            if (vinJA.Count > 0)
            {
                foreach (JObject vi in vinJA)
                {
                    //Update 字段
                    Dictionary<string, string> uDic = new Dictionary<string, string>();
                    uDic.Add("used", txid);
                    uDic.Add("useHeight", blockHeight.ToString());

                    //Where 条件字段
                    Dictionary<string, string> wDic = new Dictionary<string, string>();
                    wDic.Add("txid", vi["txid"].ToString());
                    wDic.Add("n", vi["vout"].ToString());

                    sql += MysqlConn.UpdateSqlBuilder(DataTableName, uDic, wDic);
                }
            }

            if (tx["claims"] != null)
            {
                //记录GAS领取
                JArray claimJA = (JArray)tx["claims"];
                if (claimJA.Count > 0)
                {
                    foreach (JObject cl in claimJA)
                    {
                        //Update 字段
                        Dictionary<string, string> uDic = new Dictionary<string, string>();
                        uDic.Add("claimed", cl["txid"].ToString());

                        //Where 条件字段
                        Dictionary<string, string> wDic = new Dictionary<string, string>();
                        wDic.Add("txid", cl["txid"].ToString());
                        wDic.Add("n", cl["vout"].ToString());

                        sql += MysqlConn.UpdateSqlBuilder(DataTableName, uDic, wDic);
                    }
                }
            }

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
