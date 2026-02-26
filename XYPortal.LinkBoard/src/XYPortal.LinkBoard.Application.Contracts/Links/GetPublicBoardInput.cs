using System;

namespace XYPortal.LinkBoard.Links;

public class GetPublicBoardInput
{
    public Guid? CategoryId { get; set; }

    public int SkipCount { get; set; }

    public int MaxResultCount { get; set; }
}
