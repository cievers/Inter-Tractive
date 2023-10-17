using System.Collections.Generic;
using Logic.Eventful;
using UnityEngine;

namespace Interface.Content {
	public class Loader : MonoBehaviour {
		public RectTransform container;
		public GameObject spinner;

		private bool state;
		private bool changed;

		public Loader Construct(RectTransform parent, Data component, Theming theming) {
			var instance = Instantiate(this, parent);

			theming.Construct(instance.container, component.Component);
			instance.UpdateState(component.State.State);
			component.State.Change += instance.UpdateState;

			return instance;
		}

		private void Update() {
			if (changed) {
				spinner.SetActive(state);
				changed = false;
			}
		}
		private void UpdateState(bool state) {
			this.state = state;
			changed = true;
		}
		
		public record Data(Boolean State, Component Component) : Component, Paradigm.Container {
			public Boolean State {get;} = State;
			public Component Component {get;} = Component;
			public IEnumerable<Component> Components => new[] {Component};
			public string Prefix => "";
		}
	}
}