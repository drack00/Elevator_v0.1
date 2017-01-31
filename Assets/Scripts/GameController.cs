using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public static GameController singleton
    {
        get
        {
            return FindObjectOfType<GameController>();
        }
    }

    [System.Serializable]
    public class Wave
    {
        public float startWaveDelay;

        public AnimationCurve spawnCurve = AnimationCurve.Linear(1.0f, 1.0f, 1.0f, 1.0f);
        public float spawnRate = 1.0f;
        private float spawnIndex;

        [System.Serializable]
        public enum SpawnMethod
        {
            Roundrobin, Curve, Weighted, Random
        }

        public GameObject[] prefabs;
        public SpawnMethod prefabsMethod;
        public AnimationCurve prefabsCurve = AnimationCurve.Linear(1.0f, 1.0f, 1.0f, 1.0f);
        private int prefabsIndex;
        private GameObject currentPrefab
        {
            get
            {
                switch(prefabsMethod)
                {
                    case SpawnMethod.Roundrobin:
                        while (prefabsIndex >= prefabs.Length)
                            prefabsIndex -= prefabs.Length;
                        return prefabs[prefabsIndex];
                    case SpawnMethod.Curve:
                        return prefabs[Mathf.FloorToInt(prefabsCurve.keys[prefabsIndex].value)];
                    case SpawnMethod.Weighted:
                        List<GameObject> odds = new List<GameObject>();
                        for(int i = 0; i < prefabsCurve.keys.Length; i++)
                        {
                            for(int j = 0; j < Mathf.FloorToInt(prefabsCurve.keys[i].value); j++)
                            {
                                odds.Add(prefabs[i]);
                            }
                        }
                        return odds[Random.Range(0, odds.Count)];
                    case SpawnMethod.Random:
                        return prefabs[Random.Range(0, prefabs.Length)];
                }

                return null;
            }
        }

        public Transform[] points;
        public SpawnMethod pointsMethod;
        public AnimationCurve pointsCurve = AnimationCurve.Linear(1.0f, 1.0f, 1.0f, 1.0f);
        private int pointsIndex;
        private Transform currentPoint
        {
            get
            {
                switch (pointsMethod)
                {
                    case SpawnMethod.Roundrobin:
                        while (pointsIndex >= points.Length)
                            pointsIndex -= points.Length;
                        return points[pointsIndex];
                    case SpawnMethod.Curve:
                        return points[Mathf.FloorToInt(pointsCurve.keys[pointsIndex].value)];
                    case SpawnMethod.Weighted:
                        List<Transform> odds = new List<Transform>();
                        for (int i = 0; i < pointsCurve.keys.Length; i++)
                        {
                            for (int j = 0; j < Mathf.FloorToInt(pointsCurve.keys[i].value); j++)
                            {
                                odds.Add(points[i]);
                            }
                        }
                        return odds[Random.Range(0, odds.Count)];
                    case SpawnMethod.Random:
                        return points[Random.Range(0, points.Length)];
                }

                return null;
            }
        }

        public GameObject Spawn ()
        {
            GameObject go = (GameObject)Instantiate(currentPrefab, currentPoint.position, currentPoint.rotation);

            prefabsIndex++;
            pointsIndex++;

            return go;
        }

        [HideInInspector]
        public List<GameObject> gos;
        private bool areAllSpawned
        {
            get
            {
                int totalSpawns = 0;
                foreach (Keyframe key in spawnCurve.keys)
                {
                    totalSpawns += Mathf.RoundToInt(key.value);
                }
                return gos.Count >= totalSpawns;
            }
        }
        private bool isWaveComplete
        {
            get
            {
                bool _isWaveComplete = true;
                foreach (GameObject go in gos)
                {
                    if(go.activeSelf)
                    {
                        _isWaveComplete = false;
                        break;
                    }
                }
                return _isWaveComplete && areAllSpawned;
            }
        }

        public bool despawnOnEnd;

        public float endWaveDelay;

        public IEnumerator Update (List<GameObject> _gos = null)
        {
            gos = new List<GameObject>();
            if (_gos != null)
                foreach (GameObject go in _gos)
                {
                    gos.Add(go);
                }

            pointsIndex = 0;
            prefabsIndex = 0;
            spawnIndex = 0.0f;
            int lastSpawnIndex = 0;

            float _startWaveDelay = 0.0f;
            while(_startWaveDelay < startWaveDelay)
            {
                yield return null;

                _startWaveDelay += Time.deltaTime;
            }

            interrupt = false;
            while(!interrupt && !isWaveComplete)
            {
                int currentSpawnIndex = Mathf.FloorToInt(spawnIndex);
                if (spawnIndex == 0.0f || currentSpawnIndex != lastSpawnIndex)
                {
                    int currentSpawn = Mathf.FloorToInt(spawnCurve.keys[currentSpawnIndex].value);

                    for (int i = 0; i < currentSpawn; i++)
                    {
                        gos.Add(Spawn());
                    }

                    lastSpawnIndex = currentSpawnIndex;
                }

                yield return null;

                spawnIndex = spawnRate * Time.deltaTime;
            }

            if(despawnOnEnd)
                foreach (GameObject go in gos)
                {
                    go.SetActive(false);
                }

            foreach (GameObject go in gos)
            {
                if (!go.activeSelf)
                    Destroy(go);
            }

            float _endWaveDelay = 0.0f;
            while (_endWaveDelay < endWaveDelay)
            {
                yield return null;

                _endWaveDelay += Time.deltaTime;
            }

            GameController.singleton.currentWave = GameController.singleton.nextWave;
        }
        [HideInInspector]public bool interrupt;
    }
    public Wave[] waves;
    private int currentWaveIndex
    {
        get
        {
            for(int i = 0; i < waves.Length; i++)
            {
                if (waves[i] == currentWave)
                    return i;
            }

            return 0;
        }
    }
    private Wave _currentWave;
    public Wave currentWave
    {
        get
        {
            return _currentWave;
        }
        set
        {
            if (_currentWave != null)
                StopCoroutine(_currentWave.Update());

            if (value != null)
            {
                if (_currentWave != null)
                    StartCoroutine(value.Update(_currentWave.gos));
                else
                    StartCoroutine(value.Update());
            }
            else
                EndGame();

            _currentWave = value;
        }
    }
    private Wave nextWave
    {
        get
        {
            if (waves.Length < currentWaveIndex + 1)
                return waves[currentWaveIndex + 1];
            return null;
        }
    }

    public void EndGame()
    {

    }

	void Awake ()
    {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
    void Start ()
    {
        currentWave = waves[0];
    }
}