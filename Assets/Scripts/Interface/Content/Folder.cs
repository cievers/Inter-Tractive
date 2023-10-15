using System.Collections.Generic;
using Interface.Control;
using TMPro;
using UnityEngine;

namespace Interface.Content {
	public class Folder : MonoBehaviour {
		public TextMeshProUGUI title;
		public RectTransform container;
		public GameObject iconOpen;
		public GameObject iconClosed;
		public List<RectTransform> forceUpdates;
		
		private bool state;
		
		public Folder Construct(RectTransform parent, Data folder, Theming theming) {
			var instance = Instantiate(this, parent);

			instance.state = folder.state;
			instance.title.text = folder.title;
			instance.forceUpdates.Add(parent);
			
			foreach (var component in folder.components) {
				theming.Construct(instance.container, component);
			}
			
			instance.UpdateState();

			return instance;
		}
		
		public void Toggle() {
			UpdateState(!state);
		}
		private void UpdateState(bool state) {
			this.state = state;
			UpdateState();
		}
		private void UpdateState() {
			container.gameObject.SetActive(state);
			iconOpen.SetActive(state);
			iconClosed.SetActive(!state);
			Utility.Layout.Fix(container);
		}

		public record Data : Controller {
			public readonly bool state;
			public readonly string title;
			public readonly List<Controller> components;

			public Data(string title, List<Controller> components) {
				this.title = title;
				this.components = components;
			}
			public Data(bool state, string title, List<Controller> components) : this(title, components) {
				this.state = state;
			}
		}
	}
}