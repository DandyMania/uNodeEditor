using UnityEngine;
using UnityEditor;


namespace uNodeEditor
{
	public class uLabelField : uControlBase
	{

		public string Text { set; get; }
		public Color Color { set; get; }
		public int Size { set; get; }

		public uLabelField(string text, Color color, int size = 12)
		{
			Text = text;
			Color = color;
			Size = size;

		}



		/// <summary>
		/// 更新
		/// </summary>
		public override void OnUpdate()
		{

		}
		/// <summary>
		/// 描画
		/// </summary>
		public override void OnPaint()
		{
		}
		/// <summary>
		/// GUI描画
		/// </summary>
		public override void OnDrawGUI()
		{
			DrawText(Text, Color,Size);
		}

		static public void DrawText(string text, Color color, int size = 12)
		{

			Color tmpColor = GUI.color;

			GUIStyle style = new GUIStyle();
			GUIStyleState styleState = new GUIStyleState();
			styleState.textColor = color;   // 文字色の変更.
			style.normal = styleState;
			style.fontSize = size;
			GUI.color = new Color(1, 1, 1, 1);

			EditorGUILayout.LabelField(text, style);

			GUI.color = tmpColor;

		}

	}
}
