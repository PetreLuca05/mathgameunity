using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.InputSystem; 
public class InventoryManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction inventoryAction;
    public bool canAccessInventory = true;
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
            inventoryAction = playerInput.actions["Inventory"];
        }
    }

    private void Update()
    {
        if(!canAccessInventory) return;

        if(inventoryAction.ReadValue<float>() > 0)
        {
            playerUI.UpdateInventoryPanelLocation(this.gameObject.transform);
            GetComponent<PlayerManager>().SetBehaviourState(BehaviourState.inInventory);
        } else {
            playerUI.UpdateInventoryPanelLocation(null);

            if(GetComponent<PlayerManager>().GetBehaviourState() == BehaviourState.inInventory) 
                GetComponent<PlayerManager>().SetBehaviourState(BehaviourState.Default);
        }
    }
}
