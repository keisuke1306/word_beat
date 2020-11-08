using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameAreaManager : MonoBehaviour
{
    // Music
    public AudioClip[] bgmAudioClips;
    public AudioSource bgmAudioSource;
    public AudioClip[] seAudioClips;
    public AudioSource seAudioSource;

    // スクリプト
    private SpownNodeController spownNodeController;

    // テンションゲージ
    private int tensionPoint = 0;

    // Params
    private float startGameTime;
    private bool isSpeedUp = false;
    private int speedUpThreshold = 15;
    private bool isBlackMaterial = false;
    private int blackMaterialThreshold = 30;
    private bool isGameEnd = false;
    private int tensionMaxPoint = 100;
    private int gameEndTime = 120;

    // Start is called before the first frame update
    void Start()
    {
        // Params
        startGameTime = Time.time;

        // BGM Play
        int audioIndex = Random.Range(0, bgmAudioClips.Length);
        bgmAudioSource.clip = bgmAudioClips[audioIndex];
        bgmAudioSource.Play();

        // SpownNodeController
        GameObject obj = GameObject.Find("SpownNode");
        spownNodeController = obj.GetComponent<SpownNodeController>();
    }

    void FixedUpdate()
    {
        // Speed Up
        if (!isSpeedUp)
        {
            if (tensionPoint > speedUpThreshold)
            {
                spownNodeController.SetSpownSpacing(1f);
                PlayAudio(seAudioClips[2]);
                isSpeedUp = true;
            }
        }

        // マテリアル色消し
        // Speed Up
        if (!isBlackMaterial)
        {
            if (tensionPoint > blackMaterialThreshold)
            {
                spownNodeController.SetSpownSpacing(0.65f);
                spownNodeController.SetDisplayMaterial(false);
                PlayAudio(seAudioClips[3]);
                isBlackMaterial = true;
            }
        }

        // Game End
        if (!isGameEnd)
        {
            // Tension MAX
            if (tensionPoint > tensionMaxPoint)
            {
                spownNodeController.SetSpown(false);
                isGameEnd = true;

                StartCoroutine(GameClear(true));
            }

            // Time is over
            if ((Time.time - startGameTime) > gameEndTime)
            {
                spownNodeController.SetSpown(false);
                isGameEnd = true;

                StartCoroutine(GameClear(false));
            }
        }
    }

    /**
     * SE再生
     * @param string
     */
    public void PlaySE(string nodeType)
    {
        int nodeTypeIndex = 0;
        switch (nodeType) {
            case "Positive":
                nodeTypeIndex = 0;
                break;
            case "Negative":
                nodeTypeIndex = 1;
                break;
        }
        PlayAudio(seAudioClips[nodeTypeIndex]);
    }

    /**
     * テンションポイントを1追加
     */
    public void tensionPointIncrement()
    {
        this.tensionPoint += 1;
    }

    /**
     * テンションポイントを1減少
     */
    public void tensionPointDecrement()
    {
        this.tensionPoint -= 1;
    }

    /**
     * 音再生
     */
    public void PlayAudio(AudioClip clip)
    {
        seAudioSource.clip = clip;
        seAudioSource.Play();
    }

    /**
     * ゲームクリア処理
     * @bool
     */
    IEnumerator GameClear(bool isClear = false)
    {
        if (isClear) {

            yield return new WaitForSeconds(4);

            PlayAudio(seAudioClips[4]);

        }

        StartCoroutine("GameEnd");
    }

    /**
     * ゲーム終了処理
     */
    IEnumerator GameEnd()
    {
        // 3秒停止
        yield return new WaitForSeconds(3);

        PlayAudio(seAudioClips[5]);

        yield return new WaitForSeconds(2);

        SceneManager.LoadScene("Start");
    }
}
