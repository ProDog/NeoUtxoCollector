using System.Collections.Generic;

namespace NeoUtxoCollector
{
    class SaveAddressTransaction : SaveBase
    {
        public SaveAddressTransaction() : base()
        {
            InitDataTable(TableType.Address_tx);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Address_tx, name);
            return true;
        }

        public string GetAddressTxSql(List<string> slist)
        {
            return MysqlConn.InsertSqlBuilder(DataTableName, slist);
        }
    }
}