using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System;
using YourDriver.JWT_Options;
using YourDriver.Model.AuthMangment;
using YourDriver.Model.DriversData;
using YourDriver.Model.PassengersData;
using YourDriver.Model;
using YourDriver.repository.Interfaces;
using YourDriver.Model.Db;
using YourDriver.Controllers.DTOS;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace YourDriver.AuthServices
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _UserManager;
        private readonly IOptions<JWT> _JwtOptions;
        private readonly IUnitOfWork _UnitOfWork;
        private readonly YourDriverDContext _Context;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwtOptions
            , IUnitOfWork unitOfWork, YourDriverDContext context, IConfiguration configuration
            )
        {
            _UserManager = userManager;
            _JwtOptions = jwtOptions;
            _UnitOfWork = unitOfWork;
            _Context = context;
            _configuration = configuration;
        }

        public async Task<AuthModel> Login(LoginDTO model)
        {
            var authModel = new AuthModel();
            var user = await _UserManager.FindByEmailAsync(model.Email);
            if (user == null || await _UserManager.CheckPasswordAsync(user, model.Password) == false)
            {
                return new AuthModel()
                {
                    Message = "Email Or Password Is Wrong"
                };
            }

            var JwtToken = await GenerateJWT(user);
            var refreshtoken = user.refreshtokens.FirstOrDefault(t => t.IsActive);

            if (refreshtoken == null)
            {
                var refreshToken = GenerateRefreshToken();
                user.refreshtokens.Add(refreshToken);
                await _UserManager.UpdateAsync(user);

                authModel.refreshToken = refreshToken.Token;
                authModel.refreshTokenExpiration = refreshToken.ExpireOn;
            }
            else
            {
                authModel.refreshToken = refreshtoken.Token;
                authModel.refreshTokenExpiration = refreshtoken.ExpireOn;
            }

            var roles = await _UserManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                if (role == "Driver")
                {
                    var driver = await _Context.drivers.SingleOrDefaultAsync(d => d.UserName == user.UserName);
                    driver.IsLogged = true;
                    _Context.SaveChanges();
                }
            }

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(JwtToken);
            authModel.TokenExpiration = JwtToken.ValidTo;
            authModel.UserName = user.UserName;
            authModel.Roles = roles.ToList();

            return authModel;

        }

        public async Task<AuthModel> RegiserDriver(DriverRegister newDriver)
        {

            if (await _UserManager.FindByEmailAsync(newDriver.Email) != null)
                return new AuthModel()
                {
                    Message = "Email Already Registered"
                };

            if (await _UserManager.FindByNameAsync(newDriver.UserName) != null)
                return new AuthModel()
                {
                    Message = "Username Already Registered"
                };

            ApplicationUser driver = new ApplicationUser();
            driver.Email = newDriver.Email;
            driver.UserName = newDriver.UserName;
            driver.FirstName = newDriver.FirstName;
            driver.LastName = newDriver.LastName;
            driver.PhoneNumber = newDriver.PhoneNumber;

            var res = await _UserManager.CreateAsync(driver, newDriver.Password);
            var errors = "";
            if (!res.Succeeded)
            {
                foreach (var item in res.Errors)
                {
                    errors += item.Description;
                    errors += " , ";
                }
                return new AuthModel()
                {
                    Message = errors
                };
            }

            await _UserManager.AddToRoleAsync(driver, "Driver");

            var JWtToken = await GenerateJWT(driver);
            var refreshToken = GenerateRefreshToken();

            driver.refreshtokens.Add(refreshToken);
            await _UserManager.UpdateAsync(driver);

            //Add in Drivers Table 

            await _UnitOfWork.drivers.AddAsync(new Driver
            {
                AppUserId = driver.Id,
                Email = driver.Email,
                UserName = driver.UserName,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
                PhoneNumber = driver.PhoneNumber,
                Password = newDriver.Password,
                LicenseNumber = newDriver.LicenseNumber,
                FavAreas = newDriver.FavAreas,
                IsLogged = true

            });
            _UnitOfWork.complete();

            return new AuthModel()
            {
                UserName = driver.UserName,
                Roles = new List<string> { "Driver" },
                Token = new JwtSecurityTokenHandler().WriteToken(JWtToken),
                TokenExpiration = JWtToken.ValidTo,
                IsAuthenticated = true,
                refreshToken = refreshToken.Token,
                refreshTokenExpiration = refreshToken.ExpireOn

            };


        }

        public async Task<AuthModel> RegiserPassenger(PassengerRegister newPassenger)
        {
            if (await _UserManager.FindByEmailAsync(newPassenger.Email) != null)
                return new AuthModel()
                {
                    Message = "Email Already Registered"
                };

            if (await _UserManager.FindByNameAsync(newPassenger.UserName) != null)
                return new AuthModel()
                {
                    Message = "Username Already Registered"
                };

            ApplicationUser passenger = new ApplicationUser();
            passenger.Email = newPassenger.Email;
            passenger.UserName = newPassenger.UserName;
            passenger.FirstName = newPassenger.FirstName;
            passenger.LastName = newPassenger.LastName;
            passenger.PhoneNumber = newPassenger.PhoneNumber;

            var res = await _UserManager.CreateAsync(passenger, newPassenger.Password);
            var errors = "";
            if (!res.Succeeded)
            {
                foreach (var item in res.Errors)
                {
                    errors += item.Description;
                    errors += " , ";
                }
                return new AuthModel()
                {
                    Message = errors
                };
            }


            await _UserManager.AddToRoleAsync(passenger, "Passenger");

            var JWtToken = await GenerateJWT(passenger);
            var refreshToken = GenerateRefreshToken();

            passenger.refreshtokens.Add(refreshToken);
            await _UserManager.UpdateAsync(passenger);

            await _UnitOfWork.passengers.AddAsync(new Passenger
            {
                AppUserId = passenger.Id,
                Email = passenger.Email,
                UserName = passenger.UserName,
                FirstName = passenger.FirstName,
                LastName = passenger.LastName,
                PhoneNumber = passenger.PhoneNumber,
                Password = newPassenger.Password,
            });
            _UnitOfWork.complete();

            return new AuthModel()
            {
                UserName = passenger.UserName,
                Roles = new List<string> { "Passenger" },
                Token = new JwtSecurityTokenHandler().WriteToken(JWtToken),
                TokenExpiration = JWtToken.ValidTo,
                IsAuthenticated = true,
                refreshToken = refreshToken.Token,
                refreshTokenExpiration = refreshToken.ExpireOn

            };


        }

        public async Task<AuthModel> RefreshToken(string userToken)
        {
            var user = await _UserManager.Users.SingleOrDefaultAsync(u => u.refreshtokens.Any(t => t.Token == userToken));
            if (user == null)
            {
                return new AuthModel()
                {
                    Message = "Invalid Token"
                };
            }

            var token = user.refreshtokens.Single(t => t.Token == userToken);
            if (!token.IsActive)
            {
                return new AuthModel()
                {
                    Message = "Refresh Token Is Not Active"
                };
            }

            token.RevokedON = DateTime.UtcNow;

            var JwtToken = await GenerateJWT(user);
            var refreshToken = GenerateRefreshToken();
            user.refreshtokens.Add(refreshToken);
            await _UserManager.UpdateAsync(user);

            var roles = await _UserManager.GetRolesAsync(user);

            return new AuthModel()
            {
                IsAuthenticated = true,
                refreshToken = refreshToken.Token,
                refreshTokenExpiration = refreshToken.ExpireOn,
                Token = new JwtSecurityTokenHandler().WriteToken(JwtToken),
                TokenExpiration = JwtToken.ValidTo,
                UserName = user.UserName,
                Roles = roles.ToList(),
            };

        }

        public async Task<bool> RevokeRefreshToken(string userToken)
        {
            var user = await _UserManager.Users.SingleOrDefaultAsync(u => u.refreshtokens.Any(t => t.Token == userToken));
            if (user == null)
                return false;

            var token = user.refreshtokens.Single(t => t.Token == userToken);
            if (!token.IsActive)
                return false;

            token.RevokedON = DateTime.UtcNow;
            await _UserManager.UpdateAsync(user);

            return true;

        }

        public async Task<DriverUpdateModel> UpdateDriver(DriverUpdateModel newDriver, string driverId)
        {
            var driverDb = await _UserManager.FindByIdAsync(driverId);

            //username duplicate check 
            var usernameDuplicate = await _UserManager.FindByNameAsync(newDriver.UserName) ?? null;
            if (usernameDuplicate != null && driverDb.UserName != usernameDuplicate.UserName)
            {
                return new DriverUpdateModel
                {
                    Message = "Username Already Registered"
                };
            }

            //Email duplicate check
            var EmailDuplicate = await _UserManager.FindByEmailAsync(newDriver.Email) ?? null;
            if (EmailDuplicate != null && driverDb.Email != EmailDuplicate.Email)
            {
                return new DriverUpdateModel
                {
                    Message = "Email Already Registered"

                };
            }

            //password check
            var currDriver = await _UnitOfWork.drivers.GetByAppIDAsync(d => d.AppUserId == driverId);
            var currPassword = currDriver.Password;
            var res = await _UserManager.ChangePasswordAsync(driverDb, currPassword, newDriver.Password);
            if (!res.Succeeded)
            {
                var errors = "";
                foreach (var item in res.Errors)
                {
                    errors += item.Description;
                    errors += " , ";
                }
                return new DriverUpdateModel
                {
                    Message = errors
                };
            }

            driverDb.Email = newDriver.Email;
            driverDb.FirstName = newDriver.FirstName;
            driverDb.LastName = newDriver.LastName;
            driverDb.PhoneNumber = newDriver.PhoneNumber;
            driverDb.UserName = newDriver.UserName;
            driverDb.NormalizedEmail = newDriver.Email.ToUpper();
            driverDb.NormalizedUserName = newDriver.UserName.ToUpper();

            return new DriverUpdateModel
            {
                UserName = newDriver.UserName,
                Email = newDriver.Email,
                FirstName = newDriver.FirstName,
                FavAreas = newDriver.FavAreas,
                LicenseNumber = newDriver.LicenseNumber,
                LastName = newDriver.LastName,
                Password = newDriver.Password,
                PhoneNumber = newDriver.PhoneNumber,
            };

        }

        public async Task<PassengerUpdateModel> UpdatePassenger(PassengerUpdateModel newPassenger, string passengerId)
        {
            var passengerDb = await _UserManager.FindByIdAsync(passengerId);

            //username duplicate check 
            var usernameDuplicate = await _UserManager.FindByNameAsync(newPassenger.UserName) ?? null;
            if (usernameDuplicate != null && passengerDb.UserName != usernameDuplicate.UserName)
            {
                return new PassengerUpdateModel
                {
                    Message = "Username Already Registered"
                };
            }

            //Email duplicate check
            var EmailDuplicate = await _UserManager.FindByEmailAsync(newPassenger.Email) ?? null;
            if (EmailDuplicate != null && passengerDb.Email != EmailDuplicate.Email)
            {
                return new PassengerUpdateModel
                {
                    Message = "Email Already Registered"

                };
            }

            //password check
            var currPassenger = await _UnitOfWork.passengers.GetByAppIDAsync(d => d.AppUserId == passengerId);
            var currPassword = currPassenger.Password;
            var res = await _UserManager.ChangePasswordAsync(passengerDb, currPassword, newPassenger.Password);
            if (!res.Succeeded)
            {
                var errors = "";
                foreach (var item in res.Errors)
                {
                    errors += item.Description;
                    errors += " , ";
                }
                return new PassengerUpdateModel
                {
                    Message = errors
                };
            }

            //mapping values 
            passengerDb.Email = newPassenger.Email;
            passengerDb.FirstName = newPassenger.FirstName;
            passengerDb.LastName = newPassenger.LastName;
            passengerDb.PhoneNumber = newPassenger.PhoneNumber;
            passengerDb.UserName = newPassenger.UserName;
            passengerDb.NormalizedEmail = newPassenger.Email.ToUpper();
            passengerDb.NormalizedUserName = newPassenger.UserName.ToUpper();

            return new PassengerUpdateModel
            {
                Email = newPassenger.Email,
                FirstName = newPassenger.FirstName,
                LastName = newPassenger.LastName,
                PhoneNumber = newPassenger.PhoneNumber,
                UserName = newPassenger.UserName,
                Password = newPassenger.Password,

            };

        }


        private async Task<JwtSecurityToken> GenerateJWT(ApplicationUser user)
        {
            var userClaims = await _UserManager.GetClaimsAsync(user);
            var userRoles = await _UserManager.GetRolesAsync(user);
            List<Claim> roleList = new List<Claim>();

            foreach (var role in userRoles)
            {
                roleList.Add(new Claim(ClaimTypes.Role, role));
            }

            var myClaims = new[]{
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name , user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString())
            }.Union(roleList)
            .Union(userClaims);

            var symKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var signingCredentials = new SigningCredentials(symKey, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
         
                issuer: _configuration["JWT:ValidIssure"],
                audience: _configuration["JWT:Validaudience"],
                expires: DateTime.Now.AddDays(double.Parse(_configuration["JWT:DurationInDays"])),
                claims: myClaims


                );

            return jwtToken;

        }

        private RefreshToken GenerateRefreshToken()
        {
            string token = Guid.NewGuid().ToString();
            return new RefreshToken()
            {
                CreatedOn = DateTime.UtcNow,
                ExpireOn = DateTime.UtcNow.AddDays(10),
                Token = token
            };

        }

        public async Task<bool> DeleteUser(string name)
        {
            var user = await _UserManager.FindByNameAsync(name);
            await _UserManager.DeleteAsync(user);
            _Context.SaveChanges();
            return true;

        }


    }
}
