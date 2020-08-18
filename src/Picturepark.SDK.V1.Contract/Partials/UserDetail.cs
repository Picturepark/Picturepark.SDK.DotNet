using System.Linq;

namespace Picturepark.SDK.V1.Contract
{
    /// <summary>
    /// Details of a user
    /// </summary>
    public partial class UserDetail
    {
        /// <summary>
        /// Creates an update request for this user.
        /// </summary>
        /// <returns>An update request with all the same data as this user instance</returns>
        public UserUpdateRequest AsUpdateRequest()
        {
            return new UserUpdateRequest
            {
                Id = Id,
                FirstName = FirstName,
                LastName = LastName,
                EmailAddress = EmailAddress,
                Address = Address,
                Comment = Comment,
                LanguageCode = LanguageCode,
                UserRoles = UserRoles.Select(a => a.UserRole).ToArray(),
                IdentityProviderId = IdentityProviderId
            };
        }
    }
}