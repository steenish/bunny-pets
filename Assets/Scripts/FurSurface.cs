using UnityEngine;

public class FurSurface : MonoBehaviour {
    [SerializeField]
    private int density;
    [SerializeField]
    private float noiseMax;
    [SerializeField]
    private float maxHeight;
    [SerializeField]
    private float gravity;
    [SerializeField]
    private float forceInfluence;
    [SerializeField]
    private float attenuation;
    [SerializeField]
    private float occlusionBias;
    [SerializeField]
    private int shellCount;
    [SerializeField]
    private GameObject furPrefab;

    private Material furMaterial;

    private void Start() {
        ConstructShells();
    }

    private void ConstructShells() {
        for(int i = 0; i < shellCount; i++) {
            GameObject shellGameObject = Instantiate(furPrefab, transform);
            MeshRenderer shellRenderer = shellGameObject.GetComponent<MeshRenderer>();
            if(i == 0) {
                furMaterial = shellRenderer.material;
                SetShaderValues();
            }
            shellRenderer.material = furMaterial;
            shellGameObject.name = $"Shell {i}";
            MaterialPropertyBlock mpb = new();
            shellRenderer.GetPropertyBlock(mpb);
            float normalizedHeight = (float) i / (shellCount - 1);
            mpb.SetFloat("_NormalizedHeight", normalizedHeight);
            shellRenderer.SetPropertyBlock(mpb);
        }
    }

    private void Update() {
        SetShaderValues();
    }

    private void SetShaderValues() {
        furMaterial.SetInteger("_Density", density);
        furMaterial.SetFloat("_NoiseMax", noiseMax);
        furMaterial.SetFloat("_MaxHeight", maxHeight);
        furMaterial.SetFloat("_Gravity", gravity);
        furMaterial.SetFloat("_ForceInfluence", forceInfluence);
        furMaterial.SetFloat("_Attenuation", attenuation);
        furMaterial.SetFloat("_OcclusionBias", occlusionBias);
    }

    public void SetVectorField(Texture2D vectorFieldTexture) {
        furMaterial.SetTexture("_VectorField", vectorFieldTexture);
    }
}
