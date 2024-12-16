using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TopDownCamera : MonoBehaviour
{
    [SerializeField] private LayerMask groundMask;

    private Camera mainCamera;
    private void Start()
    {

        mainCamera = Camera.main;
    }

    private void Update()
    {
        Aim();
    }

    private (bool succes, Vector3 position) GetMousePosition()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
        {
            // The Raycast hit something, return width the position.
            return (success: true, position: hitInfo.point);
        }
        else
        {
            // The Raycast hit something, return width the position.
            return (success: false, position: Vector3.zero);
        }
    }

    private void Aim()
    {
        var (success, position) = GetMousePosition();
        if (success)
        {
            // Calculate the direction
            var direction = position - transform.position;

            // Make the transform look in the direction.
            transform.forward = direction;
        }
    }

}
