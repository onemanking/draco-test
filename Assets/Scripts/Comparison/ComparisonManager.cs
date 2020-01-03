using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class ComparisonManager : MonoBehaviour
{
	[SerializeField] private MeshFilter m_Obj;
	[SerializeField] private MeshFilter m_Drc;

	private void Start()
	{
		Stopwatch watch = new Stopwatch();
		var dracoLoader = new DracoMeshLoader();
		var mesh = new List<Mesh>();

		watch.Start();
		m_Obj.mesh = Resources.Load("MachineGun", typeof(Mesh)) as Mesh;
		watch.Stop();
		Debug.LogWarning($"FINISH TIME OBJ { watch.Elapsed.TotalSeconds }");

		watch.Reset();

		watch.Start();
		dracoLoader.LoadMeshFromAsset("Models/gun/MachineGun.drc", ref mesh);
		m_Drc.mesh = mesh[0];
		watch.Stop();
		Debug.LogWarning($"FINISH TIME DRC { watch.Elapsed.TotalSeconds }");
	}
}
