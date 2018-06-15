using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    public Projectile Bullet;
    public Transform Turret;

    Unit unit;

    void Start()
    {
        unit = GetComponent<Unit>();    
    }

    public void Attack(Unit target)
    {
        if (!unit.IsPlayerControlled)
            Camera.main.transform.parent.GetComponent<CameraMovement>().Target = target.transform;

        StartCoroutine(PerformAttack(target));
    }

    public List<GameObject> GetAttackTiles(GameObject startField, bool getOccupiedOnly, BoardManager.TileType type)
    {
        BoardManager board = BoardManager.Instance;
        List<GameObject> attackFields = new List<GameObject>();


        if (unit.Type == Unit.UnitType.Grunt || unit.Type == Unit.UnitType.Drone)
        {
            attackFields = board.GetDiagonalFields(startField, unit.AttackRange, type);
        }

        if (unit.Type == Unit.UnitType.Jumpship || unit.Type == Unit.UnitType.Tank)
        {
            attackFields = board.GetOrthogonalFields(startField, unit.AttackRange, type, false);
        }

        if (unit.Type == Unit.UnitType.Dreadnought)
        {
            attackFields = board.GetOrthogonalFields(startField, unit.AttackRange, type, false)
                .Concat(board.GetDiagonalFields(startField, unit.AttackRange, type)).ToList();
        }

        if (getOccupiedOnly)
        {
            attackFields = attackFields.Where(i => BoardManager.Instance.OccupiedFieldsDict[i] != null && 
                BoardManager.Instance.OccupiedFieldsDict[i].IsPlayerControlled != unit.IsPlayerControlled).ToList();
        }

        return attackFields;
    }

    IEnumerator PerformAttack(Unit target)
    {
        GameController.Instance.SwitchAction(true);

        unit.CanAttack = false;
        GetComponent<UnitUI>().ShowMarkers(false);

        Transform rotSource = transform;
        Quaternion oldOrientation = transform.rotation;

        if (Turret != null)
        {
            rotSource = Turret;
            oldOrientation = Turret.rotation;
        }

        Vector3 oldPos = rotSource.position;

        var targetRotation = Quaternion.LookRotation(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z) - rotSource.position);
        targetRotation.eulerAngles = new Vector3(0, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);

        //this is offset for Jumpship or Dreadnought
        Vector3 offset = rotSource.position + Vector3.up / 2;

        if (unit.Type == Unit.UnitType.Jumpship || unit.Type == Unit.UnitType.Dreadnought)
        {
            while (Vector3.Distance(rotSource.position, offset) > 0.2f)
            {
                rotSource.position = Vector3.MoveTowards(rotSource.position, offset, Time.deltaTime);
                yield return null;
            }
        }

        while (Mathf.Abs(rotSource.rotation.eulerAngles.y - targetRotation.eulerAngles.y) > 1)
        {
            rotSource.rotation = Quaternion.RotateTowards(rotSource.rotation, targetRotation, Time.deltaTime * (unit.RotationSpeed * 10));

            yield return null;
        }

        //simulate loading time
        yield return new WaitForSeconds(0.5f);

        if (unit.Type == Unit.UnitType.Jumpship || unit.Type == Unit.UnitType.Dreadnought)
        {
            foreach (Unit enemy in Unit.UnitsList.Where(x => unit.IsPlayerControlled != x.IsPlayerControlled).ToList())
            {

                if (GetAttackTiles(BoardManager.Instance.GetPositionOfUnit(unit), true,BoardManager.TileType.Attack).Contains(BoardManager.Instance.GetPositionOfUnit(enemy)))
                {
                    FireProjectile(enemy);
                }
            }
        }
        else
        {
            FireProjectile(target);
        }

        targetRotation = oldOrientation;

        yield return new WaitForSeconds(1f);

        while (Mathf.Abs(rotSource.rotation.eulerAngles.y - targetRotation.eulerAngles.y) > 1)
        {
            rotSource.rotation = Quaternion.RotateTowards(rotSource.rotation, targetRotation, Time.deltaTime * (unit.RotationSpeed * 10));
            yield return null;
        }

        if (unit.Type == Unit.UnitType.Jumpship || unit.Type == Unit.UnitType.Dreadnought)
        {
            while (Vector3.Distance(rotSource.position, oldPos) > 0.01f)
            {
                rotSource.position = Vector3.MoveTowards(rotSource.position, oldPos, Time.deltaTime);
                yield return null;
            }
        }

        GameController.Instance.SwitchAction(false);

        UnitUI unitUI = GetComponent<UnitUI>();

        if (!unit.CanMove || GetComponent<UnitMovement>().GetMoveFields(BoardManager.TileType.Move).Count == 0)
        {
            unitUI.ShowActionMarker(false);
            unitUI.ShowTeamIndicator(true);
        }
        else
            unitUI.ShowMarkers(true);

        GameController.Instance.CheckEndGame();
    }

    void FireProjectile(Unit enemy)
    {
        Projectile proj = (Projectile)Instantiate(Bullet, Bullet.transform.position, Quaternion.identity);
        proj.transform.LookAt(enemy.transform);
        proj.Owner = unit;
        proj.Target = enemy;
        proj.gameObject.SetActive(true);
    }
}
