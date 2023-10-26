using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Evaluation.Geometric;
using Files.Types;
using Geometry;
using Geometry.Generators;
using Geometry.Tracts;

namespace Objects.Sources.Progressive {
	public record Summary {
		public float? CoreLength {get; set;}
		public float? CoreSpan {get; set;}
		public float[] CrossSectionAreas {get; set;}
		public float[] CrossSectionPerimeters {get; set;}
		public float? SurfaceContinuous {get; set;}
		public float? SurfaceVoxels {get; set;}
		public float? VolumeContinuous {get; set;}
		public float? VolumeRiemann {
			get {
				if (CoreLength == null || CrossSectionAreas == null) {
					return null;
				}
				return CrossSectionAreas.Select(area => area * ((float) CoreLength / CrossSectionAreas.Length)).Sum();
			}
		}
		public float? VolumeVoxels {get; set;}

		public void Core(Tract core) {
			CoreLength = new Length().Measure(core);
			CoreSpan = new Span().Measure(core);
		}
		public void CrossSections(IEnumerable<ConvexPolygon> cuts) {
			var array = cuts as ConvexPolygon[] ?? cuts.ToArray();
			CrossSectionAreas = array.Select(cut => cut.Area()).ToArray();
			CrossSectionPerimeters = array.Select(cut => cut.Perimeter()).ToArray();
		}
		// public void CrossSectionsVolume(Tract core, IEnumerable<ConvexPolygon> cuts) {
		// 	var length = new Length().Measure(core);
		// 	var segment = length / cuts.Count();
		// }
		public void Volume(Hull hull) {
			var surface = 0f;
			var volume = 0f;
			for (var i = 0; i < hull.Indices.Length; i += 3) {
				surface += Triangle.Area(
					hull.Vertices[hull.Indices[i]],
					hull.Vertices[hull.Indices[i+1]],
					hull.Vertices[hull.Indices[i+2]]
				);
				volume += Triangle.SignedVolume(
					hull.Vertices[hull.Indices[i]],
					hull.Vertices[hull.Indices[i+1]],
					hull.Vertices[hull.Indices[i+2]]
				);
			}
			SurfaceContinuous = surface;
			VolumeContinuous = volume;
		}

		public Json Json() {
			return new Json(JsonSerializer.Serialize(this));
		}
	}
}