using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteriorManager : MonoBehaviour
{
    public static InteriorManager singleton
    {
        get
        {
            return FindObjectOfType<InteriorManager>();
        }
    }

    public Transform[] spawnPoints;

    [System.Serializable]
    public class Wave
    {
        public float spawnRate = 1.0f;
        private float _spawnRate = 1.0f;
        private int spawnIndex = 0;
        public int maxSpawns;

        [System.Serializable]
        public enum SpawnMethod
        {
            Roundrobin, Curve, Weighted, Random
        }
        public SpawnMethod prefabsMethod;
        public AnimationCurve prefabsCurve = AnimationCurve.Linear(1.0f, 1.0f, 1.0f, 1.0f);
        private GameObject currentPrefab
        {
            get
            {
                switch (prefabsMethod)
                {
                    case SpawnMethod.Roundrobin:
                        while (spawnIndex >= GameController.singleton.enemyPrefabs.Length)
                            spawnIndex -= GameController.singleton.enemyPrefabs.Length;
                        return GameController.singleton.enemyPrefabs[spawnIndex];
                    case SpawnMethod.Curve:
                        return GameController.singleton.enemyPrefabs[Mathf.FloorToInt(prefabsCurve.keys[spawnIndex].value)];
                    case SpawnMethod.Weighted:
                        List<GameObject> odds = new List<GameObject>();
                        for (int i = 0; i < prefabsCurve.keys.Length; i++)
                        {
                            for (int j = 0; j < Mathf.FloorToInt(prefabsCurve.keys[i].value); j++)
                            {
                                odds.Add(GameController.singleton.enemyPrefabs[i]);
                            }
                        }
                        return odds[Random.Range(0, odds.Count)];
                    case SpawnMethod.Random:
                        return GameController.singleton.enemyPrefabs[Random.Range(0, GameController.singleton.enemyPrefabs.Length)];
                }

                return null;
            }
        }

        public SpawnMethod pointsMethod;
        public AnimationCurve pointsCurve = AnimationCurve.Linear(1.0f, 1.0f, 1.0f, 1.0f);
        private Transform currentPoint
        {
            get
            {
                switch (pointsMethod)
                {
                    case SpawnMethod.Roundrobin:
                        while (spawnIndex >= InteriorManager.singleton.spawnPoints.Length)
                            spawnIndex -= InteriorManager.singleton.spawnPoints.Length;
                        return InteriorManager.singleton.spawnPoints[spawnIndex];
                    case SpawnMethod.Curve:
                        return InteriorManager.singleton.spawnPoints[Mathf.FloorToInt(pointsCurve.keys[spawnIndex].value)];
                    case SpawnMethod.Weighted:
                        List<Transform> odds = new List<Transform>();
                        for (int i = 0; i < pointsCurve.keys.Length; i++)
                        {
                            for (int j = 0; j < Mathf.FloorToInt(pointsCurve.keys[i].value); j++)
                            {
                                odds.Add(InteriorManager.singleton.spawnPoints[i]);
                            }
                        }
                        return odds[Random.Range(0, odds.Count)];
                    case SpawnMethod.Random:
                        return InteriorManager.singleton.spawnPoints[Random.Range(0, InteriorManager.singleton.spawnPoints.Length)];
                }

                return null;
            }
        }

        public IEnumerator UpdateWave()
        {
            int lastSpawnIndex = -1;

            while (spawnIndex < maxSpawns)
            {
                yield return null;

                _spawnRate += spawnRate * Time.deltaTime;
                spawnIndex = Mathf.FloorToInt(_spawnRate);

                if (spawnIndex != lastSpawnIndex)
                    Spawn();

                lastSpawnIndex = spawnIndex;
            }
        }
        private void Spawn()
        {
            Instantiate(currentPrefab, currentPoint.position, currentPoint.rotation);
            spawnIndex++;
        }
    }
    public Wave[] waves;

    public void StartWave()
    {
        StartCoroutine(waves[GameController.singleton.currentWaveIndex].UpdateWave());
    }
}
