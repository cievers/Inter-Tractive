using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Camera;
using Interface.Automation;
using Interface.Paradigm;
using Objects.Sources;
using UnityEngine;

namespace Objects {
	public class SourceSequencer : SourceManager {
		public SourceAutomation template;
		public new OrbitingCamera camera;
		public Photo capture;
		public GameObject successor;

		private const string ROOT = "C:\\Users\\Cas\\Documents\\Computer Science\\Master\\Master Project\\Data\\Evaluation\\";
		private const string INPUT = ROOT + "Input\\";
		private const string OUTPUT = ROOT + "Output\\";

		private static readonly Dictionary<string, Tuple<Vector3, Quaternion>> VIEWS = new() {
			{"AF_left", new(new(-125.198204f,21.4417515f,12.229248f), new(0.0599409305f,0.782457352f,-0.0762485415f,0.615104735f))}, //Strong curves
			{"AF_right", new(new(125.198204f,21.4417515f,12.229248f), new(-0.0599409305f,0.782457352f,-0.0762485415f,-0.615104735f))}, //Strong curves
			{"CA", new(new(19.8660316f,-7.20070267f,52.3194351f), new(-0.0211971235f,0.978531122f,-0.156118274f,-0.132870808f))}, //Thin bundle, core tract too short
			{"CC_1", new(new(-10.0185509f,19.4937019f,73.5510254f), new(0.0376013294f,0.937155962f,-0.330038965f,0.106766976f))}, //Strong symmetrical curve
			{"CC_3", new(new(-21.9025345f,34.4530869f,72.9818954f), new(0.0104872817f,0.989243627f,-0.090882048f,0.114138812f))}, //Clear split in two sections
			{"CC_4", new(new(-20.0818939f,44.611866f,61.4295578f), new(0.0104872817f,0.989243627f,-0.090882048f,0.114138812f))}, //Strong symmetrical curve and fanning
			{"CG_left", new(new(67.3915405f,15.1112804f,22.0241356f), new(-0.0187900122f,-0.787566781f,-0.0240432937f,0.61547333f))}, //Curved fans & extensions
			{"CG_right", new(new(-67.3915405f,15.1112804f,-22.0241356f), new(0.0187900122f,-0.787566781f,-0.0240432937f,-0.61547333f))}, //Curved fans & extensions
			{"FPT_left", new(new(97.8340836f,19.9157448f,32.6554108f), new(-0.0282517709f,0.800134003f,-0.0378049165f,-0.597961724f))}, //A nice median bundle
			{"FPT_right", new(new(-97.8340836f,19.9157448f,32.6554108f), new(0.0282517709f,0.800134003f,-0.0378049165f,0.597961724f))}, //A nice median bundle
			{"ICP_left", new(new(-53.5898857f,-16.4523888f,-12.760334f), new(0.0568623841f,0.914316893f,-0.138103262f,0.376455516f))}, //Strongly curved fanning end
			{"ICP_right", new(new(53.5898857f,-16.4523888f,-12.760334f), new(-0.0568623841f,0.914316893f,-0.138103262f,-0.376455516f))}, //Strongly curved fanning end
			{"IFO_left", new(new(65.2298431f,16.0531483f,-13.5327873f), new(-0.0362181254f,0.726138115f,-0.0383649021f,-0.685521603f))}, //Straight bundle with interesting profile
			{"IFO_right", new(new(-65.2298431f,16.0531483f,-13.5327873f), new(0.0362181254f,0.726138115f,-0.0383649021f,0.685521603f))}, //Straight bundle with interesting profile
			{"MCP", new(new(-5.45059252f,14.852684f,6.8435936f), new(0.0284717493f,0.921806633f,-0.380396992f,0.0689935237f))}, //Strong symmetrical curve with problematic cross-sections and hull
			{"OR_left", new(new(-70.9143829f,37.6498299f,-15.1269989f), new(0.139138848f,0.841248989f,-0.257492393f,0.454574794f))}, //Varied curvature
			{"OR_right", new(new(70.9143829f,37.6498299f,-15.1269989f), new(-0.139138848f,0.841248989f,-0.257492393f,-0.454574794f))}, //Varied curvature
			{"POPT_left", new(new(-118.629669f,2.52603292f,-85.0568085f), new(0.0117775872f,-0.51885587f,-0.00715071894f,-0.854750633f))}, //Strong difference in thickness
			{"POPT_right", new(new(118.629669f,2.52603292f,-85.0568085f), new(-0.0117775872f,0.51885587f,-0.00715071894f,-0.854750633f))}, //Strong difference in thickness
			{"SLF_I_left", new(new(-69.2544785f,66.1066895f,-37.865963f), new(0.175325274f,0.602423728f,-0.137813583f,0.766390324f))}, //Short straight and wide
			{"SLF_I_right", new(new(69.2544785f,66.1066895f,-37.865963f), new(-0.175325274f,0.602423728f,-0.137813583f,-0.766390324f))}, //Short straight and wide
			{"ST_PREF_left", new(new(-114.242233f,55.8007851f,36.9390869f), new(0.108495325f,0.762242258f,-0.13248615f,0.624229908f))}, //Basically just a short fan from a single point
			{"ST_PREF_right", new(new(114.242233f,55.8007851f,36.9390869f), new(-0.108495325f,0.762242258f,-0.13248615f,-0.624229908f))}, //Basically just a short fan from a single point
			{"STR_left", new(new(48.6618462f,44.2534332f,22.4112396f), new(-0.0398129374f,0.890146434f,-0.0792853087f,-0.44695425f))}, //A nice simple bundle with slight curve
			{"STR_right", new(new(-48.6618462f,44.2534332f,22.4112396f), new(0.0398129374f,0.890146434f,-0.0792853087f,0.44695425f))}, //A nice simple bundle with slight curve
			{"T_POSTC_left", new(new(-58.2747688f,57.1660652f,-80.9801636f), new(0.140221402f,0.219812125f,-0.0319440812f,0.964883506f))}, //The iconic testing bundle
			{"T_POSTC_right", new(new(58.2747688f,57.1660652f,-80.9801636f), new(-0.0962854624f,0.259828955f,-0.0260469466f,-0.960489213f))}, //The iconic testing bundle's twin
		};
		private static readonly Dictionary<string, Tuple<Vector3, float, Quaternion>> PERSPECTIVES = new() {
			{"AF_left", new(new(-0.95f, 0.19f, 0.26f), 84.83055f, new(0.05894f, 0.79218f, -0.07750f, 0.60247f))},
			{"AF_right", new(new(0.96f, 0.26f, 0.15f), 83.911f, new(0.08396f, -0.75221f, 0.09773f, 0.64620f))},
			{"CA", new(new(0.37f, 0.33f, 0.87f), 54.31945f, new(-0.03280f, 0.96669f, -0.16288f, -0.19469f))},
			{"CC_1", new(new(-0.10f, 0.49f, 0.87f), 50.21772f, new(0.01445f, 0.96613f, -0.25159f, 0.05548f))},
			{"CC_3", new(new(-0.33f, 0.14f, 0.94f), 67.34836f, new(0.01172f, 0.98359f, -0.06937f, 0.16615f))},
			{"CC_4", new(new(-0.27f, 0.13f, 0.95f), 74.81879f, new(0.00879f, 0.98857f, -0.06376f, 0.13632f))},
			{"CG_left", new(new(0.95f, 0.00f, 0.31f), 86.71033f, new(0.00038f, -0.80983f, 0.00052f, 0.58666f))},
			{"CG_right", new(new(-0.96f, 0.06f, -0.26f), 85.43742f, new(0.02374f, 0.60873f, -0.01822f, 0.79282f))},
			{"FPT_left", new(new(0.97f, 0.17f, 0.19f), 116.7966f, new(0.05423f, -0.77074f, 0.06620f, 0.63137f))},
			{"FPT_right", new(new(-0.97f, 0.13f, 0.20f), 111.1932f, new(0.04237f, 0.77465f, -0.05219f, 0.62881f))},
			{"ICP_left", new(new(-0.63f, 0.35f, 0.70f), 53.47043f, new(0.06321f, 0.91882f, -0.16446f, 0.35316f))},
			{"ICP_right", new(new(0.59f, 0.33f, 0.74f), 57.06561f, new(-0.05431f, 0.93121f, -0.15555f, -0.32512f))},
			{"IFO_left", new(new(0.98f, 0.18f, 0.02f), 91.6585f, new(0.06165f, -0.71007f, 0.06267f, 0.69862f))},
			{"IFO_right", new(new(-0.99f, 0.12f, 0.02f), 95.88039f, new(0.04051f, 0.71134f, -0.04114f, 0.70047f))},
			{"MCP", new(new(-0.08f, 0.57f, 0.82f), 69.63577f, new(0.01433f, 0.95286f, -0.29962f, 0.04558f))},
			{"OR_left", new(new(-0.69f, 0.49f, 0.53f), 66.2591f, new(0.11325f, 0.86622f, -0.22823f, 0.42983f))},
			{"OR_right", new(new(0.70f, 0.45f, 0.55f), 72.44406f, new(-0.10143f, 0.87459f, -0.20826f, -0.42595f))},
			{"POPT_left", new(new(-0.91f, 0.03f, -0.41f), 97.29413f, new(0.01091f, 0.54224f, -0.00704f, 0.84012f))},
			{"POPT_right", new(new(0.91f, -0.03f, -0.41f), 97.29279f, new(-0.01068f, -0.54224f, -0.00690f, 0.84012f))},
			{"SLF_I_left", new(new(-0.87f, 0.42f, -0.25f), 50.69008f, new(0.17041f, 0.58656f, -0.12793f, 0.78137f))},
			{"SLF_I_right", new(new(0.81f, 0.48f, -0.33f), 54.39863f, new(0.20531f, -0.54188f, 0.13853f, 0.80313f))},
			{"ST_PREF_left", new(new(-0.92f, 0.34f, 0.19f), 91.28185f, new(0.10850f, 0.76224f, -0.13248f, 0.62423f))},
			{"ST_PREF_right", new(new(0.93f, 0.34f, 0.13f), 90.48748f, new(0.11329f, -0.74373f, 0.13047f, 0.64576f))},
			{"STR_left", new(new(0.79f, 0.18f, 0.59f), 80.64624f, new(-0.03981f, 0.89015f, -0.07929f, -0.44695f))},
			{"STR_right", new(new(-0.82f, 0.18f, 0.55f), 77.81198f, new(0.04341f, 0.87786f, -0.08109f, 0.47000f))},
			{"T_POSTC_left", new(new(-0.35f, 0.41f, -0.84f), 66.43409f, new(0.20504f, 0.19141f, -0.04093f, 0.95898f))},
			{"T_POSTC_right", new(new(0.31f, 0.36f, -0.88f), 75.05392f, new(0.18148f, -0.16587f, 0.03107f, 0.96881f))},
		};

