using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace uNodeEditor
{


	public enum ConnectAreaType
	{

		AREA_RIGHT = 0,
		AREA_LEFT,
		AREA_TOP,
		AREA_BOTTOM,

		AREA_MAX,
	};

	

	// 接続
	public class ConnectBox
	{
		public const int CONNECT_SIZE = 5;

		public static int HandleIndex = 0;

		public ConnectBox(ConnectAreaType area, string name = "") 
		{ 
			linkName = name;
			connectArea = area;
			boxRect = new Rect(0, 0, CONNECT_SIZE, CONNECT_SIZE);
			handle = HandleIndex;
			HandleIndex++;
		}

		public string linkName;

		public Rect boxRect;

		public int handle;        /// 固有ハンドル

		public ConnectAreaType connectArea { set; get; }
		
		public bool active;
		public bool error;

		public int rename;

		public Color color;
	}

	// ノード
	public class uDraggableNode : uControlBase
	{



		const int WINDOW_DEFAULT_SIZE = 100;
		public static Rect CurrentPos = new Rect(100, 100, WINDOW_DEFAULT_SIZE, WINDOW_DEFAULT_SIZE);
		public static int HandleIndex = 0;

		public string NodeName { set; get; }

		public int handle;        /// 固有ハンドル

		public Rect windowRect;   /// ウィンドウの矩形情報
		public Rect realWindowRect;   /// ズーム無し状態のウィンドウの矩形情報
							/// 

		// 親ウィンドウ
		private uNodeEditor parentWindow;

		// リンク場所
		public List<ConnectBox> connectBoxList = new List<ConnectBox>();

		/// <summary>
		/// 選択状態フラグ
		/// </summary>
		private bool active = false;
		public bool Active
		{
			get {
				return active;
			}
			set 
			{
				if (value)
				{
					GUI.BringWindowToFront(handle);
				}
				else
				{
					//GUI.BringWindowToBack(handle);
				}

				//Debug.Log("セット");
				active = value;
			}

		}


		


		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="name"></param>
		public uDraggableNode(uNodeEditor parent, string name = "Node")
		{
			parent.Contorols.Add(this);

			parentWindow = parent;
			handle = HandleIndex;
			windowRect = CurrentPos;
			NodeName = name;

			GUI.BringWindowToFront(handle);

			HandleIndex++;

			CurrentPos.position += new Vector2(64, 64);
			if (CurrentPos.x > parentWindow.position.width - CurrentPos.width)
			{
				CurrentPos.x = 0;
			}
			if (CurrentPos.y > parentWindow.position.height - CurrentPos.height)
			{
				CurrentPos.y = 0;
			}

			realWindowRect = CurrentPos;
			realWindowRect.width = WINDOW_DEFAULT_SIZE;
			realWindowRect.height = WINDOW_DEFAULT_SIZE;
			
			// 現在のズーム割合からサイズ算出
			windowRect.width = realWindowRect.width * parentWindow.Grid.m_GridZoomRate;
			windowRect.height = realWindowRect.height * parentWindow.Grid.m_GridZoomRate;


			AddConnectBox(new ConnectBox(ConnectAreaType.AREA_LEFT,"in"));
			AddConnectBox(new ConnectBox(ConnectAreaType.AREA_RIGHT,"out"));
			AddConnectBox(new ConnectBox(ConnectAreaType.AREA_RIGHT, "out"));

			//AddConnectBox(new ConnectBox(ConnectAreaType.AREA_TOP, "in"));
			//AddConnectBox(new ConnectBox(ConnectAreaType.AREA_BOTTOM, "out"));
		}

		public void Remove()
		{

			foreach(var con in connectBoxList){
				parentWindow.edgeList.RemoveAll((uNodeEdge edge) => {

					if (edge.edgeStart.connact == con || edge.edgeEnd.connact == con)
					{
						edge.Remove();
						return true;
					}
					return false;
				});
			}

			parentWindow.Contorols.Remove(this);
		}


		int[] areaConnectCount = new int[(int)ConnectAreaType.AREA_MAX];


		const int CONNECT_BOX_INTERVAL = 8;
		void AddConnectBox(ConnectBox con)
		{
			int cnt = areaConnectCount[(int)con.connectArea]++;

			if(cnt > 3){
				if( con.connectArea == ConnectAreaType.AREA_LEFT ||
					con.connectArea == ConnectAreaType.AREA_RIGHT )
				{
					realWindowRect.height += CONNECT_BOX_INTERVAL * (cnt - 3);
					windowRect.height = realWindowRect.height * parentWindow.Grid.m_GridZoomRate;
					

				} 
				if (con.connectArea == ConnectAreaType.AREA_BOTTOM ||
					 con.connectArea == ConnectAreaType.AREA_TOP)
				{
					realWindowRect.width += CONNECT_BOX_INTERVAL * (cnt - 3);
					windowRect.width = realWindowRect.width * parentWindow.Grid.m_GridZoomRate;

				}
			}

			connectBoxList.Add(con);
		}
		void RemoveConnect(ConnectBox con)
		{
			int cnt = areaConnectCount[(int)con.connectArea]--;

			if (cnt > 3)
			{
				if (con.connectArea == ConnectAreaType.AREA_LEFT ||
					con.connectArea == ConnectAreaType.AREA_RIGHT)
				{
					realWindowRect.height += CONNECT_BOX_INTERVAL * (cnt - 3);
					windowRect.height = realWindowRect.height * parentWindow.Grid.m_GridZoomRate;

				}
				if (con.connectArea == ConnectAreaType.AREA_BOTTOM ||
					 con.connectArea == ConnectAreaType.AREA_TOP)
				{
					realWindowRect.width += CONNECT_BOX_INTERVAL * (cnt - 3);
					windowRect.width = realWindowRect.width * parentWindow.Grid.m_GridZoomRate;

				}
			}
			
			
			connectBoxList.Remove(con);
		}

		/// <summary>
		/// 更新
		/// </summary>
		public override void OnUpdate()
		{

			ConnectBox near = null;
	
			{
				foreach (var box in connectBoxList)
				{
					if (box.boxRect.Overlaps(parentWindow.mouseData.rect) && windowRect.Overlaps(parentWindow.mouseData.rect))
					{
						if (near == null ||
							(near.boxRect.center - parentWindow.mouseData.rect.center).magnitude >=
							(box.boxRect.center - parentWindow.mouseData.rect.center).magnitude)
						{
							box.active = true;
							if (near != null)
							{
								// todo:ホントは遠い方をOFFにしたいけどうまくいかないので一旦。。。
								box.active = false;
							}
							near = box;
						}
					}
					else
					{
						box.active = false;
					}

				}
			}


			if (parentWindow.Grid.m_GridZoomRate > 0.4f)
			{
				// エッジ生成
				if (parentWindow.mouseData.IsDown(MouseButton.Left))
				{
					if (near != null)
					{
						if (parentWindow.ActiveEdge == null)
						{
							parentWindow.EdgeStart(this, near);
						}
						else
						{
							parentWindow.EdgeEnd(this, near);
						}
					}

				}
			}

			
			

			// グリッド移動
			if (parentWindow.mouseData.IsDrag(MouseButton.Middle))
			{
				windowRect.position += parentWindow.mouseData.delta;
			}
			// ズーム

			Vector2 gridPos = (windowRect.position - parentWindow.Grid.GridZoomCenterPoint) / parentWindow.Grid.preGridSize;
			if (parentWindow.mouseData.IsScroll())
			{
				// ズーム率1.0のサイズにズーム率かけてサイズ算出
				windowRect.width = realWindowRect.width * parentWindow.Grid.m_GridZoomRate;
				windowRect.height = realWindowRect.height * parentWindow.Grid.m_GridZoomRate;

				//Debug.Log("pre : " + windowRect.position.ToString());
				// ズーム無しの時の位置
				realWindowRect.position = parentWindow.Grid.GridZoomCenterPoint + GridDrawer.GRID_INTERVAL * gridPos;

				// ズーム後の位置
				windowRect.position = parentWindow.Grid.GridZoomCenterPoint + parentWindow.Grid.gridSize * gridPos;
				//Debug.Log("post : " + windowRect.position.ToString());

			}

		}


		// ドラッグ処理
		public void windowFunc(int id)
		{

			Color[] TitleBarColor = new Color[] { Color.gray, new Color(0.4f, 0.4f, 0.8f), 
				Color.cyan, Color.green, Color.yellow, Color.magenta, Color.red };


			float zoom = windowRect.height / realWindowRect.height;

			// タイトルバー
			const int titleHeight = 16;
			Rect title = windowRect;
			title.position = new Vector2(-1, 0);
			title.height = titleHeight * zoom;
			title.width += 2;

			//GUI.color = TitleBarColor[0];// TitleBarColor[handle % (int)NodeColor.Max];

			GUIStyle style = new GUIStyle("dropDownButton");
			style.fontSize = (int)((titleHeight-3) * zoom);
			//GUIStyleState styleState = new GUIStyleState();
			//styleState.textColor = Color.white;   // 文字色の変更.
			//style.normal = styleState;
			//style.fontStyle = FontStyle.Italic;
			style.fixedHeight = titleHeight * zoom; // バー高さ

			
			//float add = 0.1f;
			//GUI.color = Color.HSVToRGB(add * (handle % (1.0f / add)), 0.8f, 0.6f);
			//GUI.color = EditorGUIUtility.HSVToRGB(add * (handle % (1.0f / add)), 0.8f, 0.6f);
			//GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 1.0f);
			GUI.color = new Color(0.4f, 0.6f, 1.0f,1.0f);
			GUI.Label(title, NodeName, style);

			//float centeroffset = (NodeName.Length/2 * style.fontSize);
			//GUIHelper.DrawText(new Vector2(title.width / 2 - centeroffset, 0), NodeName, Color.white);

			// コネクタ名
			foreach (var box in connectBoxList)
			{
				int fontsize = (int)((titleHeight - 5) * zoom);
				Rect labelRect = box.boxRect;
				int length = fontsize * box.linkName.Length;
				labelRect.x = box.connectArea == ConnectAreaType.AREA_LEFT ? 5 : windowRect.width - length+8;
				labelRect.y -= windowRect.y;

				GUIStyle connectstyle = new GUIStyle();
				connectstyle.fontSize = fontsize;
				GUI.Label(labelRect, box.linkName, connectstyle);
			}


			//----------------------
			// コネクタ追加
			//----------------------
			var ev = Event.current;
			if (ev.type == EventType.MouseDown && ev.button == 1)
			{
				// MyEditorWindow内のPopup Window上でマウスを右クリックするとここに来る。
				var menu = new GenericMenu();
				ConnectAreaType type = ConnectAreaType.AREA_MAX;

				float offsety = windowRect.height / 4;
				float offsetx =  windowRect.width/4;
				if (ev.mousePosition.x < offsety && ev.mousePosition.y > offsety )
				{
					type = ConnectAreaType.AREA_LEFT;
				}
				else if (ev.mousePosition.x >= windowRect.width - offsetx && ev.mousePosition.y > offsetx )
				{
					type = ConnectAreaType.AREA_RIGHT;
				}
				else if (ev.mousePosition.y < offsety)
				{
					type = ConnectAreaType.AREA_TOP;
				}
				else if (ev.mousePosition.y > windowRect.height - offsety)
				{
					type = ConnectAreaType.AREA_BOTTOM;
				}
				if (type != ConnectAreaType.AREA_MAX)
				{
					menu.AddItem(new GUIContent("Add Link"), false, () =>
					{
						AddConnectBox(new ConnectBox(type, "out"));
					});
				}
				else
				{
					menu.AddItem(new GUIContent("テスト"), false, () =>
					{
					});
				}

				menu.ShowAsContext();
				ev.Use();
			}


			if(active ) GUI.DragWindow();

			
			
		}


		/// <summary>
		/// 描画
		/// </summary>
		public override void OnPaint()
		{
			// 画面外なら描画しない
			Rect win = parentWindow.position;
			win.x = 0;
			win.y = 0;
			if (!windowRect.Overlaps(win))
			{
				return;
			}



		}


		/// <summary>
		/// GUI描画
		/// </summary>
		public override void OnDrawGUI()
		{

			// 画面外なら描画しない
			Rect win = parentWindow.position;
			win.x = 0;
			win.y = 0;
			if (windowRect.Overlaps(win))
			{

				string nodeStyle = "flow node 0";
				if (Active) nodeStyle += " on";
				GUI.color = new Color(1, 1, 1, 1);
				GUIStyle style = new GUIStyle(nodeStyle);
				windowRect = GUI.Window(handle, windowRect, windowFunc, "", style);

			}


			// コネクトボックス位置更新
			UpdateConnect();
			if (windowRect.Overlaps(win))
			{
				DrawConnect();
			}
			//uLabelField.DrawText(windowRect.ToString(), Color.white);
			//GUIHelper.DrawText(new Vector2(0,20*handle), windowRect.ToString(), Color.white);
		}


		/// <summary>
		/// コネクトボックス位置更新
		/// </summary>
		void UpdateConnect()
		{

			int index = 0;
			// コネクトボックス表示
			int[] count = new int[(int)ConnectAreaType.AREA_MAX];
			foreach (var box in connectBoxList)
			{
				
				if (box.connectArea == ConnectAreaType.AREA_LEFT)
				{
					count[(int)ConnectAreaType.AREA_LEFT]++;

					box.boxRect.x = windowRect.x - box.boxRect.width;

					int total = areaConnectCount[(int)ConnectAreaType.AREA_LEFT];
					int cnt = count[(int)ConnectAreaType.AREA_LEFT];
					float offset = windowRect.height / (total + 1) * cnt;
					box.boxRect.y = windowRect.y + offset - box.boxRect.height / 2;


				}
				else if (box.connectArea == ConnectAreaType.AREA_RIGHT)
				{
					count[(int)ConnectAreaType.AREA_RIGHT]++;

					box.boxRect.x = windowRect.x + windowRect.width;

					int total = areaConnectCount[(int)ConnectAreaType.AREA_RIGHT];
					int cnt = count[(int)ConnectAreaType.AREA_RIGHT];
					float offset = windowRect.height / (total + 1) * cnt;

					box.boxRect.y = windowRect.y + offset - box.boxRect.height / 2;


				}
				else if (box.connectArea == ConnectAreaType.AREA_TOP)
				{
					count[(int)ConnectAreaType.AREA_TOP]++;

					int total = areaConnectCount[(int)ConnectAreaType.AREA_TOP];
					int cnt = count[(int)ConnectAreaType.AREA_TOP];
					float offset = windowRect.width / (total + 1) * cnt;
					box.boxRect.x = windowRect.x + offset - box.boxRect.width / 2;

					box.boxRect.y = windowRect.y - box.boxRect.height;


				}
				else if (box.connectArea == ConnectAreaType.AREA_BOTTOM)
				{
					count[(int)ConnectAreaType.AREA_BOTTOM]++;

					int total = areaConnectCount[(int)ConnectAreaType.AREA_BOTTOM];
					int cnt = count[(int)ConnectAreaType.AREA_BOTTOM];
					float offset = windowRect.width / (total + 1) * cnt;
					box.boxRect.x = windowRect.x + offset - box.boxRect.width / 2;

					box.boxRect.y = windowRect.y + windowRect.height;


				}




				box.boxRect.width = ConnectBox.CONNECT_SIZE * parentWindow.Grid.m_GridZoomRate;
				box.boxRect.height = ConnectBox.CONNECT_SIZE * parentWindow.Grid.m_GridZoomRate;



				// コネクトボックスの色
				float add = 0.15f;
				//Color col = Color.HSVToRGB(add * (index % (1.0f / add)), 0.3f, 1.0f);
				Color col = EditorGUIUtility.HSVToRGB(add * (index % (1.0f / add)), 0.3f, 1.0f);
				if (box.active) col = Color.red;

				box.color = col;

				index++;
			}
		}

		/// <summary>
		/// コネクトボックス表示
		/// </summary>
		void DrawConnect()
		{
			

			connectBoxList.ForEach(box => {

				GUIHelper.Fill(box.boxRect, box.color);

				//Drawing.Circle(box.boxRect, box.color);
				//GUIHelper.DrawCircle(box.boxRect.center,1.0f,box.color);
			}
			);
			
		}

	




	}

}
