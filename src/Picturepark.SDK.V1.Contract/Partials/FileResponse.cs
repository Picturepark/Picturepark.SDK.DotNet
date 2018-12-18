using System;

namespace Picturepark.SDK.V1.Contract
{
    public partial class FileResponse
    {
        public IDisposable GetResponse()
        {
            return _response;
        }
    }
}
