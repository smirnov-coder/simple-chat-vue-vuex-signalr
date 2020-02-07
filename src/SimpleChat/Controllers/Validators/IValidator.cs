using SimpleChat.Controllers.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Validators
{
    public interface IValidator
    {
        bool Validate(IContext context, ICollection<string> errors);
    }
}
