using System;
using System.IO;

namespace Picturepark.SDK.V1.AzureBlob;

/// <summary>
/// Designates an item to be uploaded to an ingest container
/// </summary>
public class IngestUploadItem
{
    /// <summary>
    /// Designates an item to be uploaded to an ingest container
    /// </summary>
    /// <param name="fileName">FileName to assign to the file upon import</param>
    /// <param name="openStream">Function to create read stream</param>
    /// <param name="leaveStreamOpen">If true, stream will not be disposed after upload</param>
    public IngestUploadItem(string fileName, Func<Stream> openStream, bool leaveStreamOpen = false)
    {
        FileName = fileName;
        OpenStream = openStream;
        LeaveStreamOpen = leaveStreamOpen;
    }

    /// <summary>FileName to assign to the file upon import</summary>
    public string FileName { get; }

    /// <summary>Function to create read stream</summary>
    public Func<Stream> OpenStream { get; }

    /// <summary>If true, stream will not be disposed after upload</summary>
    public bool LeaveStreamOpen { get; }

    public void Deconstruct(out string fileName, out Func<Stream> openStream, out bool leaveStreamOpen)
    {
        fileName = FileName;
        openStream = OpenStream;
        leaveStreamOpen = LeaveStreamOpen;
    }
}
