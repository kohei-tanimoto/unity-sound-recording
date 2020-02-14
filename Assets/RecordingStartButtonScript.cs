using System;
using System.IO;
using UnityEngine;

public class RecordingStartButtonScript : MonoBehaviour
{
    private AudioSource audioSource;
    private const int samplingFrequency = 44100; // サンプリングレート
    private const int maxTime_s = 3599; // 最大録音時間[s] ※60分未満
    private string micName = null; // マイクデバイスの名前
    public static bool recordedFlg = false; // 録音済みかどうかのフラグ

    void Start()
    {
        //マイクデバイスを探す
        foreach (string device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
            micName = device;
        }
        // 予めタグ設定しておいたAudioSourceを取得しておく
        audioSource = GameObject.FindGameObjectWithTag("RecordingAudioSource").GetComponent<AudioSource>();
    }

    void Update()
    {
        // AudioClip存在 and 録音未完 and マイク録音中じゃない → 自動終了している
        if (audioSource.clip != null && !recordedFlg && !Microphone.IsRecording(deviceName: micName))
        {
            Debug.Log("録音自動終了！");
            recordedFlg = true;

            // Wavファイルへ保存
            // ※こちらにアラート表示の処理を入れて頂くのもよし
            DateTime dt = DateTime.Now;
            string dtStr = dt.ToString("yyyyMMddHHmmss");
            string fileFullPath = Path.Combine(Application.persistentDataPath, "audiofile_" + dtStr + ".wav");
            if (!SaveAudioSourceWav.Save(fileFullPath, audioSource.clip))
            {
                Debug.Log("録音ファイルを保存することができませんでした");
            }
        }
    }

    /// <summary>
    /// 録音を開始する
    /// </summary>
    public void StartRecording()
    {
        // マイク存在確認
        if (Microphone.devices.Length == 0)
        {
            Debug.Log("マイクが見つかりません");
            return;
        }

        // AudioClipを初期化
        audioSource.clip = null;
        recordedFlg = false;

        // 録音開始
        Debug.Log("録音を開始します");
        audioSource.clip = Microphone.Start(deviceName: micName, loop: false, lengthSec: maxTime_s, frequency: samplingFrequency);
    }
}
