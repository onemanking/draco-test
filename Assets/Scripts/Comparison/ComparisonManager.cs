using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Linq;
using Unity.Collections;
using UnityEngine.Networking;
using System.IO;
using System;

public class ComparisonManager : MonoBehaviour
{
	[SerializeField] private MeshFilter m_Obj;
	[SerializeField] private MeshFilter m_Drc;

	private NativeArray<byte> _Data;
	private void Start()
	{
		Stopwatch watch = new Stopwatch();
		var dracoLoader = new DracoMeshLoader();
		dracoLoader.onMeshesLoaded += OnMeshesLoaded;
		var mesh = new List<Mesh>();

		watch.Start();
		m_Obj.mesh = Resources.Load("MachineGun", typeof(Mesh)) as Mesh;
		watch.Stop();
		Debug.LogWarning($"FINISH TIME OBJ { watch.Elapsed.TotalSeconds }");

		watch.Reset();

		watch.Start();
		var url = Path.Combine(Application.dataPath, "Resources", "Models", "gun", "MachineGun.drc.bytes");
		_Data = new NativeArray<byte>();
		ObservableWWW.GetAndGetBytes(url).Subscribe(_byte =>
		{
			Debug.LogWarning($"URL : {url} , BYTES : {_byte.Length}");
			_Data = new NativeArray<byte>(_byte, Allocator.Persistent);
			StartCoroutine(dracoLoader.DecodeMesh(_Data));
		}, _error =>
		{
			Debug.LogError(_error);
		}, () =>
		{
			watch.Stop();
			Debug.LogWarning($"FINISH TIME DRC { watch.Elapsed.TotalSeconds }");
		}).AddTo(this);
	}

	private void OnMeshesLoaded(Mesh _mesh)
	{
		m_Drc.mesh = _mesh;
	}

	private void OnDestroy() => _Data.Dispose();
}
