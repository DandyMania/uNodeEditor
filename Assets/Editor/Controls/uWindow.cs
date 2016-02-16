using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI; // for Scrollbar
using System.Collections.Generic;

namespace uNodeEditor
{

    #region マウス関連定義
    public enum MouseButton
    {
        None = -1,
        Left = 0,
        Right = 1,
        Middle = 2,

    };
    public enum MouseEventType
    {
        None = -1,
		Down = EventType.MouseDown,
		Up = EventType.MouseUp,
		Move = EventType.mouseMove,
		Drag = EventType.MouseDrag,
		Scroll = EventType.ScrollWheel,
		ContextClick = EventType.ContextClick,
    }


    /// <summary>
    /// マウスイベントデータ
    /// </summary>
    public class MouseData
    {
		
        public bool IsDown(MouseButton btn)
        {
            return (button == btn && type == MouseEventType.Down);
        }
        public bool IsDrag(MouseButton btn)
        {
            return (button == btn && type == MouseEventType.Drag);
        }
        public bool IsUp(MouseButton btn)
        {
            return (button == btn && type == MouseEventType.Up);
        }
        public bool IsScroll()
        {
            return (type == MouseEventType.Scroll);
        }
		public bool IsContextClick()
		{
			return (type == MouseEventType.ContextClick);
		}
		// コピー
		public MouseData Clone()
		{
			return (MouseData)MemberwiseClone();
		}

        /// ボタン
        public MouseButton button;
        /// イベントタイプ
        public MouseEventType type;
        /// 位置
        public Vector2 pos;
        /// 移動量
		public Vector2 delta;

		public float zoom = 1.0f;
		/// マウス判定
		public Rect rect { get
			{
				return new Rect(pos - new Vector2(5, 5) * zoom, new Vector2(10, 10) * zoom);
			} 
		}

    };
    #endregion

	public class uWindow : EditorWindow
	{

		protected String windowTitle = "uWindow";
		protected Color bgColor { get; set; }
		protected Rect windowRect
		{
			get
			{
				Rect r = this.position;
				r.x = 0;
				r.y = 0;
				return r;
			}
		}

		/// <summary>
		///  マウスデータ
		/// </summary>
		private MouseData mouse = new MouseData();
		public MouseData mouseData { get { return mouse; } }

		/// <summary>
		/// コントーロールリスト
		/// </summary>
		private List<uControlBase> controls = new List<uControlBase>();
		public List<uControlBase> Contorols { get { return controls; } }

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public uWindow()
		{
			// マウス移動の度にイベント飛ばす
			wantsMouseMove = true;

			bgColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);

			InitializeComponent();

#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
            title = windowTitle;
#else
			titleContent = new GUIContent(windowTitle);
#endif
		}

		// awakeだとビルド走った時に呼ばれないのでコンストラクタで。。。
		//void Awake()
		// {

		//     InitializeComponent();
		//}


		const float BASE_DRAW_FPS = 60.0f; // 描画更新フレームレート

		#region FPS制御




		int frameCount;
		float prevTime;
		float realfps;


		int drawFrameCount;
		float prevDrawTime;
		float fps;


		bool UpdateFPS()
		{
			bool bUpdate = false;

			++frameCount;

			// 描画FPS
			float timeDraw = Time.realtimeSinceStartup - prevDrawTime;
			if (timeDraw >= 1.0f / BASE_DRAW_FPS)
			{
				++drawFrameCount;
				fps = drawFrameCount / timeDraw;
				drawFrameCount = 0;

				prevDrawTime = Time.realtimeSinceStartup;
				bUpdate = true;
			}


			// RealFPS
			float time = Time.realtimeSinceStartup - prevTime;
			if (time >= 0.5f)
			{
				// Debug.LogFormat("{0}fps", frameCount / time);

				realfps = frameCount / time;
				frameCount = 0;
				drawFrameCount = 0;
				prevTime = Time.realtimeSinceStartup;
			}

			return bUpdate;
		}
		void DrawFPS()
		{
			// FPS
			GUIHelper.DrawText(new Vector2(position.width - 100, 0), fps.ToString(".0") + " / " + realfps.ToString(".0") + "fps", Color.white);
		}
		#endregion


