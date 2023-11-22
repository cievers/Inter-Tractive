using UnityEngine;

namespace Objects {
	public class Billboard : MonoBehaviour {
		public Vector3 forwards;

		private new UnityEngine.Camera camera;
		
		private void Start() {
			camera = UnityEngine.Camera.main;
		}
		private void Update() {
			var view = camera.transform;
			transform.rotation = Quaternion.LookRotation(view.rotation * forwards, view.up);
		}
	}
}