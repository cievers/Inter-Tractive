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

			instance.state = folder.State;
			instance.title.text = folder.Title;
			instance.forceUpdates.Add(parent);
			
			foreach (var component in folder.Components) {
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

		public record Data(string Title, IEnumerable<Component> Components) : Component, Paradigm.Container {
			public bool State {get;}
			public string Title {get;} = Title;
			public IEnumerable<Component> Components {get;} = Components;
			public string Prefix => Title + "/";

			public Data(bool state, string title, IEnumerable<Component> components) : this(title, components) {
				State = state;
			}
		}
	}
}