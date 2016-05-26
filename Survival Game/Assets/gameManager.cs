using UnityEngine;
using System.Collections;

public enum GameState { INTRO, MAIN_MENU, PLAY, PAUSE };

public delegate void OnStateChangeHandler();



public class gameManager : MonoBehaviour {
    protected gameManager() { }

    private static gameManager instance = null;

    public event OnStateChangeHandler OnStateChange;

    public GameState gameState { get; private set; }
    
    public static gameManager Instance {
        get {
                if (gameManager.instance == null)
                {
                    DontDestroyOnLoad(gameManager.instance);
                    gameManager.instance = new gameManager();
                }
                return gameManager.instance;
            }
        }

    public void SetGameState(GameState state)
    {
        this.gameState = state;
        OnStateChange();
    }

    public void OnApplicationQuit()
    {
        gameManager.instance = null;
    }
}
