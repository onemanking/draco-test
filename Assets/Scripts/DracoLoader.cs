using UnityEngine;
using UniRx;

public class DracoLoader : MonoBehaviour
{
	[SerializeField] private StringReactiveProperty m_ModelName;
	[Range(1, 200)]
	[SerializeField] private int m_FrameRate = 30;
	[SerializeField] private BoolReactiveProperty m_Loop;
	[SerializeField] private bool m_PlayAnimationAsPossible = true;
	[SerializeField] private bool m_LoadNextSceneOnEnd = true;

	private void Start()
	{
		var meshFilter = GetComponent<MeshFilter>();
		var playBack = false;
		var meshs = new Mesh[0];

		m_ModelName.Subscribe(_modelName =>
		{
			DracoMeshManager.Instance.GetMeshListAsObservable(_modelName).Subscribe(_meshs =>
			{
				meshs = _meshs;
				playBack = m_PlayAnimationAsPossible;
			}, () => playBack = true).AddTo(this);
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

		everyObserver.Where(x => playBack && meshs.Length > 0).Subscribe(_ =>
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
		everyObserver.Where(x => (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && meshs.Length > 0).Subscribe(_ =>
		{
			playBack = true;
			time = 0;
			currentFrame = 0;
		}).AddTo(this);
	}
}