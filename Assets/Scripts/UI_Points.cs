using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UI_Points : MonoBehaviour
{
    int displayPoints = 0;
    public TextMeshProUGUI pointsLabel;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.OnPointsUpdated.AddListener(UpdatePoints);
    }

    private void UpdatePoints()
    {
        StartCoroutine(UpdatePointsCoroutine());
    }

    IEnumerator UpdatePointsCoroutine()
    {
        while (displayPoints < GameManager.Instance.Points)
        {
            displayPoints++;
            pointsLabel.text=displayPoints.ToString();
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
