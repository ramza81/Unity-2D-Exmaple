using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{  
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;

    public float maxSpeed;
    public float jumpPower;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator animator;
    CapsuleCollider2D capsuleCollider2D;
    AudioSource audioSource;

    private void Awake() {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }   

    private void Update() {
        // Jump
        if (Input.GetButtonDown("Jump")&& !animator.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            animator.SetBool("isJumping", true);

            // Sound
            PlaySound("JUMP");
        }

        // Stop Speed
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }

        // Direction Sprite
        if (Input.GetButton("Horizontal"))
        {
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        }

        // Animation
        if (Mathf.Abs(rigid.velocity.x) < 0.3)
        {
            animator.SetBool("isWalking", false);
        }
        else
        {
            animator.SetBool("isWalking", true);
        }

    }
    
    private void FixedUpdate() {
        // Move Speed
        float h  = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        // Right Max Speed
        if (rigid.velocity.x > maxSpeed)
        {
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        }
        // Left Max Speed
        else if (rigid.velocity.x < maxSpeed * -1)
        {
            rigid.velocity = new Vector2(maxSpeed * -1, rigid.velocity.y);
        }

        // RayCast : 오브젝트 검색을 위해 Ray를 쏘는 방식
        // Landing Platform
        if (rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down ,new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));

            if (rayHit.collider != null)
            {
                if (rayHit.distance <  0.5f)
                {
                    // Debug.Log(rayHit.collider.name);
                    animator.SetBool("isJumping", false);

                }
            
            }            
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Enemy")
        {
            // Attack
            if (rigid.velocity.y < 0 && transform.position.y > other.transform.position.y)
            {
                OnAttack(other.transform);
            }
            // Damaged
            else
            {
                OnDamaged(other.transform.position);    
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Item")
        {
            // Point
            bool isBronze = other.gameObject.name.Contains("Bronze");
            bool isSilver = other.gameObject.name.Contains("Silver");
            bool isGold = other.gameObject.name.Contains("Gold");

            if (isBronze)
            {
                gameManager.stagePoint += 50;                
            }
            else if (isSilver)
            {
                gameManager.stagePoint += 100;                
            }
            else if (isGold)
            {
                gameManager.stagePoint += 300;                
            }            

            // Deactive Item
            other.gameObject.SetActive(false);

            // Sound
            PlaySound("ITEM");
        }
        else if (other.gameObject.tag == "Finish")
        {
            // Next Stage
            gameManager.NextStage();

            // Sound
            PlaySound("FINISH");            

        }
    }

    void OnAttack(Transform enemy)
    {
        // Sound
        PlaySound("ATTACK");

        // Point
        gameManager.stagePoint += 100;

        // Reaction Force
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        // Enemy Die
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }

    void OnDamaged(Vector2 targetPos)
    {
        // Sound
        PlaySound("DAMAGED");

        // Health Down
        gameManager.HealthDown();

        // Change Layer (Immortal Active)
        gameObject.layer = 11;

        // View Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // Reaction Force
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);

        // Animation
        animator.SetTrigger("doDamaged");

        Invoke("OffDamaged", 2);
    }

    void OffDamaged()
    {
        // Change Layer (Normal Active)
        gameObject.layer = 10;

        // View Alpha
        spriteRenderer.color = new Color(1, 1, 1, 1f);
    }

    public void OnDie()
    {
        // Sound
        PlaySound("DIE");

        // Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // Sprite Flip Y
        spriteRenderer.flipY = true;

        // Collider Disable
        capsuleCollider2D.enabled = false;

        // Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }

void PlaySound(string action)
    {
        switch (action) {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                break; 
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;       
            case "ITEM":
                audioSource.clip = audioItem;
                break;                      
            case "DIE":
                audioSource.clip = audioDie;
                break;                     
            case "FINISH":
                audioSource.clip = audioFinish;
                break;                            
            default:
                return;
        }

        audioSource.Play();
    }    
}