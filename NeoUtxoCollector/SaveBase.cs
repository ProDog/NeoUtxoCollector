using System;
using System.Collections.Generic;
using System.Text;

namespace NeoUtxoCollector
{
    internal abstract class SaveBase
    {      

        public string DataTableName { get; private set; }               

        public bool InitDataTable(string name)
        {
            DataTableName = name;            

            if (!IsTableExisted(DataTableName))
            {
                return CreateTable(DataTableName);
            }
            return true;
        }

        public bool IsTableExisted(string name)
        {
            return MysqlConn.Exist(name);
        }

        public abstract bool CreateTable(string name);
    }
}
