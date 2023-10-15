using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interface {
	public class Source : MonoBehaviour {
		public TextMeshProUGUI title;
		public GameObject loading;
		public RectTransform controls;
		public Theming styles;

		private bool loaded;
		
		private void Update() {
			loading.SetActive(!loaded);
		}
		public void UpdateTitle(string title) {
			this.title.text = title;
		}
		public void UpdateLoaded(bool loaded) {
			this.loaded = loaded;
		}
		public void UpdateControl(Component data) {
			styles.Construct(controls, data);
			UpdateLayout();
		}
		public void UpdateControls(IEnumerable<Component> data) {
			foreach (var component in data) {
				styles.Construct(controls, component);
			}
			UpdateLayout();
		}
		private void UpdateLayout() {
			Utility.Layout.Fix(controls);
		}

		public delegate void SourceAction();
		public event SourceAction Sliced;
		public event SourceAction Closed;
		public void Slice() {
			Sliced?.Invoke();
		}
		public void Close() {
			Destroy(gameObject);
			Closed?.Invoke();
		}
	}
}