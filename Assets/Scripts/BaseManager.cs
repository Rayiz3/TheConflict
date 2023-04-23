using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseManager : MonoBehaviour
{
    //game manager
    private GameObject manager;

    //color
    public int side;
    public SpriteRenderer body_renderer;
    public SpriteRenderer color_renderer;
    private Vector4 F_color;
    private Vector4 E_color;

    //effects
    Dictionary<string, GameObject> effects = new Dictionary<string, GameObject>();

    //hp
    public float base_maxhp;
    private float base_hp;

    //HP bar
    public Slider base_healthbar;
    private Text txt_hp;

    //Unit status
    enum Status { idle, destroyed };
    Status base_status;

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
        base_hp -= damage;
        if (base_hp < 0) base_hp = 0f;
    }
    void GiveState(GameObject gameObject)
    {
        gameObject.SendMessage("GetState", (int)base_status);
    }

    void ShowEffect(ArrayList enp)
    {
        string effect = enp[0].ToString();
        Vector2 pos = new Vector3(transform.position.x, (float)enp[1]);
        
        GameObject showed_effect = Instantiate(effects[effect], pos, Quaternion.identity);
        showed_effect.transform.localScale = new Vector3(side, 1, 1);
        showed_effect.GetComponent<Transform>().SetParent(transform);
        StartCoroutine(DestroyEffect(showed_effect));
    }

    IEnumerator DestroyEffect(GameObject effect)
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(effect);
    }

    // Start is called before the first frame update
    void Start()
    {
        //initialize
        manager = GameObject.Find("Manager");
        base_hp = base_maxhp;
        base_status = Status.idle;

        //effect dictionary
        effects = manager.GetComponent<GameManager>().effects;

        //set color
        GameObject.Find("Manager").GetComponent<ColorManager>().SendMessage("SendFColor", gameObject);
        GameObject.Find("Manager").GetComponent<ColorManager>().SendMessage("SendEColor", gameObject);

        color_renderer = color_renderer.GetComponent<SpriteRenderer>();
        if (side > 0) color_renderer.color = F_color;
        else color_renderer.color = E_color;

        //set healthbar
        base_healthbar.maxValue = base_maxhp;
        txt_hp = base_healthbar.GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        base_healthbar.value = base_hp;
        txt_hp.text = base_hp.ToString() + " / " + base_maxhp.ToString();

        if (base_hp <= 0)
        {
            base_status = Status.destroyed;
            GameObject.Find("Manager").SendMessage("GameEnd", side);
        }
    }
}
