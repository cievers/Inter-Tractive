using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utility {
	public static class Raycast {
		public static IEnumerable<RaycastResult> MouseEvents() {
			// Gets all event system raycast results of current pointer position.
			List<RaycastResult> output = new List<RaycastResult>();
			EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) {position = Input.mousePosition}, output);
			return output;
		}
		public static IEnumerable<RaycastResult> MouseEvents(LayerMask layers) {
			return MouseEvents().Where(result => ((layers >> result.gameObject.layer) & 1) != 0);
		}
	}
}