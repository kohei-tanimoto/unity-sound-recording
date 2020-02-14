using UnityEngine;

public class PlayButtonScript : MonoBehaviour
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
