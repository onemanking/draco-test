using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

public class DracoMeshManager : MonoBehaviour
{
	private const string _MODEL_FOLDER = "Models";

	public static DracoMeshManager Instance => _Instance;

	public bool IsInitialized => _IsInitialized;

	private static DracoMeshManager _Instance;

	private Dictionary<string, List<Mesh>> _MeshDict = new Dictionary<string, List<Mesh>>();
	private bool _IsInitialized;

#if USE_RUNTIME_LOADER
	private Dictionary<string, List<string>> _ModelDict;
	private DracoMeshLoader _DracoLoader;
#endif

	void Awake()
	{
		_Instance = GetComponent<DracoMeshManager>();
		InitResources();
	}

	private void InitResources()
	{
		var root = new DirectoryInfo(Path.Combine(Application.streamingAssetsPath, _MODEL_FOLDER));

#if !USE_RUNTIME_LOADER
		var modelDict = new Dictionary<string, List<string>>();
		var dracoLoader = new DracoMeshLoader();
		int count = 0;

		foreach (var dir in root.GetDirectories())
		{
			var files = dir.GetFiles("*.drc.bytes", SearchOption.AllDirectories);
			modelDict.Add(dir.Name, files.Select(_ => _.Name).OrderBy(x => x).ToList());
		}

		foreach (var key in modelDict.Keys)
		{
			var meshList = new List<Mesh>();
			foreach (var asset in modelDict[key])
			{
#if UNITY_IOS
				var url = "file://" + Path.Combine(Application.streamingAssetsPath, _MODEL_FOLDER, key, asset);
#elif UNITY_EDITOR
				var url = Path.Combine(Application.streamingAssetsPath, _MODEL_FOLDER, key, asset);
#endif
				ObservableWWW.GetAndGetBytes(url)
				.Subscribe
				(
					_byte =>
					{
						Debug.LogWarning($"URL : {url} , BYTES : {_byte.Length}");
						var data = new NativeArray<byte>(_byte, Allocator.Persistent);
						Observable.FromCoroutine<Mesh>((_observer) => dracoLoader.DecodeMesh(data, _observer))
								.Subscribe(_mesh =>
								{
									meshList.Add(_mesh);
									count += 1;
									_IsInitialized = count >= modelDict.Count();
								})
								.AddTo(this);
					}, _error =>
					{
						Debug.LogError(_error);
					}
				).AddTo(this);
			}
			_MeshDict.Add(key, meshList);
		}
#else
		_DracoLoader = new DracoMeshLoader();
		_ModelDict = new Dictionary<string, List<string>>();
		foreach (var dir in root.GetDirectories())
		{
			var files = dir.GetFiles("*.drc.bytes", SearchOption.AllDirectories);
			_ModelDict.Add(dir.Name, files.Select(_ => _.Name).OrderBy(x => x).ToList());
		}
		_IsInitialized = true;
#endif
	}

#if !USE_RUNTIME_LOADER
	public int ListCount(string _modelName) => _MeshDict[_modelName].Count;

	public Mesh GetMesh(string _modelName, int _index) => _MeshDict[_modelName][_index];
#else
	public int ListCount(string _modelName) => _ModelDict[_modelName].Count;

	public IObservable<Mesh> GetMesh(string _modelName, int _index)
	{

#if UNITY_IOS
		var url = "file://" + Path.Combine(Application.streamingAssetsPath, _MODEL_FOLDER, _modelName, _ModelDict[_modelName][_index]);
#elif UNITY_EDITOR
		var url =  Path.Combine(Application.streamingAssetsPath, _MODEL_FOLDER, _modelName, modelDict[_modelName][_index]);
#endif
		return Observable.Create<Mesh>(_observer =>
		{
			var disposable = ObservableWWW.GetAndGetBytes(url)
								.Subscribe
								(
									_byte =>
									{
										Debug.LogWarning($"URL : {url} , BYTES : {_byte.Length}");
										var data = new NativeArray<byte>(_byte, Allocator.Persistent);
										Observable.FromCoroutine<Mesh>((_) => _DracoLoader.DecodeMesh(data, _))
												.Subscribe(_mesh =>
												{
													_observer.OnNext(_mesh);
													_observer.OnCompleted();
												}, () => data.Dispose());
									}, _error =>
									{
										Debug.LogError(_error);
									}
								);

			return Disposable.Create(() => disposable?.Dispose());
		});
	}

#endif

}
