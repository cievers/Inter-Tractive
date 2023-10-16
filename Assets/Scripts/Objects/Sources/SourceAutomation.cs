namespace Objects.Sources {
	public class SourceAutomation : Source {
		public void Automate(string path) {
			LoadTemplates();
			Load(path);
		}
	}
}