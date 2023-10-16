namespace Objects.Sources {
	public class SourceAutomation : Source {
		public void Automate(string path) {
			LoadTemplates();
			Load(path);
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