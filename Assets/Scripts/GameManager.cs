using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //Base
    public GameObject base_F;
    public GameObject base_E;

    //Units
    public GameObject[] Unit = new GameObject[5];
    private int[] cost = new int[5];
    private float[] cooltime = new float[5];

    //Spawning
    public Transform[] spawnpoint = new Transform[5];
    public Transform[] spawnpointE = new Transform[5];
    public bool isrally;
    private int rallypoint;

    //Time
    public Text timetext;
    private float timecount;

    //Resource
    public Text goldtext;
    public Text goldleveltext;
    public Text goldrequiretext;
    private int gold_level;
    private int[] gold_level_required;
    private float[] income_rate; // G/s
    private int[] gold_max;
    private int gold;

    //Cool time
    public Image[] coolbox = new Image[5];
    private float[] cool = new float[5];

    //CMD bar panel
    public Image CMD_Select;

    //effects
    public Dictionary<string, GameObject> effects = new Dictionary<string, GameObject>();
    public GameObject effect_hit_blade;
    public GameObject effect_shot_gun;
    public GameObject effect_element_lightning;

    //enemy ai
    private int[] unitF_in_row = new int[5];
    private int[] unitF_attack_in_row = new int[5];
    private int enemyspawn;

    //Sounds
    public AudioSource m_gameplay;

    //Pause & End
    public GameObject panel_pause;
    public GameObject panel_end;
    public Text txt_end;
    private bool isgameend;

    public void Pause()
    {
        Time.timeScale = 0f;
        m_gameplay.volume = 0.5f;
        panel_pause.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        m_gameplay.volume = 1f;
        panel_pause.SetActive(false);
    }

    public void GameEnd(int side)
    {
        StartCoroutine(SendEndSignal(side));
    }

    public void IncreaseUnitFinRow(int r)
    {
        unitF_in_row[r-1] += 1;
    }
    public void DecreaseUnitFinRow(int r)
    {
        unitF_in_row[r-1] -= 1;
    }
    public void IncreaseUnitFattackinRow(int r)
    {
        unitF_attack_in_row[r - 1] += 1;
        string a = "";
        //for (int i = 0; i < 5; i++) a += unitF_attack_in_row[i].ToString();
        //Debug.Log(a);
    }
    public void DecreaseUnitFattackinRow(int r)
    {
        unitF_attack_in_row[r - 1] -= 1;
        string a = "";
        //for (int i = 0; i < 5; i++) a += unitF_attack_in_row[i].ToString();
        //Debug.Log(a);
    }

    void Setisrally(bool rally)
    {
        isrally = rally;
    }
    
    void Setrallypoint(int index)
    {
        rallypoint = index;
    }

    public void SpawnUnit(int index)
    {
        if(gold >= cost[index] && cool[index] <= 0)
        {
            CMD_Select.gameObject.SetActive(true);
            StartCoroutine(SelectRoute(index));
        }
    }

    public void SpawnUnitE(int index, int row)
    {
        GameObject enemy = Instantiate(Unit[index], spawnpointE[row].position, Quaternion.identity);
        enemy.tag = "Unit_p2";
        enemy.GetComponent<UnitManager>().side = -1;
    }

    public void GoldUpgrade()
    {
        if (gold_level < 4 && gold >= gold_level_required[gold_level])
        {
            gold -= gold_level_required[gold_level];
            gold_level++;
        }
    }

    void Getreward(int reward)
    {
        gold = Mathf.Min(gold_max[gold_level], gold + reward);
    }

    IEnumerator SendEndSignal(int side)
    {
        yield return new WaitForSeconds(3f);

        isgameend = true;
        Time.timeScale = 0f;
        panel_end.SetActive(true);

        if (side < 0) txt_end.text = "Victory!";
        else if (side > 0) txt_end.text = "Defeat";
    }

    IEnumerator SelectRoute(int index)
    {
        isrally = true;
        yield return new WaitWhile(() => isrally);
        CMD_Select.gameObject.SetActive(false);

        if(rallypoint > 0)
        {
            Instantiate(Unit[index], spawnpoint[rallypoint - 1].position, Quaternion.identity);
            //increase num in a row
            IncreaseUnitFinRow(rallypoint);
            //use gold
            gold -= cost[index];
            //count cool time
            cool[index] = cooltime[index];
            coolbox[index].gameObject.SetActive(true);

            //return to 0
            rallypoint = 0;
        }
    }

    IEnumerator Income()
    {
        yield return new WaitForSeconds(1/income_rate[gold_level]);
        if (gold < gold_max[gold_level]) gold++;
        StartCoroutine(Income());
    }

    IEnumerator Spawnenemy(float d)
    {
        yield return new WaitForSeconds(d);

        int[] num_info_row = new int[5];
        int tot = 0;
        for (int i = 0; i < 5; i++) {
            num_info_row[i] = (unitF_in_row[i] + 1) * (unitF_attack_in_row[i] + 1);
            tot += num_info_row[i];
        }

        // allocating row; Laplace
        int prob = 0;
        int r = Random.Range(0, tot);
        for (int i = 0; i < 5; i++)
        {
            prob += num_info_row[i];
            if (r < prob)
            {
                enemyspawn = i;
                break;
            }
        }
        // allocating unit
        int u = Random.Range(1, 101) - 1;
        if (u <= 40) SpawnUnitE(0, enemyspawn);      //40%
        else if (u <= 70) SpawnUnitE(1, enemyspawn); //30%
        else if (u <= 85) SpawnUnitE(2, enemyspawn); //15%
        else if (u <= 90) SpawnUnitE(3, enemyspawn); //5%
        else SpawnUnitE(4, enemyspawn);              //10%

        if (!isgameend)
        {
            u = Random.Range(0, 5);
            StartCoroutine(Spawnenemy(0.8f * Unit[u].GetComponent<UnitManager>().unit_cost/income_rate[2]));
        }
    }

    private void OnGUI()
    {
        string timeStr;
        timeStr = "" + (timecount / 60).ToString("00") + ':' + (timecount % 60 / 1).ToString("00");
        timetext.text = timeStr;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Initialize
        timecount = 0f;

        gold_level = 0;
        gold_level_required = new int[5] { 10, 20, 30, 40, 0 };
        income_rate = new float[5] { 1.1f, 1.3f, 1.5f, 1.7f, 2f };
        gold_max = new int[5] { 20, 40, 60, 80, 100 };
        gold = 10;

        isrally = false;
        rallypoint = 0;
        isgameend = false;

        //effect dictionary
        effects.Add("blade", effect_hit_blade);
        effects.Add("gun", effect_shot_gun);
        effects.Add("lightning", effect_element_lightning);

        //entry information
        for (int i=0; i<5; i++)
        {
            cost[i] = Unit[i].GetComponent<UnitManager>().unit_cost;
            cooltime[i] = Unit[i].GetComponent<UnitManager>().unit_cooltime;
            cool[i] = 0f;

            unitF_in_row[i] = 0;
            unitF_attack_in_row[i] = 0;
        }

        StartCoroutine(Income());
        StartCoroutine(Spawnenemy(3));
    }

    // Update is called once per frame
    void Update()
    {
        //play time
        timecount += Time.deltaTime;

        //gold income
        goldtext.text = "Gold : " + gold.ToString() + "/" + gold_max[gold_level].ToString() + "G";
        if (gold_level == 4)
        {
            goldleveltext.text = "Max";
            goldrequiretext.text = "-";
        }
        else
        {
            goldleveltext.text = "Lv." + gold_level.ToString();
            goldrequiretext.text = gold_level_required[gold_level].ToString();
        }

        //cool time reduce
        for (int i = 0; i < 5; i++)
        {
            if (cool[i] > 0) cool[i] -= Time.deltaTime;
            else coolbox[i].gameObject.SetActive(false);

            coolbox[i].fillAmount = cool[i] / cooltime[i];
        }

        //game pause
        if (Input.GetKeyDown(KeyCode.Escape) && Time.timeScale == 0) Resume();
        else if (Input.GetKeyDown(KeyCode.Escape) && Time.timeScale != 0) Pause();

        //spawn key
        if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnUnit(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnUnit(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SpawnUnit(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SpawnUnit(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SpawnUnit(4);

        if (Input.GetMouseButton(1) && isrally) isrally = false;

        if (Input.anyKeyDown && isgameend) GameObject.Find("Manager").GetComponent<LoadSceneManager>().Lobby();
    }
}
