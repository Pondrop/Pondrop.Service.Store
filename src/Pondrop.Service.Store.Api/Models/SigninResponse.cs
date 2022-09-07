using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Store.Api.Models;
public class SigninResponse
{
    public string AccessToken { get; set; }

    public SigninResponse(string token) => AccessToken = token;
}
