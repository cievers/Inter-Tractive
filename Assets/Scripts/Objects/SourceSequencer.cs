using Camera;
using Objects.Sources;
using UnityEngine;

namespace Objects {
	public class SourceSequencer : SourceManager {
		public SourceAutomation template;
		public new OrbitingCamera camera;

		private int i = 0;
		private static readonly string[] SEQUENCE = {
			"C:\\Users\\Cas\\Documents\\Computer Science\\Master\\Master Project\\Data\\Tract\\TOM\\T_POSTC_left.tck",
			"C:\\Users\\Cas\\Documents\\Computer Science\\Master\\Master Project\\Data\\Tract\\TOM\\T_POSTC_right.tck"
		};

		private void Start() {
			Add(SEQUENCE[i]);
		}
		private void Add(string path) {
			try {
				var source = Instantiate(template, transform);

				source.Automate(path);
				Collect(source);
				Interact(source);

				source.Loaded += loaded => Loaded(source, loaded);
				source.Focused += camera.Target;
				if (sources.Count == 1) {
					source.Focus();
				}
			} catch (UnityException) {
				
			}
		}
		private void Loaded(SourceAutomation source, bool loaded) {
			Debug.Log("Automated source is loaded: " + loaded);
			if (loaded) {
				Record(source);
			}
		}
		private void Record(SourceAutomation source) {
			source.AutomateClose();
			Next();
		}
		private void Next() {
			i++;
			if (i < SEQUENCE.Length) {
				Add(SEQUENCE[i]);
			}
		}
	}
}