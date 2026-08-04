namespace Ruptura.Application.Common;

public static class ErrorCodes
{
    public static class Auth
    {
        public const string InvalidCredentials = "Auth.InvalidCredentials";
        public const string EmailAlreadyExists = "Auth.EmailAlreadyExists";
        public const string InvalidInviteCode = "Auth.InvalidInviteCode";
        public const string InvalidRefreshToken = "Auth.InvalidRefreshToken";
        public const string UserNotFound = "Auth.UserNotFound";
    }

    public static class Invite
    {
        public const string NotFound = "Invite.NotFound";
        public const string AlreadyUsed = "Invite.AlreadyUsed";
        public const string Expired = "Invite.Expired";
        public const string Forbidden = "Invite.Forbidden";
    }
}
