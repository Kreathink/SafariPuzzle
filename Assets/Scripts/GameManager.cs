using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public int Points = 0;
    public UnityEvent OnPointsUpdated;

    // Update is called once per frame
    public void Addpoint(int newPoints)
    {
        Points += newPoints;
        OnPointsUpdated?.Invoke();
    }
}
