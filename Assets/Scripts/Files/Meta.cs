using System.Collections.Generic;
using System.Linq;

namespace Files {
	public class Meta {
		private Dictionary<string, List<string>> values;

		public Meta() {
			values = new Dictionary<string, List<string>>();
		}
		public Meta(Dictionary<string, string> values) {
			this.values = values.ToDictionary(item => item.Key, item => new List<string> {item.Value});
		}
		public Meta(Dictionary<string, List<string>> values) {
			this.values = values;
		}
		
		public void Add(string key, string value) {
			if (!values.ContainsKey(key)) {
				values[key] = new List<string>();
			}
			values[key].Add(value);
		}

		public string Get(string key) {
			return values[key][^1];
		}
		public List<string> GetAll(string key) {
			return values[key];
		}
		public string this[string key] => Get(key);
	}
}