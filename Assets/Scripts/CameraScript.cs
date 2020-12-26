﻿using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField]
    private float scrollSpeed;
    [SerializeField]
    private float moveSpeed;
    private Vector2 movement;
    private float scrollMovement;
    private Camera cameraComp;
    private GridManager gridManager;
    Vector2 cameraRealSize => new Vector2(cameraComp.orthographicSize * 2 * cameraComp.aspect, cameraComp.orthographicSize * 2);
    Rect worldView;
    bool viewChanged;

    // Start is called before the first frame update
    void Start() {
        Init();
        UpdateView();
    }
    private void Init() {
        cameraComp = GetComponent<Camera>();
        gridManager = GridManager._instance;
    }

    // Update is called once per frame
    void Update() {
        viewChanged = false;
        movement = new Vector2(
            Input.GetKey(KeyCode.D) ? 1 : 0 - (Input.GetKey(KeyCode.A) ? 1 : 0),
            Input.GetKey(KeyCode.W) ? 1 : 0 - (Input.GetKey(KeyCode.S) ? 1 : 0)
            );
        movement *= moveSpeed;
        if (movement != Vector2.zero) {
            transform.position += (Vector3)movement;
            viewChanged = true;
        }

        scrollMovement = scrollSpeed * -Input.GetAxis("Mouse ScrollWheel");
        if (scrollMovement != 0) {
            cameraComp.orthographicSize += scrollMovement;
            viewChanged = true;
        }

        if (Input.GetKey(KeyCode.Mouse0)) {
            BuildingLayer layer = (Input.GetKey(KeyCode.LeftShift)) ? BuildingLayer.Buildings : BuildingLayer.Floor;
            Vector2Int gridPosition = MouseGridPosition(BuildingLayer.Floor);
            gridManager.SetTile(new ObsidianTile(), gridPosition,  layer);

        }
        else if (Input.GetKey(KeyCode.Mouse1)) {
            BuildingLayer layer = (Input.GetKey(KeyCode.LeftShift)) ? BuildingLayer.Buildings : BuildingLayer.Floor;
            Vector2Int gridPosition = MouseGridPosition(layer);
            gridManager.SetTile(null, gridPosition,  layer);

        }
        else if (Input.GetKeyDown(KeyCode.LeftControl)) {
            BuildingLayer layer = (Input.GetKey(KeyCode.LeftShift)) ? BuildingLayer.Buildings : BuildingLayer.Floor;
            Debug.Log(gridManager.GetTileFromGrid(MouseGridPosition(layer), layer));
        }
        else if (Input.GetKeyDown(KeyCode.Mouse2)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            BuildingLayer layer = (Input.GetKey(KeyCode.LeftShift)) ? BuildingLayer.Buildings : BuildingLayer.Floor;
            TileHit hit = gridManager.GetHitFromClickPosition(mousePos, layer);

            Debug.Log(hit.tile);
            if (hit.tile != null && hit.gridPosition != null
                && hit.tile.interactionType == ToolInteraction.Any) {
                Debug.Log("Color change");
                hit.tile.GatherInteraction((Vector2Int)hit.gridPosition, layer);
            }
        }

        if (viewChanged) {
            UpdateView();
        }
    }
    private Vector2Int MouseGridPosition(BuildingLayer buildingLayer) {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return gridManager.WorldToGridPosition(mousePos, buildingLayer);
    }
    private void UpdateView() {
        Vector3 camPosition = (Vector2)transform.position;
        camPosition -= (Vector3)cameraRealSize / 2;
        worldView = new Rect(camPosition, cameraRealSize);
        gridManager.UpdateView(worldView);
    }
}
