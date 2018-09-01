using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour {
    public float moveSpeedMinZoom, moveSpeedMaxZoom;
    public float orthoMoveSpeedMinZoom, orthoMoveSpeedMaxZoom;
    public float stickMinZoom, stickMaxZoom;
    public float swivelMinZoom, swivelMaxZoom;
    public float orthoMinSize, orthoMaxSize;
    public float orthographicAngle;
    public float rotationSpeed;
    public SquareGrid grid;

    Transform swivel, stick;
    Camera mainCamera;
    float zoom = 0.5f;
    float rotationAngle;


    private void Awake()
    {
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
        mainCamera = gameObject.GetComponentInChildren<Camera>();
        AdjustZoom(0); // prevent jerky camera on startup
    }

    private void Update()
    {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if(zoomDelta!=0f)
        {
            AdjustZoom(zoomDelta);
        }

        float rotationDelta = Input.GetAxis("Rotation");
        if (rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        
        if (xDelta != 0f || zDelta != 0f)
        {
            AdjustPosition(xDelta, zDelta);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            mainCamera.orthographic = !mainCamera.orthographic;
            AdjustZoom(0);
        }
    }


    void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance;
        if (mainCamera.orthographic)
        {
            distance = Mathf.Lerp(orthoMoveSpeedMaxZoom, orthoMoveSpeedMinZoom, zoom) * damping * Time.deltaTime;
        }
        else
        {
            distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;
        }
        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = ClampPosition(position);
    }


    Vector3 ClampPosition(Vector3 position)
    {
        float xMax =
            (grid.chunkCountX * GridMetrics.chunkSizeX -1f) *
            (GridMetrics.gridSize);
        position.x = Mathf.Clamp(position.x, 0f, xMax);

        float zMax =
            (grid.chunkCountZ * GridMetrics.chunkSizeZ -1f) *
            (GridMetrics.gridSize);
        position.z = Mathf.Clamp(position.z, 0f, zMax);

        return position;
    }


    void AdjustRotation(float delta)
    {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0f)
        {
            rotationAngle += 360f;
        }
        else if (rotationAngle >= 360f)
        {
            rotationAngle -= 360f;
        }
        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }

    void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance;
        float angle;
        if (mainCamera.orthographic)
        {
            angle = orthographicAngle;
            distance = -50;
            mainCamera.orthographicSize = Mathf.Lerp(orthoMaxSize, orthoMinSize, zoom);
        }
        else
        {
            distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
            angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        }
        stick.localPosition = new Vector3(0f, 0f, distance);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }
}
