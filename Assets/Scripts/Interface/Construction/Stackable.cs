using UnityEngine;

namespace Interface.Construction {
	public class Stackable : MonoBehaviour {
		public Transform container;
		public GameObject Contained => container.GetChild(0).gameObject;
		
		public delegate void RemovalEvent();
		public event RemovalEvent Removed;
		public void Remove() {
			Removed?.Invoke();
		}
	}
}