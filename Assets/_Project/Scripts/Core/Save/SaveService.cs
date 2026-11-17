using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using VContainer.Unity;

// 저장 파일(JSON) 담당. 무엇을 저장할지는 모르고, 등록된 ISaveable들에게 읽고 쓰게 한다.
// Steam Cloud가 그대로 동기화할 수 있도록 persistentDataPath의 파일 하나에 저장한다.
public sealed class SaveService : ISaveService, IInitializable, IDisposable
{
    private const string FILE_NAME = "save.json";

    private readonly IReadOnlyList<ISaveable> _saveables;
    private readonly string _path;
    private readonly string _tempPath;
    private bool _isSaving;
    private bool _hasPendingSave;

    public SaveService(IReadOnlyList<ISaveable> saveables)
    {
        _saveables = saveables;
        _path = Path.Combine(Application.persistentDataPath, FILE_NAME);
        _tempPath = _path + ".tmp";
    }

    // 컨테이너가 만들어지자마자(게임 씬보다 먼저) 불러온다.
    public void Initialize()
    {
        SaveData data = ReadOrDefault();
        foreach (ISaveable saveable in _saveables)
        {
            saveable.Load(data);
        }

        // 저장할 틈 없이 꺼지는 경우(Alt+F4 등)를 위해 종료 직전에 동기로 한 번 더 저장한다.
        Application.quitting += SaveImmediately;
    }

    public void Dispose()
    {
        Application.quitting -= SaveImmediately;
    }

    public async UniTask SaveAsync()
    {
        if (_isSaving)
        {
            _hasPendingSave = true;
            return;
        }

        _isSaving = true;
        try
        {
            do
            {
                _hasPendingSave = false;
                string json = Serialize();
                await UniTask.RunOnThreadPool(() => WriteAtomically(json));
            }
            while (_hasPendingSave);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveService] 저장 실패: {e.Message}");
        }
        finally
        {
            _isSaving = false;
        }
    }

    public void ResetAll()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }

        var fresh = new SaveData();
        foreach (ISaveable saveable in _saveables)
        {
            saveable.Load(fresh);
        }
    }

    private void SaveImmediately()
    {
        try
        {
            WriteAtomically(Serialize());
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveService] 종료 시 저장 실패: {e.Message}");
        }
    }

    // 상태 수집은 메인 스레드에서 한다(ISaveable이 Unity 객체를 읽을 수 있으므로).
    private string Serialize()
    {
        var data = new SaveData();
        foreach (ISaveable saveable in _saveables)
        {
            saveable.Write(data);
        }

        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }

    // 임시 파일에 다 쓴 뒤 바꿔 끼운다. 쓰는 도중 꺼져도 기존 세이브는 멀쩡하다.
    private void WriteAtomically(string json)
    {
        File.WriteAllText(_tempPath, json, Encoding.UTF8);
        if (File.Exists(_path))
        {
            File.Replace(_tempPath, _path, null);
        }
        else
        {
            File.Move(_tempPath, _path);
        }
    }

    private SaveData ReadOrDefault()
    {
        if (!File.Exists(_path))
        {
            return new SaveData();
        }

        try
        {
            SaveData data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(_path, Encoding.UTF8));
            return Migrate(data ?? new SaveData());
        }
        catch (Exception e)
        {
            // 깨진 파일은 지우지 않고 옆에 보관한 뒤 새로 시작한다(나중에 복구할 수 있게).
            Debug.LogError($"[SaveService] 세이브를 읽지 못해 새로 시작합니다: {e.Message}");
            File.Copy(_path, _path + ".broken", true);
            return new SaveData();
        }
    }

    // 옛 버전 세이브를 지금 형식으로 바꾼다. 버전을 올릴 때마다 여기에 단계를 추가한다.
    private static SaveData Migrate(SaveData data)
    {
        data.UpgradeLevels ??= new Dictionary<string, int>();
        data.Version = SaveData.CURRENT_VERSION;
        return data;
    }
}
