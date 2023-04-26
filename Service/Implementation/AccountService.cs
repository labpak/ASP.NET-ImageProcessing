using ImageProcessing.DAL;
using ImageProcessing.Models.Entity;
using ImageProcessing.Models.Enum;
using ImageProcessing.Models.Helpers;
using ImageProcessing.Models.Response;
using ImageProcessing.Models.ViewModels.Account;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _db;
        public AccountService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<BaseResponse<ClaimsIdentity>> Registration(RegistrationViewModel model)
        {
            try 
            { 
                var user = await _db.Users.FirstOrDefaultAsync(x => x.Name == model.Name);
                if (!user.Equals(null))
                {
                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Description = "Пользователь с таким именем уже существует"
                    };
                }

                user = new User
                {
                    Name = model.Name,
                    Role = ImageProcessing.Models.Enum.Role.User,
                    Password = HashPasswordHelper.HashPassowrd(model.Password)
                };

                await _db.AddAsync(user);
                await _db.SaveChangesAsync();


                return new BaseResponse<ClaimsIdentity>()
                {
                    Data = Authenticate(user),
                    Description = "Объект добавился",
                    StatusCode = StatusCode.OK
                };
            }
            catch(Exception ex) 
            {
                return new BaseResponse<ClaimsIdentity>()
                {
                    Description = ex.Message,
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<ClaimsIdentity>> Login(LoginViewModel model)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(x => x.Name == model.Name);
                if (user.Equals(null))
                {
                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Description = "Пользователь не найден"
                    };
                }

                if (!user.Password.Equals(HashPasswordHelper.HashPassowrd(model.Password)))
                {
                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Description = "Неверный пароль или логин"
                    };
                }
                var result = Authenticate(user);

                return new BaseResponse<ClaimsIdentity>()
                {
                    Data = result,
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ClaimsIdentity>()
                {
                    Description = ex.Message,
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<bool>> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(x => x.Name == model.Name);
                if (user.Equals(null))
                {
                    return new BaseResponse<bool>()
                    {
                        StatusCode = StatusCode.UserNotFound,
                        Description = "Пользователь не найден"
                    };
                }

                user.Password = HashPasswordHelper.HashPassowrd(model.NewPassword);
                _db.Users.Update(user);
                await _db.SaveChangesAsync();

                return new BaseResponse<bool>()
                {
                    Data = true,
                    StatusCode = StatusCode.OK,
                    Description = "Пароль обновлен"
                };

            }
            catch (Exception ex)
            {                
                return new BaseResponse<bool>()
                {
                    Description = ex.Message,
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }


        private ClaimsIdentity Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.ToString())
            };
            return new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        }
    }
}
