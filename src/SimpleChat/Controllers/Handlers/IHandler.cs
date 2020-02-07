using SimpleChat.Controllers.Core;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    public interface IHandler
    {
        IHandler Next { get; set; }

        Task<IAuthResult> HandleAsync(IContext context);
    }
}
