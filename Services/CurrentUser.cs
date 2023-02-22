using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Newtonsoft.Json;

public static class CurrentUser {
    public static string GetDisplayName(this IPrincipal user)
    {
        var claim = ((ClaimsIdentity)user.Identity).FindFirst(f => f.Type == "userName");
        return claim == null ? null : claim.Value;
    }
    public static string GetUserId(this IPrincipal user)
    {
        var claim = ((ClaimsIdentity)user.Identity).FindFirst(f => f.Type == "userId");
        return claim == null ? null : claim.Value;
    }
    public static string GetEmail(this IPrincipal user)
    {
        var claim = ((ClaimsIdentity)user.Identity).FindFirst(f => f.Type == "email");
        return claim == null ? null : claim.Value;
    }
}