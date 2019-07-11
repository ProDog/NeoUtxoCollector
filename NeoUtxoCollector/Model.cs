using System;
using System.Collections.Generic;
using System.Text;

namespace NeoUtxoCollector
{
    class TableType
    {
        public const string Block = "block";
        public const string Address = "address";
        public const string Address_tx = "address_tx";
        public const string Transaction = "transaction";
        public const string Notify = "notify";
        public const string NEP5Asset = "nep5asset";
        public const string NEP5Transfer = "nep5transfer";
        public const string UTXO = "utxo";
        public const string Hash_List = "hashlist";
        public const string Appchainstate = "appchainstate";
        public const string Height = "blockheight";
        public const string Address_Asset = "address_asset";
        public const string Tx_Script_Method = "tx_script_method";
        public const string Contract_State = "contract_state";
        public const string NFT_Address = "nft_address";
    }

    class TransType
    {
        public const string Get = "get";
        public const string Send = "send";
    }
}
