using System;
using Camera;
using Interface.Automation;
using Interface.Paradigm;
using Objects.Sources;
using UnityEngine;

namespace Objects {
	public class SourceSequencer : SourceManager {
		public SourceAutomation template;
		public new OrbitingCamera camera;

		private int i = 0;
		private static readonly string[] SEQUENCE = {
			"C:\\Users\\Cas\\Documents\\Computer Science\\Master\\Master Project\\Data\\Tract\\TOM\\T_POSTC_left.tck",
			"C:\\Users\\Cas\\Documents\\Computer Science\\Master\\Master Project\\Data\\Tract\\TOM\\T_POSTC_right.tck"
		};

		private int j = 0;
		private Func<SourceAutomation, Automation, bool>[] tasks;

		private void Start() {
			tasks = new Func<SourceAutomation, Automation, bool>[] {
				TaskAutomationListing,
				TaskLowResolution,
				TaskCompletion
			};
			Source(SEQUENCE[i]);
		}
		
		private void Source(string path) {
			try {
				var source = Instantiate(template, transform);
				var automation = source.Automate(path);
				
				Collect(source);
				Interact(source);

				source.Loaded += loaded => SourceLoaded(source, automation, loaded);
				source.Focused += camera.Target;
				if (sources.Count == 1) {
					source.Focus();
				}
			} catch (UnityException) {
				
			}
		}
		private void SourceLoaded(SourceAutomation source, Automation automation, bool loaded) {
			if (loaded) {
				Task(source, automation);
			}
		}
		private void SourceCompletion() {
			i++;
			j = 0;
			if (i < SEQUENCE.Length) {
				Source(SEQUENCE[i]);
			} else {
				Debug.Log("Completed the sequence of automated tasks");
			}
		}

		private void Task(SourceAutomation source, Automation automation) {
			while (j < tasks.Length) {
				// Ask each task to return a boolean representing whether it completes after the instance is done loading or instantaneously (true and false respectively)
				// And thank the magic of postincrement for letting it all be one line statement
				if (tasks[j++].Invoke(source, automation)) {
					break;
				}
			}
		}
		private bool TaskAutomationListing(SourceAutomation source, Automation automation) {
			foreach (var component in automation.Component<Controller>()) {
				Debug.Log(component);
			}
			return false;
		}
		private bool TaskLowResolution(SourceAutomation source, Automation automation) {
			automation.Range("Rendering/Resolution").Simulate(0.75f);
			return true;
		}
		private bool TaskCompletion(SourceAutomation source, Automation automation) {
			source.AutomateClose();
			SourceCompletion();
			return true;
		}
	}
}