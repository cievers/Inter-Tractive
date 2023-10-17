using Interface.Automation;

namespace Objects.Sources {
	public class SourceAutomation : Source {
		public Automation Automate(string path) {
			LoadTemplates();
			Load(path);
			return new Automation(instance.Controls());
		}
		public void AutomateSlice() {
			Slice();
			// Would be cool if we could somehow return the newly created slice for further automation on that
		}
		public void AutomateClose() {
			Close();
		}
	}
}