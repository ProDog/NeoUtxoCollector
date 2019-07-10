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
        private SaveNEP5Transfer nep5Transfer;

        public SaveNotify() : base()
        {
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

                sql = nep5Transfer.GetNep5TransferSql(blockHeight, blockTime, txid, notifications);
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