using System;
using TMPro;
using UnityEngine;

namespace Interface.Control {
	public class ActionToggle : MonoBehaviour {
		public bool state;
		public GameObject displayTrue;
		public GameObject displayFalse;
		public TextMeshProUGUI displayName;
		
		private Action<bool> action;

		public ActionToggle Construct(Transform parent, string name, bool state, Action<bool> action) {
			var instance = Instantiate(gameObject, parent);
			var component = instance.GetComponent<ActionToggle>();

			component.displayName.text = name;
			component.action = action;
			component.state = state;
			component.UpdateState();
			
			return component;
		}
		public ActionToggle Construct(Transform parent, Data toggle) {
			return Construct(parent, toggle.Name, toggle.State, toggle.Action);
		}

		private void Start() {
			UpdateState();
		}
		public void Toggle() {
			state = !state;
			UpdateState();
		}
		private void UpdateState() {
			action?.Invoke(state);
			displayTrue.SetActive(state);
			displayFalse.SetActive(!state);
		}
		
		public record Data(string Name, bool State, Action<bool> Action) : Controller {
			public string Name {get; private set;} = Name;
			public bool State {get; private set;} = State;
			public Action<bool> Action {get; private set;} = Action;
		}
	}
}