using System.Collections.Generic;
using Geometry;
using Geometry.Generators;
using Geometry.Tracts;
using UnityEngine;
using UnityEngine.Serialization;
using Plane = Geometry.Plane;

namespace Objects {
	public class ProjectTest : MonoBehaviour {
		public MeshFilter plane;
		public MeshFilter lineMesh;
		[FormerlySerializedAs("polygon")] public MeshFilter polygonMesh;
		public MeshFilter a;
		public MeshFilter b;
		public GameObject dot;

		public Vector3 planeOrigin = new Vector3(0, 0, 0);
		public Vector3 planeNormal = new Vector3(0, 0, 1);
		public Vector3 lineOrigin = new Vector2(1, -1);
		public Vector3 lineDirection = new Vector2(0, 1);
		
		private void Update() {
			var points = new List<Vector3> {
				new(0, 0, 19),
				new(2, 9, 13),
				new(4, 13, 15),
				new(8, 17, 12),
				new(7, 8, 10),
				new(9, 19, 15),
				new(6, 14, 19),
				new(5, 12, 11),
				new(4, 10, 10),
				new(8, 11, 20),
				new(4, 12, 20),
				new(2, 4, 12),
				new(1, 1, 11),
				new(8, 10, 12),
				new(3, 15, 20),
				new(5, 9, 17),
				new(2, 11, 18),
				new(6, 0, 10),
			};
			// var line = new Line(points[3], points[12] - points[3]);
			// var origin = new Vector3(5, 10, 15);
			// var normal = new Vector3(1, 1, 1);

			// var points = new List<Vector3> {
			// 	new(0, 0),
			// 	new(2, 0),
			// 	new(0, 1),
			// 	new(2, 1),
			// };
			var line = new Line(lineOrigin, lineDirection);
			var polygon = new ConvexPolygon(points, planeOrigin, planeNormal);
			
			// Instantiate(dot, line.Origin, Quaternion.identity);
			// Instantiate(dot, line.Origin + line.Direction, Quaternion.identity);
			plane.mesh = Plane.Mesh(planeOrigin, planeNormal, 20);
			lineMesh.mesh = new WireframeRenderer().Render(new ArrayTract(new[] {line.Origin, line.Origin + line.Direction}));
			polygonMesh.mesh = polygon.Hull().Mesh();
			
			var split = polygon.Split(line);
			
			a.mesh = split[0].Hull().Mesh();
			b.mesh = split[1].Hull().Mesh();
		}
	}
}