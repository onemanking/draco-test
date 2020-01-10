using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Unity.Collections;
using System.IO;
using System.Linq;

public class DracoMesh : MonoBehaviour
{
	[SerializeField] private StringReactiveProperty m_ModelName;

	public bool IsLoaded => _IsLoaded;
	public bool CanPlay => _CanPlay;

	private bool _IsLoaded;
	private bool _CanPlay;

	void Start()
	{
		var meshFilter = GetComponent<MeshFilter>();
		Mesh[] meshs = null;

		IDisposable disposable = null;
		m_ModelName.Subscribe(_modelName =>
		{
			disposable?.Dispose();
			disposable = DracoMeshManager.GetMeshListAsObservable(_modelName)
				.Subscribe
				(
					_meshs =>
					{
						meshs = _meshs;
						_CanPlay = true;
						if (!DracoMeshController.Instance.Inited)
							DracoMeshController.Instance.InitEndFrame(meshs.Length);
					}
					, _error =>
					{
						Debug.LogError(_error.Message);
					}
					, () =>
					{
						_IsLoaded = true;
					}
				).AddTo(this);
		}).AddTo(this);

		Observable.EveryUpdate().Where(x => DracoMeshController.Instance.PlayBack && meshs != null && DracoMeshController.Instance.CurrentFrame <= meshs.Length - 1)
		.Subscribe(_ => meshFilter.mesh = meshs[DracoMeshController.Instance.CurrentFrame]).AddTo(this);
	}
}
