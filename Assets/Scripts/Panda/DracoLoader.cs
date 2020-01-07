using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.IO;
using Unity.Collections;
using System.Linq;
using System;

public class DracoLoader : MonoBehaviour
{
	private const string _MODEL_FOLDER = "Models";

	[SerializeField] private StringReactiveProperty m_ModelName;
	[Range(1, 200)]
	[SerializeField] private int m_FrameRate = 30;
	[SerializeField] private BoolReactiveProperty m_Loop;
	[SerializeField] private bool m_PlayAnimationAsPossible = true;
	[SerializeField] private bool m_LoadNextSceneOnEnd = true;

	private void Start()
	{
		var meshFilter = GetComponent<MeshFilter>();
		var dracoLoader = new DracoMeshLoader();
		var disposable = new CompositeDisposable();
		var playBack = true;
		var meshs = new Mesh[0];

		m_ModelName.Subscribe(modelName =>
		{
			try
			{
				disposable?.Dispose();
				disposable = new CompositeDisposable();
				var allFiles = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, _MODEL_FOLDER, modelName), "*.drc.bytes", SearchOption.AllDirectories).OrderBy(x => x).ToList();
				meshs = new Mesh[allFiles.Count];

				for (int i = 0; i < allFiles.Count; i++)
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
									Observable.FromCoroutine<Mesh>((_) => dracoLoader.DecodeMesh(data, _))
											.Subscribe
											(
												_mesh =>
												{
													meshs[index] = _mesh;
													playBack = m_PlayAnimationAsPossible ? true : index >= allFiles.Count - 1;
												}
											).AddTo(this);
								},
								_error =>
								{
									throw _error;
								}
							).AddTo(this)
					);
				}
			}
			catch (Exception _error)
			{
				Debug.LogError(_error.Message);
			}
		}).AddTo(this);

		//PlayBack
		var time = 0f;
		var currentFrame = 0;
		var everyObserver = Observable.EveryUpdate();
		var isEnd = false;
		m_Loop.Subscribe(_ =>
		{
			if (!playBack) playBack = m_Loop.Value;
		}).AddTo(this);

		everyObserver.Where(x => playBack).Subscribe(_ =>
		{
			meshFilter.mesh = meshs[currentFrame];

			time += Time.deltaTime;
			currentFrame = Mathf.FloorToInt(time * m_FrameRate);
			isEnd = currentFrame >= meshs.Length;

			if (isEnd)
			{
				time = 0;
				currentFrame = 0;
				playBack = m_Loop.Value;

				if (m_LoadNextSceneOnEnd) SceneController.Instance.LoadNextScene();
			}

		}).AddTo(this);

		//ResetPlayback
		everyObserver.Where(x => (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && meshs.Length > 0).Subscribe(_ => playBack = true).AddTo(this);
	}
}