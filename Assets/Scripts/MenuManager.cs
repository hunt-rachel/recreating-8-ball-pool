using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    
    //initial controls menu leads directly to player 1 turn
    //pause control menu leads to pause menu prior to opening
    public GameObject MainMenu, InitControlsMenu, PauseControlMenu, PauseMenu, WinScreen;
    public Button pauseMenuButton;
    
    void Awake()
    {
        //subscribes to event
        GameManager.OnGameStateChanged += GMGameStateChanged; 
        PauseMenu.gameObject.SetActive(false);
        pauseMenuButton.gameObject.SetActive(false);
    }
    
    void OnDestroy()
    {
        //unsusbscribes to event
        GameManager.OnGameStateChanged -= GMGameStateChanged;
    }

    private void GMGameStateChanged(GameState state)
    {
        MainMenu.SetActive(state == GameState.MainMenuOpen);
        InitControlsMenu.SetActive(state == GameState.InitControlsMenuOpen);
        WinScreen.SetActive(state == GameState.P1Win || state == GameState.P2Win);
    }
    
    //moves from main menu to initial controls menu
    public void StartGame()
    {
        GameManager.Instance.UpdateGameState(GameState.InitControlsMenuOpen);
    }

    //closes the game
    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        Application.Quit(); 
    }

    //updates the game state from the initial controls menu to player 1's turn
    public void closeInitialControlsMenu()
    {
        GameManager.Instance.UpdateGameState(GameState.P1Turn);
        pauseMenuButton.gameObject.SetActive(true);
    }

    //opens the controls menu from the options screen
    public void openPauseControlsMenu()
    {
        PauseControlMenu.SetActive(true);
    }
    
    //closes the controls menu to allow player to return to the options menu
    public void closePauseControlsMenu()
    {
        PauseControlMenu.SetActive(false);
    }

    //opens the options menu, pausing the game for the players
    public void openPauseMenu()
    {
        Time.timeScale = 0.0f;
        pauseMenuButton.gameObject.SetActive(false);
        PauseMenu.SetActive(true);

    }

    //closes the options menu, resuming the game for the players
    public void closePauseMenu()
    {
        PauseMenu.SetActive(false);
        Time.timeScale = 1.0f;
        pauseMenuButton.gameObject.SetActive(true);
    }
}
