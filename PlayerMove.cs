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
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private CapsuleCollider2D capsule_pl;
    
    AudioSource audioSource;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsule_pl = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }
    
    private void Update()
    {
        // Jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            
            // Sound
            PlaySound("Jump");
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

        if (Mathf.Abs(rigid.velocity.x) < 0.3)
        {
            anim.SetBool("isWalking", false);
        }
        else
        {
            anim.SetBool("isWalking", true);
        }
    }

    private void FixedUpdate()
    {
        // Move Speed
        float h = Input.GetAxisRaw("Horizontal");
        
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);
        
        // Max Speed
        if(rigid.velocity.x > maxSpeed)
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if (rigid.velocity.x < maxSpeed * (-1))
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);
        
        // Landing Platform
        if (rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.5f)
                    anim.SetBool("isJumping", false);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            // Attack
            if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
                
                // Sound
                PlaySound("Attack");
            }
            else
            {
                // Damaged
                OnDamaged(collision.transform.position);
                
                // Sound
                PlaySound("Damaged");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Point
        if (collision.gameObject.tag == "Item")
        {
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if (isBronze)
                gameManager.stagePoint += 50;
            else if (isSilver)
                gameManager.stagePoint += 100;
            else if (isGold)
                gameManager.stagePoint += 300;
            
            // Deactive Item
            collision.gameObject.SetActive(false);
            
            // Sound
            PlaySound("Item");
        }
        else if (collision.gameObject.tag == "Finish")
        {
            collision.gameObject.SetActive(false);
            // Next Stage
            gameManager.NextStage();
            
            // Sound
            PlaySound("Finish");
        }
    }

    void OnAttack(Transform enemy)
    {
        // Point
        gameManager.stagePoint += 100;
        
        // Reaction Force
        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
        
        //Enemy Die
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }

    void OnDamaged(Vector2 tragetPos)
    {
        // Health Down
        gameManager.HealthDown();
        
        // change Layer (Immortal Active)
        gameObject.layer = 8;

        // View Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        
        // Reaction Force
        int dirc = transform.position.x - tragetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 7 ,ForceMode2D.Impulse);
        
        // Animation
        anim.SetTrigger("doDamaged");
        
        Invoke("OffDamaged", 3);
    }

    void OffDamaged()
    {
        gameObject.layer = 7;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void OnDie()
    {
        // Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        // Sprite Flip Y
        spriteRenderer.flipY = true;
        // Collider Disable
        capsule_pl.enabled = false;
        // Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        
        PlaySound("Die");
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }
    
    void PlaySound(string action)
    {
        switch (action)
        {
            case "Jump":
                audioSource.clip = audioJump;
                audioSource.Play();
                break;
            case "Attack":
                audioSource.clip = audioAttack;
                audioSource.Play();
                break;
            case "Damaged":
                audioSource.clip = audioDamaged;
                audioSource.Play();
                break;
            case "Item":
                audioSource.clip = audioItem;
                audioSource.Play();
                break;
            case "Die":
                audioSource.clip = audioDie;
                audioSource.Play();
                break;
            case "Finish":
                audioSource.clip = audioFinish;
                audioSource.Play();
                break;
        }
    }
}
