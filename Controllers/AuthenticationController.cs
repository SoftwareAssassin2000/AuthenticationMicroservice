using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

using AuthenticationService.Model;

namespace AuthenticationService.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AuthenticationController : Controller
    {
        private IOptions<OktaSettings> settings;
        public AuthenticationController(IOptions<OktaSettings> settings)
        {
            this.settings = settings;
        }

        [HttpPost]
        public async Task<AuthenticationResponse> Authenticate(Credentials input)
        {
            var ret = new AuthenticationResponse();

            AuthenticatedSession session = null;
            AuthenticatedUser user = null;

            //generate URL for service call using your configured Okta Domain
            string url = string.Format("{0}/api/v1/authn", this.settings.Value.Domain);

            //build the package we're going to send to Okta
            var data = new OktaAuthenticationRequest() { username = input.PublicKey, password = input.PrivateKey };

            //serialize input as json
            var json = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            //create HttpClient to communicate with Okta's web service
            using (HttpClient client = new HttpClient())
            {
                //Set the API key
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("SSWS", this.settings.Value.ApiToken);

                //Post the json data to Okta's web service
                using (HttpResponseMessage res = await client.PostAsync(url, json))

                //Get the response from the server
                using (HttpContent content = res.Content)
                {
                    //get json string from the response
                    var responseJson = await content.ReadAsStringAsync();
                    Console.WriteLine(responseJson);

                    //deserialize json into complex object
                    dynamic responseObj = JsonConvert.DeserializeObject(responseJson);

                    //determine if the returned status is success
                    if (responseObj.status == "SUCCESS")
                    {
                        //get session data
                        session = new AuthenticatedSession()
                        {
                            Token = responseObj.sessionToken,
                            ExpiresAt = responseObj.expiresAt
                        };

                        //get user data
                        user = new AuthenticatedUser()
                        {
                            Id = responseObj._embedded.user.id,
                            Login = responseObj._embedded.user.login,
                            Locale = responseObj._embedded.user.locale,
                            TimeZone = responseObj._embedded.user.timeZone,
                            FirstName = responseObj._embedded.user.firstName,
                            LastName = responseObj._embedded.user.lastName
                        };
                    }
                }
            }

            //return results of the operation packaged in a AuthenticationResponse object
            var wasSuccess = session != null && user != null;
            return new AuthenticationResponse()
            {
                WasSuccessful = wasSuccess,
                Message = wasSuccess ? "Success" : "Invalid username and password",

                Session = session,
                User = user
            };
        }
    }
}
      
