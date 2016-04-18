using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CellController : MonoBehaviour
{
    public Player state;
    public int id;
    public Vector2 pos;

    Button button;
    Text text;
    
    void Awake()
    {
        button = GetComponent<Button>();
        text = GetComponentInChildren<Text>();
        button.onClick.AddListener(OnTouch);
        pos = new Vector2(id / 3, id % 3);
    }

	// Use this for initialization
	void Start ()
    {
        SetState(Player.none);
	}
	
    public void SetState(Player s)
    {
        state = s;
        switch (state)
        {
            case Player.none:
                button.interactable = true;
                text.text = "";
                break;
            case Player.red:
                button.interactable = false;
                text.text = "X";
                text.color = Color.red;
                break;
            case Player.blue:
                button.interactable = false;
                text.text = "O";
                text.color = Color.blue;
                break;
            default:
                break;
        }
    }

    public Player GetState()
    {
        return state;
    }

    public void Reset()
    {
        SetState(Player.none);
    }

    public void OnTouch()
    {
        SetState(GameInstanceController.Instance.PlayerID);
        GameInstanceController.Instance.Move(id);
    }

    public void SetMove(Player playerID)
    {
        SetState(playerID);
    }
}
