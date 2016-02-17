/*
 * Copyright (c) 2016 DandyMania
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 * Latest version: https://github.com/DandyMania/uNodeEditor
*/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace uNodeEditor
{
	public class uNodeEditor : uWindow
	{

		/// <summary>
		/// ノードエディタ表示
		/// </summary>
		[MenuItem("Sample/uNodeEditor")]
		static void ShowWindow()
		{
			//uNodeEditor edit = EditorWindow.GetWindow<uNodeEditor>();
			uNodeEditor parent = ScriptableObject.CreateInstance<uNodeEditor>();
			parent.Show();
		}


		#region コンポーネント追加
		/// <summary>
		/// コンポーネント追加
		/// </summary>
		protected override void InitializeComponent()
		{

			windowTitle = "Node Test";

			// グリッド
			gridContorol = new GridDrawer(this);
			Contorols.Add(gridContorol);
		}


		#endregion

		private GridDrawer gridContorol;
		public GridDrawer Grid { get { return gridContorol; } }



		/// <summary>
		/// 定期更新
		/// </summary>
		protected override void OnUpdate()
		{


			// 右クリック
			if (mouseData.IsContextClick())
			{
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent("ノード追加"), false, () =>
				{
					Debug.Log("ノード追加");

					uDraggableNode node = new uDraggableNode(this,myString);
					node.windowRect.position = mouseData.pos;
					nodeList.Add(node);
				}
				);
				menu.ShowAsContext();

				//Event.current.Use(); //他のイベントを抑制
			}


			// ノード選択
			SelectArea();

			// ノードの更新
			//this.nodeList.ForEach(node => node.OnUpdate());
		}


		/// <summary>
		///  ウィンドウ描画
		/// </summary>
		protected override void OnPaint()
		{
			// ノード描画(ラインとか)
			//this.nodeList.ForEach(node => node.OnPaint());


			

			// 範囲選択描画
			GUIHelper.Fill(m_SelectedArea, new Color(0.5f, 0.5f, 0.5f, 0.5f));
			Rect pos = m_SelectedArea;
			GUIHelper.DrawRect(pos, Color.white, 2);
			//DrawConnect();
			// 簡易図表示
			if (mouseData.IsDrag(MouseButton.Middle))
			{
				float scale = 0.3f;
				float cx = position.width / 2 - (position.width * scale)/2;
				float cy = position.height / 2 - (position.height * scale) / 2;
				foreach (var node in nodeList)
				{
					Rect r = node.windowRect;

					r.x *= scale;
					r.x += cx;
					r.y *= scale;
					r.y +=  cy;
					r.width *= scale;
					r.height *= scale;
					GUIHelper.DrawRect(r, Color.red);
				}

				Vector2 winpos = Grid.GridZoomCenterPoint;
				winpos.x = cx;
				winpos.y = cy;
				Rect window = new Rect(winpos, new Vector2(position.width * scale, position.height * scale));
				GUIHelper.DrawRect(window, Color.white);
			}


			
		}



		string myString = "node";


		/// <summary>
		/// テキスト描画/ボタン描画等
		/// </summary>
		protected override void OnDrawGUI()
		{

			// ウィンドウ描画
			
			//this.nodeList.ForEach(node => node.OnDrawGUI());
			

			// Deleteキーで選択中のノード削除
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
			{
				nodeList.RemoveAll((uDraggableNode node) => { if (node.Active) { node.Remove(); } return node.Active; });
			}



			// if (GUI.Button(new Rect(20, 20, 100, 50), "Button"))
			// {
			//     Debug.Log("Button is clicked.");
			// }

			GUILayout.FlexibleSpace();

			EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.Width(300));
			
			

			//EditorGUILayout.BeginHorizontal();
			myString = EditorGUILayout.TextField("ノード名", myString);


			if (GUILayout.Button("全部削除", GUILayout.Width(80), GUILayout.Height(20)))
			{

				ActiveEdge = null;
				foreach (var edge in edgeList)
				{
					edge.Remove();
				}
				edgeList.Clear();

				foreach (var node in nodeList)
				{
					node.Remove();
				}

				nodeList.Clear();

				
				
			}
			if (GUILayout.Button("ノード追加", GUILayout.Width(80), GUILayout.Height(20)))
			{
				uDraggableNode node = new uDraggableNode(this, myString);
				nodeList.Add(node);
			}

			uLabelField.DrawText("ノード数 : " + this.nodeList.Count.ToString(),Color.black);

			
			//EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();


			
		}

		#region 範囲選択関連

		// 範囲選択
		Rect m_SelectedArea;

		// ノードリスト
		public List<uDraggableNode> nodeList = new List<uDraggableNode>();

		// 選択中のノード
		public List<uDraggableNode> selectedNodeList = new List<uDraggableNode>();

		// 選択中のノード
		public uDraggableNode selectedNode;

		public List<uNodeEdge> edgeList = new List<uNodeEdge>();

		public uNodeEdge ActiveEdge { get; set; }

		/// <summary>
		/// エッジ伸ばし開始
		/// </summary>
		/// <param name="start"></param>
		public void EdgeStart(uDraggableNode node,ConnectBox start)
		{

			// 既に接続されてないかチェック
			foreach (var edge in edgeList)
			{
				if (edge.edgeEnd != null && edge.edgeEnd.connact == start)
				{
					edge.edgeEnd = null;
					ActiveEdge = edge;
					return;
				}
				if( edge.edgeStart.connact == start)
				{
					edge.edgeStart = null;
					ActiveEdge = edge;
					return;
				}
			}


			uNodeEdge newedge = new uNodeEdge(this, new uNodeEdge.ConnectNode(node, start));
			ActiveEdge = newedge;
			edgeList.Add(newedge);
		}
		/// <summary>
		/// エッジ伸ばし終了
		/// </summary>
		/// <param name="start"></param>
		public void EdgeEnd(uDraggableNode node,ConnectBox end)
		{

			// 既に接続されてないかチェック
			foreach (var edge in edgeList)
			{
				if (edge.edgeEnd != null && (edge.edgeEnd.connact == end) ||
					edge.edgeStart!= null && (edge.edgeStart.connact == end ) )
				{
					return;
				}
			}

			if ((ActiveEdge.edgeStart != null && ActiveEdge.edgeStart.node == node) ||
				(ActiveEdge.edgeEnd != null && ActiveEdge.edgeEnd.node == node))
			{
				return;

			}

			if (ActiveEdge.edgeEnd == null)
			{
				ActiveEdge.edgeEnd = new uNodeEdge.ConnectNode(node, end);
			}
			else if (ActiveEdge.edgeStart == null)
			{
				ActiveEdge.edgeStart = new uNodeEdge.ConnectNode(node, end);
			}
			ActiveEdge = null;
		}

		void CleareSelectArea()
		{
			m_SelectedArea.width = 0;
			m_SelectedArea.height = 0;
			m_SelectedArea.x = 0;
			m_SelectedArea.y = 0;


		}

		void ClearSelectedNodes()
		{
			foreach (var node in selectedNodeList)
			{
				node.Active = false;
			}
			selectedNodeList.Clear();
		}

		/// <summary>
		/// 範囲選択
		/// </summary>
		void SelectArea()
		{

			// ズーム中心を選択範囲の中心に
			gridContorol.GridZoomCenterPoint = new Vector2(position.width / 2, position.height / 2);

			// ノード単体選択
			bool isOverrapped = false;
			if (mouseData.IsDown(MouseButton.Left))
			{
				

				foreach (var node in this.nodeList)
				{
					if (node.windowRect.Overlaps(mouseData.rect))
					{
						node.Active = true;
						isOverrapped = true;

						if (selectedNodeList.Count == 0)
						{
							if (selectedNode != null)
							{
								if (selectedNode != node)
								{
									selectedNode.Active = false;

								}
							}
						}
						selectedNode = node;
						// ズーム中心を選択したノードに
						//gridContorol.GridZoomCenterPoint = mouseData.pos;
					}
					else
					{
						if (selectedNodeList.Count == 0)
						{
							node.Active = false;
						}
					}
				}
				
			}

			//------------------------------
			// 複数ノードドラッグ
			//------------------------------
			if (mouseData.IsDrag(MouseButton.Left))
			{


				// ズーム中心を選択範囲の中心に
				//if (selectedNode != null)
				//{
				//	gridContorol.GridZoomCenterPoint = selectedNode.windowRect.center;
				//}

				var find = selectedNodeList.Find(n => n.handle == selectedNode.handle);
				if (find == null || selectedNode == null)
				{
					ClearSelectedNodes();
				}
				foreach(var node in selectedNodeList)
				{
					isOverrapped = true;
					//if (!node.windowRect.Overlaps(mouseData.rect))
					if(selectedNode!= node)
					{
						node.windowRect.position += mouseData.delta;
					}
					else
					{
						
					}
				}
			}
			if (mouseData.IsUp(MouseButton.Left) /*|| mouseData.IsDrag(MouseButton.Left)*/)
			{

				// 選択されてる人たち一旦クリア
				bool bOtherNodeSelect = true;
				foreach (var node in selectedNodeList)
				{
					if (node.windowRect.Overlaps(mouseData.rect))
					{
						bOtherNodeSelect = false;
					}
				}
				if (bOtherNodeSelect)
				{
					isOverrapped = false;
					ClearSelectedNodes();
				}
				//-------------------------------
			}


			if (isOverrapped)
			{
				CleareSelectArea();
				return;
			}


			// 接続中のエッジ削除
			if (mouseData.IsDown(MouseButton.Left))
			{

				if (ActiveEdge != null)
				{
					ActiveEdge.Remove();
					edgeList.Remove(ActiveEdge);
					ActiveEdge = null;
				}
			}

			// 範囲選択
			if (mouseData.IsDown(MouseButton.Left))
			{
				m_SelectedArea.position = mouseData.pos;

				
				

			}
			else if (mouseData.IsDrag(MouseButton.Left))
			{
				m_SelectedArea.width = mouseData.pos.x - m_SelectedArea.position.x;
				m_SelectedArea.height = mouseData.pos.y - m_SelectedArea.position.y;
			}
			else if (mouseData.IsUp(MouseButton.Left))
			{
				
				GUIHelper.RectAdjust(ref m_SelectedArea);

				// 選択範囲内のノードをアクティブに
				if ((m_SelectedArea.xMax - m_SelectedArea.xMin) != 0 && (m_SelectedArea.yMax - m_SelectedArea.yMin) != 0)
				{
					foreach (var node in this.nodeList)
					{
						if (node.windowRect.Overlaps(m_SelectedArea))
						{
							node.Active = true;
							selectedNodeList.Add(node);

							// ズーム中心を選択範囲の中心に
							//gridContorol.GridZoomCenterPoint = m_SelectedArea.center;
						}
						else
						{
							node.Active = false;
						}
					}
				}

				CleareSelectArea();
			}
			else
			{
				CleareSelectArea();
			}

			// ノードドラッグ中たまに現点から伸びちゃうので。。。
			if (m_SelectedArea.position.x == 0 && m_SelectedArea.position.y == 0)
			{
				CleareSelectArea();
			}

			//Debug.Log(mouseData.type.ToString() + "(" + mouseData.button.ToString() +  "):" + mouseData.pos.ToString());
		}

		#endregion // 範囲選択ここまで

	}


}