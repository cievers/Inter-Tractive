using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interface.Control {
	public class DropdownStack : MonoBehaviour {
		public RectTransform container;
		public Stackable wrapper;
		public TMP_Dropdown dropdown;
		public List<TMP_Dropdown.OptionData> options;
		public List<RectTransform> forceUpdates;
		private readonly List<Stackable> entries = new();

		private IEnumerable<TMP_Dropdown> Dropdowns => entries.Select(entry => entry.Contained.GetComponent<TMP_Dropdown>());
		public IEnumerable<int> Keys => Dropdowns.Select(dropdown => dropdown.value);
		public IEnumerable<string> Values => Dropdowns.Select(dropdown => dropdown.options[dropdown.value].text);

		private void Start() {
			ForceUpdates();
		}

		public void Add() {
			var wrap = Instantiate(wrapper, container);
			wrap.Removed += () => Remove(wrap);
			var instance = Instantiate(dropdown, wrap.container);
			instance.options = options;
			instance.onValueChanged.AddListener(_ => Configure());
			entries.Add(wrap);
			ForceUpdates();
			Configure();
		}
		private void Remove(Stackable entry) {
			entries.Remove(entry);
			Destroy(entry.gameObject);
		}

		private void Configure() {
			Configured?.Invoke(Values);
		}

		private void ForceUpdates() {
			// I hate this hack, or the fact that something like it is even needed
			foreach (var element in forceUpdates) {
				LayoutRebuilder.ForceRebuildLayoutImmediate(element);
			}
		}
		
		public delegate void StackUpdate(IEnumerable<string> values);
		public event StackUpdate Configured;
	}
}