using UnityEngine;
using UnityEditor;

namespace uNodeEditor
{


	public class GridDrawer : uControlBase
	{
		#region constant param
		public const float GRID_INTERVAL = 25.0f;

		#endregion


		// グリッドのズームレート
		public float m_GridZoomRate = 1.0f;
		// ズーム差分
		public float m_GridZoomDelta = 0.0f;
		// グリッドサイズ
		public int gridSize = (int)GRID_INTERVAL;
		// 前回のグリッドサイズ
		public int preGridSize = (int)GRID_INTERVAL;

		// グリッドの基準点
		public Vector2 GridCenterPoint {get;set;}
		// ズーム基準点
		public Vector2 GridZoomCenterPoint { get; set; }
		
		// 親ウィドウ
		uWindow window;
		// マウス
		MouseData mouseData { get { return window.mouseData; } }

		public GridDrawer(uWindow win)
		{
			window = win;
			UpdateGridSize();

		}

		/// <summary>
		/// 初期化
		/// </summary>
		public override void OnInit()
		{

			// ズーム中心初期化
			GridZoomCenterPoint = new Vector2(window.position.width/2,window.position.height/2);
		}

		/// <summary>
		/// 更新
		/// </summary>
		public override void OnUpdate()
		{

			// グリッド操作
			UpdateGrid();



		}

		/// <summary>
		/// 描画
		/// </summary>
		public override void OnPaint()
		{

			// グリッド描画
			DrawGrid();


		}

		/// <summary>
		/// テキスト描画/ボタン描画等
		/// </summary>
		public override void OnDrawGUI()
		{

			// グリッド中心
			Rect center = new Rect(new Vector2(-10, -10) + GridCenterPoint, new Vector2(20, 20));
			GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
			GUI.Box(center, "+");

			Rect zoomcenter = new Rect(new Vector2(-5, -5) + GridZoomCenterPoint, new Vector2(5, 5));
			GUI.color = new Color(1, 1, 1, 0.2f);
			GUI.Box(zoomcenter, "+");
			

			DrawMouseCorsor();

			EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.Width(40));
			uLabelField.DrawText((mouseData.pos - GridCenterPoint).ToString() + ":" + m_GridZoomRate.ToString(), Color.white);


			uLabelField.DrawText(GridCenterPoint.ToString(), Color.white);
			EditorGUILayout.EndVertical();


			if (Event.current.type== EventType.KeyDown && Event.current.keyCode == KeyCode.R)
			{
				GridCenterPoint = new Vector2(0, 0);

			}
		}

		void UpdateGridSize()
		{
			// グリッドサイズ
			m_GridZoomRate = Mathf.Clamp(m_GridZoomRate, 0.2f, 2.0f);
			preGridSize = gridSize;
			gridSize = (int)((GRID_INTERVAL) * m_GridZoomRate);

			mouseData.zoom = m_GridZoomRate;
		}

		/// <summary>
		/// グリッド操作
		/// </summary>
		void UpdateGrid()
		{

			float preZoomRate = m_GridZoomRate;
			m_GridZoomDelta = 0.0f;
			if (mouseData.IsScroll())
			{
				if (Mathf.Abs(mouseData.delta.y) >= 1)
				{

					m_GridZoomDelta = mouseData.delta.y > 0 ? -0.1f : 0.1f;
					m_GridZoomRate += m_GridZoomDelta;
					
					UpdateGridSize();

					if (preZoomRate == m_GridZoomRate) m_GridZoomDelta = 0.0f;

					//GridZoomCenterPoint = mouseData.pos;

					Vector2 gridPos = (GridZoomCenterPoint - GridCenterPoint) / preGridSize;
					GridCenterPoint += gridPos * (preGridSize-gridSize);

					

					//Debug.Log(gridPos.ToString());



				}
			}

			


			if (mouseData.IsDrag(MouseButton.Middle))
			{
				Vector2 delta = mouseData.delta;
				GridCenterPoint += delta;

				// 中クリック移動中も選択中のノードにズーム中心に
				//if (window.selectedNode.Active)
				//{
				//	GridZoomCenterPoint += delta;
				//}
			}
		}

		/// <summary>
		/// カーソル
		/// </summary>
		void DrawMouseCorsor()
		{
			// マウス位置
			Color col = new Color(1, 1, 1, 0.2f);
			GUIHelper.Fill(mouseData.rect,col);

		}

		/// <summary>
		/// グリッド表示
		/// </summary>
		void DrawGrid()
		{

			

			float height = window.position.height;


			int MARGIN = 4;
			//GUI.color = new Color(0,0,0,1);

			// 横軸
			int maxw = (int)(window.position.width / gridSize);
			for (int i = -MARGIN; i < maxw + MARGIN; i++)
			{
				bool thick = (i % MARGIN == 0);

				float offset = GridCenterPoint.x % (gridSize * MARGIN);
				Vector2 start = new Vector2(gridSize * i + offset, 0);
				Vector2 end = new Vector2(gridSize * i + offset, height);

				GUIHelper.DrawLine(start, end, thick ? Color.black : new Color(0.8f, 0.8f, 0.8f, 0.2f), thick ? 1 : 1);
			}

			// 縦軸
			int maxh = (int)(window.position.height / gridSize);
			for (int j = -MARGIN; j < maxh + MARGIN; j++)
			{
				bool thick = (j % MARGIN == 0);

				float offset = GridCenterPoint.y % (gridSize * MARGIN);
				Vector2 start = new Vector2(0, gridSize * j + offset);
				Vector2 end = new Vector2(window.position.width, gridSize * j + offset);

				GUIHelper.DrawLine(start, end, thick ? Color.black : new Color(0.8f, 0.8f, 0.8f, 0.2f), thick ? 1 : 1);
			}

		}



	}

}
