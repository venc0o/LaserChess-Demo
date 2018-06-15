using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public enum GameState {Initialize, PlayerTurn, EnemyTurn, Win, Loss, PerformingAction };

    public GameState State = GameState.Initialize;

    public GameObject EndTurnPanelConfirm;
    public GameObject EndGamePanel;

    public CanvasGroup UICanvas;
    public Text EndGamePanelText;
    
    public Button EndTurnBtn;
    public CanvasGroup EndTurnCanvas;

    public Button MenuBtn;
    public Button StartGameBtn;
    public Dropdown DifficultySelector;

    public int Difficulty = 1;

    GameState prevState;

    [System.Serializable]
    private struct DifficultySett
    {
        [SerializeField]
        public int GruntSpawnTime;
        [SerializeField]
        public int JumpshipSpawnTime;
        [SerializeField]
        public int TankSpawnTime;
        [SerializeField]
        public int DroneSpawnTime;
        [SerializeField]
        public int DreadnoughtSpawnTime;
        [SerializeField]
        public int CommanderSpawnTime;
    }

    [SerializeField]
    private DifficultySett Difficulty1;
    [SerializeField]
    private DifficultySett Difficulty2;
    [SerializeField]
    private DifficultySett Difficulty3;

    void Awake()
    {
        Instance = this;    
    }

    void Start()
    {
        Time.timeScale = 1;

        EndTurnBtn.onClick.AddListener(CheckAvActionsOnEndTurn);
        StartGameBtn.onClick.AddListener(StartGame);

        Camera.main.transform.parent.GetComponent<CameraMovement>().enabled = false;
        Camera.main.transform.parent.GetComponent<CameraEdgeMovement>().enabled = false;

        EndTurnBtn.gameObject.SetActive(false);
        MenuBtn.gameObject.SetActive(false);
    }

    public void EndTurn()
    {
        if (EndTurnPanelConfirm != null)
            EndTurnPanelConfirm.SetActive(false);

        State = (State == GameState.EnemyTurn || State == GameState.Initialize) ? GameState.PlayerTurn : GameState.EnemyTurn;
        EndTurnBtn.enabled = State == GameState.PlayerTurn ? true : false;

        foreach (Unit u in Unit.UnitsList)
        {
            u.CanAttack = State == GameState.PlayerTurn ? u.IsPlayerControlled : !u.IsPlayerControlled;
            u.CanMove = State == GameState.PlayerTurn ? u.IsPlayerControlled : !u.IsPlayerControlled;

            BoardManager.Instance.UnitSelection(u, false);
            
            if (State == GameState.EnemyTurn)
                u.GetComponent<UnitUI>().ShowActionMarker(false);
            else
                u.GetComponent<UnitUI>().ShowActionMarker(true);
        }

        if (State == GameState.EnemyTurn)
        {
            GetComponent<AI>().PerformTurn();
            StartCoroutine(ShowTurnText("enemy"));
        }
        else
        {
            StartCoroutine(ShowTurnText("your"));
        }
    }

    public void CheckEndGame()
    {
        List<Unit> commUnits = Unit.UnitsList.Where(i => i.Type == Unit.UnitType.CommandUnit).ToList();
        List<Unit> friendlyUnits = Unit.UnitsList.Where(i => i.IsPlayerControlled).ToList();
        List<Unit> enemyUits = Unit.UnitsList.Where(i => !i.IsPlayerControlled).ToList();
        List<Unit> droneUnits = Unit.UnitsList.Where(i => i.Type == Unit.UnitType.Drone && !i.IsPlayerControlled).ToList();

        if (friendlyUnits.Count == 0)
        {
            State = GameState.Loss;        
        }

        foreach (Unit un in droneUnits)
            if (BoardManager.Instance.GetIndexOfField(BoardManager.Instance.GetPositionOfUnit(un)).y == 0)
            {
                State = GameState.Loss;
                break;
            }

        if (commUnits.Count == 0 || commUnits.Count == enemyUits.Count)
        {
            State = GameState.Win;
            EndGamePanelText.text = "VICTORY!";
        }

        if (State == GameState.Loss)
            EndGamePanelText.text = "DEFEAT!";

        if (State == GameState.Win || State == GameState.Loss)
        {
            GetComponent<AI>().StopAllCoroutines();
            Time.timeScale = 0;
            EndGamePanel.SetActive(true);
        }
    }

    public void SwitchAction(bool state)
    {
        if (state)
        {
            prevState = State;
            State = GameState.PerformingAction;
            UICanvas.interactable = false;
        }
        else
        {
            State = prevState;
            UICanvas.interactable = true;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void StartGame()
    {
        Difficulty = DifficultySelector.value + 1;

        List<DifficultySett> sett = new List<DifficultySett>() { Difficulty1, Difficulty2, Difficulty3 };

        InitializeUnits(0, sett[Difficulty - 1].GruntSpawnTime, 1, 1, true);
        InitializeUnits(1, sett[Difficulty - 1].JumpshipSpawnTime, 0, 0, true);
        InitializeUnits(2, sett[Difficulty - 1].TankSpawnTime, 0, 0, true);

        InitializeUnits(5, sett[Difficulty - 1].CommanderSpawnTime, 7, 5, false);
        InitializeUnits(3, sett[Difficulty - 1].DroneSpawnTime, 6, 5, false);
        InitializeUnits(4, sett[Difficulty - 1].DreadnoughtSpawnTime, 6, 4, false);


        StartGameBtn.transform.parent.gameObject.SetActive(false);
        EndTurnBtn.gameObject.SetActive(true);
        MenuBtn.gameObject.SetActive(true);

        Camera.main.transform.parent.GetComponent<CameraMovement>().enabled = true;
        Camera.main.transform.parent.GetComponent<CameraEdgeMovement>().enabled = true;

        EndTurn();
    }


    void InitializeUnits(int spawnUnitIndex, int spawnTimes, int startRow, int endRow, bool isPlayer)
    {
        BoardManager board = BoardManager.Instance;

        int ranPos = 0;
        int row = startRow;

        for (int i = 0; i < spawnTimes; i++)
        {
            ranPos = Random.Range(0, 8);

            while (board.IsOccupied(board.BoardPartsArray[ranPos, row]))
            {
                ranPos = Random.Range(0, 8);
            }

            board.SpawnUnit(spawnUnitIndex, board.BoardPartsArray[ranPos, row], isPlayer);

            if (row == endRow)
                row = startRow;
            else
            {
                int c_value = startRow > endRow ? row - 1 : row + 1;
                row = c_value;
            }

           
        }

    }
    void CheckAvActionsOnEndTurn()
    {
        if (State != GameState.PlayerTurn || EndTurnPanelConfirm == null)
            return;

        bool isAllDone = true;

        foreach (Unit unit in Unit.UnitsList.Where(i => i.IsPlayerControlled).ToList())
        {
            List<GameObject> possibleMoves = unit.GetComponent<UnitMovement>().GetMoveFields(BoardManager.TileType.Move);
            List<GameObject> possibleAttacks = unit.GetComponent<UnitAttack>().GetAttackTiles(BoardManager.Instance.GetPositionOfUnit(unit),true,BoardManager.TileType.Attack);

            if ((unit.CanMove && possibleMoves.Count > 0) || (unit.CanAttack && possibleAttacks.Count > 0))
            {
                isAllDone = false;
                EndTurnPanelConfirm.SetActive(true);
                break;
            }
        }

        if (isAllDone)
            EndTurn();
    }

    public IEnumerator ShowTurnText(string PlayerName)
    {
        yield return new WaitForSeconds(1f);

        Text msgText = EndTurnCanvas.transform.GetComponentInChildren<Text>();

        EndTurnCanvas.gameObject.SetActive(true);
        EndTurnCanvas.alpha = 1f;

        msgText.text = ("It's <color=#ffa500ff>" + PlayerName + "</color> turn.").ToUpper();

        yield return new WaitForSeconds(2f);

        while (EndTurnCanvas.alpha > 0)
        {
            EndTurnCanvas.alpha -= Time.deltaTime;
            yield return null;
        }

        EndTurnCanvas.gameObject.SetActive(false);
    }




}
