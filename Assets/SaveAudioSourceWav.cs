using System;
using System.IO;
using UnityEngine;

public static class SaveAudioSourceWav {

    const int HEADER_SIZE = 44;

    /// <summary>
    /// AudioClipをWavファイルへ保存する
    /// </summary>
    /// <param name="filename">保存ファイル名</param>
    /// <param name="clip">保存するAudioClip</param>
    /// <returns>true: 保存成功 | false: 保存失敗</returns>
    public static bool Save(string filename, AudioClip clip)
    {
        if (!filename.ToLower().EndsWith(".wav"))
        {
            filename += ".wav";
        }

        var filepath = Path.Combine(Application.persistentDataPath, filename);

        Debug.Log("保存ファイルフルパス: " + filepath);

        // 保存先ディレクトリがない場合作成する
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        using (var fileStream = CreateEmpty(filepath))
        {
            ConvertAndWrite(fileStream, clip);
            WriteHeader(fileStream, clip);
        }

        return true;
    }

    /// <summary>
    /// 空ファイルを作成する
    /// </summary>
    /// <param name="filepath">保存先フルパス</param>
    /// <returns>保存時FileStream</returns>
    static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        // Header書き込み
        for(int i = 0; i < HEADER_SIZE; i++)
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="clip"></param>
    static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {
        var samples = new float[clip.samples];
        clip.GetData(samples, 0);

        // 変換は以下の2段階
        // ・float[] → Int16[]
        // ・Int16[] → Byte[]
        Int16[] intData = new Int16[samples.Length];

        // Int16変換のfloatは2ByteのためByte配列へは2倍にする
        Byte[] bytesData = new Byte[samples.Length * 2];

        // float → Int16へ変換用
        float rescaleFactor = 32767;
        for (int i = 0; i < samples.Length; i++) {
            intData[i] = (short) (samples[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    /// <summary>
    /// Headerへ書き込み
    /// </summary>
    /// <param name="fileStream">保存するFileStream</param>
    /// <param name="clip">保存するAudioClip</param>
    static void WriteHeader(FileStream fileStream, AudioClip clip) {

        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
        fileStream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort) (channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        fileStream.Write(subChunk2, 0, 4);

        fileStream.Close();
    }
}