using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour {

    public GameObject ActionMarker;

    public Image TeamIndicator;
    public Text HitPointsText;
    public Text AttackPointsText;

    Unit unit;

    public void UpdateHitPoints(int amount)
    {
        if (HitPointsText == null)
            return;

        if (amount < 0)
            amount = 0;

        HitPointsText.text = amount.ToString();
    }

    public void UpdateAttackPoints(int amount)
    {
        if (AttackPointsText == null)
            return;

        AttackPointsText.text = amount.ToString();
    }

    public void ShowMarkers(bool state)
    {
        if (TeamIndicator != null)
            TeamIndicator.enabled = state;

        ShowActionMarker(state);

    }

    public void ShowTeamIndicator(bool state)
    {
        if (TeamIndicator == null)
            return;

            TeamIndicator.enabled = state;
    }
    public void ShowActionMarker(bool state)
    {
        if (ActionMarker == null)
            return;

        ActionMarker.SetActive(state);
    }

    void Start()
    {
        unit = GetComponent<Unit>();

        UpdateHitPoints(unit.HitPoints);
        UpdateAttackPoints(unit.AttackPower);
    }

}
