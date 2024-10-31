using System;
using System.Collections.Generic;
using Picturepark.SDK.V1.Contract.Providers;

namespace Picturepark.SDK.V1.Contract.Attributes;

/// <summary>
/// Used to specify options for a field of type <see cref="FieldTreeView"/>
/// </summary>
/// <remarks>This field is not returned in metadata and can be used only with TreeView aggregations.</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PictureparkTreeViewAttribute : Attribute, IPictureparkAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PictureparkTreeViewAttribute"/> class.
    /// </summary>
    /// <param name="levelProvider"><see cref="ITreeViewLevelProvider"/> to obtain tree view level configuration</param>
    /// <exception cref="ArgumentException"><paramref name="levelProvider"/> is of wrong type</exception>
    public PictureparkTreeViewAttribute(Type levelProvider)
    {
        if (Activator.CreateInstance(levelProvider) is ITreeViewLevelProvider provider)
            Levels = provider.GetTreeLevels();
        else
            throw new ArgumentException($"The parameter {nameof(levelProvider)} is not of type {nameof(ITreeViewLevelProvider)}.");
    }

    /// <summary>
    /// Levels of TreeView field.
    /// </summary>
    public IReadOnlyList<TreeLevelItem> Levels { get; }
}
