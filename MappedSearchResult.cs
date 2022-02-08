using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.DirectoryServices;

namespace Brat.Drivers {
    public abstract class MappedSearchResult : JObject {
        public readonly SearchResult SourceRecord;
        public static Dictionary<string, string> Mapping = null;
        public MappedSearchResult(SearchResult source) {
            SourceRecord = source;

            Dictionary<string, string> CurrentMap = null;


            if (Mapping is not null) {
                CurrentMap = Mapping;
            }
            CurrentMap = (Dictionary<string, string>)this.GetType().GetField("Mapping").GetValue(null);

            foreach (KeyValuePair<string, string> map in CurrentMap) {
                if (SourceRecord.Properties[map.Value].Count > 0) {
                    if (SourceRecord.Properties[map.Value].Count == 1) {
                        this[map.Key] = JToken.FromObject(SourceRecord.Properties[map.Value][0].ToString());
                        //                        this.TryAdd(map.Key, JToken.FromObject(SourceRecord.Properties[map.Value][0]));
                    } else {
                        this.TryAdd(map.Key, JToken.FromObject(SourceRecord.Properties[map.Value]));
                    }
                }
            }
        }
    }
}
