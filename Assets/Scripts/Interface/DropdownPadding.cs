using System.Collections;
using UnityEngine;

namespace Interface {
	public class DropdownPadding : MonoBehaviour {
		public float vertical;

		private void OnTransformChildrenChanged() {
			foreach (Transform child in transform) {
				if (child.gameObject.name == "Dropdown List") {
					StartCoroutine(Apply(child.gameObject.GetComponent<RectTransform>()));
				}
			}
		}
		private IEnumerator Apply(RectTransform transform) {
			yield return 0;
			
			var delta = transform.sizeDelta;
			delta = new Vector2(delta.x, delta.y + vertical);
			transform.sizeDelta = delta;
		}
	}
}