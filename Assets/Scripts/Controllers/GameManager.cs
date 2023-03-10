using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static GameState State;

    private static int secondsToNight;

   
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(this);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        secondsToNight = GameConfigManager.SecondsToNightStarts;
        Debug.Log(secondsToNight);

        UpdateGameState(GameState.Day);
    }

    public static void UpdateGameState(GameState newState)
    {
        State = newState;
        switch (newState)
        {
            case GameState.GameStart:
                StartScreenHandler();
                break;
            case GameState.Day:
                DayHandler();
                break;
            case GameState.Night:
                NightHandler();
                break;
            default:
                break;
        }
    }

    

    private static void StartScreenHandler()
    {
      
    }

    private static void DayHandler()
    {
        TavernEventsManager.OnDayStarts();
        TavernEventsManager.OnCameraSwitchToFollowPlayer();
        TavernEventsManager.OnSwitchToDayCanvas();
        Instance.StartCoroutine(NightTimerCoroutine());
    }

    private static void NightHandler()
    {
        TavernEventsManager.OnNightStarts();
        Instance.StartCoroutine(CardGameTiming(6));
    }

    private static IEnumerator NightTimerCoroutine()
    {
        yield return new WaitForSeconds(secondsToNight);
        UpdateGameState(GameState.Night);
    }

    private static IEnumerator CardGameTiming(int seconds)
    {
        int counter = seconds;
        TavernEventsManager.OnCameraSwitchToCardGame();
        while (counter > 0)
        {
            counter--;
            if(counter == 3)
            {
                TavernEventsManager.OnSwitchToNightCanvas();
            }
            if(counter == 2)
            {
                TavernEventsManager.OnRenderCards();
            }
            yield return new WaitForSeconds(1);
        }
    }
    
    public enum GameState
    {
        GameStart,
        Day,
        Night
    }
}