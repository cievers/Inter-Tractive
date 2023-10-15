using System.IO;
using SFB;

namespace Objects.Sources {
	public class SourceSelector : Source {
		private bool loaded;
		private ProminentPath path;
		private Voxels instance;
		
		public void Awake() {
			LoadTemplates();
			Browse();
		}
		private void Browse() {
			var selection = StandaloneFileBrowser.OpenFilePanel("Tract files", "", "tck", false);
			switch (selection.Length) {
				case 1:
					Load(selection[0]);
					break;
				case > 1:
					throw new InvalidDataException("Please select only one file at a time");
			}
		}
	}
}