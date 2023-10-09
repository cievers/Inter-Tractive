using Interface.Control;
using Logic.Eventful;
using UnityEngine;

namespace Interface.Content {
	public class Loader : MonoBehaviour {
		public RectTransform container;
		public GameObject spinner;

		public Loader Construct(RectTransform parent, Data component, Theming theming) {
			var instance = Instantiate(this, parent);

			theming.Construct(instance.container, component.component);
			instance.UpdateState(component.state.State);
			component.state.Change += instance.UpdateState;

			return instance;
		}
		
		private void UpdateState(bool state) {
			spinner.SetActive(state);
		}
		
		public record Data : Controller {
			public readonly Boolean state;
			public readonly Controller component;

			public Data(Boolean state, Controller component) {
				this.state = state;
				this.component = component;
			}
		}
	}
}