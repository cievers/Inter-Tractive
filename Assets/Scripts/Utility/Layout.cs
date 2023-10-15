using UnityEngine;
using UnityEngine.UI;

namespace Utility {
	public static class Layout {
		public static void Fix(RectTransform container) {
			// I hate this hack, or the fact that something like it is even needed
			var element = container.transform;
			while (element != null && element.GetComponent<ContentSizeFitter>() != null) {
				LayoutRebuilder.ForceRebuildLayoutImmediate(element.GetComponent<RectTransform>());
				element = element.parent;
			}
		}
	}
}