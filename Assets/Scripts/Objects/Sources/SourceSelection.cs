using System.IO;
using SFB;

namespace Objects.Sources {
	public class SourceSelection : Source {
		private void Awake() {
			LoadTemplates();
			Browse();
		}
		private void Browse() {
			var selection = StandaloneFileBrowser.OpenFilePanel("Tract & volume files", "", new []{new ExtensionFilter("Tractography", "tck", "nii")}, false);
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