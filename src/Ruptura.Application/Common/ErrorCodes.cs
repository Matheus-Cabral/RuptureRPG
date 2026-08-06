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

    public static class Campaign
    {
        public const string NotFound = "Campaign.NotFound";
        public const string PlayerNotInRoster = "Campaign.PlayerNotInRoster";
        public const string AlreadyMember = "Campaign.AlreadyMember";
    }

    public static class Catalog
    {
        public const string NotFound = "Catalog.NotFound";
        public const string InvalidType = "Catalog.InvalidType";
        public const string AlreadyExists = "Catalog.AlreadyExists";
        public const string CannotModifyGlobalEntry = "Catalog.CannotModifyGlobalEntry";
        public const string AlreadyArchived = "Catalog.AlreadyArchived";
    }

    public static class CharacterSheet
    {
        public const string NotFound = "CharacterSheet.NotFound";
        public const string PlayerNotMember = "CharacterSheet.PlayerNotMember";
        public const string AlreadyHasAliveCharacter = "CharacterSheet.AlreadyHasAliveCharacter";
        public const string OnlyGameMasterCanChangeStatus = "CharacterSheet.OnlyGameMasterCanChangeStatus";
    }

    public static class Journal
    {
        public const string NotFound = "Journal.NotFound";
        public const string OnlyOwnerCanWrite = "Journal.OnlyOwnerCanWrite";
    }

    public static class Media
    {
        public const string InvalidEntityType = "Media.InvalidEntityType";
        public const string FileRequired = "Media.FileRequired";
        public const string FileTooLarge = "Media.FileTooLarge";
        public const string UnsupportedFileType = "Media.UnsupportedFileType";
        public const string TooManyImages = "Media.TooManyImages";
        public const string NotFound = "Media.NotFound";
    }

    public static class Notification
    {
        public const string NotFound = "Notification.NotFound";
        public const string NotPromotable = "Notification.NotPromotable";
    }
}
