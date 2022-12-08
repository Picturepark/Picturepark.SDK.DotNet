using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.FluentAssertions
{
    public class UserAssertions : ReferenceTypeAssertions<User, UserAssertions>
    {
        public UserAssertions(User instance) : base(instance)
        {
        }

        protected override string Identifier => "user";

        public AndConstraint<UserAssertions> BeResolved(string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(!string.IsNullOrEmpty(Subject.Id))
                .FailWith($"Expected {nameof(Subject.Id)} not to be null or empty, but found {{0}}", Subject.Id)
                .Then
                .ForCondition(!string.IsNullOrEmpty(Subject.EmailAddress))
                .FailWith($"Expected {nameof(Subject.EmailAddress)} not to be null or empty, but found {{0}}", Subject.EmailAddress)
                .Then
                .ForCondition(!string.IsNullOrEmpty(Subject.FirstName))
                .FailWith($"Expected {nameof(Subject.FirstName)} not to be null or empty, but found {{0}}", Subject.FirstName)
                .Then
                .ForCondition(!string.IsNullOrEmpty(Subject.LastName))
                .FailWith($"Expected {nameof(Subject.LastName)} not to be null or empty, but found {{0}}", Subject.LastName);

            return new AndConstraint<UserAssertions>(this);
        }
    }
}
