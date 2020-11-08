using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class InputNodeJson
{
    public NodeParams[] positive;
    public NodeParams[] negative;
}

[Serializable]
public class NodeParams
{
    public string message;
    public string se;
}

public class SpownNodeController : MonoBehaviour
{
    // node state
    private enum NodeState {
        POSITIVE,
        NEGATIVE,
    }

    // json
    private InputNodeJson inputNodeJson;

    // Spown
    [SerializeField]
    public bool isSpown = true;

    // Spown spacing
    private float spawnSpacing = 2f;
    private float timeElapsed = 0f;

    // Materials
    [SerializeField]
    public Material positiveMaterial;

    [SerializeField]
    public Material negativeMaterial;

    // material flag
    private bool displayMaterial = true;

    // Start is called before the first frame update
    void Start()
    {
        // Spown
        isSpown = true;
        
        // jsonをテキストファイルとして読み取り、string型で受け取る
        string inputString = Resources.Load<TextAsset>("Config/Node").ToString();

        // 上で作成したクラスへデシリアライズ
        inputNodeJson = JsonUtility.FromJson<InputNodeJson>(inputString);

        GenerateNode();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timeElapsed += Time.deltaTime;

        if (isSpown
            && timeElapsed >= spawnSpacing) {
            // Do anything
            GenerateNode();

            timeElapsed = 0.0f;
        }
    }

    /**
     * ノードを生成
     */  
    public void GenerateNode()
    {
        GameObject nodeObject = (GameObject)Resources.Load("Prefab/Node");
        GameObject obj = Instantiate(nodeObject, new Vector3(0.0f, 10000.0f, 100.0f), Quaternion.identity);

        // ポジティブ or ネガティブ
        switch (GetPositiveOrNegative()) {
            case NodeState.POSITIVE:
                GeneratePositiveNode(obj);
                break;

            case NodeState.NEGATIVE:
                GenerateNegativeNode(obj);
                break;
        }

    }

    /**
     * ポジティブかネガティブのステートを取得
     * @param  float
     * @return NodeState
     */
    NodeState GetPositiveOrNegative(float threshold = 0.67f)
    {
        float trial = UnityEngine.Random.Range(0f, 1.0f);
        if (trial > threshold) {
            return NodeState.POSITIVE;
        }
        else {
            return NodeState.NEGATIVE;
        }
    }

    /**
     * ポジティブのノードを生成
     * @param  GameObject
     */
    void GeneratePositiveNode(GameObject obj)
    {
        int trial = UnityEngine.Random.Range(0, inputNodeJson.positive.Length);
        string message = inputNodeJson.positive[trial].message;
        string se = inputNodeJson.positive[trial].se;

        NodeController script = obj.GetComponent<NodeController>();
        script.SetParams(positiveMaterial, displayMaterial, message, se, "Positive");
    }

    /**
     * ネガティブのノードを生成
     * @param  GameObject
     */
    void GenerateNegativeNode(GameObject obj)
    {
        int trial = UnityEngine.Random.Range(0, inputNodeJson.negative.Length);
        string message = inputNodeJson.negative[trial].message;
        string se = inputNodeJson.negative[trial].se;

        NodeController script = obj.GetComponent<NodeController>();
        script.SetParams(negativeMaterial, displayMaterial, message, se, "Negative");
    }

    /**
     * スポーン間隔をセット
     * @param float
     */
    public void SetSpownSpacing(float spawnSpacing)
    {
        this.spawnSpacing = spawnSpacing;
    }

    /**
     * ノードのマテリアル情報をセット
     * @param bool
     */
    public void SetDisplayMaterial(bool displayMaterial)
    {
        this.displayMaterial = displayMaterial;
    }

    /**
     * スポーンフラグをセット
     * @bool
     */
    public void SetSpown(bool isSpown)
    {
        this.isSpown = isSpown;
    }
}
