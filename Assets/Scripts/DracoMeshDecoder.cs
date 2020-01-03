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
	void Start()
	{
		_MeshFilter = GetComponent<MeshFilter>();
		var count = 0;

		Observable
			.EveryUpdate().Where(_ => DracoMeshManager.Instance.IsInitialized)
			.Subscribe(_ =>
			{
				var endFrame = DracoMeshManager.Instance.ListCount(m_ModelName);
				if (count < endFrame)
					_MeshFilter.mesh = DracoMeshManager.Instance.GetMesh(m_ModelName, count++);
				else if (endFrame == count && m_Loop)
					count = 0;
			}).AddTo(this);
	}
}
