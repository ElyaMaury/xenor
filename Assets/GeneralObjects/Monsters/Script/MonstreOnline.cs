using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using Pathfinding;



public class MonstreOnline : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float speed;//speed of the monster 
    private float PlayerDetectTime;//time to detect the player 
    public float PlayerDetectRate;//speed to detect the player 
    public float chaseRange;//distance to detect the player
    int round=1;//number of round 
    public float pv;//life of the monster
    bool dead = false;//bool make sure taht the monster is not dead


    [Header("Attack")]
    [SerializeField] float attackRange;//distance to attack
    [SerializeField] float attaclRate;//speed to attack
    private float lastAttackTime;


    [Header("Component")]
    Rigidbody2D rb;//Rigidbody of the monster 
    private playerwalkOnline targetPlayer; // component that make the player walk 
    public Animator animator;//animator of the monster 



    [Header("PathFinding")]
    public float NextWayPointDistance = 2f;//diatance player monster
    Path path;
    int currentPath = 0;//number of path
    bool reachEndPath = false;//valid path
    Seeker seeker;//the detection part of the map 

    void Start()//start the path finding
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        //InvokeRepeating("UpdatePath", 0f, .5f);
    }
    

    void OnPathComplete(Path p)//verify if the path is valid 
    {
        if (!p.error)
        {
            path = p;
            currentPath = 0;
        }
    }

    void UpdatePath()
    {
        if (seeker.IsDone() && targetPlayer!=null && !dead)
        {
            seeker.StartPath(rb.position, targetPlayer.transform.position, OnPathComplete);// research a valid path
        }

    }

    private void FixedUpdate()
    {

        if (targetPlayer != null && !dead )
        {
            float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);// distance


           
            if (dist > attackRange)//check if the distance 
            {
                if (path == null)
                    return;

                if (currentPath >= path.vectorPath.Count)//check the good path
                {
                    reachEndPath = true;
                    return;
                }
                else
                {
                    reachEndPath = false;
                }

                Vector2 direction = ((Vector2)path.vectorPath[currentPath] - rb.position).normalized;//create the direction
                Vector2 force = direction * speed * Time.fixedDeltaTime; //create a mouvement 
                walk(direction);
                

                rb.velocity = force;//move
                float distance = Vector2.Distance(rb.position, path.vectorPath[currentPath]);
                if (distance < NextWayPointDistance)//check the distance between the next position and the current one
                {
                    currentPath++;
                }

            }
            else
            {

                rb.velocity = Vector2.zero;//stop the monster 
            }
        }
        DetectPlayer();
        StartCoroutine(waiter());//make the attack all the 15 sec 
    }

    void DetectPlayer()
    {
        if(Time.time-PlayerDetectTime>PlayerDetectRate)//can the player be detect
        {
            PlayerDetectTime = Time.time;//detect the player on the moment
            foreach(playerwalkOnline player in FindObjectsOfType<playerwalkOnline>())//research all the player 
            {
                if(player!=null)
                {
                    float dist = Vector2.Distance(transform.position, player.transform.position);//go to the player


                    if(player == targetPlayer)
                    {
                        if(dist>chaseRange)
                        {
                            targetPlayer = null;//no player on the path
                        }
                    }

                    else if (dist < chaseRange)
                    {
                        if (targetPlayer == null)
                            targetPlayer = player;//change the player 
                    }
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D colision)
    {
        switch (colision.gameObject.tag)//take the tag of the object
        {
            case "Player":
                Attack();//attack animation
                colision.gameObject.GetComponent<playerOnline>().GetDamage();//reduce player life
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D colision)
    {
        switch (colision.gameObject.tag)//take the tag of the object
        {
            case "Potion"://damage potion 
                playerwalkOnline current_player = ChangeTarget(GameObject.FindObjectsOfType<playerwalkOnline>()[0]);
                GetDamage((float)current_player.GetComponent<playerOnline>().Strength);//have damage
                Destroy(colision.gameObject);
                break;
        }
    }

    public playerwalkOnline ChangeTarget(playerwalkOnline current_player)//change the target player 
    {
        foreach(playerwalkOnline other_player in FindObjectsOfType<playerwalkOnline>())//research all the player
        {
            if (other_player != current_player && current_player.GetComponent<playerOnline>().Strength < other_player.GetComponent<playerOnline>().Strength)
                return other_player;//change player 
        }
        return current_player;

    }

    public void GetDamage(float damage)
    {
        pv -= damage;//reduce the life 

        if (pv <= 0)
            Die();//die animation 
        
    }

    void Die()//kill the monster 
    {
        
        
        animator.SetFloat("Die", 1);// Run death animation
        dead = true;
    }

    void Attack()//attack the player 
    {
        if (dead) return;

        // Run Attack animation
        if (round % 2 == 0)//make a animation depending on the round 
        {
            animator.SetFloat("Attack", 1f); //Animation attack number 1
            round += 1;
        }
        else
        {
            animator.SetFloat("Attack", 0.1f);//Animation attack number 2
            round += 1;
        }
    }


    /*
     * make wait 15 before remove the attack animation
     */
    IEnumerator waiter()
    {
            yield return new WaitForSeconds(15);
            animator.SetFloat("Attack", 0f);//remove the attack animation 
    }

    public void walk(Vector2 mouvement)
    {
        animator.SetFloat("Horizontal", mouvement.x);//mise en place de l'animation 
        animator.SetFloat("Vertical", mouvement.y);
        animator.SetFloat("Magnitude", mouvement.magnitude);
    }

}
