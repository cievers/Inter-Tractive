using UnityEngine;

namespace Interface {
	public class ObjectToggle : MonoBehaviour {
		public bool state;
		public GameObject subject;
		public GameObject displayTrue;
		public GameObject displayFalse;

		public ObjectToggle Instantiate() {
			var instance = Instantiate(gameObject);
			var component = instance.GetComponent<ObjectToggle>();
			return component;
		}

		private void Start() {
			UpdateState();
		}
		public void Toggle() {
			state = !state;
			UpdateState();
		}
		private void UpdateState() {
			subject.SetActive(state);
			displayTrue.SetActive(state);
			displayFalse.SetActive(!state);
		}
	}
}