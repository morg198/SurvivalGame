using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour {
    gameManager GM;

    public void HandleOnStateChange()
    {
        Debug.Log("State change!");
    }

    public void StartGame()
    {
        GM.SetGameState(GameState.PLAY);
        SceneManager.LoadScene("dylanTest001");
        Debug.Log(GM.gameState);
    }


    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    void Awake()
    {
        GM = gameManager.Instance;
        GM.OnStateChange += HandleOnStateChange;
    }

  
    public void OnGUI()
    {
        GUI.BeginGroup(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 50, 100, 800));
        GUI.Box(new Rect(0, 0, 100, 200), "Menu");
        if (GUI.Button(new Rect(10, 40, 80, 30), "Start"))
        {
            StartGame();
            
        }
        if (GUI.Button(new Rect(10, 160, 80, 30), "Quit"))
        {
            Quit();
        }
        GUI.EndGroup();
    }

  


}
