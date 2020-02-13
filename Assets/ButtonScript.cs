using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    public AudioSource audioSource;

    const int samplingFrequency = 44100; // サンプリングレート
    const int maxTime_s = 3599; // 最大録音時間[s] ※60分未満
    string micName = "null"; // マイクデバイスの名前
    bool recordedFlg = false; // 録音済みかどうかのフラグ

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
        if (audioSource.clip != null)
        {
            if (!recordedFlg && !Microphone.IsRecording(deviceName: micName))
            {
                Debug.Log("録音自動終了！");
                recordedFlg = true;

                // Wavファイルへ保存
                // ※こちらにアラート表示の処理を入れて頂くのもよし
                DateTime dt = DateTime.Now;
                string dtStr = dt.ToString("yyyyMMddHHmmss");
                if (!SaveAudioSourceWav.Save("audiofile_" + dtStr + ".wav", audioSource.clip))
                {
                    Debug.Log("録音ファイルを保存することができませんでした");
                }
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

    /// <summary>
    /// 録音を停止する
    /// </summary>
    public void EndRecording()
    {
        if (!Microphone.IsRecording(deviceName: micName))
        {
            Debug.Log("録音が開始されていません");
            return;
        }

        Debug.Log("録音を停止します");

        // マイクの録音位置を取得
        int position = Microphone.GetPosition(micName);

        // 録音を停止し録音済みフラグを立てる
        Microphone.End(deviceName: micName);
        recordedFlg = true;

        // 音声データ一時退避用の領域を確保し、audioClipからのデータを格納
        float[] soundData = new float[audioSource.clip.samples * audioSource.clip.channels];
        audioSource.clip.GetData(soundData, 0);

        // 新しい音声データ領域を確保し、positonの分だけ格納できるサイズにする。
        float[] newData = new float[position * audioSource.clip.channels];

        // positionの分だけデータをコピー
        for (int i = 0; i < newData.Length; i++)
        {
            newData[i] = soundData[i];
        }

        // 新しいAudioClipのインスタンスを生成し、音声データをセット
        AudioClip newClip = AudioClip.Create(audioSource.clip.name, position, audioSource.clip.channels, audioSource.clip.frequency, false);
        newClip.SetData(newData, 0);
        AudioClip.Destroy(audioSource.clip);
        audioSource.clip = newClip;

        // Wavファイルへ保存
        DateTime dt = DateTime.Now;
        string dtStr = dt.ToString("yyyyMMddHHmmss");
        if (!SaveAudioSourceWav.Save("audiofile_" + dtStr + ".wav", audioSource.clip))
        {
            Debug.Log("録音ファイルを保存することができませんでした");
        }
    }

    /// <summary>
    /// 録音音源を再生する
    /// </summary>
    public void PlaySound()
    {
        if (Microphone.IsRecording(deviceName: micName) == true)
        {
            Debug.Log("まだ録音中です！！！");
            return;
        }

        Debug.Log("音源を再生します");
        audioSource.Play();
    }
}
