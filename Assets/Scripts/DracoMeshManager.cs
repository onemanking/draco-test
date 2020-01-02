using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DracoMeshManager : MonoBehaviour
{
	private const string _RESOURCES_FOLDER = "Resources";
	private const string _MODEL_FOLDER = "Models";

	public static DracoMeshManager Instance => _Instance;
	private static DracoMeshManager _Instance;

	private Dictionary<string, List<Mesh>> _MeshDict = new Dictionary<string, List<Mesh>>();

	void Awake()
	{
		_Instance = GetComponent<DracoMeshManager>();
		InitResources();
	}

	private void InitResources()
	{
		var root = new DirectoryInfo(Path.Combine(Application.dataPath, _RESOURCES_FOLDER, _MODEL_FOLDER));
		var modelDict = new Dictionary<string, List<string>>();

		foreach (var dir in root.GetDirectories())
		{
			var files = dir.GetFiles("*.drc.bytes");
			modelDict.Add(dir.Name, files.Select(_ => Path.GetFileNameWithoutExtension(_.Name)).OrderBy(x => x).ToList());
		}

		var dracoLoader = new DracoMeshLoader();

		foreach (var key in modelDict.Keys)
		{
			var meshList = new List<Mesh>();
			foreach (var asset in modelDict[key])
			{
				dracoLoader.LoadMeshFromAsset(Path.Combine(_MODEL_FOLDER, key, asset), ref meshList);
			}
			_MeshDict.Add(key, meshList);
		}

		GC.Collect();
	}

	public int ListCount(string _modelName) => _MeshDict[_modelName].Count;

	public Mesh GetMesh(string _modelName, int _index) => _MeshDict[_modelName][_index];

}
