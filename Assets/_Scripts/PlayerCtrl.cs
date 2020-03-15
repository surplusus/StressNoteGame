using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerCtrl : MonoBehaviour
{
    NavMeshAgent _agent;
    Camera _camera;
    Vector3 _dest;
    [SerializeField] Animator _anim;

    void Awake()
    {
        _agent = gameObject.GetComponent<NavMeshAgent>();
        _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var screenPoint = Input.mousePosition;
            Ray ray = _camera.ScreenPointToRay(screenPoint);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                print(_dest= hit.point);
                _agent.SetDestination(_dest);
            }
        }
        _anim.SetFloat("Speed", _agent.velocity.magnitude);
    }
}
