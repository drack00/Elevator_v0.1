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

    public GameObject[] enemyPrefabs;

    [System.Serializable]
    public struct Wave
    {
        public bool exterior;
        public float startWaveDelay;
        public bool interior;
        public float endWaveDelay;
    }
    public Wave[] waves;
    [HideInInspector]
    public int currentWaveIndex = 0;
    public Wave currentWave
    {
        get
        {
            return waves[currentWaveIndex];
        }
    }

    private bool upcomingLevel
    {
        get
        {
            if (ExteriorManager.singleton.queuedSpawns.Count > 0)
                return true;

            foreach (Level level in FindObjectsOfType<Level>())
            {
                if (level.stopToUnload && !level.unload)
                    return true;
            }

            return false;
        }
    }

    IEnumerator UpdateWaves()
    {
        float _introDelay = 0.0f;
        while (_introDelay < introDelay)
        {
            yield return null;

            _introDelay += Time.deltaTime;
        }

        while (currentWaveIndex < waves.Length)
        {
            if (currentWave.exterior)
            {
                ExteriorManager.singleton.ScheduleSpawn();

                while (upcomingLevel)
                {
                    yield return null;
                }

                float _startWaveDelay = 0.0f;
                while (_startWaveDelay < currentWave.startWaveDelay)
                {
                    yield return null;

                    _startWaveDelay += Time.deltaTime;
                }
            }

            if (currentWave.interior)
            {
                InteriorManager.singleton.StartWave();

                while (FindObjectsOfType<Enemy>().Length > 0)
                {
                    yield return null;
                }
            }

            float _endWaveDelay = 0.0f;
            while (_endWaveDelay < currentWave.endWaveDelay)
            {
                yield return null;

                _endWaveDelay += Time.deltaTime;
            }

            currentWaveIndex++;
        }

        float _outroDelay = 0.0f;
        while (_outroDelay < outroDelay)
        {
            yield return null;

            _outroDelay += Time.deltaTime;
        }
    }

    public float introDelay;
    public float outroDelay;

    void Awake ()
    {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
    void Start()
    {
        StartCoroutine(UpdateWaves());
    }
}