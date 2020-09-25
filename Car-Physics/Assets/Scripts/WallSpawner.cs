using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallSpawner : MonoBehaviour
{
    public GameObject wallObj;  // Wall to be spawned around tracks
    public Vector3 offset;      // Offset from center. X- = away from track.
    public LayerMask wallAvoid; // Layers which the wall CAN'T clip
    Vector3 wall;
    Vector3 track;
    Vector3 start, inc;     // Spawn start coord & incrementation

    // Start is called before the first frame update
    void Start()
    {
        // Get collider size. Bounds will give real size, instead of local.
        wall = wallObj.transform.localScale;
        track = transform.localScale;

        // Spawn walls in all four sides of the track
        // Rotate so that the walls are facing inside (x touches track)
        // Top = Front = Z+ track

        // Up-down
        inc = transform.forward * wall.x;
        // Top-left => Bottom-left
        start = transform.position - (track.x + wall.x) * 0.5f * transform.right - track.z * 0.5f * transform.forward;
        SpawnWalls(track.z, wall.z, start, inc, 0);
        // Top-right => Bottom-right
        start = transform.position + (track.x + wall.x) * 0.5f * transform.right - track.z * 0.5f * transform.forward;
        SpawnWalls(track.z, wall.z, start, inc, 180);

        // Left-right
        inc = transform.right * wall.x;
        // Top-left => Top-right
        start = transform.position - track.x * 0.5f * transform.right + (track.z + wall.z) * 0.5f * transform.forward;
        SpawnWalls(track.x, wall.z, start, inc, 90);
        // Bottom-left => Bottom-right
        start = transform.position - track.x * 0.5f * transform.right - (track.z + wall.z) * 0.5f * transform.forward;
        SpawnWalls(track.x, wall.z, start, inc, 270);

    }

    void SpawnWalls(float lt, float lw, Vector3 start, Vector3 inc, float rot) {
        int N = (int) (lt / lw);
        Quaternion angle = transform.rotation * Quaternion.AngleAxis(rot, transform.up);
        Vector3 relOffset = angle * offset;     // Relative offset

        // Spawn along the sideline
        for (int i=0; i<N+1; i++) {
            Vector3 loc = start + inc * i + relOffset;
            if (!Physics.CheckBox(loc, wall/2, angle, wallAvoid.value))
                Instantiate(wallObj, loc, angle);
        }
    }
}
