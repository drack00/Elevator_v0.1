using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        [System.Serializable]
        public enum TimeLimit
        {
            None, Fail, Succeed
        }
        public TimeLimit useTimeLimit;
        public float timeLimit;
        private float _timeLimit;

        public int minTargets;
        public GameObject[] targetPrefabs;
        private List<GameObject> targets;
        public GameObject[] otherPrefabs;
        private List<GameObject> others;

        public AnimationCurve staggerSpawns;
        public float spawnCoefficent;

        public bool despawnOnEnd;

        public IEnumerator Update ()
        {
            yield return null;
        }
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
        private set
        {
            if (_currentWave != null)
                StopCoroutine(_currentWave.Update());

            _currentWave = value;

            if (_currentWave != null)
                StartCoroutine(_currentWave.Update());
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