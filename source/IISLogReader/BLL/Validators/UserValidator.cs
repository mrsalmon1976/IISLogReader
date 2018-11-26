using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IISLogReader.BLL.Repositories;

namespace IISLogReader.BLL.Validators
{
    public interface IUserValidator
    {
        ValidationResult Validate(UserModel model);
    }

    public class UserValidator : IUserValidator
    {
        private IUserRepository _userRepo;

        public UserValidator(IUserRepository userRepo)
        {
            this._userRepo = userRepo;
        }

        public ValidationResult Validate(UserModel model)
        {
            ValidationResult result = new ValidationResult();

            if (String.IsNullOrWhiteSpace(model.UserName))
            {
                result.Messages.Add("User name cannot be empty");
            }
            if (String.IsNullOrWhiteSpace(model.Password))
            {
                result.Messages.Add("Password cannot be empty");
            }
            if (String.IsNullOrWhiteSpace(model.Role))
            {
                result.Messages.Add("Role cannot be empty");
            }

            UserModel existinguser = _userRepo.GetByUserName(model.UserName);
            if (existinguser != null && existinguser.Id != model.Id) 
            {
                result.Messages.Add("A user with the supplied user name already exists.");
            }

            return result;
        }
    }
}
