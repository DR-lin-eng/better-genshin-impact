using System;

namespace BetterGenshinImpact.Model;

public enum HotKeyTypeEnum
{
    GlobalRegister, // 全局热键
    KeyboardMonitor, // 键盘监听
}

public static class HotKeyTypeEnumExtension
{
    public static string ToLocalizationKey(this HotKeyTypeEnum type)
    {
        return type switch
        {
            HotKeyTypeEnum.GlobalRegister => "hotkey.type.globalRegister",
            HotKeyTypeEnum.KeyboardMonitor => "hotkey.type.keyboardMonitor",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    public static string ToChineseName(this HotKeyTypeEnum type)
    {
        return type switch
        {
            HotKeyTypeEnum.GlobalRegister => "全局热键",
            HotKeyTypeEnum.KeyboardMonitor => "键鼠监听",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}
