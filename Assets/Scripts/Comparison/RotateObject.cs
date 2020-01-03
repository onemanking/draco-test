using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
	[SerializeField] private float m_Speed = 10;
	[SerializeField] private float m_X;
	[SerializeField] private float m_Y = 1;
	[SerializeField] private float m_Z;

	void Start()
	{
		Observable.EveryUpdate().Subscribe(_ =>
			{
				transform.Rotate(m_Speed * Time.deltaTime * new Vector3(m_X, m_Y, m_Z));

			}).AddTo(this);
	}
}
