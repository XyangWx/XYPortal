using System;

namespace XYPortal.LinkBoard;

public static class LinkBoardConsts
{
    public const int CategoryNameMaxLength = 64;
    public const int CategoryDisplayNameMaxLength = 128;
    public const int CategoryDescriptionMaxLength = 512;
    public const int CategoryIconMaxLength = 256;

    public const int LinkTitleMaxLength = 256;
    public const int LinkUrlMaxLength = 2048;
    public const int LinkDescriptionMaxLength = 1024;
    public const int LinkIconMaxLength = 512;

    public const int ReviewCommentMaxLength = 512;

    /// <summary>
    /// Default category: General Links (通用链接)
    /// </summary>
    public static class DefaultCategory
    {
        public static readonly Guid Id = new Guid("00000000-0000-0000-0001-000000000001");
        public const string Name = "General";
        public const string DisplayName = "通用链接";
        public const string Description = "默认链接分类";
        public const string Icon = "fas fa-link";
    }
}
