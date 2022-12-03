using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [ReadOnly] public Rigidbody rigidBody; //Rigidbody игрока
    [SerializeField] private float jumpMultiplyer; //Множитель высоты прыжков
    private static Player instance;
    public float speed; //Скорость передвижения игрока

    public bool blockJump; //Флаг для блокировки прыжков
    internal short JumpCount; //Количество прыжков
    internal short FramesDelay; //Задержка в кадрах между проверкой на касание земли игроком

    public SkinnedMeshRenderer playerMesh; //Mesh модели игрока
    public Transform reflection; //Transform отражения игрока в зеркале
    public Animator anim;

    public delegate void OnSpace();
    public OnSpace OnSpaceEvent { get; set; }
    
    void Awake()
    {
        instance = this;
        anim = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
        reflection = transform.Find("PlayerReflection");
        blockJump = false;
        JumpCount = 0;
        OnSpaceEvent += Jump;
        OnSpaceEvent += () => DialogueManager.GetInstance().ContinueDialogue();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && JumpCount < 2 && !blockJump && !DialogueManager.GetInstance().DialoguePlaying) Jump();
        if (FramesDelay == 0 && Utilities.IsGrounded(transform)) {
            JumpCount = 0;
            anim.SetBool("MidAir", false);
        }
        if (FramesDelay > 0) FramesDelay--;
    }

    private void FixedUpdate()
    {
        var manager = DialogueManager.GetInstance();
        if (manager && manager.DialoguePlaying) return;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movementVector = new Vector3(horizontal, 0, vertical);
        if (GameManager.gameMode)
        {
            Movement2DManager.Movement2D(movementVector);
        }
        else
        {
            Movement3DManager.Movement3D(movementVector);
        }
    }

    private void Jump()
    {
        if(JumpCount==0)anim.SetTrigger("Jump");
        anim.SetBool("MidAir", true);
        rigidBody.AddForce(Vector3.up * jumpMultiplyer, ForceMode.VelocityChange);
        GameManager.reflectionTeleport = false;
        FramesDelay = 5;
        JumpCount++;
    }
    
    public static Player GetPlayer()
    {
        return instance;
    }

}
