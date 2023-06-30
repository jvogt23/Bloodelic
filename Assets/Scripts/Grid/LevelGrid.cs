using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//[ExecuteInEditMode]
public class LevelGrid : MonoBehaviour
{
    [Header("Debug Grid")]
    [SerializeField]
    private GameObject corner1;
    [SerializeField]
    private GameObject corner2;

    [Header("Creating Grid")]

    [SerializeField]
    private float gridSpaceSize = 1f;
    [SerializeField]
    private GameObject levelObject;
    [SerializeField]
    private GameObject gridCellPrefab;

    private Dictionary<GridCellPosition, GridCell> gridCellExistence;
    private Dictionary<GridCell, List<GridCell>> grid;

    private GridCell testGridCell;
    

    // Start is called before the first frame update
    void Start()
    {
        //DrawGridOutline();
        gridCellExistence = new Dictionary<GridCellPosition, GridCell>();
        grid = new Dictionary<GridCell, List<GridCell>>();

        CallCreateGrid(levelObject.transform);
        ConnectGridCells();
        TurnAllWhite(testGridCell);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected() {
        if (corner1 == null || corner2 == null || gridSpaceSize < 0.5) {
            return;
        }

        Vector3 cornerTemp1 = corner1.transform.position;
        Vector3 cornerTemp2 = corner2.transform.position;

        Vector3 cornerPos1 = new Vector3(Mathf.Max(cornerTemp1.x, cornerTemp2.x), Mathf.Max(cornerTemp1.y, cornerTemp2.y), Mathf.Max(cornerTemp1.z, cornerTemp2.z));
        Vector3 cornerPos2 = new Vector3(Mathf.Min(cornerTemp1.x, cornerTemp2.x), Mathf.Min(cornerTemp1.y, cornerTemp2.y), Mathf.Min(cornerTemp1.z, cornerTemp2.z));

        for (float x = cornerPos2.x; x <= cornerPos1.x; x += gridSpaceSize) {
            for (float y = cornerPos2.y; y <= cornerPos1.y; y += gridSpaceSize) {
                Vector3 start = new Vector3(x, y, cornerPos2.z);
                Vector3 end = new Vector3(x, y, cornerPos1.z);
                Gizmos.color = new Color(0, 0, 1, 0.5f);
                Gizmos.DrawLine(start, end);
            }
        }
        for (float x = cornerPos2.x; x <= cornerPos1.x; x += gridSpaceSize) {
            for (float z = cornerPos2.z; z <= cornerPos1.z; z += gridSpaceSize) {
                Vector3 start = new Vector3(x, cornerPos2.y, z);
                Vector3 end = new Vector3(x, cornerPos1.y, z);
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawLine(start, end);
            }
        }
        for (float y = cornerPos2.y; y <= cornerPos1.y; y += gridSpaceSize) {
            for (float z = cornerPos2.z; z <= cornerPos1.z; z += gridSpaceSize) {
                Vector3 start = new Vector3(cornerPos2.x, y, z);
                Vector3 end = new Vector3(cornerPos1.x, y, z);
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawLine(start, end);
            }
        }
    }
    
    private void CallCreateGrid(Transform transform) {
        int numChild = transform.childCount;
        if (numChild <= 0) {
            return;
        }

        if (gridCellPrefab == null) {
            Debug.LogError("ERROR: Null Grid Cell Prefab");
            return;
        }

        CreateGrid();
    }

    private void CreateGrid() {
        if (corner1 == null || corner2 == null) {
            return;
        }

        Vector3 cornerTemp1 = corner1.transform.position;
        Vector3 cornerTemp2 = corner2.transform.position;

        Vector3 cornerPos1 = new Vector3(Mathf.Min(cornerTemp1.x, cornerTemp2.x), Mathf.Min(cornerTemp1.y, cornerTemp2.y), Mathf.Min(cornerTemp1.z, cornerTemp2.z));
        Vector3 cornerPos2 = new Vector3(Mathf.Max(cornerTemp1.x, cornerTemp2.x), Mathf.Max(cornerTemp1.y, cornerTemp2.y), Mathf.Max(cornerTemp1.z, cornerTemp2.z));

        float offset = gridSpaceSize / 2.0f;
        //DOWN (facing UP) xz
        for (float x = cornerPos1.x; x < cornerPos2.x; x += gridSpaceSize) {
            for (float z = cornerPos1.z; z < cornerPos2.z; z += gridSpaceSize) {
                Ray ray = new Ray(new Vector3(x + offset, cornerPos2.y, z + offset), Vector3.down);
                float y = (Mathf.Round((cornerPos2.y - cornerPos1.y) * 100f) / 100f) + 0.001f;
                RaycastHit[] hits = Physics.RaycastAll(ray, y, 1);
                if (hits.Length == 0) {
                    continue;
                }
                foreach (RaycastHit hit in hits)
                {
                    Vector3 pos = hit.point;
                    float yf = RoundToNearest(pos.y);
                    CreateGridCell(new Vector3(x + offset, yf, z + offset), Quaternion.identity, GridCellPositionEnum.BOTTOM);
                }
            }
        }
        //FRONT (facing BACK) yz
        for (float x = cornerPos1.x; x < cornerPos2.x; x += gridSpaceSize) {
            for (float y = cornerPos1.y; y < cornerPos2.y; y += gridSpaceSize) {
                Ray ray = new Ray(new Vector3(x + offset, y + offset, cornerPos1.z), Vector3.forward);
                float z = (Mathf.Round((cornerPos2.z - cornerPos1.z) * 100f) / 100f) + 0.001f;
                RaycastHit[] hits = Physics.RaycastAll(ray, z, 1);
                if (hits.Length == 0) {
                    continue;
                }
                foreach (RaycastHit hit in hits)
                {
                    Vector3 pos = hit.point;
                    float zf = RoundToNearest(pos.z);
                    CreateGridCell(new Vector3(x + offset, y + offset, zf), Quaternion.Euler(-90, 0, 0), GridCellPositionEnum.FRONT);
                }
            }
        }
        //BACK (facing FRONT) yz
        for (float x = cornerPos1.x; x < cornerPos2.x; x += gridSpaceSize) {
            for (float y = cornerPos1.y; y < cornerPos2.y; y += gridSpaceSize) {
                Ray ray = new Ray(new Vector3(x + offset, y + offset, cornerPos2.z), Vector3.back);
                float z = (Mathf.Round((cornerPos2.z - cornerPos1.z) * 100f) / 100f) + 0.001f;
                RaycastHit[] hits = Physics.RaycastAll(ray, z, 1);
                if (hits.Length == 0) {
                    continue;
                }
                foreach (RaycastHit hit in hits)
                {
                    Vector3 pos = hit.point;
                    float zf = RoundToNearest(pos.z);
                    CreateGridCell(new Vector3(x + offset, y + offset, zf), Quaternion.Euler(-90, 0, -180), GridCellPositionEnum.BACK);
                }
            }
        }
        //RIGHT (facing LEFT) xy
        for (float y = cornerPos1.y; y < cornerPos2.y; y += gridSpaceSize) {
            for (float z = cornerPos1.z; z < cornerPos2.z; z += gridSpaceSize) {
                Ray ray = new Ray(new Vector3(cornerPos1.x, y + offset, z + offset), Vector3.right);
                float x = (Mathf.Round((cornerPos2.x - cornerPos1.x) * 100f) / 100f) + 0.001f;
                RaycastHit[] hits = Physics.RaycastAll(ray, x, 1);
                if (hits.Length == 0) {
                    continue;
                }
                foreach (RaycastHit hit in hits)
                {
                    Vector3 pos = hit.point;
                    float xf = RoundToNearest(pos.x);
                    CreateGridCell(new Vector3(xf, y + offset, z + offset), Quaternion.Euler(-90, 0, 90), GridCellPositionEnum.RIGHT);
                }
            }
        }
        //LEFT (facing RIGHT) xy
        for (float y = cornerPos1.y; y < cornerPos2.y; y += gridSpaceSize) {
            for (float z = cornerPos1.z; z < cornerPos2.z; z += gridSpaceSize) {
                Ray ray = new Ray(new Vector3(cornerPos2.x, y + offset, z + offset), Vector3.left);
                float x = (Mathf.Round((cornerPos2.x - cornerPos1.x) * 100f) / 100f) + 0.001f;
                RaycastHit[] hits = Physics.RaycastAll(ray, x, 1);
                if (hits.Length == 0) {
                    continue;
                }
                foreach (RaycastHit hit in hits)
                {
                    if (hits.Length == 2) {
                        //Debug.Log(hit.collider.gameObject.name);
                    }
                    Vector3 pos = hit.point;
                    float xf = RoundToNearest(pos.x);
                    CreateGridCell(new Vector3(xf, y + offset, z + offset), Quaternion.Euler(-90, 0, -90), GridCellPositionEnum.LEFT);
                }
            }
        }
    }

    private void CreateGridCell(Vector3 pos, Quaternion rot, GridCellPositionEnum posE) {
        GameObject temp = Instantiate(gridCellPrefab, pos, rot);
        temp.transform.parent = gameObject.transform;
        temp.name = $"GridCell; Position: {pos.x}, {pos.y}, {pos.z}; Enum: {posE.ToString()}";
        GridCell gridCell = temp.GetComponent<GridCell>();
        gridCell.Position = new GridCellPosition(pos, posE);
        gridCell.PositionE = posE;
        grid.Add(gridCell, new List<GridCell>());
        switch (posE)
        {
            case GridCellPositionEnum.BOTTOM:
                temp.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
                break;
            case GridCellPositionEnum.FRONT:
                temp.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.magenta;
                break;
            case GridCellPositionEnum.BACK:
                temp.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.cyan;
                break;
            case GridCellPositionEnum.RIGHT:
                temp.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.green;
                break;
            case GridCellPositionEnum.LEFT:
                temp.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.yellow;
                break;
            default:
                temp.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.black;
                break;
        }
        // = Color.blue;
    }

    private void ConnectGridCells() {
        GridCell[] allGridCells = grid.Keys.ToArray();
        Debug.Log(allGridCells.Length);
        testGridCell = allGridCells[0];
        foreach (GridCell gridCell in allGridCells) {
            ConnectGraph(gridCell);
        }
    }

    private void ConnectGraph(GridCell gridCell) {
        bool testing = false;
        if (gridCell.Equals(testGridCell)) {
            testing = true;
        }



        Vector3 position = gridCell.Position.Position;
        if (testing) {
            Debug.Log(position);
        }
        Quaternion rotation = gridCell.gameObject.transform.rotation;
        Quaternion invertRot = Quaternion.Inverse(rotation);
        Vector3 rotVec = rotation * Vector3.forward;

        List<GridCell> gridCellConnections = new List<GridCell>(4);
        Dictionary<GridCell, float> distanceChart = new Dictionary<GridCell, float>();

        Collider[] colliders = Physics.OverlapSphere(position, gridSpaceSize, 1 << 3);
        if (testing) {
            Debug.Log($"Collider Length: {colliders.Length}");
        }
        foreach (Collider collider in colliders) {
            if (testing) {
                Debug.Log(collider.gameObject.name);
            }
            if (collider.Equals(gridCell.Collider)) {
                continue;
            }
            GridCell otherGridCell = collider.gameObject.GetComponent<GridCell>();
            Vector3 dirToCol = collider.gameObject.transform.position - position;
            if (testing) {
                Debug.Log(collider.gameObject.transform.position);
            }
            Vector3 correctDir = invertRot * dirToCol;
            int edgeNum = -1;
            if ((correctDir.x != 0 && correctDir.z != 0) || (correctDir.x == 0 && correctDir.z == 0)) {
                continue;
            }
            if (correctDir.z > 0) {
                edgeNum = 0;
            } else if (correctDir.z < 0) {
                edgeNum = 1;
            } else if (correctDir.x < 0) {
                edgeNum = 2;
            } else if (correctDir.x > 0) {
                edgeNum = 3;
            } 

            if (testing) {
                if (edgeNum == -1) {
                    Debug.Log("BRUH");
                }
                Debug.Log($"Enum: {(int)gridCell.PositionE}; EdgeNum: {edgeNum}; Y: {correctDir.y}");
            }

            GridCellPositionEnum corPosition;
            if (correctDir.y == 0) {
                corPosition = ConstantValues.PositionArray[(int)gridCell.PositionE, edgeNum, 0];
            } else if (correctDir.y > 0) {
                corPosition = ConstantValues.PositionArray[(int)gridCell.PositionE, edgeNum, 1];
            } else {
                corPosition = ConstantValues.PositionArray[(int)gridCell.PositionE, edgeNum, 2];
            }
            
            if (gridCellConnections[edgeNum] == null || distanceChart[gridCellConnections[edgeNum]] > dirToCol.magnitude) {
                gridCellConnections[edgeNum] = otherGridCell;
            } else {
                continue;
            }

            if (otherGridCell.PositionE != corPosition) {
                gridCellConnections[edgeNum] = null;
            } else {
                distanceChart.Add(otherGridCell, dirToCol.magnitude);
            }
        }

        bool isEmpty = true;
        foreach(GridCell otherGridCell in gridCellConnections) {
            if (otherGridCell == null) {
                continue;
            }
            isEmpty = false;
            grid[gridCell].Add(otherGridCell);
        }
        if (isEmpty) {
            //Destroy(gridCell.gameObject);
        }
    }

    private void AddToGraph(GridCell gridCell, GridCellPosition gridCellPosition) {
        GridCell temp = gridCellExistence[gridCellPosition];
        grid[temp].Add(gridCell);
        grid[gridCell].Add(temp);
    }

    private float RoundToNearest(float num) {
        float numRound = Mathf.Round(num * 100f) / 100f;
        float r = numRound % gridSpaceSize;
        return r >= gridSpaceSize / 2.0f ? (numRound - r) + gridSpaceSize : numRound - r;
    }

    //TEST METHODS
    private void TurnAllWhite(GridCell gridCell) {
        gridCell.gameObject.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.blue;
        List<GridCell> list = grid[gridCell];
        foreach (GridCell connectedGridCell in list) {
            TurnAllWhite(connectedGridCell);
        }
    }
}