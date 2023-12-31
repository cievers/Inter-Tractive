﻿using System;
using Interface.Content;
using Interface.Control;
using UnityEngine;

namespace Interface {
	// This sounds like something Unity's scriptable objects are the intended solution for, if I know how they worked
	public class Theming : MonoBehaviour {
		public Folder folder;
		public Description description;
		public Divider divider;
		public Loader loader;
		
		public ActionToggle toggle;
		public Slider slider;
		public TransformedSlider transformedSlider;
		public SmoothStepper stepper;
		public Dropdown dropdown;
		public Control.Evaluation evaluation;
		public Exporter exporter;

		public void Construct(RectTransform parent, Component data) {
			switch (data) {
				case Folder.Data folderData:
					folder.Construct(parent, folderData, this);
					break;
				case Description.Data descriptionData:
					description.Construct(parent, descriptionData);
					break;
				case Divider.Data dividerData:
					divider.Construct(parent, dividerData);
					break;
				case Loader.Data loaderData:
					loader.Construct(parent, loaderData, this);
					break;
				case ActionToggle.Data toggleData:
					toggle.Construct(parent, toggleData);
					break;
				case TransformedSlider.Data sliderData1:
					transformedSlider.Construct(parent, sliderData1);
					break;
				case Slider.Data sliderData2:
					slider.Construct(parent, sliderData2);
					break;
				case Stepper.Data stepperData:
					stepper.Construct(parent, stepperData);
					break;
				case Control.Data.Coloring dropdownColoring:
					dropdown.Construct(parent, dropdownColoring);
					break;
				case Control.Evaluation.Data evaluationData:
					evaluation.Construct(parent, evaluationData);
					break;
				case Exporter.Data exporterData:
					exporter.Construct(parent, exporterData);
					break;
				default:
					throw new NotSupportedException("No interface element is implemented for component type " + data.GetType());
			}
		}
	}
}