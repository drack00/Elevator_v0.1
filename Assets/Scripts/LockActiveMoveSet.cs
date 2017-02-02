using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockActiveMoveSet : MonoBehaviour
{
    public Player player;
	void OnEnable ()
    {
        player.lockActiveMoveSet = true;
	}
    void OnDisable()
    {
        player.lockActiveMoveSet = false;
    }
}