﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IOutputFormatClient
    {
        Task<ICollection<OutputFormat>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}