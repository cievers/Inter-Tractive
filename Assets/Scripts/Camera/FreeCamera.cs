using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Camera {
	public class FreeCamera : MonoBehaviour {
		[Header("Targets")]
		public UnityEngine.Camera cam;

		[Header("Movement")]
		public float translationSpeed = 8;
		[Range(-90, 90)] public int pitchMinimum = 20;
		[Range(-90, 90)] public int pitchMaximum = 90;
		public float zoomSpeed = 8;

		private bool cameraIsMoving; // Returns true if the camera is moving
		private bool resettingIsMoving; // Returns true of the cameraIsMoving will be reset
		private bool inputLocked; // Variables that locks the camera's inputs
		private Vector3 translationStart; // Where the mouse is focussed when they start translating
		private Vector3 rotationStart; // Where the mouse is focussed when they start rotating

		private void Update() {
			if (!inputLocked) {
				UpdateTranslation();
				UpdateRotation();
				UpdateZoom();

				// Only reset is moving variable if no invoke is called
				if (!resettingIsMoving) {
					StartCoroutine(ResetIsMoving(2f));
					resettingIsMoving = true;
				}
			}
		}
		private void UpdateTranslation() {
			if (Input.GetMouseButtonDown(0)) {
				// Get starting position as soon as mouse is pressed
				
				// Mouseposition in viewport coordinates (0, 1)
				translationStart = cam.ScreenToViewportPoint(Input.mousePosition);
			} else if (Input.GetMouseButton(0)) {
				// For any other frame where the mouse is still held process its movement
				
				cameraIsMoving = true;

				var newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
				var direction = translationStart - newPosition;
				var view = cam.transform;
				
				view.position += view.TransformDirection(direction) * translationSpeed;

				translationStart = newPosition;
			}
		}
		private void UpdateRotation() {
			if (Input.GetMouseButtonDown(1)) {
				// Get starting position as soon as mouse is pressed
				
				// Mouseposition in viewport coordinates (0, 1)
				rotationStart = cam.ScreenToViewportPoint(Input.mousePosition);
			} else if (Input.GetMouseButton(1)) {
				// For any other frame where the mouse is still held process its movement

				// Set to true, since we are moving the camera
				cameraIsMoving = true;

				// Mouse position in viewport coordinates (0, 1)
				Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);

				// Calculate what direction the mouse is moving to
				Vector3 direction = rotationStart - newPosition;

				// Rotation all the way from left to right should rotate cam 180 degrees
				float rotationY = -direction.x * 180;
				float rotationX = direction.y * 180;

				// Temporarily set camera position to target
				// cam.transform.position = target.position;

				// Get the current x axis rotation
				Vector3 currentRotation = cam.transform.rotation.eulerAngles;

				// Only perform movement if allowed
				if (currentRotation.x + rotationX > pitchMinimum && currentRotation.x + rotationX < pitchMaximum) {
					cam.transform.Rotate(new Vector3(1, 0, 0), rotationX);
				} else if (currentRotation.x + rotationX < pitchMinimum) {
					// Reset angle to minAngle, so that the camera does not get stuck when changing parameter fields
					cam.transform.rotation = Quaternion.Euler(pitchMinimum, currentRotation.y, currentRotation.z);
				} else if (currentRotation.x + rotationX > pitchMaximum) {
					// Reset angle to maxAngle, so that the camera does not get stuck when changing parameter fields
					cam.transform.rotation = Quaternion.Euler(pitchMaximum, currentRotation.y, currentRotation.z);
				}

				cam.transform.Rotate(new Vector3(0, 1, 0), rotationY, Space.World);

				// Translate camera back (undo cam.transform.position)
				// cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));

				// Update position
				rotationStart = newPosition;
			}
		}
		private void UpdateZoom() {
			if (Input.GetAxis("Mouse ScrollWheel") != 0f) {
				// Set to true, since we are moving the camera
				cameraIsMoving = true;

				cam.transform.Translate(cam.transform.TransformDirection(new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed)));
			}
		}

		public void LockCamera() {
			inputLocked = true;
		}

		public void UnlockCamera() {
			inputLocked = false;
		}

		public bool CameraIsMoving() {
			return cameraIsMoving;
		}

		public UnityEngine.Camera GetCurrentCam() {
			return cam;
		}

		private IEnumerator ResetIsMoving(float delay) {
			yield return new WaitForSecondsRealtime(delay);
			cameraIsMoving = false;
			resettingIsMoving = false;
		}
	}
}