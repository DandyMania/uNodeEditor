using UnityEngine;

public class uControlBase
{


	public bool bInit = false;

	/// <summary>
	/// 初期化
	/// </summary>
	public virtual void OnInit()
	{

	}


	/// <summary>
	/// 更新
	/// </summary>
	public virtual void OnUpdate()
	{

	}
	/// <summary>
	/// 描画
	/// </summary>
	public virtual void OnPaint()
	{
	}
	/// <summary>
	/// GUI描画
	/// </summary>
	public virtual void OnDrawGUI()
	{

	}


}
