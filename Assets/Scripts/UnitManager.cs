using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitManager : MonoBehaviour
{
    //game manager
    private GameObject manager;

    //unit collider
    public BoxCollider2D col_range;
    public BoxCollider2D col_body;

    //unit animator
    private Animator unit_anim;

    //effects
    Dictionary<string, GameObject> effects = new Dictionary<string, GameObject>();

    //unit color
    public SpriteRenderer unit_color_hit;
    public SpriteRenderer[] color_parts = new SpriteRenderer[9];
    // 0 | head   2 | Rarm    4 | Rleg    6 | Rshold    8 | armed //
    // 1 | body   3 | Larm    5 | Lleg    7 | Lshold              //
    private Vector4 F_color;
    private Vector4 E_color;
    private Vector4 unit_color;

    //Cost
    public int unit_cost;

    //Cool time
    public float unit_cooltime;

    //HP
    public float unit_maxhp;
    private float unit_hp;

    //HP bar
    public Slider healthbar; //prefab
    private Slider unit_healthbar; //generated healthbar
    public GameObject HUP; //head up position

    //Speed
    public float unit_speed;
    private float net_speed;

    //Damage
    public float unit_damage;

    //Weapon
    public string unit_weapon;

    //Range
    public float unit_range;

    //Attack target
    private GameObject target;
    private bool wasattackbase;

    //Side (1 or -1)
    public int side;

    //Target queue
    private Queue<GameObject> target_queue;

    //Unit status
    enum Status { idle, destroyed, move, attack };
    Status unit_status;
    Status target_status;

    //Unit position
    private int unit_r;
    private int unit_c;

    void SetUnitR(int r)
    {
        unit_r = r;
    }
    void SetUnitC(int c)
    {
        unit_c = c;
    }
    void GetFColor(Vector4 c)
    {
        F_color = c;
    }
    void GetEColor(Vector4 c)
    {
        E_color = c;
    }

    void Damaged(float damage)
    {
        unit_hp -= damage;
    }

    void ShowEffect(string effect)
    {
        GameObject showed_effect = Instantiate(effects[effect], transform.position, Quaternion.identity);
        showed_effect.transform.localScale = new Vector3(side, 1, 1);
        showed_effect.GetComponent<Transform>().SetParent(transform);
        StartCoroutine(DestroyEffect(showed_effect));
    }

    void Attack()
    {
        if (target)
        {
            target.SendMessage("Damaged", unit_damage);
            if (target.tag.Contains("Unit")) {
                target.SendMessage("ShowEffect", unit_weapon);
            }
            else if (target.tag.Contains("Base")) {
                ArrayList enp = new ArrayList();
                enp.Add(unit_weapon);
                enp.Add(transform.position.y);
                target.SendMessage("ShowEffect", enp);
            }
        }
    }

    void GiveState(GameObject gameObject)
    {
        gameObject.SendMessage("GetState", (int)unit_status);
    }

    void GetState(int status)
    {
        target_status = (Status)status;
    }

    IEnumerator DestroyEffect(GameObject effect)
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(effect);
    }

    IEnumerator DestroyAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
        Destroy(unit_healthbar.gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.IsTouching(col_range) && (collision.name != "Col_range")) // not [range] > < [range]
        {
            string col_tag = collision.transform.parent.tag;
            if ((col_tag.Contains("p1") && tag.Contains("p2")) || (col_tag.Contains("p2") && tag.Contains("p1")))
            {
                target_queue.Enqueue(collision.transform.parent.gameObject);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //initialize
        manager = GameObject.Find("Manager");
        unit_hp = unit_maxhp;
        net_speed = unit_speed;
        unit_status = Status.move;
        target = null;
        wasattackbase = false;

        //effect dictionary
        effects = manager.GetComponent<GameManager>().effects;

        //animator
        unit_anim = GetComponent<Animator>();
        unit_anim.SetInteger("anim_state", 1);

        //set color
        manager.GetComponent<ColorManager>().SendMessage("SendFColor", gameObject);
        manager.GetComponent<ColorManager>().SendMessage("SendEColor", gameObject);

        //for test
        if (F_color == new Vector4(0, 0, 0, 0)) F_color = new Vector4(1, 1, 1, 1);

        if (side > 0) unit_color = F_color;
        else unit_color = E_color;

        foreach (SpriteRenderer c in color_parts)
        {
            if(c) c.color = unit_color;
        }

        //flip enemy's sprite
        transform.localScale = new Vector3(side, 1, 1);

        //setting body collider [ㅁ]
        col_body.offset = new Vector2(0, -12);
        col_body.size = new Vector2(50, 50);
        //setting range collider [ㅁㅁㅁㅁ]
        col_range.offset = new Vector2(45 * unit_range, -12);
        col_range.size = new Vector2(90 * unit_range, 50);

        //make unit health bar *====*
        unit_healthbar = Instantiate(healthbar);
        unit_healthbar.maxValue = unit_maxhp;
        unit_healthbar.transform.position = HUP.transform.position;
        //make health bar a child of the canvas
        unit_healthbar.GetComponent<Transform>().SetParent(GameObject.Find("Canvas").GetComponent<Transform>());
        //makse health bar the lowest layer in UI
        unit_healthbar.transform.SetAsFirstSibling();

        //target queue
        target_queue = new Queue<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        //unit health (bar)
        unit_healthbar.value = unit_hp;
        unit_healthbar.transform.position = HUP.transform.position;
        
        // (move, attack, idle) => death
        if (unit_hp <= 0)
        {
            if (unit_status != Status.destroyed) //flag
            {
                unit_status = Status.destroyed;
                unit_anim.SetInteger("anim_state", 3);
                //decrase unit in a row
                if (tag.Contains("p1")) manager.SendMessage("DecreaseUnitFinRow", unit_r);
                else if (tag.Contains("p2")) manager.SendMessage("Getreward", (int)(unit_cost * 0.2f));

                StartCoroutine(DestroyAfter(1.0f)); //motion time of dead
            }
        }

        else
        {
            // (move, idle) => attack
            if (unit_status != Status.attack && target_queue.Count > 0)
            {
                target = target_queue.Dequeue();
                if (target)
                {
                    unit_status = Status.attack;
                    target.SendMessage("GiveState", gameObject); //target_status
                    unit_anim.SetInteger("anim_state", 2);

                    if (tag.Contains("p1") && target.tag.Contains("Base")) manager.SendMessage("IncreaseUnitFattackinRow", unit_r);
                }
            }

            if (unit_status == Status.attack && target != null)
            {
            // base attack => attack
                if (target.tag.Contains("Base") && target_queue.Count > 0)
                {
                    target_queue.Enqueue(target);
                    target = target_queue.Dequeue();

                    if (tag.Contains("p1")) manager.SendMessage("DecreaseUnitFattackinRow", unit_r);
                }
            // attack => move
                target.SendMessage("GiveState", gameObject); //target_status
                if (target_status == Status.destroyed)
                {
                    unit_status = Status.move;
                    unit_anim.SetInteger("anim_state", 1);
                }
            }

            //unit moving
            if (unit_status == Status.move) net_speed = side * 90 * unit_speed * Time.deltaTime;
            else net_speed = 0f;
            transform.Translate(net_speed, 0, 0);
        }
    }
}
