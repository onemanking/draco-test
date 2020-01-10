using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using UniRx;

namespace ARFoundationExtension.PeopleOcclusion
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	[RequireComponent(typeof(ARCameraManager))]
	[RequireComponent(typeof(ARCameraBackground))]
	public class ARHumanOcculusion : MonoBehaviour
	{
		[SerializeField]
		private AROcclusionManager m_ArOcclusionManager;

		private Material _Material;

		const string DepthTexName = "_textureDepth";
		const string StencilTexName = "_textureStencil";

		static readonly int DepthTexId = Shader.PropertyToID(DepthTexName);
		static readonly int StencilTexId = Shader.PropertyToID(StencilTexName);

		void Start()
		{
			_Material = GetComponent<ARCameraBackground>().material;
			Observable.EveryUpdate()
						.Where(x => m_ArOcclusionManager != null
							&& m_ArOcclusionManager.humanDepthTexture != null
							&& m_ArOcclusionManager.humanStencilTexture)
						.Subscribe(_ =>
						{
							_Material.SetTexture(DepthTexId, m_ArOcclusionManager.humanDepthTexture);
							_Material.SetTexture(StencilTexId, m_ArOcclusionManager.humanStencilTexture);
						}).AddTo(this);
		}
	}
}
