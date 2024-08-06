namespace Equinor.ProCoSys.Auth.Authentication;

public class MainApiAuthenticatorOptions
{
    public bool DisableProjectUserDataClaims { get; set; }
    public bool DisableRestrictionRoleUserDataClaims { get; set; }

    public required string MainApiScope { get; set; }
}
