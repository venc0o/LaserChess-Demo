using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    public AudioSource MoveSound;

    Unit unit;

    float m_soundLevel;

    void Start()
    {
        unit = GetComponent<Unit>();

        if (MoveSound != null)
            m_soundLevel = MoveSound.volume;
    }

    public void Move(GameObject field)
    {
        BoardManager.Instance.OccupiedFieldsDict[BoardManager.Instance.GetPositionOfUnit(unit)] = null;

        BoardManager.Instance.OccupiedFieldsDict[field] = unit;

        unit.CanMove = false;

        Vector3 newPos = field.transform.position + new Vector3(0, unit.Model.lossyScale.y / 2, 0);
        Quaternion originOrientation = unit.transform.rotation;


        if (!unit.IsPlayerControlled)
            Camera.main.transform.parent.GetComponent<CameraMovement>().Target = unit.transform;

        StartCoroutine(PerformMove(originOrientation,newPos));

    }

    public List<GameObject> GetMoveFields(BoardManager.TileType type)
    {
        BoardManager board = BoardManager.Instance;

        List<GameObject> moveFields = new List<GameObject>();

        GameObject unitPos = board.GetPositionOfUnit(unit);

        if (unit.Movement == Unit.MovementType.Forward)
        {
            if (unit.IsPlayerControlled)
            {
               moveFields = board.GetForwardFields(unitPos, unit.MovementRange);
            }
            else
            {
                moveFields = board.GetForwardFields(unitPos, -unit.MovementRange);
            }
        }

        if (unit.Movement == Unit.MovementType.Side)
        {
            moveFields = board.GetOrthogonalFields(unitPos, unit.MovementRange, type, true);
        }

        if (unit.Movement == Unit.MovementType.Orthogonal)
        {
            moveFields = board.GetOrthogonalFields(unitPos, unit.MovementRange, type, false);
        }

        if (unit.Movement == Unit.MovementType.Diagonal)
        {
            moveFields = board.GetDiagonalFields(unitPos, unit.MovementRange, type);
        }

        if (unit.Movement == Unit.MovementType.Knight)
        {
            moveFields = board.GetKnightFields(unitPos);
        }

        if (unit.Movement == Unit.MovementType.All)
        {
            moveFields = board.GetOrthogonalFields(unitPos, unit.MovementRange, type, false)
                .Concat(board.GetDiagonalFields(unitPos, unit.MovementRange, type)).ToList();
        }

        return moveFields;
    }

    IEnumerator PerformMove(Quaternion oldOrientation, Vector3 newPos)
    {

        GameController.Instance.SwitchAction(true);

        GetComponent<UnitUI>().ShowMarkers(false);

        StartCoroutine(PlayMoveSound(true));

        var targetRotation = Quaternion.LookRotation(new Vector3(newPos.x, transform.position.y, newPos.z) - transform.position);
        targetRotation.eulerAngles = new Vector3(0, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);

        //this is offset for Jumpship
        Vector3 offset = transform.position + Vector3.up/2;

        //this is only for the Grunt model, usually unit.Model is declared as Animator
        Animation anim = unit.Model.GetComponent<Animation>();

        if (anim != null && anim.GetClipCount() > 1)
            unit.Model.GetComponent<Animation>().CrossFade("walk_forward");

        while (Mathf.Abs(transform.rotation.eulerAngles.y - targetRotation.eulerAngles.y) > 1)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * (unit.RotationSpeed * 10));

            if (unit.Type == Unit.UnitType.Jumpship)
                transform.position = Vector3.MoveTowards(transform.position, offset, Time.deltaTime);

           yield return null;
        }

        while (Vector3.Distance(transform.position, newPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, newPos, Time.deltaTime * (unit.MovemenSpeed/10));
            yield return null;
        }

        if (anim != null && anim.GetClipCount() > 1)
            unit.Model.GetComponent<Animation>().CrossFade("idle");

        targetRotation = oldOrientation;

        while (Mathf.Abs(transform.rotation.eulerAngles.y - targetRotation.eulerAngles.y) > 1)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * (unit.RotationSpeed * 10));
            yield return null;
        }

        StartCoroutine(PlayMoveSound(false));

        GameController.Instance.SwitchAction(false);

        if (unit.Type == Unit.UnitType.Drone)
            GameController.Instance.CheckEndGame();

        UnitUI unitUI = GetComponent<UnitUI>();

        if (!unit.CanAttack || GetComponent<UnitAttack>().GetAttackTiles(BoardManager.Instance.GetPositionOfUnit(unit), true, BoardManager.TileType.Attack).Count == 0)
        {
            unitUI.ShowActionMarker(false);
            unitUI.ShowTeamIndicator(true);
        }
        else
            unitUI.ShowMarkers(true);        
    }

    IEnumerator PlayMoveSound(bool state)
    {
        if (MoveSound != null)
        {
            
            
            if (state)
            {
                MoveSound.Play();

                MoveSound.volume = 0;

                while (MoveSound.volume < m_soundLevel - 0.01f)
                {
                    MoveSound.volume = Mathf.Lerp(MoveSound.volume, m_soundLevel, Time.deltaTime * 5);
                    yield return null;
                }
            }
            else
            {
                while (MoveSound.volume > 0.01f)
                {
                    MoveSound.volume = Mathf.Lerp(MoveSound.volume, 0, Time.deltaTime * 5);
                    yield return null;
                }

                MoveSound.Stop();
                MoveSound.volume = m_soundLevel;
            }
        }
    }

}
