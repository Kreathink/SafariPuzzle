using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UIGameOver : MonoBehaviour
{
    public int displayedPoints = 0;
    public TextMeshProUGUI pointsUI;
    void Start()
    {
        GameManager.Instance.OnGameStateUpdated.AddListener(GameStateUpdated);
    }
    private void OnDestroy()
    {
        GameManager.Instance.OnGameStateUpdated.RemoveListener(GameStateUpdated);
    }
    private void GameStateUpdated(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.GameOver)
        {
            displayedPoints = 0;
            StartCoroutine(DisplayPointsCoroutine());
        }
    }
    IEnumerator DisplayPointsCoroutine()
    {
        while (displayedPoints < GameManager.Instance.Points)
        {
            displayedPoints++;
            pointsUI.text = displayedPoints.ToString();
            yield return new WaitForFixedUpdate();
        }
        displayedPoints = GameManager.Instance.Points;
        pointsUI.text = displayedPoints.ToString();
        yield return null;
    }

    public void PlayAgainButon()
    {
        GameManager.Instance.RestartGame();
    }
    public void ExitGameButon()
    {
        GameManager.Instance.ExitGame();
    }

}
