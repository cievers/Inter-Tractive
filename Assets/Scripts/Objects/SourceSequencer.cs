using System;
using System.IO;
using System.Linq;
using Camera;
using Interface.Automation;
using Interface.Paradigm;
using Objects.Sources;
using UnityEngine;
using UnityEngine.Windows;

namespace Objects {
	public class SourceSequencer : SourceManager {
		public SourceAutomation template;
		public new OrbitingCamera camera;
		public Photo capture;
		public GameObject successor;

		private const string ROOT = "C:\\Users\\Cas\\Documents\\Computer Science\\Master\\Master Project\\Data\\Evaluation\\";
		private const string INPUT = ROOT + "Input\\";
		private const string OUTPUT = ROOT + "Output\\";

		private int i = 0;
		private string[] sequence;

		private int j = 0;
		private Func<SourceAutomation, Automation, bool>[] tasks;

		private void Start() {
			sequence = System.IO.Directory.EnumerateFiles(INPUT, "*.tck", SearchOption.AllDirectories).ToArray();
			tasks = new Func<SourceAutomation, Automation, bool>[] {
				TaskAutomationListing,
				TaskViewPostCRight,
				TaskCapture,
				TaskLowResolution,
				TaskNifti,
				TaskCompletion
			};
			Source(sequence[i]);
		}
		
		private void Source(string path) {
			try {
				var source = Instantiate(template, transform);
				var automation = source.Automate(path, path.Replace(INPUT, OUTPUT));
				
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
			if (i < sequence.Length) {
				Source(sequence[i]);
			} else {
				Debug.Log("Completed the sequence of automated tasks");
				gameObject.SetActive(false);
				successor.SetActive(true);
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
		private bool TaskViewPostCRight(SourceAutomation source, Automation automation) {
			camera.View(new Vector3(-58.2747688f,57.1660652f,-80.9801636f), new Quaternion(0.140221402f,0.219812125f,-0.0319440812f,0.964883506f));
			return false;
		}
		private bool TaskCapture(SourceAutomation source, Automation automation) {
			// capture.Capture(source.Output);
			return false;
		}
		private bool TaskLowResolution(SourceAutomation source, Automation automation) {
			automation.Range("Rendering/Resolution").Simulate(0.75f);
			return true;
		}
		private bool TaskNifti(SourceAutomation source, Automation automation) {
			automation.Action("Exporting/Map").Automate();
			return false;
		}
		private bool TaskCompletion(SourceAutomation source, Automation automation) {
			source.AutomateClose();
			SourceCompletion();
			return true;
		}
	}
}