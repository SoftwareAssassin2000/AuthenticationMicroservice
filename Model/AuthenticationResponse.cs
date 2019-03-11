using System;

namespace AuthenticationService.Model
{
    public class AuthenticationResponse
    {
        public bool WasSuccessful { get; set; }
        public string Message { get; set; }

        public AuthenticatedSession Session { get; set; }
        public AuthenticatedUser User { get; set; }
    }
    public class AuthenticatedSession
    {
        public string Token { get; set; }
        public string ExpiresAt { get; set; }
    }
    public class AuthenticatedUser
    {
        public string Id { get; set; }
        public DateTime PasswordChanged { get; set; }
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Locale { get; set; }
        public string TimeZone { get; set; }
    }
}
