using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class InputManager : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField]
    PlayerInput playerInput;
    [SerializeField]
    [Range(0, 1)]
    float gamepadSensitive;
    [SerializeField]
    float cursorSpeed = 1000f;
    [Header("Detect")]
    public Vector2 move;
    public bool front;
    public bool back;
    public bool left;
    public bool right;
    public bool isWalk;
    public bool run;
    public bool jump;
    //public string currentInput;
    public bool pick;
    public bool comfirm;
    public bool cancel;
    public Vector2 pointOnScreen;
    public Vector2 uiMove;

    [SerializeField]
    RectTransform cursorTransform;
    [SerializeField]
    RectTransform canvasRectTransform;
    [SerializeField]
    Canvas canvas;
    Mouse virtualMouse;
    Camera mainCamera;

    public string previousControlScheme = "";
    const string gamepadScheme = "Gamepad";
    const string mouseScheme = "Keyboard";

    PlayerControls controls;
    GameManager manager;

    void Start()
    {
        controls = new PlayerControls();
        controls.GamePlay.Enable();

        controls.GamePlay.Jump.started += ctx => jump = manager.mouse.mouseOn ? false : true;
        controls.GamePlay.Jump.started += ctx => manager.player.anim.Jump();
        controls.GamePlay.Jump.canceled += ctx => jump = false;
        controls.GamePlay.Jump.canceled += ctx => manager.player.anim.Jump();

        controls.GamePlay.PickUp.started += ctx => pick = true;
        controls.GamePlay.PickUp.canceled += ctx => pick = false;

        controls.GamePlay.Throw.started += ctx => comfirm = manager.mouse.mouseOn ? true : false;
        controls.GamePlay.Throw.canceled += ctx => comfirm = false;

        controls.GamePlay.ThrowCancel.started += ctx => cancel = manager.mouse.mouseOn ? true : false;
        controls.GamePlay.ThrowCancel.canceled += ctx => cancel = false;
        controls.GamePlay.ThrowCancel.started += ctx => manager.craft.Clear();

        controls.GamePlay.Craft.performed += ctx => manager.uiSetting.CraftUI();
        controls.GamePlay.Setting.performed += ctx => manager.uiSetting.SettingUI();
        controls.GamePlay.Backpack.performed += ctx => manager.uiSetting.BackpackUI();

        controls.GamePlay.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
        controls.GamePlay.Move.canceled += ctx => move = Vector2.zero;

        controls.GamePlay.Navigate.performed += ctx => uiMove = ctx.ReadValue<Vector2>();
        controls.GamePlay.Navigate.canceled += ctx => uiMove = Vector2.zero;

        playerInput.onControlsChanged += OnControlsChanged;
    }

  private void OnEnable()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        mainCamera = Camera.main;

          if (virtualMouse == null)
        {
            virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
        } else if (!virtualMouse.added)
        {
            InputSystem.AddDevice(virtualMouse);
        }
        InputUser.PerformPairingWithDevice(virtualMouse, playerInput.user);

        if (cursorTransform != null)
        {
            Vector2 position = cursorTransform.anchoredPosition;
            InputState.Change(virtualMouse.position, position);
        }

        InputSystem.onAfterUpdate += UpdateMotion;
    }

    void OnDisable()
    {
        playerInput.user.UnpairDevice(virtualMouse);
        InputSystem.RemoveDevice(virtualMouse);
        controls.GamePlay.Disable();
        playerInput.onControlsChanged -= OnControlsChanged;

        InputSystem.onAfterUpdate -= UpdateMotion;
    }

    void UpdateMotion()
    {
        if (virtualMouse == null || Gamepad.current == null)
        {
            return;
        }
        if (manager.mouse.mouseOn)
        {
            //read the value of gamepad pipe it into the virtual mouse
            Vector2 deltaValue = Gamepad.current.rightStick.ReadValue();
            deltaValue *= cursorSpeed * Time.deltaTime;

            Vector2 currentPosition = virtualMouse.position.ReadValue();
            Vector2 newPosition = currentPosition + deltaValue;

            //Clamp in screen position
            newPosition.x = Mathf.Clamp(newPosition.x, 0, Screen.width);
            newPosition.y = Mathf.Clamp(newPosition.y, 0, Screen.height);

            InputState.Change(virtualMouse.position, newPosition);
            InputState.Change(virtualMouse.delta, deltaValue);
            //change the position of the cursor image on the screen
            AnchorCursor(pointOnScreen);
        }
        else
        {
            InputState.Change(virtualMouse.position, new Vector2(Screen.width/2,Screen.height/2));
        }
    }

    void AnchorCursor(Vector2 position)
    {
        Vector2 anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, position, canvas.renderMode
            == RenderMode.ScreenSpaceOverlay ? null : mainCamera,out anchoredPosition);
        cursorTransform.anchoredPosition = anchoredPosition;
    }

    void OnControlsChanged(PlayerInput input)
    {
        if (playerInput.currentControlScheme == mouseScheme && (previousControlScheme != mouseScheme || previousControlScheme == ""))
        {//if player change control to mouse
            //change cursor image
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            previousControlScheme = mouseScheme;
        }
        
        if(playerInput.currentControlScheme == gamepadScheme && (previousControlScheme != gamepadScheme || previousControlScheme == ""))
        {//if player change control to gamepad
         //change cursor image
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            previousControlScheme = gamepadScheme;
        }
    }

    void Update()
    {
        OnControlsChanged(playerInput);
        if (playerInput.currentControlScheme == gamepadScheme)
        {
            if(manager.mouse.mouseOn)
                cursorTransform.gameObject.SetActive(true);
            else
                cursorTransform.gameObject.SetActive(false);
            pointOnScreen = virtualMouse.position.ReadValue();
        }
        else if (playerInput.currentControlScheme == mouseScheme)
        {
            pointOnScreen = Mouse.current.position.ReadValue();
            cursorTransform.gameObject.SetActive(false);
        }
        MoveAnimSetting();
    }

    void MoveAnimSetting()
    {

        if (move.y > gamepadSensitive || move.x > gamepadSensitive || move.y < -gamepadSensitive || move.x < -gamepadSensitive)
            run = true;
        else
            run = false;

        if (move.y != 0 || move.x != 0)
            isWalk = true;
        else if (move.y == 0 && move.x == 0)
            isWalk = false;

        if (move.y > 0)
        {
            front = true;
            back = false;
            //currentInput = "front";
        }
        else if (move.y < 0)
        {
            front = false;
            back = true;
            //currentInput = "back";
        }
        else if (move.y == 0)
        {
            front = false;
            back = false;
        }

        if (move.x > 0)
        {
            right = true;
            left = false;
            //currentInput = "right";
        }
        else if (move.x < 0)
        {
            right = false;
            left = true;
            //currentInput = "left";
        }
        else if (move.x == 0)
        {
            right = false;
            left = false;
        }
    }

    //OLD SETTING

    // Update is called once per frame
    /*    void Update()
        {
            if (gamepad)
                GamepadSetting();
            else
                KeyboardSetting();

        }

        void KeyboardSetting()
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                run = true;
            else
                run = false;
            pick = Input.GetKeyDown(KeyCode.E);
            pointOnScreen = Input.mousePosition;
            comfirm = Input.GetMouseButtonDown(0);
            cancel = Input.GetMouseButtonDown(1);
            jump = Input.GetKeyDown(KeyCode.Space);

            if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
                isWalk = true;
            else if (Input.GetAxisRaw("Vertical") == 0 && Input.GetAxisRaw("Horizontal") == 0)
                isWalk = false;

            if (Input.GetAxisRaw("Vertical") > 0)
            {
                front = true;
                back = false;
                //currentInput = "front";
            }
            else if (Input.GetAxisRaw("Vertical") < 0)
            {
                front = false;
                back = true;
                //currentInput = "back";
            }
            else if (Input.GetAxisRaw("Vertical") == 0)
            {
                front = false;
                back = false;
            }

            if (Input.GetAxisRaw("Horizontal") > 0)
            {
                right = true;
                left = false;
                //currentInput = "right";
            }
            else if (Input.GetAxisRaw("Horizontal") < 0)
            {
                right = false;
                left = true;
                //currentInput = "left";
            }
            else if (Input.GetAxisRaw("Horizontal") == 0)
            {
                right = false;
                left = false;
            }
        }

        void GamepadSetting()
        {
            pick = Input.GetKeyDown(KeyCode.E);
            pointOnScreen = Input.mousePosition;
            comfirm = Input.GetMouseButtonDown(0);
            cancel = Input.GetMouseButtonDown(1);
            jump = Input.GetKeyDown(KeyCode.Space);

            if (Input.GetAxis("Vertical") > gamepadSensitive || Input.GetAxisRaw("Horizontal") > gamepadSensitive || Input.GetAxis("Vertical") < -gamepadSensitive || Input.GetAxisRaw("Horizontal") < -gamepadSensitive)
                run = true;
            else
                run = false;

            if (Input.GetAxis("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
                isWalk = true;
            else if (Input.GetAxis("Vertical") == 0 && Input.GetAxisRaw("Horizontal") == 0)
                isWalk = false;

            if (Input.GetAxis("Vertical") > 0)
            {
                front = true;
                back = false;
                //currentInput = "front";
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                front = false;
                back = true;
                //currentInput = "back";
            }
            else if (Input.GetAxis("Vertical") == 0)
            {
                front = false;
                back = false;
            }

            if (Input.GetAxis("Horizontal") > 0)
            {
                right = true;
                left = false;
                //currentInput = "right";
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                right = false;
                left = true;
                //currentInput = "left";
            }
            else if (Input.GetAxis("Horizontal") == 0)
            {
                right = false;
                left = false;
            }
        }*/
}
