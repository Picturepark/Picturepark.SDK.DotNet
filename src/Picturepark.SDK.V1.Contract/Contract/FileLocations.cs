using System;
using System.IO;

namespace Picturepark.SDK.V1.Contract
{
    /// <summary>
    /// Specifies the location where to upload the file from and optional filename if it should be renamed on upload
    /// </summary>
    public class FileLocations
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileLocations"/> class.
        /// </summary>
        /// <param name="absoluteSourcePath">Physical (absolute) source file path</param>
        /// <param name="fileNameOverride">
        /// Specify if you want to upload the file under a different filename than the source name.
        /// If a path is specified, only the filename will be used.
        /// </param>
        /// <param name="identifier">
        /// The identifier of the file for internal book keeping. Usually this is auto-generated.
        /// </param>
        public FileLocations(string absoluteSourcePath, string fileNameOverride = null, string identifier = null)
        {
            AbsoluteSourcePath = absoluteSourcePath ?? throw new ArgumentNullException(nameof(absoluteSourcePath));
            UploadAs = fileNameOverride ?? Path.GetFileName(absoluteSourcePath);
            Identifier = identifier ?? $"{Guid.NewGuid():N}";
        }

        /// <summary>
        /// Physical (absolute) source file path
        /// </summary>
        public string AbsoluteSourcePath { get; }

        /// <summary>
        /// The filename under which the file will be uploaded
        /// </summary>
        public string UploadAs { get; }

        /// <summary>
        /// The identifier of the file for internal book keeping
        /// Usually this is auto-generated.
        /// </summary>
        public string Identifier { get; }

        public static implicit operator FileLocations(string path)
        {
            return new FileLocations(path);
        }

        public override string ToString()
            => $"{AbsoluteSourcePath} => {UploadAs}";
    }
}