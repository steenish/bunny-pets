using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public struct InputData {
    public float rightLeftAxisRaw;
    public float forwardBackAxisRaw;
    public float upDownAxisRaw;
    public bool unlockMouse;
    public float mouseXAxis;
    public float mouseYAxis;
    public bool quit;
    public bool mousePressed;
    public Vector2 mouseScreenPosition;
}

public class PetsController : MonoBehaviour {
    [SerializeField]
    private float translateSpeed;
    [SerializeField]
    private float translateSmoothing;
    [SerializeField]
    private float rotateSpeed;
    [SerializeField]
    private int vectorFieldSize;
    [SerializeField]
    private RawImage rawImage;
    [SerializeField]
    private FurSurface furSurface;

    private Camera mainCamera;
    private InputData inputData;
    private Vector3 targetPosition;
    private Texture2D vectorFieldTexture;
    private bool hitLastUpdate;
    private Vector2 lastHitUv;

    private void Awake() {
        targetPosition = transform.position;
        mainCamera = GetComponent<Camera>();
        vectorFieldTexture = new(vectorFieldSize, vectorFieldSize, TextureFormat.RG16, false, true);
        Color[] initPixels = new Color[vectorFieldSize * vectorFieldSize];
        vectorFieldTexture.SetPixels(initPixels);
        vectorFieldTexture.Apply();
    }

    private void Start() {
        rawImage.texture = vectorFieldTexture;
    }

    private void Update() {
        GetInputData();
        HandleQuitting();
        MoveCamera();
        Pets();
    }

    private void GetInputData() {
        inputData.rightLeftAxisRaw = Input.GetAxisRaw("Horizontal");
        inputData.forwardBackAxisRaw = Input.GetAxisRaw("Vertical");
        inputData.upDownAxisRaw = Input.GetKey(KeyCode.Space) ? 1.0f : (Input.GetKey(KeyCode.LeftControl) ? -1.0f : 0.0f);
        inputData.unlockMouse = Input.GetKey(KeyCode.E);
        inputData.mouseXAxis = Input.GetAxis("Mouse X");
        inputData.mouseYAxis = Input.GetAxis("Mouse Y");
        inputData.quit = Input.GetKeyDown(KeyCode.Escape);
        inputData.mousePressed = Input.GetMouseButton(0);
        inputData.mouseScreenPosition = Input.mousePosition;
    }

    private void HandleQuitting() {
        if(!inputData.quit) {
            return;
        }
        Application.Quit();
    }

    private void MoveCamera() {
        TranslateCamera();
        RotateCamera();
    }

    private void TranslateCamera() {
        Vector3 rawTranslation = inputData.rightLeftAxisRaw * transform.right + inputData.forwardBackAxisRaw * transform.forward + inputData.upDownAxisRaw * Vector3.up;
        targetPosition += rawTranslation.normalized * translateSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, targetPosition, translateSmoothing * Time.deltaTime);
        furSurface.SetVectorField(vectorFieldTexture);
    }

    private void RotateCamera() {
        if(inputData.unlockMouse) {
            Cursor.lockState = CursorLockMode.Confined;
            return;
        }
        Cursor.lockState = CursorLockMode.Locked;
        float rotateIncrement = rotateSpeed * Time.deltaTime;
        float xMouseMovement = -inputData.mouseYAxis * rotateIncrement;
        float yMouseMovement = inputData.mouseXAxis * rotateIncrement;
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        eulerAngles += xMouseMovement * Vector3.right + yMouseMovement * Vector3.up;
        eulerAngles.z = 0.0f;
        transform.rotation = Quaternion.Euler(eulerAngles);
        ClampRotation();
    }

    private void ClampRotation() {
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        float xAngle = eulerAngles.x;
        float clampToAngle = 70.0f;
        if(xAngle >= clampToAngle && xAngle < 180.0f) {
            xAngle = clampToAngle;
        }
        else if(xAngle <= 360.0f - clampToAngle && xAngle >= 180.0f) {
            xAngle = 360.0f - clampToAngle;
        }
        eulerAngles.x = xAngle;
        transform.rotation = Quaternion.Euler(eulerAngles);
    }

    private void Pets() {
        if(!inputData.unlockMouse || !inputData.mousePressed) {
            hitLastUpdate = false;
            return;
        }
        Ray ray = mainCamera.ScreenPointToRay(inputData.mouseScreenPosition);
        if(!Physics.Raycast(ray, out RaycastHit hitInfo)) {
            hitLastUpdate = false;
            return;
        }
        Vector2 hitUv = hitInfo.textureCoord;
        if(hitLastUpdate && hitUv != lastHitUv) {
            //Vector2 petDirection = 0.5f * ((hitUv - lastHitUv).normalized + Vector2.right + Vector2.up);
            //int pixelX = Mathf.FloorToInt(hitUv.x * vectorFieldSize);
            //int pixelY = Mathf.FloorToInt(hitUv.y * vectorFieldSize);
            //Color directionColor = new Color(petDirection.x, petDirection.y, 0.0f);
            //vectorFieldTexture.SetPixel(pixelX, pixelY, directionColor);
            //vectorFieldTexture.Apply();
            Vector2 petDirection = (hitUv - lastHitUv).normalized;
            Vector2 projectedTangentDirection = Vector2.right;
            float signedAngle = Vector2.SignedAngle(projectedTangentDirection, petDirection);
            float angle = signedAngle > 0.0f ? signedAngle : 360.0f - signedAngle;
            int pixelX = Mathf.FloorToInt(hitUv.x * vectorFieldSize);
            int pixelY = Mathf.FloorToInt(hitUv.y * vectorFieldSize);
            Color directionColor = new Color(angle / 360.0f, 1.0f, 0.0f);
            vectorFieldTexture.SetPixel(pixelX, pixelY, directionColor);
            vectorFieldTexture.Apply();
            furSurface.SetVectorField(vectorFieldTexture);
        }
        lastHitUv = hitUv;
        hitLastUpdate = true;
    }
}
