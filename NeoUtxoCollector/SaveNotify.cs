using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NeoUtxoCollector
{
    internal class SaveNotify : SaveBase
    {
        private SaveAddressTransaction address_tx;
        private SaveNEP5Transfer nep5Transfer;

        public SaveNotify() : base()
        {
            address_tx = new SaveAddressTransaction();
            nep5Transfer = new SaveNEP5Transfer();
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Notify, name);
            return true;
        }

        public string GetNotifySqlText(JToken jToken, uint blockHeight, uint blockTime)
        {
            string sql = "";
            JToken result = null;
            JToken executions = null;
            string script = jToken["script"].ToString();
            string txid = jToken["txid"].ToString();
            WebClient wc = new WebClient();
            wc.Proxy = null;
            try
            {
                result = GetApplicationlog(wc, txid, blockHeight).Result;
                if (result != null)
                    executions = result["executions"].First as JToken;
            }
            catch (Exception e)
            {
                Program.Log($"error occured when call getapplicationlog, txid:{txid}, reason:{e.Message}", Program.LogLevel.Error);
                Thread.Sleep(3000);
                result = GetApplicationlog(wc, txid, blockHeight).Result;
            }

            if (result != null && executions != null && (string)executions["vmstate"] != "FAULT, BREAK")
            {
                JToken notifications = executions["notifications"];

                foreach (JObject notify in notifications)
                {
                    JToken values = notify["state"]["value"];

                    if (values[0]["type"].ToString() == "ByteArray")
                    {
                        string method = Encoding.UTF8.GetString(ThinNeo.Helper.HexString2Bytes(values[0]["value"].ToString()));
                        string contract = notify["contract"].ToString();

                        //只监控 ZORO
                        if (contract == Settings.Default.ZoroHash && method == "transfer")
                        {
                            //存储 Transfer 内容
                            JObject tx = new JObject();
                            tx["blockindex"] = blockHeight;
                            tx["txid"] = txid;
                            tx["n"] = 0;
                            tx["asset"] = contract;
                            tx["from"] = values[1]["value"].ToString() == "" ? "" : ThinNeo.Helper_NEO.GetAddress_FromScriptHash(new ThinNeo.Hash160(values[1]["value"].ToString()));
                            tx["to"] = ThinNeo.Helper_NEO.GetAddress_FromScriptHash(new ThinNeo.Hash160(values[2]["value"].ToString()));
                            tx["value"] = values[3]["type"].ToString() == "ByteArray" ? new BigInteger(ThinNeo.Helper.HexString2Bytes(values[3]["value"].ToString())).ToString() :
                            tx["value"] = BigInteger.Parse(values[3]["value"].ToString(), NumberStyles.AllowHexSpecifier).ToString();

                            sql += address_tx.GetAddressTxSql(tx["to"].ToString(), tx["txid"].ToString(), blockHeight, blockTime);
                            sql += nep5Transfer.GetNep5TransferSql(tx);

                        }
                    }
                }
            }

            return sql;
        }

        public async Task<JToken> GetApplicationlog(WebClient wc, string txid, uint blockHeight)
        {
            try
            {
                var getUrl = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=getapplicationlog&params=['{txid}']";
                var info = await wc.DownloadStringTaskAsync(getUrl);
                var json = JObject.Parse(info);
                var result = json["result"];
                return result;
            }
            catch (WebException e)
            {
                Program.Log($"error occured when call getapplicationlog, height:{blockHeight}, reason:{e.Message}", Program.LogLevel.Error);
                Thread.Sleep(3000);
                return await GetApplicationlog(wc, txid, blockHeight);
            }
        }
    }
}