using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication3
{
    public interface IAuthenticationService
    {
        AppUser Login(string username, string password);
    }
}
