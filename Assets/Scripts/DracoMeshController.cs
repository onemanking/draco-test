using UnityEngine;
using UniRx;
using System;

public class DracoMeshController : MonoBehaviour
{
	[SerializeField] private StringReactiveProperty m_ModelName;
	[Range(1, 200)]
	[SerializeField] private int m_FrameRate = 30;
	[SerializeField] private BoolReactiveProperty m_Loop;
	[SerializeField] private bool m_PlayAnimationOnStart = true;
	[SerializeField] private bool m_PlayAnimationAsPossible = true;
	[SerializeField] private bool m_LoadNextSceneOnEnd = true;

	public bool IsLoaded => _IsLoaded;

	private bool _IsLoaded;
	private bool _PlayBack;
	private float _Time;
	private int _CurrentFrame;

	private void Start()
	{
		var meshFilter = GetComponent<MeshFilter>();
		Mesh[] meshs = null;
		IDisposable disposable = null;

		m_ModelName.Subscribe(_modelName =>
		{
			ResetPlayback(false);
			disposable?.Dispose();
			disposable = DracoMeshManager.Instance.GetMeshListAsObservable(_modelName)
				.Subscribe
				(
					_meshs =>
					{
						meshs = _meshs;
						_PlayBack = m_PlayAnimationAsPossible && m_PlayAnimationOnStart;
					}
					, _error =>
					{
						Debug.LogError(_error.Message);
					}
					, () =>
					{
						_PlayBack = m_PlayAnimationOnStart;
						_IsLoaded = true;
					}
				).AddTo(this);
		}).AddTo(this);

		//PlayBack
		var everyObserver = Observable.EveryUpdate();
		var isEnd = false;
		m_Loop.Subscribe(_ =>
		{
			if (!_PlayBack) _PlayBack = m_Loop.Value;
		}).AddTo(this);

		everyObserver.Where(x => _PlayBack && meshs != null).Subscribe(_ =>
		{
			meshFilter.mesh = meshs[_CurrentFrame];

			_Time += Time.deltaTime;
			_CurrentFrame = Mathf.FloorToInt(_Time * m_FrameRate);
			isEnd = _CurrentFrame >= meshs.Length;

			if (isEnd)
			{
				_Time = 0;
				_CurrentFrame = 0;
				_PlayBack = m_Loop.Value;

				if (m_LoadNextSceneOnEnd) SceneController.Instance.LoadNextScene();
			}
		}).AddTo(this);

		//ResetPlayback
		everyObserver.Where(x => (Input.GetKeyDown(KeyCode.R) || (Input.touchCount >= 2 && Input.GetTouch(1).phase == TouchPhase.Began)) && meshs != null).Subscribe(_ =>
		{
			ResetPlayback();
		}).AddTo(this);

		everyObserver.Where(x => (Input.GetKeyDown(KeyCode.Space) || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)) && meshs != null).Subscribe(_ =>
		{
			TogglePlayback();
		}).AddTo(this);
	}

	#region ANIMATION PLAYBACK

	public void ResetPlayback(bool _playback = true)
	{
		_PlayBack = true;
		_Time = 0;
		_CurrentFrame = 0;
	}

	public void Playback()
	{
		_PlayBack = true;
	}

	public void PausePlayback()
	{
		_PlayBack = false;
	}

	public void TogglePlayback()
	{
		_PlayBack = !_PlayBack;
	}

	#endregion

}