using Microsoft.AspNetCore.Identity;

namespace SolarWatch.Services.Authentication;

public class AuthService(UserManager<IdentityUser> userManager, ITokenService tokenService) : IAuthService
{
	public async Task<AuthResult> RegisterAsync(string email, string username, string password, string role)
	{
		var user = new IdentityUser { UserName = username, Email = email };
		var result = await userManager.CreateAsync( user
			, password);

		if (!result.Succeeded)
			return FailedRegistration(result, email, username);
		
		await userManager.AddToRoleAsync(user, role);
		return new AuthResult(true, email, username, "");
	}

	private static AuthResult FailedRegistration(IdentityResult result, string email, string username)
	{
		var authResult = new AuthResult(false, email, username, "");

		foreach (var error in result.Errors)
			authResult.ErrorMessages.Add(error.Code, error.Description);
		

		return authResult;
	}
	
	public async Task<AuthResult> LoginAsync(string email, string password)
	{
		var managedUser = await userManager.FindByEmailAsync(email);

		if (managedUser == null)
		{
			return InvalidEmail(email);
		}

		var isPasswordValid = await userManager.CheckPasswordAsync(managedUser, password);
		
		if (!isPasswordValid)
		{
			return InvalidPassword(email, managedUser.UserName);
		}

		var roles = await userManager.GetRolesAsync(managedUser);
		var accessToken = tokenService.CreateToken(managedUser,roles[0]);

		return new AuthResult(true, managedUser.Email, managedUser.UserName, accessToken);
	}

	private static AuthResult InvalidEmail(string email)
	{
		var result = new AuthResult(false, email, "", "");
		result.ErrorMessages.Add("Bad credentials", "Invalid email");
		return result;
	}

	private static AuthResult InvalidPassword(string email, string userName)
	{
		var result = new AuthResult(false, email, userName, "");
		result.ErrorMessages.Add("Bad credentials", "Invalid password");
		return result;
	}

}