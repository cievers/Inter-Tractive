using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

namespace Interface.Control {
	public class Dropdown : MonoBehaviour {
		public TMP_Dropdown dropdown;

		public Dropdown Construct<T>(RectTransform parent, Data<T> data) {
			var instance = Instantiate(this, parent);

			instance.dropdown.options = data.options.Keys.Select(name => new TMP_Dropdown.OptionData(name)).ToList();
			instance.dropdown.onValueChanged.AddListener(_ => data.update.Invoke(instance.dropdown.options[instance.dropdown.value].text));
			
			return instance;
		}
		
		public record Data<T> : Component {
			public readonly Dictionary<string, T> options;
			public readonly Action<string> update;

			public Data(Dictionary<string, T> options, Action<T> update) {
				this.options = options;
				this.update = s => update.Invoke(options[s]);
			}
		}
	}
}