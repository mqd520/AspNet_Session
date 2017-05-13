using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using System.Diagnostics;

namespace StateServer2
{
    public class PartitionResolver : IPartitionResolver
    {
        private String[] partitions;

        public void Initialize()
        {
            partitions = ConfigurationManager.AppSettings["StateServers"].Split(new char[] { ',' });
        }

        public String ResolvePartition(Object key)
        {
            String sid = key as string;
            int partitionID = Math.Abs(sid.GetHashCode()) % partitions.Length;
            Debug.WriteLine(string.Format("sessionID: {0}, session服务器: {1}", sid, partitions[partitionID]));
            return partitions[partitionID];
        }
    }
}