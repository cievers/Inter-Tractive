using TMPro;
using UnityEngine;

namespace Interface.Control {
	public class Exporter : MonoBehaviour {
		public TextMeshProUGUI display;

		private Files.Exporter file;

		public Exporter Construct(Transform parent, Data exporter) {
			var instance = Instantiate(this, parent);

			instance.display.text = exporter.Name;
			instance.file = exporter.File;
			
			return instance;
		}

		public void Export() {
			file.Publish();
		}

		public record Data(string Name, Files.Exporter File) : Controller {
			public string Name {get; private set;} = Name;
			public Files.Exporter File {get; private set;} = File;
		}
	}
}