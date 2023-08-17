using UnityEngine;

namespace Interface.Effects {
	public class Spinner : MonoBehaviour {
		public float speed = 300;

		private void Update() {
			transform.Rotate(Vector3.back, speed * Time.deltaTime);
		}
	}
}