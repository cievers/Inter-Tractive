using System;
using Interface.Control;
using UnityEngine;

namespace Interface {
	// This sounds like something Unity's scriptable objects are the intended solution for, if I know how they worked
	public class ControlStyles : MonoBehaviour {
		public ActionToggle toggle;
		public Slider slider;
		public DelayedSlider delayedSlider;
		public SmoothStepper stepper;
		public Control.Evaluation evaluation;

		public void Construct(Transform parent, Controller data) {
			switch (data) {
				case ActionToggle.Data toggleData:
					toggle.Construct(parent, toggleData);
					break;
				case DelayedSlider.Data delayedSliderData:
					delayedSlider.Construct(parent, delayedSliderData);
					break;
				case Slider.Data sliderData:
					slider.Construct(parent, sliderData);
					break;
				case Stepper.Data stepperData:
					stepper.Construct(parent, stepperData);
					break;
				case Control.Evaluation.Data evaluationData:
					evaluation.Construct(parent, evaluationData);
					break;
				default:
					throw new NotSupportedException("No interface element is implemented for controller type " + typeof(Controller));
			}
		}
	}
}