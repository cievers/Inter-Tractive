using Interface.Paradigm;
using TMPro;
using UnityEngine;
using Action = System.Action;

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
			file.Publish(); // Maybe just use the created action from the Data record
		}

		public record Data(string Name, Files.Exporter File) : Component, Paradigm.Action {
			public string Name {get;} = Name;
			public Files.Exporter File {get;} = File;
			public Action Action => File.Publish;
		}
	}
}