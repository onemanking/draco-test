using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using Unity.Collections;
using UnityEngine;

public class DracoMeshManager : MonoBehaviour
{
	private const string _MODEL_FOLDER = "Models";
	public static DracoMeshManager Instance => _Instance;

	private static DracoMeshManager _Instance;

	void Awake()
	{
		_Instance = GetComponent<DracoMeshManager>();
		DontDestroyOnLoad(this);
	}

	private DracoMeshLoader _DracoLoader = new DracoMeshLoader();
	public IObservable<Mesh[]> GetMeshListAsObservable(string _modelName)
	{
		return Observable.Create<Mesh[]>(_observer =>
		{
			var disposable = new CompositeDisposable();
			try
			{
				var allFiles = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, _MODEL_FOLDER, _modelName), "*.drc.bytes", SearchOption.AllDirectories).OrderBy(x => x).ToArray();
				var meshs = new Mesh[allFiles.Length];

				for (int i = 0; i < allFiles.Length; i++)
				{
					string file = allFiles[i];
					var index = i;
					var path = "file://" + file;
					disposable.Add
					(
						ObservableWWW.GetAndGetBytes(path)
							.Subscribe
							(
								_byte =>
								{
									var data = new NativeArray<byte>(_byte, Allocator.Persistent);
									Observable.FromCoroutine<Mesh>((_) => _DracoLoader.DecodeMesh(data, _))
											.Subscribe
											(
												_mesh =>
												{
													meshs[index] = _mesh;
													_observer.OnNext(meshs);
													if (index >= allFiles.Length - 1)
													{
														_observer.OnCompleted();
													}
												}
												, _error =>
												{
													throw _error;
												}
#if UNITY_EDITOR
												, () => data.Dispose()
#endif
											);
								},
								_error =>
								{
									_observer.OnError(_error);
								}
							)
					);
				}
			}
			catch (Exception error)
			{
				_observer.OnError(error);
			}

			return Disposable.Create(() => disposable?.Dispose());
		});
	}
}
