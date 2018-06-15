using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public enum TileType { Move,Attack,Display };

    public GameObject[,] BoardPartsArray;
    public List<Unit> UnitsPrefabList = new List<Unit>();

    public Dictionary<GameObject,Unit> OccupiedFieldsDict = new Dictionary<GameObject, Unit>();

    [HideInInspector]
    public List<GameObject> ActiveFields = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> InactiveFields = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> attackMarkers = new List<GameObject>();

    public GameObject BoardPartPrefab;
    public GameObject GridOverlay;
    public GameObject SelectionMarker;

    public LineRenderer AttackMarkerPrefab;

    public Transform BoardPartHolder;
    public Transform BoardLineHolder;

    public Color ActiveTileColor;
    public Color InactiveTileColor;
    public Color ActiveAttackTileColor;

    public bool ShowBoardLines;

    Vector3 pieceSize;

    int sizeX = 8;
    int sizeY = 8;

    public Vector2Int GetIndexOfField(GameObject field)
    {
        Vector2Int result = new Vector2Int(-1, -1);

        for (int r = 0; r < sizeX; r++)
        {
            for (int c = 0; c < sizeY; c++)
            {
                if (BoardManager.Instance.BoardPartsArray[r, c] == field)
                {
                    result = new Vector2Int(r, c);
                    break;
                }
            }
        }

        return result;
    }

    public bool IsOccupied(GameObject field)
    {
        if (OccupiedFieldsDict.ContainsKey(field) && OccupiedFieldsDict[field] != null)
            return true;
        else
            return false;
    }

    public GameObject GetPositionOfUnit(Unit unit)
    {
        GameObject obj = null;

        foreach (GameObject field in OccupiedFieldsDict.Keys)
        {
            if (OccupiedFieldsDict[field] == unit)
            {
                obj = field;
                break;
            }
        }

        return obj;
    }

    public void SpawnUnit(int unitPrefabIndex,GameObject currBoardPart, bool isPlayer)
    {
        GameObject unit = UnitsPrefabList[unitPrefabIndex].gameObject;

        GameObject go = (GameObject)Instantiate(unit, currBoardPart.transform.position + new Vector3(0,unit.GetComponent<Unit>().Model.lossyScale.y/2,0), Quaternion.identity);

        if (!isPlayer)
            go.transform.eulerAngles = new Vector3(0, 180, 0);

        OccupiedFieldsDict[currBoardPart] = go.GetComponent<Unit>();
        go.GetComponent<Unit>().IsPlayerControlled = isPlayer;      
    }

    public void UnitSelection(Unit unit, bool isSelected)
    {
        RemoveActiveFields();
        RemoveInactiveFields();
        RemoveAttackLines();

        SelectionMarker.SetActive(false);

        foreach (Unit u in Unit.UnitsList)
        {
            u.IsSelected = false;

            if (u == unit)
                unit.IsSelected = isSelected;

            if (u.IsSelected)
            {
                SelectionMarker.transform.position = new Vector3(u.transform.position.x, 0, u.transform.position.z);
                SelectionMarker.SetActive(true);

                List<GameObject> moveFieldsAll = u.GetComponent<UnitMovement>().GetMoveFields(TileType.Display);
                List<GameObject> moveFieldsAv = u.GetComponent<UnitMovement>().GetMoveFields(TileType.Move);

                foreach (GameObject obj in moveFieldsAll)
                {
                    if (moveFieldsAv.Contains(obj))
                        ShowField(obj, false, u.IsPlayerControlled && u.CanMove);
                    else
                        ShowField(obj, false, false);
                }

                List<GameObject> attackFieldsAll = u.GetComponent<UnitAttack>().GetAttackTiles(GetPositionOfUnit(u),false, TileType.Display);
                List<GameObject> attackFieldsAv = u.GetComponent<UnitAttack>().GetAttackTiles(GetPositionOfUnit(u), false, TileType.Attack);

                foreach (GameObject obj in attackFieldsAll)
                {
                    bool isOccupied = false;

                    foreach (Unit un in Unit.UnitsList.Where(i => !i.IsPlayerControlled).ToList())
                    {
                        if (GetPositionOfUnit(un) == obj && attackFieldsAv.Contains(obj))
                        {
                            ShowField(obj, true, unit.CanAttack);
                            isOccupied = true;
                        }

                    }

                    if (!isOccupied)
                        ShowField(obj, true, false);

                }
            }

        }
    }

    public List<GameObject> GetForwardFields(GameObject startField, int range)
    {
        List<GameObject> fields = new List<GameObject>();
        Vector2Int pos = GetIndexOfField(startField);

        if (range > 0)
        {
            for (int c = pos.y + 1; c < pos.y + 1 + range; c++)
            {
                if (c >= 0 && c < sizeY)
                {
                    if (IsOccupied(BoardPartsArray[pos.x, c]))
                    {
                        break;
                    }
              
                    fields.Add(BoardPartsArray[pos.x, c]);
                }
            }
        }
        else
        {
            for (int c = pos.y - 1; c > pos.y - 1 + range; c--)
            {
                if (c >= 0 && c < sizeY)
                {
                    if (IsOccupied(BoardPartsArray[pos.x, c]))
                    {
                        break;
                    }

                    fields.Add(BoardPartsArray[pos.x, c]);
                }
            }
        }

        return fields;
    }

    public List<GameObject> GetOrthogonalFields(GameObject startField, int range, TileType type, bool sideOnly)
    {
        List<GameObject> fields = new List<GameObject>();
        Vector2Int pos = GetIndexOfField(startField);

        for (int rangeX = pos.x + 1; rangeX <= pos.x + range; rangeX++)
        {

            if (rangeX < sizeX && rangeX >= 0 && rangeX != pos.x)
            {
                if (OccupiedFieldsDict[BoardPartsArray[rangeX, pos.y]] != null)
                {
                    if (type == TileType.Attack)
                        fields.Add(BoardPartsArray[rangeX, pos.y]);

                    if (type == TileType.Move || type == TileType.Attack)
                        break;
                }

                fields.Add(BoardPartsArray[rangeX, pos.y]);
            }
        }

        for (int rangeX = pos.x - 1; rangeX >= pos.x - range; rangeX--)
        {
            if (rangeX < sizeX && rangeX >= 0 && rangeX != pos.x)
            {
                if (IsOccupied(BoardPartsArray[rangeX, pos.y]))
                {
                    if (type == TileType.Attack)
                        fields.Add(BoardPartsArray[rangeX, pos.y]);
                    if (type == TileType.Move || type == TileType.Attack)
                        break;
                }

                fields.Add(BoardPartsArray[rangeX, pos.y]);
            }
        }

        if (!sideOnly)
        {
            for (int rangeY = pos.y + 1; rangeY <= pos.y + range; rangeY++)
            {
                if (rangeY < sizeY && rangeY >= 0 && rangeY != pos.y)
                {
                    if (IsOccupied(BoardPartsArray[pos.x, rangeY]))
                    {
                        if (type == TileType.Attack)
                            fields.Add(BoardPartsArray[pos.x, rangeY]);
                        if (type == TileType.Move || type == TileType.Attack)
                            break;
                    }

                    fields.Add(BoardPartsArray[pos.x, rangeY]);
                }
            }


            for (int rangeY = pos.y - 1; rangeY >= pos.y - range; rangeY--)
            {
                if (rangeY < sizeY && rangeY >= 0 && rangeY != pos.y)
                {
                    if (IsOccupied(BoardPartsArray[pos.x, rangeY]))
                    {
                        if (type == TileType.Attack)
                            fields.Add(BoardPartsArray[pos.x, rangeY]);

                        if (type == TileType.Move || type == TileType.Attack)
                            break;
                    }

                    fields.Add(BoardPartsArray[pos.x, rangeY]);
                }
            }
        }
        
        return fields;
    }

    public List<GameObject> GetDiagonalFields(GameObject startField, int range, TileType type)
    {
        List<GameObject> fields = new List<GameObject>();
        Vector2Int pos = GetIndexOfField(startField);

        for (int rng = 1; rng <= range; rng++)
        {
            if (pos.x + rng >= 0 && pos.x + rng < sizeX && pos.y + rng >= 0 && pos.y + rng < sizeY)
            {

                if (IsOccupied(BoardPartsArray[pos.x + rng, pos.y + rng]))
                {
                    if (type == TileType.Attack)
                        fields.Add(BoardPartsArray[pos.x + rng, pos.y + rng]);

                    if (type == TileType.Move || type == TileType.Attack)
                        break;
                }

                fields.Add(BoardPartsArray[pos.x + rng, pos.y + rng]);
            }
        }

        for (int rng = 1; rng <= range; rng++)
        {
            if (pos.x - rng >= 0 && pos.x - rng < sizeX && pos.y + rng >= 0 && pos.y + rng < sizeY)
            {
                if (IsOccupied(BoardPartsArray[pos.x - rng, pos.y + rng]))
                {
                    if (type == TileType.Attack)
                        fields.Add(BoardPartsArray[pos.x - rng, pos.y + rng]);

                    if (type == TileType.Move || type == TileType.Attack)
                        break;
                }

                fields.Add(BoardPartsArray[pos.x - rng, pos.y + rng]);
            }
        }

        for (int rng = -1; rng >= -range; rng--)
        {
            if (pos.x - rng >= 0 && pos.x - rng < sizeX && pos.y + rng >= 0 && pos.y + rng < sizeY)
            {
                if (IsOccupied(BoardPartsArray[pos.x - rng, pos.y + rng]))
                {
                    if (type == TileType.Attack)
                        fields.Add(BoardPartsArray[pos.x - rng, pos.y + rng]);

                    if (type == TileType.Move || type == TileType.Attack)
                        break;
                }

                fields.Add(BoardPartsArray[pos.x - rng, pos.y + rng]);
            }
        }

        for (int rng = -1; rng >= -range; rng--)
        {
            if (pos.x + rng >= 0 && pos.x + rng < sizeX && pos.y + rng >= 0 && pos.y + rng < sizeY)
            {
                if (IsOccupied(BoardPartsArray[pos.x + rng, pos.y + rng]))
                {
                    if (type == TileType.Attack)
                        fields.Add(BoardPartsArray[pos.x + rng, pos.y + rng]);

                    if (type == TileType.Move || type == TileType.Attack)
                        break;
                }

                fields.Add(BoardPartsArray[pos.x + rng, pos.y + rng]);
            }
        }


        return fields;
    }

    public List<GameObject> GetKnightFields(GameObject startField)
    {
        List<GameObject> fields = new List<GameObject>();
        Vector2Int pos = GetIndexOfField(startField);

        for (int rngX = pos.x - 2; rngX <= pos.x + 2; rngX += 4)
        {
            if (rngX >= 0 && rngX < sizeX && pos.y + 1 < sizeY)
            {
                if (!IsOccupied(BoardPartsArray[rngX, pos.y + 1]))
                {
                    fields.Add(BoardPartsArray[rngX, pos.y + 1]);
                }
            }

            if (rngX >= 0 && rngX < sizeX && pos.y - 1 >= 0 && pos.y - 1 < sizeY)
            {
                if (!IsOccupied(BoardPartsArray[rngX, pos.y - 1]))
                {
                    fields.Add(BoardPartsArray[rngX, pos.y - 1]);
                }
            }
        }

        for (int rngX = pos.x - 1; rngX <= pos.x + 1; rngX += 2)
        {
            if (rngX >= 0 && rngX < sizeX && pos.y + 2 < sizeY)
            {
                if (!IsOccupied(BoardPartsArray[rngX, pos.y + 2]))
                {
                    fields.Add(BoardPartsArray[rngX, pos.y + 2]);
                }
            }

            if (rngX >= 0 && rngX < sizeX && pos.y - 2 >= 0 && pos.y - 2 < sizeY)
            {
                if (!IsOccupied(BoardPartsArray[rngX, pos.y - 2]))
                {
                    fields.Add(BoardPartsArray[rngX, pos.y - 2]);
                }
            }
        }
        return fields;
    }

    public void ShowField(GameObject field,bool isAttackTile, bool isActive)
    {
        
        if (isActive)
        {
            field.GetComponent<MeshRenderer>().enabled = true;

            if (!isAttackTile)
                field.GetComponent<Renderer>().material.color = ActiveTileColor;
            else
            {
                DrawSquareAroundField(field);
                field.GetComponent<Renderer>().material.color = ActiveAttackTileColor;
            }

            ActiveFields.Add(field);
        }
        else
        {
            if (!isAttackTile)
            {
                field.GetComponent<MeshRenderer>().enabled = true;
                field.GetComponent<Renderer>().material.color = InactiveTileColor;
            }
            else
            {
                DrawSquareAroundField(field);
            }
            
            InactiveFields.Add(field);
        }

        
    }

    void GenerateBoard()
    {

        for (int r = 0; r < sizeX; r++)
        {
            for (int c = 0; c < sizeY; c++)
            {
                Vector3 pos = new Vector3(r * pieceSize.x, 0f, c * pieceSize.z);

                BoardPartsArray[r, c] = (GameObject)Instantiate(BoardPartPrefab, pos, Quaternion.identity);
                BoardPartsArray[r, c].transform.SetParent(BoardPartHolder);

                OccupiedFieldsDict.Add(BoardPartsArray[r, c],null);
            }
        }
    }

    void ShowLines()
    {
        if (!ShowBoardLines || GridOverlay == null)
            return;

        
        GridOverlay.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(sizeX, sizeY));
        GridOverlay.transform.localScale = new Vector3((float)sizeX / 10, 1, (float)sizeY / 10);
        GridOverlay.transform.position = new Vector3((pieceSize.x * sizeX)/2 - 0.5f, GridOverlay.transform.position.y, (pieceSize.z * sizeY)/2 - 0.5f);
        GridOverlay.SetActive(ShowBoardLines);
    }

    void DrawSquareAroundField(GameObject field)
    {
        for (int i = 0; i < 4; i++)
        {
            LineRenderer rend = (LineRenderer)Instantiate(AttackMarkerPrefab, field.transform.position, Quaternion.identity);

            Vector3 fsize = field.GetComponent<Renderer>().bounds.size;
            Vector3 fpos = field.transform.position;

            if (i == 0)
            {
                rend.SetPosition(0, new Vector3(fpos.x - (fsize.x / 2), 0.01f, fpos.z - (fsize.z / 2)));
                rend.SetPosition(1, new Vector3(fpos.x - (fsize.x / 2), 0.01f, fpos.z + (fsize.z / 2)));
            }
            if (i == 1)
            {
                rend.SetPosition(0, new Vector3(fpos.x - (fsize.x / 2), 0.01f, fpos.z - (fsize.z / 2)));
                rend.SetPosition(1, new Vector3(fpos.x + (fsize.x / 2), 0.01f, fpos.z - (fsize.z / 2)));
            }
            if (i == 2)
            {
                rend.SetPosition(0, new Vector3(fpos.x - (fsize.x / 2), 0.01f, fpos.z + (fsize.z / 2)));
                rend.SetPosition(1, new Vector3(fpos.x + (fsize.x / 2), 0.01f, fpos.z + (fsize.z / 2)));
            }
            if (i == 3)
            {
                rend.SetPosition(0, new Vector3(fpos.x + (fsize.x / 2), 0.01f, fpos.z + (fsize.z / 2)));
                rend.SetPosition(1, new Vector3(fpos.x + (fsize.x / 2), 0.01f, fpos.z - (fsize.z / 2)));
            }

            rend.transform.SetParent(BoardLineHolder);

            attackMarkers.Add(rend.gameObject);
        }
    }

    void RemoveActiveFields()
    {
        foreach (GameObject field in ActiveFields)
            field.GetComponent<MeshRenderer>().enabled = false;

        ActiveFields.Clear();
    }

    void RemoveInactiveFields()
    {
        foreach (GameObject field in InactiveFields)
            field.GetComponent<MeshRenderer>().enabled = false;

        InactiveFields.Clear();
    }

    void RemoveAttackLines()
    {
        foreach (GameObject field in attackMarkers)
            Destroy(field);

        attackMarkers.Clear();
    }

    void Awake()
    {
        Instance = this;

        BoardPartsArray = new GameObject[sizeX, sizeY];
        
        pieceSize = BoardPartPrefab.GetComponent<Renderer>().bounds.size;

        GenerateBoard();

        ShowLines();
    }

}
