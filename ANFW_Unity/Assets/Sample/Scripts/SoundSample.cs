using ANFW;
using ANFW.Sound;
using UnityEngine;
using UnityEngine.InputSystem;

public class SoundSample : MonoBehaviour
{
    [SerializeField] private string _bgmAddress = "Sound/BGM/test";
    [SerializeField] private string _seAddress1 = "Sound/SE/test1";
    [SerializeField] private string _seAddress2 = "Sound/SE/test2";
    [SerializeField] private string _seAddress3 = "Sound/SE/test3";
    [SerializeField] private string _seAddress4 = "Sound/SE/test4";

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Space キー: BGM 再生
        if (keyboard.spaceKey.wasPressedThisFrame)
            EventBus.Emit(new PlayBGMEvent { Address = _bgmAddress });

        // X キー: BGM 停止
        if (keyboard.xKey.wasPressedThisFrame)
            EventBus.Emit(new StopBGMEvent());

        // 1〜4 キー: SE 再生（同時再生確認用）
        if (keyboard.digit1Key.wasPressedThisFrame)
            EventBus.Emit(new PlaySEEvent { Address = _seAddress1 });

        if (keyboard.digit2Key.wasPressedThisFrame)
            EventBus.Emit(new PlaySEEvent { Address = _seAddress2 });

        if (keyboard.digit3Key.wasPressedThisFrame)
            EventBus.Emit(new PlaySEEvent { Address = _seAddress3 });

        if (keyboard.digit4Key.wasPressedThisFrame)
            EventBus.Emit(new PlaySEEvent { Address = _seAddress4 });

        // 上下矢印キー: マスター音量
        if (keyboard.upArrowKey.wasPressedThisFrame)
            EventBus.Emit(new SetMasterVolumeEvent { Volume = 1.0f });

        if (keyboard.downArrowKey.wasPressedThisFrame)
            EventBus.Emit(new SetMasterVolumeEvent { Volume = 0.2f });
    }
}
