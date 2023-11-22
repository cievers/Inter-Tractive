using System.Collections.Generic;
using Camera;
using Evaluation.Coloring;
using Evaluation.Coloring.Gradients;
using Files.Types;
using Geometry.Generators;
using Interface.Control;
using UnityEngine;

namespace Objects.Sources {
	public class Volume : Voxels {
		public MeshFilter mesh;
		
		private Nii<float> volume;
		private Coloring coloring;

		protected override void New(string path) {
			volume = Nii<float>.Load(path);
			coloring = new Grayscale();
			UpdateMap();
			Focus(new Focus(Vector3.zero, 0));
		}
		private void UpdateMap() {
			mesh.mesh = new GridRenderer(0).Render(volume, coloring).Mesh();
		}
		private void UpdateEvaluation(Coloring coloring) {
			this.coloring = coloring;
			UpdateMap();
		}
		
		public override IEnumerable<Interface.Component> Controls() {
			return new Interface.Component[] {
				new ActionToggle.Data("Map", true, mesh.gameObject.SetActive),
				new Interface.Control.Data.Coloring(new Dictionary<string, Coloring> {
					{"Grayscale", new Grayscale()},
					{"Temperature", new Temperature()},
					{"Plasma", new Plasma()},
					{"Viridis", new Viridis()}
				}, UpdateEvaluation)
			};
		}
		
		public override Map Map() {
			throw new System.NotImplementedException();
		}
	}
}