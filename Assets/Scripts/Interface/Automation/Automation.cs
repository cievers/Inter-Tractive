using System;
using System.Collections.Generic;
using System.Linq;

namespace Interface.Automation {
	public class Automation {
		private readonly Dictionary<string, Component> components;

		public Automation(IEnumerable<Component> components) {
			this.components = components.SelectMany(Unfold).ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
		}
		private static IEnumerable<Tuple<string, Component>> Unfold(Component component) {
			return component switch {
				Paradigm.Controller controller => new List<Tuple<string, Component>> {new(controller.Name, component)},
				Paradigm.Container container => container.Components.SelectMany(Unfold).Select(tuple => new Tuple<string, Component>(container.Prefix + tuple.Item1, tuple.Item2)),
				_ => new List<Tuple<string, Component>>()
			};
		}

		public IEnumerable<T> Component<T>() {
			return components.Values.OfType<T>();
		}
		public T Component<T>(string name) {
			return (T) components[name];
		}

		public Action Action(string name) {
			return new Action(Component<Paradigm.Action>(name));
		}
		public Range Range(string name) {
			return new Range(Component<Paradigm.Range<float>>(name));
		}
		public Value<T> Value<T>(string name) {
			return new Value<T>(Component<Paradigm.Value<T>>(name));
		}
	}
}