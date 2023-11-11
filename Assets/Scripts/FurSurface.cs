using UnityEngine;

public class FurSurface : MonoBehaviour {
    [SerializeField]
    private int density;
    [SerializeField]
    private float noiseMax;
    [SerializeField]
    private float maxHeight;
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
                furMaterial.SetInteger("_Density", density);
                furMaterial.SetFloat("_NoiseMax", noiseMax);
                furMaterial.SetFloat("_MaxHeight", maxHeight);
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
}
