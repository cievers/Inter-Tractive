using UnityEngine;

namespace Interface.Content {
	public class Divider : MonoBehaviour {
		public Divider Construct(Transform parent, Data divider) {
			return Instantiate(this, parent);
		}
		
		public record Data : Component;
	}
}