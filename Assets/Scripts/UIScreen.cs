using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class UIScreen : MonoBehaviour
{
    public RectTransform containerRects;
    public CanvasGroup containerCanvas;
    public Image background;
    public GameManager.GameState visibleState;
    public float transitionTime;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.OnGameStateUpdated.AddListener(GameStateUpdated);
        bool initialState = GameManager.Instance.gameState == visibleState;
        background.enabled=initialState;
        containerRects.gameObject.SetActive(initialState);  

    }

    private void GameStateUpdated(GameManager.GameState newState)
    {
        if (newState == visibleState)
        {
            ShowScreen();
        }
        else
        {
            HideScreen();
        }
    }

    private void HideScreen()
    {
        //background animation
        var bgColor=background.color;
        bgColor.a = 0;
        background.DOColor(bgColor, transitionTime * 0.5f);
        //container
        containerCanvas.alpha = 1;
        containerRects.anchoredPosition =Vector2.zero;
        containerCanvas.DOFade(0f, transitionTime * 0.5f);
        containerRects.DOAnchorPos(new Vector2(0,-100), transitionTime* 0.5f).onComplete = () =>
        {
            background.enabled = false;
            containerRects.gameObject.SetActive(false);
        };
    }

    private void ShowScreen()
    {
        //enable elements
        background.enabled = true;
        containerRects.gameObject.SetActive(true);
        //background animation
        var bgColor=background.color;
        bgColor.a = 0;
        background.color = bgColor;
        bgColor.a = 1;
        background.DOColor(bgColor,transitionTime);
        //container animation
        containerCanvas.alpha = 0;
        containerRects.anchoredPosition = new Vector2(0,100);
        containerCanvas.DOFade(1f,transitionTime);
        containerRects.DOAnchorPos(Vector2.zero,transitionTime);
    }
}
