using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaypointGroup : MonoBehaviour {

    public List<Transform> Waypoints = new List<Transform>();

    void Update () {
        if (transform.childCount < Waypoints.Count)
        {
            Waypoints.Clear();
        }

        if (transform.childCount > Waypoints.Count)
        {
            foreach(Transform t in transform)
            {
                Waypoints.Add(t);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (Waypoints.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform t in Waypoints)
            {
                Gizmos.DrawCube(t.position, new Vector3(0.5f, 0.5f, 0.5f));
            }
        }
    }
}
