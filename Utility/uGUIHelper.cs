using UnityEngine;
using System.Collections;
using ProtoTurtle.BitmapDrawing;

namespace uNodeEditor
{
	public class GUIHelper
	{

		static GUIHelper()
		{
			Initialize();
		}

		private static Texture2D lineTex = null;
		private static void Initialize()
		{

			if (lineTex == null)
			{
				lineTex = new Texture2D(32, 32, TextureFormat.ARGB32, false);
				//lineTex.SetPixel(0, 1, Color.white);
				lineTex.Apply();
			}
		}

		/// <summary>
		/// Rectの最大値/最小値が逆転しないようにする
		/// </summary>
		/// <param name="rect"></param>
		public static void RectAdjust(ref Rect rect)
		{

			// 画面外クリップ
			//if (rect.xMax > window.xMax) rect.xMax = window.xMax;
			//if (rect.yMax > window.yMax) rect.yMax = window.yMax;

			// 逆方向に選択したら判定できなくなるので矩形範囲フリップ 
			if (rect.xMin > rect.xMax)
			{
				float tmp = rect.xMin;
				rect.xMin = rect.xMax;
				rect.xMax = tmp;
			}
			if (rect.yMin > rect.yMax)
			{
				float tmp = rect.yMin;
				rect.yMin = rect.yMax;
				rect.yMax = tmp;
			}
		}


		public struct Line
		{
			public Line(Vector2 s, Vector2 e) { start = s; end = e; }
			public Vector2 start;
			public Vector2 end;
		}
		
		static public void DrawText(Vector2 pos,string text, Color color, int size = 12)
		{

			Color tmpColor = GUI.color;

			GUIStyle style = new GUIStyle();
			GUIStyleState styleState = new GUIStyleState();
			styleState.textColor = color;   // 文字色の変更.
			style.normal = styleState;
			style.fontSize = size;
			GUI.color = new Color(1, 1, 1, 1);

			GUI.Label(new Rect(pos, new Vector2(text.Length * size, size)), text, style);

			GUI.color = tmpColor;

		}

		/// <summary>
		/// 矩形塗りつぶし
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="color"></param>
		static public void Fill(Rect rect, Color color)
		{
			if (rect.xMax == 0 || rect.yMax == 0) { return; }
			
			Color tmp = GUI.color;
#if true
			Drawing.FillRect(rect,color);
#else
			// GUIを使ってるのでOnDrawGUIで描画しないと他のボタンとかが反応しなくなることが...何それ。
			//Texture2D tex = GUI.skin.box.normal.background;

			GUI.color = color;
			//GUI.Box(rect, "");

			//Texture2D texture = new Texture2D(1, 1);
			//texture.SetPixel(0, 0, color);
			//texture.Apply();
			//GUI.skin.box.normal.background = texture;
			GUI.Box(rect, GUIContent.none);

			//GUI.skin.box.normal.background = tex;
			
#endif
			GUI.color = tmp;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="color"></param>
		/// <param name="thickness"></param>
		static public void DrawRect(Rect rect, Color color, int thickness = 1)
		{


			if ((rect.xMax - rect.xMin) == 0 || (rect.yMax - rect.yMin) == 0) { return; }

			Line[] lines = new Line[]{ 
					new Line(rect.position+new Vector2(0, 0), rect.position + new Vector2(rect.width, 0)),
					new Line(rect.position+new Vector2(0,0), rect.position + new Vector2(0, rect.height)),
					new Line(rect.position + new Vector2(rect.width, 0), rect.position + new Vector2(rect.width, rect.height)),
					new Line(rect.position + new Vector2(0, rect.height), rect.position + new Vector2(rect.width, rect.height))
			};

			foreach (var line in lines)
			{
				DrawLine(line.start, line.end, color, thickness, true);
			}

		}

		/// <summary>
		/// http://forum.unity3d.com/threads/17066-How-to-draw-a-GUI-2D-quot-line-quot?p=407005&viewfull=1#post407005
		/// →処理ちょっと重かったので、yoyo氏のに変更
		/// http://forum.unity3d.com/threads/71979-Drawing-lines-in-the-editor
		/// </summary>
		/// <param name="lineStart"></param>
		/// <param name="lineEnd"></param>
		/// <param name="color"></param>
		/// <param name="thickness"></param>
		/// <param name="antiAlias"></param>
		public static void DrawLine(Vector2 lineStart, Vector2 lineEnd, Color color, int thickness = 1, bool antiAlias = false)
		{
			Drawing.DrawLine(lineStart, lineEnd, color, thickness, antiAlias);
		}

		public static void DrawCircle(Vector2 center, float radius, Color color)
		{
			BitmapDrawingExtensions.DrawCircle(lineTex, (int)center.x, (int)center.y, (int)radius, color);
		}


	}

}