		/// <summary>
		/// 更新
		/// </summary>
		void Update()
		{
			if (UpdateFPS())
			{
				// マウスの値をマイフレ取るため
				// Updateは200フレとかになっちゃうのでフレーム固定
				Repaint();
			}
		}


		/// <summary>
		///  ウィンドウ描画
		/// </summary>
		void OnGUI()
		{

			ClearMouse();

			// 更新
			Color colorBackup = GUI.color;
			if (Event.current.type != EventType.Layout &&
				Event.current.type != EventType.Repaint)
			{

				colorBackup = GUI.color;

				// マウス更新
				UpdateMouse();

				this.controls.ForEach(control =>{
						if( !control.bInit ){
							control.OnInit();
							control.bInit=true;
						}
						control.OnUpdate();
					}
				);

				OnUpdate();

				GUI.color = colorBackup;

			}

			// ライン描画とか
			colorBackup = GUI.color;
			if (Event.current.type != EventType.Layout ||
				Event.current.type != EventType.Repaint)
			{
				GUIHelper.Fill(windowRect, bgColor);

				this.controls.ForEach(control => control.OnPaint());

				OnPaint();
			}
			GUI.color = colorBackup;



			DrawFPS();

			

			// ボタンとか
			BeginWindows();
			colorBackup = GUI.color;
			this.controls.ForEach(control => control.OnDrawGUI());
			GUI.color = colorBackup;
			EndWindows();

			OnDrawGUI();
			
			
	

		}

		#region マウス関連

		/// <summary>
		/// マウス情報クリア
		/// </summary>
		void ClearMouse()
		{

			mouse.delta = new Vector2(0, 0);
			mouse.button = MouseButton.None;
			mouse.type = MouseEventType.None;
		}


		private MouseData prevMouse = new MouseData();

		/// <summary>
		/// マウスデータ更新
		/// </summary>
		void UpdateMouse()
		{

			mouse.pos = Event.current.mousePosition;
			mouse.delta = Event.current.delta;

			if (Event.current.type == EventType.MouseDown ||
				Event.current.type == EventType.MouseUp ||
				Event.current.type == EventType.MouseDrag ||
				Event.current.type == EventType.ScrollWheel||
				Event.current.type == EventType.ContextClick )
			{
				mouse.button = (MouseButton)Event.current.button;
				mouse.type = (MouseEventType)Event.current.type;

				prevMouse = mouse.Clone();
			}
			if (Event.current.type == EventType.MouseMove)
			{
				mouse.type = (MouseEventType)Event.current.type;
				prevMouse = mouse.Clone(); ;
			}

			// ドラッグ中、ウィンドウ外にカーソルが言ってもちゃんとイベント返すように。。。
			if (Event.current.type == EventType.ignore )
			{
				mouse = prevMouse.Clone();
				mouse.type = MouseEventType.Up;
			}

			






			//Debug.Log(prevMouse.type.ToString() + ":" + prevMouse.pos.ToString());

		}

		#endregion // マウス関連ここまで

		#region 継承先コールバック


		/// <summary>
		/// コンポーネント追加
		/// </summary>
		protected virtual void InitializeComponent()
		{

		}


		/// <summary>
		/// 初期化
		/// </summary>
		protected virtual void OnInit()
		{

		}

		/// <summary>
		/// 更新
		/// </summary>
		protected virtual void OnUpdate()
		{

		}
		/// <summary>
		/// 描画
		/// </summary>
		protected virtual void OnPaint()
		{
		}
		/// <summary>
		/// GUI描画
		/// </summary>
		protected virtual void OnDrawGUI()
		{

		}


		#endregion

	}


}