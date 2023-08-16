using System;
using Interface.Content;
using Interface.Control;
using UnityEngine;

namespace Interface {
	// This sounds like something Unity's scriptable objects are the intended solution for, if I know how they worked
	public class Theming : MonoBehaviour {
		public Description description;
		public Divider divider;
		
		public ActionToggle toggle;
		public Slider slider;
		public TransformedSlider transformedSlider;
		public SmoothStepper stepper;
		public Control.Evaluation evaluation;

		public void Construct(RectTransform parent, Component data) {
			switch (data) {
				case Description.Data descriptionData:
					description.Construct(parent, descriptionData);
					break;
				case Divider.Data dividerData:
					divider.Construct(parent, dividerData);
					break;
				case ActionToggle.Data toggleData:
					toggle.Construct(parent, toggleData);
					break;
				case TransformedSlider.Data sliderData:
					transformedSlider.Construct(parent, sliderData);
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
					throw new NotSupportedException("No interface element is implemented for component type " + data.GetType());
			}
		}
	}
}