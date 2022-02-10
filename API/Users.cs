using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brat.Drivers {
    partial class ActiveDirectoryAPI {
        public JArray AD_GetActiveUsers() {
            JArray results = new JArray();
            DirectorySearcher query = AD_CreateUserQuery();
            query.Filter = "(&(objectCategory=User)(objectClass=person)(!userAccountControl:1.2.840.113556.1.4.803:=2)(userPrincipalName=*)(homeDirectory=*))";

            foreach (SearchResult user_record in query.FindAll()) {
                results.Add(new ActiveDirectoryUser(user_record));
            }
            return results;
        }
        public ActiveDirectoryUser AD_GetUser(dynamic identifier) {
            List<string> searchTags = new List<string> { "distinguishedName", "userPrincipalName", "objectSid", "mail", "name" };
            DirectorySearcher query = AD_CreateUserQuery();
            foreach (string ldap_col in searchTags) {
                query.Filter = $"(&({ldap_col}={identifier})(objectCategory=User))";
                SearchResultCollection results = query.FindAll();
                if (results.Count == 1) {
                    return new ActiveDirectoryUser(results[0]);
                }
            }
            return null;
        }

        public DirectorySearcher AD_CreateUserQuery() {
            DirectorySearcher query = new DirectorySearcher(Driver.Connection);
            foreach (string ldap_property in ActiveDirectoryUser.Mapping.Values) {
                query.PropertiesToLoad.Add(ldap_property);
            }
            return query;

        }
        public dynamic HelloWorld() {
            return "Hello World";
        }
    }

    public class ActiveDirectoryUser : MappedSearchResult {
        public static new Dictionary<string, string> Mapping = new Dictionary<string, string> {
            {"Location", "l" },
            {"LdapName", "distinguishedName" },
            {"Email", "userPrincipalName" },
            {"Groups", "memberOf" },
            {"Name", "displayName" },
            {"Position", "title" },
            {"Manager", "manager" }
        };
        public ActiveDirectoryUser(SearchResult source) : base(source) { }

        public static List<string> LdapFields() {
            return new List<string>(Mapping.Values);
        }
    }
}