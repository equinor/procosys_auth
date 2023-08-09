namespace Equinor.ProCoSys.Auth.Authorization
{
    public interface IRestrictionRolesChecker
    {
        bool HasCurrentUserExplicitNoRestrictions();
        bool HasCurrentUserExplicitAccessToContent(string responsibleCode);
    }
}
