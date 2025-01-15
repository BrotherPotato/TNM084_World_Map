using UnityEngine;
using UnityEngine.InputSystem;
//https://www.youtube.com/watch?v=jxecc2IGlWA
//https://www.youtube.com/watch?v=HHzQMYxtmU4
public class CameraMovement : MonoBehaviour
{
    [SerializeField] float mouseSens = 3f;
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float acceleration = 20f;
    [SerializeField] Transform cameraTransform;
    Vector3 velocity;

    
    Vector2 view;
    CharacterController controller;




    PlayerInput playerInput;
    InputAction moveAction;
    InputAction sprintAction;
    InputAction viewAction;
    InputAction flyUpDownAction;
    

    void Awake(){
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["move"];
        viewAction = playerInput.actions["look"];
        flyUpDownAction = playerInput.actions["flyUpDown"];
        sprintAction = playerInput.actions["sprint"];
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovement();
        UpdateView();
    }

    Vector3 GetMovementInput(){
        var moveInput = moveAction.ReadValue<Vector2>();
        var flyUpDownInput = flyUpDownAction.ReadValue<float>();
        var input = new Vector3();
        var referenceTransform = cameraTransform;
        input += referenceTransform.forward * moveInput.y;
        input += referenceTransform.right * moveInput.x;
        input += transform.up * flyUpDownInput;
        input = Vector3.ClampMagnitude(input, 1f);

        float sprint = sprintAction.ReadValue<float>();
        if(sprint > 0){
            input *= moveSpeed * 3f;
        } else{
            input *= moveSpeed;
        }
        return input;
    }

    void UpdateMovement(){
        var movementInput = GetMovementInput();

        var delta = acceleration * Time.deltaTime;
        velocity = Vector3.Lerp(velocity, movementInput, delta);
        controller.Move(velocity * Time.deltaTime);


        //transform.Translate(input * 2f * Time.deltaTime, Space.World);
    }



    void UpdateView(){
        var lookInput = viewAction.ReadValue<Vector2>();
        view.x += lookInput.x * mouseSens;
        view.y += lookInput.y * mouseSens;
        

        view.y = Mathf.Clamp(view.y, -89f, 89f);

        cameraTransform.localRotation = Quaternion.Euler(-view.y,0,0);
        transform.localRotation = Quaternion.Euler(0,view.x,0);


    }
}
