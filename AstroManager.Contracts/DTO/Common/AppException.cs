using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Model.DTO.Common
{
    public class AppException : Exception
    {
        public string UserFriendlyMessage { get; }

        public AppException(string userFriendlyMessage, string logMessage = null, Exception innerException = null)
            : base(logMessage ?? userFriendlyMessage, innerException)
        {
            UserFriendlyMessage = userFriendlyMessage;
        }
    }
}
