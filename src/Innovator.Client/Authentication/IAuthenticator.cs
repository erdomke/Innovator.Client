using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  internal interface IAuthenticator
  {
    IPromise<IEnumerable<KeyValuePair<string, string>>> GetAuthHeaders(bool async);

    ExplicitHashCredentials HashCredentials(ICredentials credentials);

    IPromise<ExplicitHashCredentials> HashCredentials(ICredentials credentials, bool async);
  }
}
