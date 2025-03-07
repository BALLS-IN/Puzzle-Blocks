using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;

public class Cube : MonoBehaviour
{
    public delegate void OnCubeTouchedEventHandler(Cube cube, Vector3 hit);
    public event OnCubeTouchedEventHandler OnCubeTouched;

    public delegate void OnCubePlacedEventHandler(Cube cube);
    public OnCubePlacedEventHandler OnCubePlaced;

    public bool isMoving = false;
    private Vector3 direction;
    private float speed = 12f;


    private InputController inputControls;

    private InputAction touchAction;
    private bool listenInput;

    private void Awake()
    {

    }

    private void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.Lerp(transform.position, direction, speed * Time.deltaTime);

            // V?rifie si la pi?ce est proche de la position cible
            if (Vector3.Distance(transform.position, direction) < 0.01f)
            {
                // Position finale d?finie pour eviter les imprecisions
                transform.position = direction;
                isMoving = false; // Arrete le deplacement
                if (OnCubePlaced != null)
                {
                    OnCubePlaced(this);
                }
            }
        }
    }

    // met à jour la direction de la piece grâce au board
    public void Move(int x, int y)
    {
        isMoving = true;
        direction = new Vector3(x, y);
    }



    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Mouse.current != null && Mouse.current.position.ReadValue() != null)
            {
                DetectTouchOrClick(Mouse.current.position.ReadValue()); // Pour PC
            }
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.position.ReadValue() != null)
            {
                DetectTouchOrClick(Touchscreen.current.primaryTouch.position.ReadValue()); // Pour Mobile
            }
        }
    }



    private void DetectTouchOrClick(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                Debug.Log("Cube touché : " + hit.collider.gameObject);
                OnCubeTouched?.Invoke(this, hit.point);
            }
        }
    }
}
