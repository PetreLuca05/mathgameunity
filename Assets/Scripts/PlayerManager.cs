using Unity.Cinemachine;
using UnityEngine;

public enum BehaviourState
{
    Default,
    inCatapult,
    inLaunch,
    inLandAfterLaunch,
    inInventory,
    inDialog
}


public class PlayerManager : MonoBehaviour
{
    public BehaviourState behaviourState = BehaviourState.Default;

    public GameObject DefaultCamera;

    [Header("UI")]
    public GameObject UIHolder;

    [Header("Dialog Settings")]
    public string nameOfPlayer = "Player";
    public Color playerDialogNameColor = Color.cyan;

    [Header("Mission Settings")]
    public PlayerLogic_Quest_CoinCollect coinCollectMission;

    PlayerMovement playerMovement;
    PlayerModel playerModel;
    Collider playerCollider;
    GameObject cameraToDisable;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerModel = GetComponentInChildren<PlayerModel>();
        playerCollider = GetComponent<Collider>();

        SetBehaviourState(BehaviourState.Default);
    }

    void Update()
    {
        if(behaviourState == BehaviourState.inLandAfterLaunch)
         if(playerMovement.moveAction.ReadValue<Vector2>().magnitude > 0.1f)
            SetBehaviourState(BehaviourState.Default);

    }

    void ResetToDefault()
    {
        playerModel.SetPhysicsState(PlayerModel.PhysicsState.Kinematic);

        DefaultCamera.SetActive(true);
        UIHolder.SetActive(true);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetAllMonoBehavioursEnabled(true);
    }

    void SetAllMonoBehavioursEnabled(bool enabled)
    {
        var components = GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            if (comp != this)
                comp.enabled = enabled;
        }
    }

    public BehaviourState GetBehaviourState()
    {
        return behaviourState;
    }

    public void SetBehaviourState(BehaviourState newState, GameObject inLandAfterLaunchCamera = null)
    {
        behaviourState = newState;
        ResetToDefault();

        if (newState == BehaviourState.Default)
        {
            DefaultCamera.SetActive(true);
            if(cameraToDisable != null)
                cameraToDisable.SetActive(false);

            playerCollider.enabled = true;
            playerMovement.enabled = true;
        }
        else if (newState == BehaviourState.inCatapult)
        {
            DefaultCamera.SetActive(false);

            playerCollider.enabled = false;
            playerMovement.enabled = false;

            playerModel.transform.localRotation = Quaternion.identity;
        } 
        else if (newState == BehaviourState.inLaunch)
        {
            DefaultCamera.SetActive(false);

            playerMovement.enabled = false;

            playerModel.SetPhysicsState(PlayerModel.PhysicsState.Ragdoll);
        }
        else if (newState == BehaviourState.inLandAfterLaunch)
        {
            playerModel.SetPhysicsState(PlayerModel.PhysicsState.Kinematic);

            DefaultCamera.SetActive(false);

            playerCollider.enabled = true;
            playerMovement.enabled = true;

            cameraToDisable = inLandAfterLaunchCamera;
        }
        else if (newState == BehaviourState.inInventory)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } 
        else if (newState == BehaviourState.inDialog)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            DefaultCamera.SetActive(false);

            playerModel.transform.localRotation = Quaternion.identity;

            UIHolder.SetActive(false);
            SetAllMonoBehavioursEnabled(false);
        } 
    }
}
