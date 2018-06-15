using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{

    bool isInAction = false;

    public void PerformTurn()
    {
        List<Unit> dronesList = Unit.UnitsList.Where(i => i.Type == Unit.UnitType.Drone).ToList();

        foreach (Unit drone in dronesList)
        {
            var moveFields = drone.GetComponent<UnitMovement>().GetMoveFields(BoardManager.TileType.Move);

            if (moveFields.Count > 0 && drone.GetComponent<UnitAttack>().GetAttackTiles(moveFields[0], true,BoardManager.TileType.Attack).Count > 0)
            {
                StartCoroutine(MoveAIUnit(drone));
                StartCoroutine(AIUnitAttack(drone));
            }
            else
            {
                StartCoroutine(AIUnitAttack(drone));
                StartCoroutine(MoveAIUnit(drone));
            }
        }

        List<Unit> dreadList = Unit.UnitsList.Where(i => i.Type == Unit.UnitType.Dreadnought).ToList();

        foreach (Unit dread in dreadList)
        {

            StartCoroutine(MoveAIUnit(dread));
            StartCoroutine(AIUnitAttack(dread));
        }

        List<Unit> commList = Unit.UnitsList.Where(i => i.Type == Unit.UnitType.CommandUnit).ToList();

        foreach (Unit comm in commList)
        {
            comm.CanAttack = false;
            StartCoroutine(MoveAIUnit(comm));
        }

        StartCoroutine(EndAITurn());
    }

    IEnumerator MoveAIUnit(Unit unit)
    {
        while (isInAction)
            yield return null;

        isInAction = true;

        //simulate thinking
        yield return new WaitForSeconds(1f);

        UnitMovement dmove = unit.GetComponent<UnitMovement>();
        List<GameObject> moveFields = dmove.GetMoveFields(BoardManager.TileType.Move);

        while (GameController.Instance.State == GameController.GameState.PerformingAction)
            yield return null;


        if (unit.Type == Unit.UnitType.CommandUnit)
        {
            if (isInDangerField(BoardManager.Instance.GetPositionOfUnit(unit)) && moveFields.Count > 0)
            {
                GameObject fPos = BoardManager.Instance.GetPositionOfUnit(unit);

                GameObject mFiled = FindSafestField(fPos, moveFields);

                if (mFiled != fPos)
                {
                    //simulate thinking
                    yield return new WaitForSeconds(1f);

                    dmove.Move(FindSafestField(fPos, moveFields));
                }
            }
        }
        else
        {
            if (moveFields.Count > 0 && unit.CanMove)
            {
                //simulate thinking
                yield return new WaitForSeconds(1f);

                if (unit.Type == Unit.UnitType.Dreadnought)
                    dmove.Move(AttackFieldClosestToEnemy(unit, moveFields, true));
                else
                {
                    GameObject m_field = AttackFieldClosestToEnemy(unit, moveFields, false);
                    //move drones only if the field is not in danger
                    if (!isInDangerField(m_field))
                        dmove.Move(m_field);
                }
        
            }
          
        }

        unit.CanMove = false;

        //simulate thinking
        yield return new WaitForSeconds(1f);

        isInAction = false;
    }

    IEnumerator AIUnitAttack(Unit unit)
    {
        while (isInAction)
            yield return null;

        isInAction = true;

        //simulate thinking
        yield return new WaitForSeconds(1f);

        UnitAttack dattack = unit.GetComponent<UnitAttack>();

        GameObject unitPos = BoardManager.Instance.GetPositionOfUnit(unit);

        List<GameObject> attackFields = dattack.GetAttackTiles(unitPos, true,BoardManager.TileType.Attack)
            .OrderBy(i => Vector3.Distance(i.transform.position, unitPos.transform.position)).ToList();

        while (GameController.Instance.State == GameController.GameState.PerformingAction)
            yield return null;


        if (attackFields.Count > 0)
        {
            //simulate thinking
            yield return new WaitForSeconds(1f);

            Unit closestEnemy = Unit.UnitsList.Where(i => BoardManager.Instance.GetPositionOfUnit(i) == attackFields[0]).ToList()[0];

            if (unit.CanAttack)
                dattack.Attack(closestEnemy);

        }
        
        unit.CanAttack = false;
        
        //simulate thinking
        yield return new WaitForSeconds(0.5f);

        isInAction = false;
    }

    bool isInDangerField(GameObject field)
    {
        bool result = false;

        List<Unit> enemyList = Unit.UnitsList.Where(i => i.IsPlayerControlled).ToList();

        foreach (Unit enemy in enemyList)
        {
            if (enemy.GetComponent<UnitAttack>().GetAttackTiles(BoardManager.Instance.GetPositionOfUnit(enemy),true,BoardManager.TileType.Attack).Contains(field))
            {
                result = true;
                break;
            }
        }

        return result;
    }

    GameObject FindSafestField(GameObject startPosField, List<GameObject> availableFields)
    {
        GameObject safestF = startPosField;

        //add the current field as first to check if it is the best place
        availableFields.Insert(0, startPosField);

        foreach (GameObject field in availableFields)
        {
            if (!isInDangerField(field))
            {
                safestF = field;
                break;
            }
        }

        //find the weakest enemy in case all fields are endangered
        if (safestF == startPosField && isInDangerField(safestF))
        {
            List<Unit> enemyList = Unit.UnitsList.Where(i => i.IsPlayerControlled).OrderBy(i => i.AttackPower).ToList();

            List<GameObject> allAttFields = new List<GameObject>();

            foreach (Unit enemy in enemyList)
            {
                UnitAttack enemy_attack = enemy.GetComponent<UnitAttack>();
                allAttFields.Concat(enemy_attack.GetAttackTiles(BoardManager.Instance.GetPositionOfUnit(enemy), true,BoardManager.TileType.Attack));
            }

            //sort by least endangered field (one field can be endangered from more than one unit)
            allAttFields = allAttFields.Distinct().ToList();

            for (int i = allAttFields.Count; i >= 0; i--)
            {
                if (availableFields.Contains(allAttFields[i - 1]))
                {
                    safestF = allAttFields[i-1];
                    break;
                }
            }
        }

        return safestF;
    }

    GameObject AttackFieldClosestToEnemy(Unit unit, List<GameObject> availableFields,bool getWithMostSurrounds)
    {
        List<GameObject> allAttFields = new List<GameObject>();

        GameObject endField = BoardManager.Instance.GetPositionOfUnit(unit);

        foreach (GameObject field in availableFields)
            allAttFields.Concat(unit.GetComponent<UnitAttack>().GetAttackTiles(field,true,BoardManager.TileType.Attack));

        //for dreadnought get the field with most surrounding enemies for maximum damage
        if (getWithMostSurrounds)
            allAttFields = allAttFields.OrderByDescending(i => unit.GetComponent<UnitAttack>().GetAttackTiles(i, true, BoardManager.TileType.Attack).Count).ToList();
        else
            allAttFields = allAttFields.OrderBy(i => Vector3.Distance(i.transform.position, endField.transform.position)).ToList();

        List<Unit> enemyList = Unit.UnitsList.Where(i => i.IsPlayerControlled).OrderBy(i => Vector3.Distance(i.transform.position,unit.transform.position)).ToList();

        bool fieldFound = false;

        foreach (GameObject field in allAttFields)
        {
            if (BoardManager.Instance.OccupiedFieldsDict[field] != null)
            {
                endField = field;
                fieldFound = true;
                break;
            }

        }

        //if there is no direct position to attack then move to closest enemy
        if (!fieldFound)
            endField = availableFields.OrderBy(i => Vector3.Distance(enemyList.First().transform.position, i.transform.position)).ToList().First();

        return endField;
    }

    IEnumerator EndAITurn()
    {
        while (isInAction)
            yield return null;


        //simulate thinking
        yield return new WaitForSeconds(1f);

        GameController.Instance.EndTurn();

    }
}
