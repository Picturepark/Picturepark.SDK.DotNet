using System.Collections.Generic;

namespace Picturepark.SDK.V1.Contract.Providers;

public interface ITreeViewLevelProvider
{
    IReadOnlyList<TreeLevelItem> GetTreeLevels();
}
