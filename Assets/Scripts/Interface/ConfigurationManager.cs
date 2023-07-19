using UnityEngine;
using UnityEngine.UI;

namespace Interface {
	public class ConfigurationManager : MonoBehaviour {
		public SourceManager sources;
		
		public Slider resolutionSlider;
		public float resolutionDelay;
		
		public float resolution;
		private float resolutionPrevious;
		private float resolutionTime;
		private bool resolutionWaiting;

		private void Start() {
			resolutionSlider.onValueChanged.AddListener(ResolutionUpdate);
			resolution = resolutionSlider.value;
			resolutionPrevious = resolution;
		}
		private void Update() {
			if (resolutionWaiting) {
				if (resolution == resolutionPrevious) {
					resolutionTime += Time.deltaTime;
				}
				if (resolutionTime >= resolutionDelay) {
					resolutionTime = 0;
					resolutionWaiting = false;
					Debug.Log("Resolution definitively changed to "+resolution);
					sources.ConfigureResolution(resolution);
				}
			}
			if (resolution != resolutionPrevious) {
				resolutionTime = 0;
				resolutionWaiting = true;
				resolutionPrevious = resolution;
			}
		}
		[ContextMenu("Change resolution")]
		private void ResolutionUpdate(float value) {
			Debug.Log("Slider updated to "+value);
			resolution = value;
		}
	}
}