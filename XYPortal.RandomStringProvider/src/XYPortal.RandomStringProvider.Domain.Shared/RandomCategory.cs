using System;

namespace XYPortal.RandomStringProvider;

/// <summary>
/// 随机字符串字符类型枚举
/// </summary>
[Flags]
public enum RandomCategory : long
{
    /// <summary>
    /// 小写字母
    /// </summary>
    LowercaseLetters = 1 << 0,
    /// <summary>
    /// 大写字母
    /// </summary>
    UppercaseLetters = 1 << 1,
    /// <summary>
    /// 阿拉伯数字
    /// </summary>
    ArabicNumerals  = 1 << 2,
    /// <summary>
    /// 英文标点符号
    /// </summary>
    EnglishPunctuation  = 1 << 3,
    /// <summary>
    /// 中文大写数字
    /// </summary>
    ChineseCapitalNumbers  = 1 << 4,
    /// <summary>
    /// 天干
    /// </summary>
    HeavenlyStems = 1 << 5,
    /// <summary>
    /// 地支
    /// </summary>
    EarthlyBranches = 1 << 6,
    /// <summary>
    /// 八卦
    /// </summary>
    BaguaSymbols = 1 << 7,
    /// <summary>
    /// 六十四卦
    /// </summary>
    SixtyFourHexagrams  = 1 << 8,
    /// <summary>
    /// 杂项Unicode符号
    /// </summary>
    UnicodeMiscSymbols  = 1 << 9,
    /// <summary>
    /// 所有项目
    /// </summary>
    All = LowercaseLetters | UppercaseLetters | ArabicNumerals | EnglishPunctuation | ChineseCapitalNumbers | HeavenlyStems | EarthlyBranches | BaguaSymbols | SixtyFourHexagrams | UnicodeMiscSymbols,
    /// <summary>
    /// 密码选项
    /// </summary>
    PasswordOptions = LowercaseLetters | UppercaseLetters | ArabicNumerals | EnglishPunctuation
}