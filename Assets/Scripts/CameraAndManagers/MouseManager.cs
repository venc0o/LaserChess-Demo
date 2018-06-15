using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour
{
    public AudioSource clickSoundActive;
    public AudioSource clickSoundInactive;

    BoardManager board;

    Unit selectedUnit;
    EventSystem eventSys;

    void Start()
    {
        board = BoardManager.Instance;
        eventSys = GameObject.Find("EventSystem").GetComponent<EventSystem>();
    }

    void Update()
    {
        if (Unit.UnitsList.Count == 0 || GameController.Instance.State == GameController.GameState.PerformingAction)
            return;

        if (eventSys.currentSelectedGameObject != null)
        {

            if (Input.GetMouseButtonUp(0))
            {

                if (eventSys.currentSelectedGameObject.transform.tag == "Button" && clickSoundActive != null)
                    clickSoundActive.Play();

            }

            foreach (Unit unit in Unit.UnitsList)
                board.UnitSelection(unit, false);

            selectedUnit = null;
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {

                if (hit.transform.tag == "Unit")
                {

                    Unit unit = hit.transform.GetComponent<Unit>();

                    board.UnitSelection(unit, !unit.IsSelected);

                    if (unit.IsSelected)
                    {
                        if (clickSoundActive != null)
                            clickSoundActive.Play();

                        selectedUnit = unit;
                    }
                    else
                    {
                        if (clickSoundInactive != null)
                            clickSoundInactive.Play();
                    }
                }

                else if (hit.transform.tag == "BoardPart")
                {
                    if (board.OccupiedFieldsDict[hit.transform.gameObject] == null || board.OccupiedFieldsDict[hit.transform.gameObject].IsSelected)
                    {

                        foreach (Unit unit in Unit.UnitsList)
                            board.UnitSelection(unit, false);

                        selectedUnit = null;

                        if (clickSoundInactive != null)
                            clickSoundInactive.Play();
                    }

                    else
                    {
                        foreach (Unit unit in Unit.UnitsList)
                            board.UnitSelection(unit, false);

                        board.UnitSelection(board.OccupiedFieldsDict[hit.transform.gameObject], true);
                        selectedUnit = board.OccupiedFieldsDict[hit.transform.gameObject];

                        if (clickSoundActive != null)
                            clickSoundActive.Play();
                    }
                }
            }
            else
            {
                foreach (Unit unit in Unit.UnitsList)
                    board.UnitSelection(unit, false);

                selectedUnit = null;
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {

                if (hit.transform.tag == "Unit")
                {
                    if (selectedUnit.IsPlayerControlled && selectedUnit.CanAttack)
                    { 
                        if (!hit.transform.GetComponent<Unit>().IsPlayerControlled && board.ActiveFields.Contains(board.GetPositionOfUnit(hit.transform.GetComponent<Unit>())))
                        {
                            if (clickSoundActive != null)
                                clickSoundActive.Play();

                            selectedUnit.GetComponent<UnitAttack>().Attack(hit.transform.GetComponent<Unit>());
                            board.UnitSelection(selectedUnit, false);

                            selectedUnit = null;
                        }
                    }
                }

                else if (hit.transform.tag == "BoardPart")
                {
                    if (board.ActiveFields.Contains(hit.transform.gameObject))
                    {
                        if (clickSoundActive != null)
                            clickSoundActive.Play();

                        
                        if (board.OccupiedFieldsDict[hit.transform.gameObject] != null)
                            selectedUnit.GetComponent<UnitAttack>().Attack(board.OccupiedFieldsDict[hit.transform.gameObject]);
                        else
                            selectedUnit.GetComponent<UnitMovement>().Move(hit.transform.gameObject);
                        
                        board.UnitSelection(selectedUnit, false);
              
                        selectedUnit = null;
                    }
                }

            }
        }
    }

}
