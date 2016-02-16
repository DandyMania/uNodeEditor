using UnityEngine;
using System.Collections;
using UnityEditor;

namespace uNodeEditor
{

	public class uNodeEdge : uControlBase
	{

		// 接続情報
		public class ConnectNode
		{
			public ConnectNode(uDraggableNode n ,ConnectBox c)
			{
				node = n;
				connact = c;
			}
			public uDraggableNode node;
			public ConnectBox connact;
		};

		public ConnectNode edgeStart;
		public ConnectNode edgeEnd;

		uNodeEditor parentWindow;

		public uNodeEdge(uNodeEditor parent,ConnectNode start, ConnectNode end=null)
		{
			parent.Contorols.Add(this);

			edgeStart = start;
			edgeEnd = end;
			parentWindow = parent;
		}

		public void Remove()
		{
			edgeStart = null;
			edgeEnd = null;
			parentWindow.Contorols.Remove(this);
		}



		/// <summary>
		/// 初期化
		/// </summary>
		public override void OnInit()
		{
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
			if (edgeEnd == null)
			{

				ConnectAreaType type = parentWindow.ActiveEdge.edgeStart.connact.connectArea == ConnectAreaType.AREA_LEFT ? ConnectAreaType.AREA_RIGHT : ConnectAreaType.AREA_LEFT;
				ConnectBox mouse = new ConnectBox(type);
				mouse.boxRect = parentWindow.mouseData.rect;
				DrawNodeCurve(edgeStart.connact, mouse);
			}
			else if (edgeStart == null)
			{
				ConnectAreaType type = parentWindow.ActiveEdge.edgeEnd.connact.connectArea == ConnectAreaType.AREA_LEFT ? ConnectAreaType.AREA_RIGHT : ConnectAreaType.AREA_LEFT;
				ConnectBox mouse = new ConnectBox(type);
				mouse.boxRect = parentWindow.mouseData.rect;
				DrawNodeCurve(mouse,edgeEnd.connact);
			}
			else
			{
				DrawNodeCurve(edgeStart.connact, edgeEnd.connact);
			}
		}


		/// <summary>
		/// カーブ描画
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		void DrawNodeCurve(ConnectBox start, ConnectBox end)
		{
			/*
			Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
			Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
			Vector3 startTan = startPos + Vector3.right * 50;
			Vector3 endTan = endPos + Vector3.left * 50;

			Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
			*/

			// x成分
			Vector2 vecX = (end.boxRect.center - start.boxRect.center);
			const int tanMaxX = 800;
			const int tanMaxY = 300;
			vecX.y = Mathf.Abs(vecX.y);
			//if (startInd == endInd) vecX.y = tanMaxY - vecX.y;
			float lengthX = ((tanMaxX - Mathf.Clamp(vecX.x, 0, tanMaxX)) / 5) * Mathf.Clamp(vecX.y, 0, tanMaxY) / tanMaxY;



			// y成分
			//Vector2 vecY = (end.center - start.center);
			//vecY.x = 0.0f;
			//float lengthY = (tanMax - Mathf.Clamp(vecX.magnitude, 0, tanMax)) / 5;


			//Debug.Log(length.ToString());



			var startPos = new Vector2(start.boxRect.center.x, start.boxRect.center.y);
			float x1 = start.connectArea == ConnectAreaType.AREA_RIGHT ? start.boxRect.x + lengthX : start.boxRect.x - lengthX;
			float y1 = start.boxRect.y;
			var startTan = new Vector3(x1, y1, 0f);


			var endPos = new Vector2(end.boxRect.center.x, end.boxRect.center.y);
			float x2 = end.connectArea == ConnectAreaType.AREA_RIGHT ? endPos.x + lengthX : endPos.x - lengthX;
			float y2 = endPos.y;
			var endTan = new Vector3(x2, y2, 0f);



			Color shadowCol = new Color(0, 0, 0.3f, 0.06f);

			if (start.active) { shadowCol.b = 0.8f; shadowCol.g = 0.4f; shadowCol.a = 0.2f; }
			if (end.active) { shadowCol.b = 0.8f; shadowCol.g = 0.4f; shadowCol.a = 0.2f; }
			if (start.error) { shadowCol.b = 0.2f; shadowCol.g = 0.2f; shadowCol.r = 1.0f; shadowCol.a = 0.2f; }
			if (end.error) { shadowCol.b = 0.2f; shadowCol.g = 0.2f; shadowCol.r = 1.0f; shadowCol.a = 0.2f; }


			for (int i = 0; i < 3; i++) // Draw a shadow
				Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);

			Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.gray, null, 3f);

			// 接続先
			GUIHelper.Fill(new Rect(endPos - new Vector2(3, 3), new Vector2(6, 6)), Color.gray);
		}
	}

}