using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeafHearingAID.Models
{



    public class AIDItem : TableEntity
    {
        public AIDItem()
        {

        }
        public AIDItem(string skey, string srow)
        {
            this.PartitionKey = skey;
            this.RowKey = srow;
        }
        public DateTime SavedTime { get; set; }
        public string Language { get; set; }
        public string AudioText { get; set; }

    }
}
