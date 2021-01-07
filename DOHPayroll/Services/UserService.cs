using DOHPayroll.Databases;
using DOHPayroll.Models.PostModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Generators;
using System.Transactions;

namespace DOHPayroll.Services
{
    public interface IUserService
    {
        Task<(bool, CookiesModel)> ValidateUserCredentialsAsync(string username, string password);

    }
    public class UserService : IUserService
    {
        public UserService() { }

        #region VALIDATE LOGIN
        public async Task<(bool, CookiesModel)> ValidateUserCredentialsAsync(string username, string password)
        {
            CookiesModel user = null;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return (false, user);

            user = await DTRDatabase.Instance.GetUserByUsername(username);

            if (user == null)
                return (false, user);

            else
            {
                return (true, user);
                /*user.Password = "";
                if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                    return (true, user);
                else
                    return (false, user);*/
            }
        }
        #endregion
    }
}
