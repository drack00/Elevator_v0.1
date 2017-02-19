using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExteriorManager : MonoBehaviour
{
    public static ExteriorManager singleton
    {
        get
        {
            return FindObjectOfType<ExteriorManager>();
        }
    }

    public float roomRadius;
    public float spawnHeight;
    public float despawnHeight;

    public float speed;

    public float unloadHeight;
    private bool unloading = false;

    private IEnumerator Unloading (Level level)
    {
        unloading = true;

        level.unload = true;
        yield return new WaitWhile(delegate { return level.unloading; });

        unloading = false;
    }

    public Transform levels;

    public float levelSpacing;

    public GameObject[] fillerPiecePrefabs;
    public GameObject[] currentFillerPrefabs
    {
        get
        {
            return new GameObject[4]
            {
                fillerPiecePrefabs[Random.Range(0, fillerPiecePrefabs.Length)],
                fillerPiecePrefabs[Random.Range(0, fillerPiecePrefabs.Length)],
                fillerPiecePrefabs[Random.Range(0, fillerPiecePrefabs.Length)],
                fillerPiecePrefabs[Random.Range(0, fillerPiecePrefabs.Length)]
            };
        }
    }

    [System.Serializable]
    public enum SpawnMethod
    {
        Roundrobin, Curve, Weighted, Random
    }
    public SpawnMethod spawnMethod;
    public GameObject[] levelPiecePrefabs;
    public AnimationCurve[] spawnCurves = new AnimationCurve[4] {
        AnimationCurve.Linear(1.0f, 1.0f, 1.0f, 1.0f),
        AnimationCurve.Linear(1.0f, 1.0f, 1.0f, 1.0f) ,
        AnimationCurve.Linear(1.0f, 1.0f, 1.0f, 1.0f),
        AnimationCurve.Linear(1.0f, 1.0f, 1.0f, 1.0f) };
    private int spawnIndex = 0;

    private GameObject[] currentLevelPrefabs
    {
        get
        {
            GameObject[] _currentLevelPrefabs = new GameObject[4] { null, null, null, null };

            for (int i = 0; i < _currentLevelPrefabs.Length; i++)
            {
                AnimationCurve spawnCurve = spawnCurves[i];

                switch (spawnMethod)
                {
                    case SpawnMethod.Roundrobin:
                        while (spawnIndex >= levelPiecePrefabs.Length)
                            spawnIndex -= levelPiecePrefabs.Length;
                        _currentLevelPrefabs[i] = levelPiecePrefabs[spawnIndex];
                        break;
                    case SpawnMethod.Curve:
                        _currentLevelPrefabs[i] = levelPiecePrefabs[Mathf.FloorToInt(spawnCurve.keys[spawnIndex].value)];
                        break;
                    case SpawnMethod.Weighted:
                        List<GameObject> odds = new List<GameObject>();
                        for (int j = 0; j < spawnCurve.keys.Length; j++)
                        {
                            for (int k = 0; k < Mathf.FloorToInt(spawnCurve.keys[j].value); k++)
                            {
                                odds.Add(levelPiecePrefabs[j]);
                            }
                        }
                        _currentLevelPrefabs[i] = odds[Random.Range(0, odds.Count)];
                        break;
                    case SpawnMethod.Random:
                        _currentLevelPrefabs[i] = levelPiecePrefabs[Random.Range(0, levelPiecePrefabs.Length)];
                        break;
                }
            }

            return _currentLevelPrefabs;
        }
    }

    [HideInInspector]
    public List<GameObject[]> queuedSpawns = new List<GameObject[]>();
    private void Spawn()
    {
        GameObject level = new GameObject("Level", typeof(Level));
        level.transform.SetParent(levels);
        level.transform.localPosition = new Vector3(0, spawnHeight, 0);
        level.transform.rotation = Quaternion.identity;

        if (queuedSpawns.Count < 1)
        {
            level.GetComponent<Level>().stopToUnload = false;
            queuedSpawns.Add(currentFillerPrefabs);
        }

        for (int i = 0; i < queuedSpawns[0].Length; i++)
        {
            if (queuedSpawns[0][i] != null)
            {
                GameObject levelPiece = Instantiate(queuedSpawns[0][i]);
                levelPiece.transform.SetParent(level.transform);
                Vector3 piecePosition = new Vector3(roomRadius, 0, 0);
                Quaternion pieceRotation = Quaternion.Euler(0, 90 * i, 0);
                piecePosition = pieceRotation * piecePosition;
                levelPiece.transform.localPosition = piecePosition;
                levelPiece.transform.rotation = pieceRotation;
            }
        }

        spawnIndex++;

        queuedSpawns.RemoveAt(0);
    }
    public void ScheduleSpawn()
    {
        queuedSpawns.Add(currentLevelPrefabs);
    }

    void Update()
    {
        if (!unloading)
            foreach (Transform level in levels)
            {
                if (level.localPosition.y <= unloadHeight && !level.GetComponent<Level>().unload && level.GetComponent<Level>().stopToUnload)
                    StartCoroutine(Unloading(level.GetComponent<Level>()));
            }

        if (unloading)
            return;

        foreach(Transform level in levels)
        {
            if (level.localPosition.y < despawnHeight)
                Destroy(level.gameObject);
            else
                level.localPosition = new Vector3(0, level.localPosition.y + speed, 0);
        }

        foreach (Transform level in levels)
        {
            if (level.position.y >= spawnHeight - levelSpacing)
                return;
        }
        Spawn();
    }
}
