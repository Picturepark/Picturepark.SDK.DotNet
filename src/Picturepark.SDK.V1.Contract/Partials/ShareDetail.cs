using System;

namespace Picturepark.SDK.V1.Contract
{
    public partial class ShareDetail
    {
        public ShareBasicUpdateRequest AsBasicUpdateRequest(Action<ShareBasicUpdateRequest> update = null)
            => AsUpdateRequest(update);

        public ShareEmbedUpdateRequest AsEmbedUpdateRequest(Action<ShareEmbedUpdateRequest> update = null)
            => AsUpdateRequest(update);

        private T AsUpdateRequest<T>(Action<T> update = null)
            where T : ShareBaseUpdateRequest, new()
        {
            var result = new T
            {
                Description = Description,
                ExpirationDate = ExpirationDate,
                LayerSchemaIds = LayerSchemaIds,
                Name = Name,
                OutputAccess = OutputAccess,
                Contents = Contents
            };

            update?.Invoke(result);

            return result;
        }
    }
}