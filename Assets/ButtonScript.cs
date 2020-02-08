using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    public AudioSource audioSource;

    const int samplingFrequency = 44100; // サンプリングレート
    const int maxTime_s = 3599; // 最大録音時間[s] ※60分未満
    string micName = "null"; // マイクデバイスの名前

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

        // 録音を停止
        Microphone.End(deviceName: micName);

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
        if (!SaveAudioSourceWav.Save("audiofile.wav", audioSource.clip))
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
