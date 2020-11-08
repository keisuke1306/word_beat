using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class NodeController : MonoBehaviour
{
    // ステート管理
    public enum State {
        SPOWN,
        RUNNING,
        CUT,
        CUTTED
    };

    // AudioSource
    public AudioClip[] audioClips;

    /******************************************/

    // GameAreaManager
    private GameObject gameAreaManagerObject;
    private GameAreaManager gameAreaManager;

    // AudioSource
    private AudioSource audioSource;

    // 現在のステート
    private State currentState = State.SPOWN;

    // オブジェクト関係
    private GameObject Parent;
    private List<GameObject> Paneles;

    // スポーン情報
    private Vector3 spownPosition;
    private Vector3 playerPosition = new Vector3(0, 0, 0);
    private float spownTime = 0f;

    // 二点間の距離を入れる
    private float distance_two;

    // スピード
    private float speed = 15.0f;

    // カット時の分裂するスピード
    private Vector3[] cutSpeeds;

    // Start is called before the first frame update
    void Start()
    {
        // スポーン位置を設定
        spownPosition = new Vector3(R(-15f, 15f), R(-15f, 15f), 50f);
        spownTime = Time.time;

        GameObject Parent = this.gameObject;
        Paneles = new List<GameObject>();
        foreach(Transform child in Parent.transform.Find("CubeList"))
        {
            Paneles.Add(child.gameObject);
        }

        cutSpeeds = new Vector3[4] {
            GenerateDestroyVec(),
            GenerateDestroyVec(),
            GenerateDestroyVec(),
            GenerateDestroyVec()
        };

        // 2点の距離
        distance_two = Vector3.Distance(spownPosition, playerPosition);

        // 斬撃時のSE
        audioSource = this.gameObject.GetComponent<AudioSource>();

        // GameAreaManager
        gameAreaManagerObject = GameObject.Find("GameAreaManager");
        gameAreaManager = gameAreaManagerObject.GetComponent<GameAreaManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (currentState) {
            case State.SPOWN:
                transform.position = spownPosition;
                currentState = State.RUNNING;
                break;

            case State.RUNNING:
                // 現在の位置
                float present_Location = ((Time.time - spownTime) * speed) / distance_two;

                // オブジェクトの移動
                transform.position = Vector3.Slerp(spownPosition, playerPosition, present_Location);
                break;

            case State.CUT:
                StartCoroutine("DeleteNode");
                currentState = State.CUTTED;
                break;

            case State.CUTTED:
                int i;
                for (i = 0; i < Paneles.Count; i++)
                {
                    Paneles[i].transform.Translate(cutSpeeds[i] * Time.deltaTime);
                    Paneles[i].transform.Rotate(new Vector3(0, 0, 5));
                }
                break;
        }

        // DEBUG
        if (Input.GetKey(KeyCode.Space)) {
            Cut();
        }
    }

    /**
     * ノード切断処理
     */
    void Cut()
    {
        if (currentState == State.CUT
            || currentState == State.CUTTED) {
            return;
        }

        // 振動
        StartCoroutine(Vivration());
        
        // ステート CUTへ変更
        currentState = State.CUT;

        // 斬撃音再生
        int audioKey = Random.Range(0, audioClips.Length);
        audioSource.clip = audioClips[audioKey];
        audioSource.Play();

        // ネガティブノードを切った場合はテンションポイントを1追加
        switch (this.gameObject.tag) {
            case "Negative":
                gameAreaManager.tensionPointIncrement();
                break;
        }
    }

    /**
     * 衝突処理
     */
    void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag) {
            case "Sword":
                Cut();
                break;

            case "Player":
                CollisionPlayer();
                break;
        }
    }

    /**
     * カット時のブロックの飛散方向を作成
     */
    Vector3 GenerateDestroyVec()
    {
        return new Vector3(R(), R(), R(-40.0f, -20.0f));
    }

    /**
     * ランダム関数
     * @param float
     * @param float
     */
    float R(float startRange = -5.0f, float endRange = 5.0f)
    {
        return Random.Range(startRange, endRange);
    }

    /**
     * プレイヤーと衝突
     */
    void CollisionPlayer()
    {
        if (currentState != State.RUNNING) {
            return;
        }

        // 衝突時のSE
        gameAreaManager.PlaySE(this.gameObject.tag);

        // ポジティブノードにぶつかった時はテンションポイント1追加
        // ネガティブノードにぶつかった時はテンションポイント1減少
        switch (this.gameObject.tag) {
            case "Positive":
                gameAreaManager.tensionPointIncrement();
                break;

            case "Negative":
                gameAreaManager.tensionPointDecrement();
                break;
        }

        // 自身のノードを削除
        Destroy(this.gameObject);
    }

    /**
     * 自身のノードを削除
     */
    IEnumerator DeleteNode()
    {
        yield return new WaitForSeconds(2.5f);
        Destroy(this.gameObject);
    }

    /**
     * コントローラー振動
     */
    IEnumerator Vivration(float time = 0.2f)
    {
        // 握られているコントローラーを検出
        var activeController = OVRInput.GetActiveController();

        // 振動させる
        OVRInput.SetControllerVibration(1, 1, activeController);

        // 振動を止めるまで待機
        yield return new WaitForSeconds(time);

        // 振動を止める
        OVRInput.SetControllerVibration(0, 0, activeController);
    }

    /**
     * ノードを操作
     * @param Material
     * @param bool
     * @param string
     * @param string
     * @param string
     */
    public void SetParams(Material material,
                        bool displayMaterial = true,
                        string message = "",
                        string seNum = "001",
                        string nodeType = "Positive")
    {
        // Tag設定
        this.gameObject.tag = nodeType;

        // テキスト設定
        GameObject child = this.gameObject.transform.Find("Text").gameObject;
        TextMeshPro textMeshPro = child.GetComponent<TextMeshPro>();
        textMeshPro.text = message;

        // ボイス再生
        AudioSource m_audioSource = this.gameObject.GetComponent<AudioSource>();
        m_audioSource.clip = Resources.Load("Music/SE/Voice/" + nodeType + "/" + seNum) as AudioClip;
        m_audioSource.Play();

        // マテリアル
        if (displayMaterial)
        {
            foreach(Transform childCube in this.gameObject.transform.Find("CubeList"))
            {
                childCube.GetComponent<Renderer>().material = material;
            }
        }
    }
}
