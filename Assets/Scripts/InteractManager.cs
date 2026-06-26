using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.InputSystem; 

public class InteractManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction interactAction;

    InteractParent foundInteractable;

    PlayerUI playerUI;

    void Start()
    {
        playerUI = GetComponent<PlayerUI>();
    }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            interactAction = playerInput.actions["Interact"];
        }
    }

    private void Update()
    {
        ManageInteractPanel();

        if (interactAction.WasPerformedThisFrame())
        {
            if (foundInteractable != null)
            {
                foundInteractable.StartInteractTimer();
            }
        }
    }

    void ManageInteractPanel()
    {
        if(playerUI == null)
            return;
        
        if(foundInteractable != null) {
            playerUI.UpdateInteractPanel(foundInteractable.transform, 
            foundInteractable.interactPrompt, 
            interactAction.bindings[0].effectivePath.Replace("<Keyboard>/","").ToUpper());
        }
        else {
            playerUI.UpdateInteractPanel(null, "", "");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Trigger entered with " + other.name);
        if(other.GetComponent<InteractParent>())
        {
            foundInteractable = other.GetComponent<InteractParent>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Debug.Log("Trigger exited with " + other.name);
        if (other.GetComponent<InteractParent>())
        {
            ClearFloundInteractable();
        }
    }

    public void ClearFloundInteractable()
    {
        foundInteractable = null;
    }
}
