using System.IO;
using Files;
using Files.Publication;
using Interface.Automation;

namespace Objects.Sources {
	public class SourceAutomation : Source {
		private string output;

		public Automation Automate(string input, string output) {
			this.output = Path.ChangeExtension(output, null);
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
			Artifact.Write(file, output, type);
		}
		public void Write(Publication file, string type) {
			Write(file, "", type);
		}
	}
}