using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brat.Drivers {
    partial class ActiveDirectoryAPI {
        public dynamic AD_GetGroups() {
            JArray results = new JArray();
            DirectorySearcher query = AD_CreateGroupQuery();
            query.Filter = "(&(objectClass=group))";

            foreach (SearchResult user_record in query.FindAll()) {
                results.Add(
                    JToken.FromObject(
                        new ActiveDirectoryGroup(user_record)
                    )
                );
            }
            return results;
        }
        public ActiveDirectoryGroup AD_GetGroup(dynamic identifier) {
            List<string> searchTags = new List<string> { "distinguishedName", "name", "objectSid" };
            DirectorySearcher query = AD_CreateGroupQuery();
            foreach (string ldap_col in searchTags) {
                query.Filter = $"(&({ldap_col}={identifier})(objectCategory=group)(name=*))";
                SearchResultCollection results = query.FindAll();
                if (results.Count == 1) {
                    ActiveDirectoryGroup group = new ActiveDirectoryGroup(results[0]);
                    return group;
                }
            }
            return null;
        }

        public DirectorySearcher AD_QueryObject<T>() where T : MappedSearchResult {
            return null;
        }

        public DirectorySearcher AD_CreateGroupQuery() {
            DirectorySearcher query = new DirectorySearcher(Driver.Connection); // searchroot
            foreach (string ldap_property in ActiveDirectoryGroup.Mapping.Values) {
                query.PropertiesToLoad.Add(ldap_property);
            }
            return query;

        }
    }

    public class ActiveDirectoryGroup : MappedSearchResult {
        public static new Dictionary<string, string> Mapping = new Dictionary<string, string> {
                    {"Name", "name" },
                    {"LdapName", "distinguishedName" },
                    {"Members", "member" },
                    {"Description", "description" }
                };
        public ActiveDirectoryGroup(SearchResult source) : base(source) { }

        public static List<string> LdapFields() {
            return new List<string>(Mapping.Values);
        }
    }
}