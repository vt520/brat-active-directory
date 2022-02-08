using System;
using System.Collections.Generic;
using System.DirectoryServices;

namespace Brat.Drivers {
    public class ActiveDirectory : Driver {
        private Dictionary<ProcessorHost,DriverAPI> api = new Dictionary<ProcessorHost, DriverAPI> { };

        public readonly DirectoryEntry Connection;

        public ActiveDirectory(string profile_name = "") : base(profile_name) {
            //Settings = PersistantSettings.Get<ActiveDirectorySettings>();
            DirectoryEntry ad_Info = new DirectoryEntry("LDAP://RootDSE");
            Connection = new DirectoryEntry($"LDAP://{ad_Info.Properties["defaultNamingContext"][0]}");
            Running = true;
        }
        public override DriverAPI GetAPI(ProcessorHost host) {
            if (!api.ContainsKey(host)) {
                api.Add(host, new ActiveDirectoryAPI(this, host));
            }
            return api[host];
        }
    }
    public partial class ActiveDirectorySettings : PersistantSettings {
        public ActiveDirectorySettings() : base("activedirectory") { }
    }
    public partial class ActiveDirectoryAPI : DriverAPI {
        private ActiveDirectory Driver;
        public ActiveDirectoryAPI(ActiveDirectory instance, ProcessorHost host): base(host) { Driver = instance; }
    }
}