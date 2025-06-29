using System.Collections.Generic;
using UnityEngine;

public class Location : MonoBehaviour
{
    public Vector2Int min;
    public Vector2Int max;

    public List<GameObject> blockers;

    public Transform spawnArea;
    public Transform nextStagingArea;

    public Transform currentStagingArea;

    public Transform exitFromStaging;

    public Transform exit;

    public SoundManager.Sound music;

    public int foodGoal;

}
