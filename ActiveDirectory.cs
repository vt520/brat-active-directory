using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;

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
        public static readonly Dictionary<string, Dictionary<string, string>> LdapAttributeMap = new Dictionary<string, Dictionary<string, string>> {
            { "", new Dictionary<string, string> {
                {"name", "Name" },
                {"objectClass", "Type" },
                {"objectGUID", "GUID" },

            }}
        };
        public ActiveDirectoryAPI(ActiveDirectory instance, ProcessorHost host): base(host) { 
            Driver = instance; 
        }
        public DirectoryEntry OpenPath(string path) {
            List<string> pathParts = new List<string>(path.Split("/"));
            pathParts.Reverse();
            StringBuilder ldapPath = new StringBuilder("LDAP://");
            foreach(string part in pathParts) {
                ldapPath.Append($"OU={part},");
            }
            ldapPath.Append(Driver.Connection.Properties["distinguishedName"].Value);
            return new DirectoryEntry(ldapPath.ToString()); 
        }
        public dynamic ReadDirectory(DirectoryEntry root) {
            IDictionary<string, object> children = new ExpandoObject() as IDictionary<string, object>;
            dynamic result = ReadLeaf(root);
            foreach (DirectoryEntry item in root.Children) {
                dynamic child = null;
                if (item.SchemaClassName.Equals("organizationalUnit")) {
                    child = ReadDirectory(item);
                } else {
                    child = ReadLeaf(item);
                }
                if (child is not null) {
                    children.Add(child.Name, child);
                }
            }
            if(children.Count > 0) result.Children = children;
            return result;
        }
        public dynamic ReadLeaf(DirectoryEntry leaf) {
            IDictionary<string, object> result = new ExpandoObject() as IDictionary<string, object>;
            Regex nameFixer = new Regex(@"^([^=]+=)?([^=].+)$");
            Match nameResult = nameFixer.Match(leaf.Name);

            result.Add("Name", nameResult.Groups[2].Value);
            result.Add("Qualifier", nameResult.Groups[1].Value.Replace("=", ""));
            result.Add("Type", leaf.SchemaClassName);
            return result;
        }
    }
}