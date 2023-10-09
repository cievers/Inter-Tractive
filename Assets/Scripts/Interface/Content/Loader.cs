using System.Collections.Generic;
using Interface.Control;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interface.Content {
	public class Loader : MonoBehaviour {
		public RectTransform container;
		public GameObject spinner;
		
		private bool state = true;
		
		public Loader Construct(RectTransform parent, Data component, Theming theming) {
			var instance = Instantiate(this, parent);
			
			theming.Construct(instance.container, component.component);
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
			spinner.SetActive(state);
		}
		
		public record Data : Controller {
			public readonly Controller component;

			public Data(Controller component) {
				this.component = component;
			}
		}
	}
}