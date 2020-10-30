using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    public int nextMove;
    float nextThinktime ;
    Animator animator;
    SpriteRenderer sprite;

    private void Awake() {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        Think();

        nextThinktime = Random.Range(2f, 5f);
        Invoke("Think", nextThinktime);
    }
    
    private void FixedUpdate() {
        // Move
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        // Platform Check
        Vector2 frontVec = new Vector2(rigid.position.x + (nextMove * 0.2f), rigid.position.y);
        Debug.DrawRay(frontVec, Vector3.down ,new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));

        if (rayHit.collider == null)
        {       
            Turn();
        }  
    }

    void Think()
    {
        // Set Next Active
        nextMove = Random.Range(-1, 2);

        // Sprite Animation
        animator.SetInteger("WalkSpeed", nextMove);

        // Flip Sprite
        if (nextMove != 0)
        {
            sprite.flipX = nextMove == 1;
        }

        // Recursive
        nextThinktime = Random.Range(2f, 5f);
        Invoke("Think", nextThinktime);        
    }

    void Turn()
    {
        nextMove = nextMove * -1;
        sprite.flipX = nextMove == 1;
        
        CancelInvoke();
        Invoke("Think", 3);           
    }
}