		private int i = 0;
		private string[] sequence;

		private int j = 0;
		private Func<SourceAutomation, Automation, bool>[] tasks;

		private int delay;
		private bool waiting = true;
		private bool signal = false;
		private SourceAutomation signalSource;
		private Automation signalAutomation;

		private void Start() {
			sequence = System.IO.Directory.EnumerateFiles(INPUT, "*.tck", SearchOption.AllDirectories).ToArray();
			tasks = new Func<SourceAutomation, Automation, bool>[] {
				TaskPerspective,
				TaskNifti,
				TaskSummary,
				TaskCore,
				TaskResetDefaultVisuals,
				TaskBundleCoreVisuals,
				TaskCapture,
				TaskBundleCoreVisuals,
				TaskBundlePerimeterVisuals,
				TaskDelay,
				TaskCapture,
				TaskBundlePerimeterVisuals,
				TaskBundleThicknessVisuals,
				TaskCapture,
				TaskBundleThicknessVisuals,
				TaskBundleVolumeVisuals,
				TaskCapture,
				TaskBundleVolumeVisuals,
				TaskBundleMapVisuals,
				TaskCapture,
				TaskCompletion
			};
			Source(sequence[i]);
		}
		private void Update() {
			if (delay > 0) {
				delay--;
				if (delay == 0) {
					signal = true;
				}
			}
			if (signal) {
				signal = false;
				Task(signalSource, signalAutomation);
			}
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
			if (waiting && loaded) {
				// Task(source, automation);
				waiting = false;
				signal = true;
				signalSource = source;
				signalAutomation = automation;
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
					waiting = true;
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
		private bool TaskFocusListing(SourceAutomation source, Automation automation) {
			var pair = VIEWS[source.Name()];
			source.Focused += focus => TaskFocusLog(pair.Item1, pair.Item2, focus);
			source.Focus();
			return false;
		}
		private static void TaskFocusLog(Vector3 origin, Quaternion direction, Focus focus) {
			Debug.Log(origin);
			Debug.Log(direction);
			Debug.Log(focus.Origin);
			Debug.Log(focus.Distance);
			Debug.Log((origin - focus.Origin).normalized);
			Debug.Log((origin - focus.Origin).magnitude);
			Debug.Log(Quaternion.LookRotation(focus.Origin - origin, Vector3.up));
			Debug.Log("----");
		}
		private bool TaskView(SourceAutomation source, Automation automation) {
			var pair = VIEWS[source.Name()];
			camera.View(pair.Item1, pair.Item2);
			return false;
		}
		private bool TaskPerspective(SourceAutomation source, Automation automation) {
			source.Focused += focus => TaskPerspectiveApplication(PERSPECTIVES[source.Name()], focus);
			source.Focus();
			return false;
		}
		private void TaskPerspectiveApplication(Tuple<Vector3, float, Quaternion> perspective, Focus focus) {
			camera.View(focus.Origin + perspective.Item1 * (perspective.Item2 * 1.5f), perspective.Item3);
		}
		private bool TaskDelay(SourceAutomation source, Automation automation) {
			delay = 10;
			signalSource = source;
			signalAutomation = automation;
			return true;
		}
		private bool TaskCapture(SourceAutomation source, Automation automation) {
			source.Write(capture.Publication(), "png");
			return false;
		}
		private bool TaskViridis(SourceAutomation source, Automation automation) {
			return false;
		}
		private bool TaskToggleAllVisuals(SourceAutomation source, Automation automation) {
			automation.Action("Tracts").Automate();
			automation.Action("Global measuring/Mean").Automate();
			automation.Action("Global measuring/Span").Automate();
			automation.Action("Global measuring/Minimum diameter").Automate();
			automation.Action("Global measuring/Endpoint diameter").Automate();
			automation.Action("Global measuring/Cross-section").Automate();
			automation.Action("Global measuring/Volume").Automate();
			automation.Action("Local measuring/Map").Automate();
			return false;
		}
		private bool TaskResetDefaultVisuals(SourceAutomation source, Automation automation) {
			automation.Action("Tracts").Automate();
			automation.Action("Global measuring/Mean").Automate();
			automation.Action("Local measuring/Map").Automate();
			return false;
		}
		private bool TaskBundleCoreVisuals(SourceAutomation source, Automation automation) {
			automation.Action("Tracts").Automate();
			automation.Action("Global measuring/Mean").Automate();
			automation.Action("Global measuring/Minimum diameter").Automate();
			return false;
		}
		private bool TaskBundlePerimeterVisuals(SourceAutomation source, Automation automation) {
			automation.Action("Tracts").Automate();
			automation.Action("Global measuring/Span").Automate();
			automation.Action("Global measuring/Endpoint diameter").Automate();
			return false;
		}
		private bool TaskBundleThicknessVisuals(SourceAutomation source, Automation automation) {
			automation.Action("Global measuring/Mean").Automate();
			automation.Action("Global measuring/Cross-section").Automate();
			return false;
		}
		private bool TaskBundleVolumeVisuals(SourceAutomation source, Automation automation) {
			automation.Action("Tracts").Automate();
			automation.Action("Global measuring/Volume").Automate();
			return false;
		}
		private bool TaskBundleMapVisuals(SourceAutomation source, Automation automation) {
			automation.Action("Local measuring/Map").Automate();
			return false;
		}
		private bool TaskLowResolution(SourceAutomation source, Automation automation) {
			automation.Range("Local measuring/Resolution").Simulate(0.75f);
			return true;
		}
		private bool TaskNifti(SourceAutomation source, Automation automation) {
			automation.Action("Exporting/Map").Automate();
			return false;
		}
		private bool TaskSummary(SourceAutomation source, Automation automation) {
			automation.Action("Exporting/Numerical summary").Automate();
			return false;
		}
		private bool TaskCore(SourceAutomation source, Automation automation) {
			automation.Action("Exporting/Core tract").Automate();
			return false;
		}
		private bool TaskCompletion(SourceAutomation source, Automation automation) {
			source.AutomateClose();
			SourceCompletion();
			return true;
		}
	}
}