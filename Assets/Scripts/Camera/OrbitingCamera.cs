using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Utility;

namespace Camera {
	public class OrbitingCamera : MonoBehaviour {
		[Header("Targets")]
		public UnityEngine.Camera cam;
		public Vector3 target;
		public LayerMask ignoredLayers;

		[Header("Movement")]
		public float translationSpeed = 1;
		[Range(-90, 90)] public int pitchMinimum = -90;
		[Range(-90, 90)] public int pitchMaximum = 90;
		public int zoomMinimum = 5;
		public int zoomMaximum = 120;
		public float zoomSpeed = 8;
		public float interpolationSpeed = 0.05f;

		private Rotation pitch;
		private Rotation pitchBoundaryMinimum;
		private Rotation pitchBoundaryMaximum;
		private float zoom = 20;

		private Transform view;
		private bool cameraIsMoving; // Returns true if the camera is moving
		private bool resettingIsMoving; // Returns true of the cameraIsMoving will be reset
		private bool inputLocked; // Variables that locks the camera's inputs
		private bool followTarget = false; // If true, the camera will follow the target
		private bool translating;
		private bool rotating;
		private Vector3 translationStart; // Where the mouse is focussed when they start translating
		private Vector3 rotationStart; // Where the mouse is focussed when they start rotating


		private void Awake() {
			view = cam.transform;
			pitch = new Rotation(view.rotation.eulerAngles.x, Rotations.Euler);
			pitchBoundaryMinimum = new Rotation(pitchMinimum, Rotations.Degrees);
			pitchBoundaryMaximum = new Rotation(pitchMaximum, Rotations.Degrees);
		}
		private void Update() {
			// If target moves, move camera with it
			if (followTarget) {
				// Set to true, since we are moving the camera
				cameraIsMoving = true;

				if (view.rotation.eulerAngles.x < 85) {
					// Calculate the relative position (vector cam-target direction)
					Vector3 relativePos = target - cam.transform.position;

					// Calculate rotation
					Quaternion toRotation = Quaternion.LookRotation(relativePos);

					// Interpolate rotation to make it smooth
					view.rotation = Quaternion.Lerp(view.rotation, toRotation, interpolationSpeed * Time.unscaledDeltaTime);

					// Set distanceToTarget to avoid flickering on mouse input
					zoom = Vector3.Distance(view.position, target);
				} else {
					// Get the destination in terms of x and z
					Vector3 destinationPos = new Vector3(target.x, cam.transform.position.y, target.z);

					// Interpolate to that destination
					Vector3 smoothPos = Vector3.Slerp(view.position, destinationPos, interpolationSpeed * Time.unscaledDeltaTime);

					// Move camera
					view.position = smoothPos;
				}

				if (view.position == target) {
					// This would/should do something to mark the position as no longer changing
				}
			}

			if (!inputLocked) {
				UpdateTranslation();
				UpdateRotation();
				UpdateZoom();

				if (Input.GetMouseButtonDown(2)) {
					Debug.Log("Middle mouse");
					Debug.Log(ignoredLayers.value);
					foreach (var result in Raycast.MouseEvents(ignoredLayers)) {
						Debug.Log(result);
						Debug.Log(result.gameObject.layer);
						Debug.Log(ignoredLayers >> result.gameObject.layer);
					}
				}
				
				// Only reset is moving variable if no invoke is called
				if (!resettingIsMoving) {
					StartCoroutine(ResetIsMoving(2f));
					resettingIsMoving = true;
				}
			}
		}
		private void UpdateTranslation() {
			if (Input.GetMouseButtonDown(0) && ValidNavigationStart()) {
				// Get starting position as soon as mouse is pressed
				
				translating = true;
				translationStart = cam.ScreenToViewportPoint(Input.mousePosition); // Mouse position in viewport coordinates (0, 1)
			} else if (Input.GetMouseButton(0) && translating) {
				// For any other frame where the mouse is still held process its movement
				
				cameraIsMoving = true;

				var newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
				var direction = translationStart - newPosition;
				var translation = view.TransformDirection(direction) * (translationSpeed * zoom);
				
				view.position += translation;
				target += translation;

				translationStart = newPosition;
			} else if (Input.GetMouseButtonUp(0)) {
				translating = false;
			}
		}
		private void UpdateRotation() {
			if (Input.GetMouseButtonDown(1) && ValidNavigationStart()) {
				// Get starting position as soon as mouse is pressed

				rotating = true;
				rotationStart = cam.ScreenToViewportPoint(Input.mousePosition); // Mouse position in viewport coordinates (0, 1)
			} else if (Input.GetMouseButton(1) && rotating) {
				// For any other frame where the mouse is still held process its movement

				// Set to true, since we are moving the camera
				cameraIsMoving = true;

				var newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
				var direction = rotationStart - newPosition;

				// Rotation all the way from left to right should rotate cam 180 degrees
				var rotationY = -direction.x * 180;
				var rotationX = direction.y * 180;

				// Temporarily set camera position to target in order to rotate around it as pivot
				view.position = target;
				
				// Easy rotation using built-in quaternions, but hard to clamp
				// view.Rotate(new Vector3(1, 0, 0), rotationX);
				
				// Manual rotation using manually tracked pitch, and then other steps for yaw
				pitch += new Rotation(rotationX, Rotations.Degrees);
				var current = view.rotation.eulerAngles;
				view.rotation = Quaternion.Euler(pitch.Clamp(pitchBoundaryMinimum, pitchBoundaryMaximum).ToFloat(Rotations.Degrees), current.y, current.z);
				view.Rotate(new Vector3(0, 1, 0), rotationY, Space.World);

				// Translate camera back to viewing from a distance
				view.Translate(new Vector3(0, 0, -zoom));

				// Update position
				rotationStart = newPosition;
			} else if (Input.GetMouseButtonUp(1)) {
				rotating = false;
			}
		}
		private void UpdateZoom() {
			if (Input.GetAxis("Mouse ScrollWheel") != 0f && ValidNavigationStart()) {
				// Set to true, since we are moving the camera
				cameraIsMoving = true;

				// Set the camera position to 0.0
				view.position = target;

				// Change distanceToTarget upon scrolling and keep distance between 5 and 15
				zoom = Mathf.Clamp(zoom - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, zoomMinimum, zoomMaximum);

				// Transform 
				view.Translate(new Vector3(0, 0, -zoom));
			}
		}

		public void Target(Vector3 position) {
			target = position;
			view.position = target;
			view.Translate(new Vector3(0, 0, -zoom));
		}
		public void Target(Focus focus) {
			zoom = Math.Min(Math.Max(focus.Distance, zoomMinimum), zoomMaximum);
			Target(focus.Origin);
		}

		public void LockCamera() {
			inputLocked = true;
		}
		public void UnlockCamera() {
			inputLocked = false;
		}
		public void FollowTarget() {
			followTarget = true;
		}
		public void UnfollowTarget() {
			followTarget = false;
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
		private bool ValidNavigationStart() {
			return !Raycast.MouseEvents(ignoredLayers).Any();
		}
	}
}