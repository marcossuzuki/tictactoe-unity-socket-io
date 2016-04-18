using UnityEngine;
using System.Collections;
using SocketIO;
using UnityEngine.UI;
using System.Collections.Generic;

public enum Player
{
    none,
    red,
    blue
}

public class GameInstanceController : MonoBehaviour
{
    #region Singleton
    private static GameInstanceController instance;

    public static GameInstanceController Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    public SocketIOComponent socketIO;
    public GameObject overlay;
    public Text overlayText;
    public Text youTurnText;
    public GameObject restartButton;
    public CellController[] board;

    public Player PlayerID { get; set; }
    public bool InTurn { get; set; }
    public bool IsGameStarted { get; set; }

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        instance = null;
    }
	
	void Start ()
    {
        socketIO.On("startGame", OnStartGameEvent);
        socketIO.On("move", OnOpponetMoveEvent);
        socketIO.On("endgame", OnEndGameEvent);
        socketIO.On("restart", e =>
        {
            ClearBoard();
            SetInTurn(false);
            restartButton.SetActive(false);
        });
    }	

    void OnStartGameEvent(SocketIOEvent e)
    {
        Debug.Log(e.data);
        IsGameStarted = true;
        var data = e.data.ToDictionary();
        if (data["color"] == "red")
        {
            PlayerID = Player.red;
            youTurnText.color = Color.red;
            SetInTurn(true);
        }
        else
        {
            PlayerID = Player.blue;
            youTurnText.color = Color.blue;
            SetInTurn(false);
        }
    }

    public void OnOpponetMoveEvent(SocketIOEvent e)
    {
        Player id;
        var data = e.data.ToDictionary();
        int pos = int.Parse(data["movePos"]);
        if (data["color"] == "red")
        {
            id = Player.red;
        }
        else
        {
            id = Player.blue;
        }

        board[pos].SetMove(id);
        SetInTurn(true);
    }

    public void OnEndGameEvent(SocketIOEvent e)
    {
        var data = e.data.ToDictionary();
        string winner = data["winner"];

        if (winner == "none")
        {
            SetDraw();
            return;
        }

        if (data["winner"] == PlayerID.ToString())
        {
            SetWin(true);
        }
        else
        {
            SetWin(false);
        }

        restartButton.SetActive(true);
    }

    public void Move(int pos)
    {
        var data = new Dictionary<string, string>();
        data.Add("color", PlayerID.ToString());
        data.Add("movePos", pos.ToString());

        socketIO.Emit("move", JSONObject.Create(data));
        
        SetInTurn(false);

        if (IskWin())
        {
            var winner = new Dictionary<string, string>();
            winner.Add("winner", PlayerID.ToString());
            socketIO.Emit("endgame", JSONObject.Create(winner));
            SetWin(true);
        }
        else
        {
            if (IsDraw())
            {
                var winner = new Dictionary<string, string>();
                winner.Add("winner", "none");
                socketIO.Emit("endgame", JSONObject.Create(winner));
                SetDraw();
            }
        }
    }

    void ClearBoard()
    {
        for (int i = 0; i < board.Length; i++)
        {
            board[i].SetState(Player.none);
        }
    }

    public void RestartGame()
    {
        ClearBoard();
        SetInTurn(true);
        restartButton.SetActive(false);
        socketIO.Emit("restart", JSONObject.Create());
    }

    public void SetInTurn(bool val)
    {
        InTurn = val;
        youTurnText.text = "YOU TURN";
        if (InTurn)
        {
            overlay.SetActive(false);            
            youTurnText.gameObject.SetActive(true);            
        }
        else
        {
            overlay.SetActive(true);
            youTurnText.gameObject.SetActive(false);
            overlayText.text = "Opponent turn!";
        }
    }

    public void SetWin(bool val)
    {
        SetInTurn(true);
        
        if (val)
        {
            youTurnText.text = "YOU WIN";
        }
        else
        {
            youTurnText.text = "YOU LOSE";
        }
    }

    public void SetDraw()
    {
        SetInTurn(false);
        youTurnText.color = Color.white;
        youTurnText.text = "DRAW!";
    }

    bool IsRight(int pos)
    {
        return board[pos].state == PlayerID;
    }

    bool IskWin()
    {
        if (IsRight(1))
        {
            if (IsRight(0) && IsRight(2))
                return true;
        }

        if (IsRight(3))
        {
            if (IsRight(0) && IsRight(6))
                return true;
        }

        if (IsRight(5))
        {
            if (IsRight(2) && IsRight(8))
                return true;    
        }

        if (IsRight(7))
        {
            if (IsRight(6) && IsRight(8))
                return true;
        }

        if (IsRight(4))
        {
            if (IsRight(0) && IsRight(8))
                return true;

            if (IsRight(1) && IsRight(7))
                return true;

            if (IsRight(2) && IsRight(6))
                return true;

            if (IsRight(3) && IsRight(5))
                return true;
        }

        return false;
    }

    bool IsDraw()
    {
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i].state == Player.none)
                return false;
        }
        return true;
    }
}
