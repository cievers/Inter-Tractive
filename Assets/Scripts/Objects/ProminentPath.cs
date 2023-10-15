using System.Collections.Generic;
using System.Linq;

namespace Objects {
	public class ProminentPath {
		private readonly string path;
		private readonly List<ProminentPath> context;

		public ProminentPath(string path) {
			this.path = path;
			context = new List<ProminentPath>();
		}
		public ProminentPath(string path, List<ProminentPath> context) {
			this.path = path;
			this.context = context;
		}
		
		public void UpdateContext(ProminentPath source) {
			if (context.Contains(source)) {
				context.Remove(source);
			} else {
				context.Add(source);
			}
		}

		public string Name() {
			return Significance(1).Split(".")[0];
		}
		public string Extension() {
			return path.Split(".")[^1];
		}
		public string Prominence() {
			for (var i = 1; i < Significance(); i++) {
				var display = Significance(i);
				if (context.Any(source => source.Significance(i) == display)) {
					continue;
				}
				return display;
			}
			return Significance(Significance());
		}
		private int Significance() {
			return path.Split("/").Length;
		}
		private string Significance(int length) {
			var result = "";
			var split = path.Split("/");
			for (var i = 0; i < split.Length && i < length; i++) {
				result = split[^(i+1)] + "/" + result;
			}
			return result.TrimEnd('/');
		}
	}
}