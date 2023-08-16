using TMPro;
using UnityEngine;

namespace Interface.Content {
	public class Description : MonoBehaviour {
		public TextMeshProUGUI text;
		
		public Description Construct(Transform parent, Data description) {
			var instance = Instantiate(this, parent);
			instance.text.text = description.Text;
			return instance;
		}

		public record Data(string Text) : Component {
			public string Text {get;} = Text;
		}
	}
}