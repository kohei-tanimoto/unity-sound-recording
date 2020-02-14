using System;
using System.IO;
using UnityEngine;

public class RecordingEndButtonScript : MonoBehaviour
{
    private AudioSource audioSource;
    private string micName = null; // マイクデバイスの名前

    void Start()
    {
        //マイクデバイスを探す
        foreach (string device in Microphone.devices)
        {
            micName = device;
        }
        // 予めタグ設定しておいたAudioSourceを取得しておく
        audioSource = GameObject.FindGameObjectWithTag("RecordingAudioSource").GetComponent<AudioSource>();
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
        RecordingStartButtonScript.recordedFlg = true;

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
        string fileFullPath = Path.Combine(Application.persistentDataPath, "audiofile_" + dtStr + ".wav");
        if (!SaveAudioSourceWav.Save(fileFullPath, audioSource.clip))
        {
            Debug.Log("録音ファイルを保存することができませんでした");
        }
    }
}
