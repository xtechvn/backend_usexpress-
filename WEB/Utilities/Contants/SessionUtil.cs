using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Utilities.Contants
{
    public class SessionUtil
    {
        public List<Claim> GetCurrentUser()
        {
            var claims = new List<Claim>();
            //claims.Add(new Claim(ClaimTypes.NameIdentifier, model.Entity.Id.ToString()));
            //claims.Add(new Claim(ClaimTypes.Name, model.Entity.UserName));
            //claims.Add(new Claim(ClaimTypes.Email, model.Entity.Email));
            //claims.Add(new Claim(ClaimTypes.Role, string.Join(",", model.RoleIdList)));
            return claims;
        }
    }
}
