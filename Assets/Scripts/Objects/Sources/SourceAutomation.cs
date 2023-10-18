using System;
using System.IO;
using Files.Publication;
using Interface.Automation;

namespace Objects.Sources {
	public class SourceAutomation : Source {
		private string output;
		
		public Automation Automate(string input, string output) {
			this.output = output;
			LoadTemplates();
			Load(input);
			return new Automation(instance.Controls());
		}
		public void AutomateSlice() {
			Slice();
			// Would be cool if we could somehow return the newly created slice for further automation on that
		}
		public void AutomateClose() {
			Close();
		}
		protected override void Write(Publication file, string description, string type) {
			file.Write(Path.Join(Path.ChangeExtension(output, null), $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss.ms}.{type}"));
		}
	}
}