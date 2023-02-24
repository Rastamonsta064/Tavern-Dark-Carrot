using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class VisitorsManager : MonoBehaviour
{
    [SerializeField] private GameObject visitorPrefab;
    [SerializeField] private Transform visitorSpawnPoint;
    [SerializeField] private int visitorsSpawnQuantity;

    private System.Random random = new System.Random();
    private Table[] tables;
    private List<VisitorAI> defenders = new List<VisitorAI>();
    private static List<GameObject> activeVisitors = new List<GameObject>();

    private ObjectPool<GameObject> pool;
    private Coroutine spawnCoroutine;
    private Coroutine respawnCoroutine;
    private Table emptyTable;

    private void OnEnable()
    {
        TavernEventsManager.HeartRepaired += StartVisitersSpawn;
        TavernEventsManager.VisitorLeftTavern += OnVisitorLeft;
        TavernEventsManager.DefenderAdded += AddVisitorToDefendersList;
        TavernEventsManager.NightStarted += NightHandler;
    }

    private void OnDisable()
    {
        TavernEventsManager.HeartRepaired -= StartVisitersSpawn;
        TavernEventsManager.VisitorLeftTavern -= OnVisitorLeft;
        TavernEventsManager.DefenderAdded -= AddVisitorToDefendersList;
        TavernEventsManager.NightStarted -= NightHandler;
    }
   
    private void AddVisitorToDefendersList(VisitorAI visitor)
    {
        defenders.Add(visitor);
    }

    private void Start() => tables = FindObjectsOfType<Table>();
    
    private void Awake() => pool = new ObjectPool<GameObject>(CreateVisitor, OnGetVisitorFromPool, OnReturnVisitorToPool);

    private GameObject CreateVisitor()
    {
        var v = Instantiate(visitorPrefab, visitorSpawnPoint);
        v.GetComponent<VisitorAI>().SetPool(pool);
        v.SetActive(false);
        return v;
    }

    private void OnGetVisitorFromPool(GameObject visitor)
    {
        visitor.SetActive(true);
        visitor.GetComponent<VisitorAI>().SetStats(10, 1);
        visitor.transform.position = visitorSpawnPoint.transform.position;
    }

    private void OnReturnVisitorToPool(GameObject visitor)
    {
        visitor.SetActive(false);
    }

    private void StartVisitersSpawn()
    {
        spawnCoroutine = StartCoroutine(SpawnDelayCoroutine());
    }

    IEnumerator SpawnDelayCoroutine()
    {
        while(activeVisitors.Count <= GameConfigManager.MaxVisitersQuantity)
        {
            TrySpawnVisitor();
            yield return new WaitForSeconds(random.Next(GameConfigManager.VisitorsSpawnDelayMin, GameConfigManager.VisitorsSpawnDelayMax));
        }
    }
    
    private void TrySpawnVisitor()
    {
        VisitorAI v;
        if (TryGetEmptyTable(out emptyTable))
        {
            emptyTable.isEmpty = false;
            GameObject visitor = pool.Get();
            v = visitor.GetComponent<VisitorAI>();
            v.SetStats(10, 1);
            v.SetTarget(emptyTable.transform);
        } else
        {
            Debug.Log("No empty chairs");
        }
    }

    private bool TryGetEmptyTable(out Table emptyTable)
    {
        emptyTable = tables.Where(table => table.isEmpty).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
        return (emptyTable != null);
    }

    private void OnVisitorLeft(GameObject visitor)
    {
        activeVisitors.Remove(visitor);
        pool.Release(visitor);
    }

    private void NightHandler()
    {
        if(respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }
       if(spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        foreach (Table chair in tables)
        {
            chair.isEmpty = true;
        }
        if (defenders.Count > 0)
        {
            TavernEventsManager.OnDefendersToCards(new List<VisitorAI>(defenders));
        }
        foreach (GameObject visitor in activeVisitors)
        {
            pool.Release(visitor);
        }

        defenders.Clear();
        activeVisitors.Clear();
    }
   
}