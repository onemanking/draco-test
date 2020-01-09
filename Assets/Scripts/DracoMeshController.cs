using UnityEngine;
using UniRx;
using System;
using System.Linq;

public class DracoMeshController : MonoBehaviour
{
	public static DracoMeshController Instance => _Instance;

	private static DracoMeshController _Instance;

	[Range(1, 200)]
	[SerializeField] private int m_FrameRate = 30;
	[SerializeField] private BoolReactiveProperty m_Loop;
	[SerializeField] private bool m_PlayAnimationOnStart = true;
	[SerializeField] private bool m_PlayAnimationAsPossible = true;
	[SerializeField] private bool m_LoadNextSceneOnEnd = true;

	public int CurrentFrame => _CurrentFrame;
	public bool PlayBack => _PlayBack;
	public bool Inited => _Inited;

	private bool _PlayBack;
	private float _Time;
	private int _CurrentFrame;
	private bool _Inited;
	private int _EndFrame;

	private void Awake() => _Instance = GetComponent<DracoMeshController>();

	private void Start()
	{
		var everyObserver = Observable.EveryUpdate();
		var allMesh = FindObjectsOfType<DracoMesh>();

		IDisposable checkAnyCanPlay = null;
		checkAnyCanPlay = everyObserver.Where(x => m_PlayAnimationAsPossible && allMesh.Any(mesh => mesh.CanPlay) && _Inited).Subscribe(_ =>
		{
			_PlayBack = m_PlayAnimationOnStart;
			checkAnyCanPlay?.Dispose();
		}).AddTo(this);

		IDisposable checkAllLoaded = null;
		checkAllLoaded = everyObserver.Where(x => !m_PlayAnimationAsPossible && allMesh.All(mesh => mesh.IsLoaded) && _Inited).Subscribe(_ =>
		{
			_PlayBack = m_PlayAnimationOnStart;
			checkAllLoaded?.Dispose();
		}).AddTo(this);

		m_Loop.Subscribe(_ =>
		{
			if (!_PlayBack) _PlayBack = m_Loop.Value;
		}).AddTo(this);

		everyObserver.Where(x => _PlayBack).Subscribe(_ =>
		{
			_Time += Time.deltaTime;
			_CurrentFrame = Mathf.FloorToInt(_Time * m_FrameRate);
			if (_CurrentFrame >= _EndFrame)
			{
				_Time = 0;
				_CurrentFrame = 0;
				_PlayBack = m_Loop.Value;

				if (m_LoadNextSceneOnEnd) SceneController.Instance.LoadNextScene();
			}
		}).AddTo(this);

		//ResetPlayback
		everyObserver.Where(x => (Input.GetKeyDown(KeyCode.R) || (Input.touchCount >= 2 && Input.GetTouch(1).phase == TouchPhase.Began))).Subscribe(_ =>
		{
			ResetPlayback();
		}).AddTo(this);

		everyObserver.Where(x => (Input.GetKeyDown(KeyCode.Space) || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began))).Subscribe(_ =>
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

	public void InitEndFrame(int _endFrame)
	{
		_Inited = true;
		_EndFrame = _endFrame;
	}

	#endregion

}