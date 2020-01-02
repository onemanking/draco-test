using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class DracoMeshDecoder : MonoBehaviour
{
	[SerializeField] private string m_ModelName;
	[SerializeField] private bool m_Loop = true;

	private MeshFilter _MeshFilter;
	private IDisposable _Disposable;
	void Start()
	{
		_MeshFilter = GetComponent<MeshFilter>();
		var endFrame = DracoMeshManager.Instance.ListCount(m_ModelName);
		var count = 0;
		Debug.Log($"Model : {m_ModelName} EndFrame : {endFrame}");

		_Disposable?.Dispose();
		_Disposable = Observable
			.EveryUpdate()
			.Subscribe(_ =>
			{
				if (count < endFrame)
					_MeshFilter.mesh = DracoMeshManager.Instance.GetMesh(m_ModelName, count++);
				else if (endFrame == count && m_Loop)
					count = 0;
			}).AddTo(this);
	}
}
