using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InputData {
    public float rightLeftAxisRaw;
    public float forwardBackAxisRaw;
    public float upDownAxisRaw;
    public bool unlockMouse;
    public float mouseXAxis;
    public float mouseYAxis;
    public bool quit;
}

public class PetsController : MonoBehaviour {
    [SerializeField]
    private float translateSpeed;
    [SerializeField]
    private float translateSmoothing;
    [SerializeField]
    private float rotateSpeed;

    private InputData inputData;
    private Vector3 targetPosition;

    private void Awake() {
        targetPosition = transform.position;
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

    }
}
