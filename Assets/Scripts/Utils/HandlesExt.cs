using UnityEngine;

namespace UnityEditor
{
	public static class HandlesExt
	{
		private static Material _material;

		public static void GLColor(Color color)
		{
			if (_material == null)
			{
				Shader shader = Shader.Find("Hidden/Internal-Colored");

				_material = new Material(shader);
				_material.hideFlags = HideFlags.HideAndDontSave;
				_material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				_material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				_material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
				_material.SetInt("_ZWrite", 0);
				_material.SetInt("_ZTest", 0);
			}
			_material.SetColor("_Color", color);

			_material.SetPass(0);
		}
		
		public static void GLDrawLine(Vector3 p1, Vector3 p2)
		{
			GL.Begin(GL.LINES);
			GL.Vertex(p1);
			GL.Vertex(p2);
			GL.End();
		}
		public static void GLDrawSolidRectangle(Vector3[] verts)
		{
			GL.Begin(GL.QUADS);
			GL.Vertex(verts[0]);
			GL.Vertex(verts[1]);
			GL.Vertex(verts[2]);
			GL.Vertex(verts[3]);
			GL.End();
		}
	}
}