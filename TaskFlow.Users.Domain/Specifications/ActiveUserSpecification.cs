using System.Linq.Expressions;
using TaskFlow.Users.Domain.Entities;

namespace TaskFlow.Users.Domain.Specifications;

public class ActiveUserSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.IsActive;
    }
}