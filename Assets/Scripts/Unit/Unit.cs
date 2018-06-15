using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public static List<Unit> UnitsList = new List<Unit>();

    public enum UnitType {Grunt, Jumpship, Tank, Drone, Dreadnought, CommandUnit};
    public enum MovementType { Forward, Side,Orthogonal,Diagonal,Knight,All};

    public UnitType Type;
    public MovementType Movement;
    public Transform Model;
    public int HitPoints;
    public int AttackPower;
    public int AttackRange;
    public int MovementRange;
    public float MovemenSpeed = 1;
    public float RotationSpeed = 2;
    public bool IsPlayerControlled;
    public bool IsSelected;
    public bool CanAttack = true;
    public bool CanMove = true;

    public void GetDamage(int amount)
    {
        HitPoints -= amount;
        GetComponent<UnitUI>().UpdateHitPoints(HitPoints);


        if (HitPoints <= 0)
        {
            Destroy(this.gameObject);
            //BoardManager.Instance.OccupiedFieldsDict.Remove
            GameController.Instance.CheckEndGame();
        }
    }

    void Awake()
    {
        UnitsList.Add(this);
    }

    void OnDestroy()
    {
        UnitsList.Remove(this);
    }
}
