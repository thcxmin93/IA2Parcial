using System.Collections.Generic;
using UnityEngine;

public class EnemyFollowPath : MonoBehaviour
{
    private List<Transform> _waypoints;
    private int _i;
    public float speed = 3f;
    public float arriveThreshold = 0.05f;
    
    public void Init(List<Transform> waypoints, float speed)
    {
        _waypoints = waypoints;
        this.speed = speed;
        _i = 0;
    }

    void Update()
    {
        if (_waypoints == null || _waypoints.Count == 0 || _i >= _waypoints.Count) return;

        Vector3 target = _waypoints[_i].position;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        
        Vector3 dir = (target - transform.position); dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        
        if ((target - transform.position).sqrMagnitude <= arriveThreshold * arriveThreshold)
        {
            _i++;
            if (_i >= _waypoints.Count)
                Destroy(gameObject); 
        }
    }
}