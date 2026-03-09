using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections.Generic;

public class SplitScreenManager : MonoBehaviour
{
    public CinemachineCamera cam;
    private int index;
    
   
    [SerializeField] List<PlayerInput> players = new List<PlayerInput>();
    [SerializeField] PlayerHUD playerHudPrefab;

    private void Awake()
    {
        var pim = FindFirstObjectByType<PlayerInputManager>();
        if (pim != null)
            pim.onPlayerJoined += HandlePlayerJoined;
        else
            Debug.LogError("No PlayerInputManager found in the scene!");
    }
    private void HandlePlayerJoined(PlayerInput input)
    {
        players.Add(input);
        PlayerController player = input.gameObject.GetComponent<PlayerController>();

        input.SwitchCurrentControlScheme(input.devices[0]);

        Camera playerCam = input.GetComponentInChildren<Camera>();
        if (!playerCam)
        {
            Debug.LogError("Cannot access Camera");
            return;
        }
        input.camera = playerCam;
        index = input.playerIndex;

        // Setup Cinemachine if used
        CinemachineCamera cmCam = input.GetComponentInChildren<CinemachineCamera>();
        if (cmCam != null)
        {
            var inputController = cmCam.GetComponent<CinemachineInputAxisController>();
            if (inputController != null)
            {
                var lookAction = input.actions["Look"];
                var zoomAction = input.actions["Zoom"];
                inputController.PlayerIndex = index;
                inputController.AutoEnableInputs = true;
                cmCam.OutputChannel = (OutputChannels)(1 << index);
                CinemachineBrain brain = playerCam.GetComponent<CinemachineBrain>();
                brain.ChannelMask = (OutputChannels)(1 << index);

                for (int i = 0; i < inputController.Controllers.Count; i++)
                {
                    var c = inputController.Controllers[i];
                    if (i == 0 || i == 1)
                        c.Input.InputAction = InputActionReference.Create(lookAction);
                    else if (i == 2)
                        c.Input.InputAction = InputActionReference.Create(zoomAction);

                    c.Input.Gain = 15;
                }
            }
        }

        // Spawn the HUD prefab (which has Canvas & EventSystem)
        PlayerHUD hud = Instantiate(playerHudPrefab);
       

        // Assign the player camera to the Canvas
        Canvas canvas = hud.GetComponentInChildren<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = playerCam;
        canvas.planeDistance = 1f;

        // Find the actual UI panel inside the Canvas
        RectTransform hudPanel = hud.transform.GetComponentInChildren<RectTransform>();
        hudPanel.localPosition = new Vector3(hudPanel.localPosition.x, hudPanel.localPosition.y, 0f);
        if (hudPanel == null)
        {
            Debug.LogError("HUD prefab is missing a child named 'UI'");
            return;
        }

        // Set player type and anchor the HUD panel
        if (input.playerIndex == 0)
        {
            Debug.Log("Setting player type to Fungi");
            player.InitializePlayerType(PlayerType.Fungi);
            hud.Initialize(PlayerType.Fungi);
            hudPanel.anchorMin = new Vector2(0, 1); // top-left
            hudPanel.anchorMax = new Vector2(0, 1);
            hudPanel.pivot = new Vector2(0, 1);
            hudPanel.anchoredPosition = new Vector2(10, -10);
        }
        else
        {
            Debug.Log("Setting player type to Prickly");
            player.InitializePlayerType(PlayerType.Prickly);
            hud.Initialize(PlayerType.Prickly);
            hudPanel.anchorMin = new Vector2(1, 1); // top-right
            hudPanel.anchorMax = new Vector2(1, 1);
            hudPanel.pivot = new Vector2(1, 1);
            hudPanel.anchoredPosition = new Vector2(-10, -10);
        }

        hudPanel.localScale = Vector3.one;

        // Assign the player's actions to the HUD EventSystem
        var uiModule = hud.GetComponentInChildren<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (uiModule != null)
        {
            uiModule.actionsAsset = input.actions; // ensures each player controls only their own HUD
            input.uiInputModule = uiModule;
        }


    }


}
