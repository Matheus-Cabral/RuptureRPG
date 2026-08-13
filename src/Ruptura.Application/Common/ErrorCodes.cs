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
        public const string FloorStateInvalid = "Campaign.FloorStateInvalid";
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

    public static class Guild
    {
        public const string NotFound = "Guild.NotFound";
        public const string Forbidden = "Guild.Forbidden";
        public const string Conflict = "Guild.Conflict";
        public const string InstallationInvalid = "Guild.InstallationInvalid";
        public const string BuildingNotConstructible = "Guild.BuildingNotConstructible";
        public const string BuildingLevelInvalid = "Guild.BuildingLevelInvalid";
        public const string BuildingExists = "Guild.BuildingExists";
        public const string BuildingNotFound = "Guild.BuildingNotFound";
        public const string StaffNotFound = "Guild.StaffNotFound";
        public const string StaffKindInvalid = "Guild.StaffKindInvalid";
        public const string ExpeditionKindInvalid = "Guild.ExpeditionKindInvalid";
        public const string DoctrineInvalid = "Guild.DoctrineInvalid";
        public const string ResearchNotFound = "Guild.ResearchNotFound";
        public const string ResearchComplexityInvalid = "Guild.ResearchComplexityInvalid";
        public const string ResearchStageInvalid = "Guild.ResearchStageInvalid";
        public const string CraftingNotFound = "Guild.CraftingNotFound";
        public const string CraftingCategoryInvalid = "Guild.CraftingCategoryInvalid";
        public const string CraftingStatusInvalid = "Guild.CraftingStatusInvalid";
        public const string InterludeDaysInvalid = "Guild.InterludeDaysInvalid";
        public const string InterludeKindInvalid = "Guild.InterludeKindInvalid";
    }

    public static class Bestiary
    {
        public const string NotFound = "Bestiary.NotFound";
        public const string Forbidden = "Bestiary.Forbidden";
        public const string FraquezaRequired = "Bestiary.FraquezaRequired";
        public const string BehaviorInvalid = "Bestiary.BehaviorInvalid";
        public const string CategoryInvalid = "Bestiary.CategoryInvalid";
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

    public static class Encounter
    {
        public const string NotFound = "Encounter.NotFound";
        public const string Forbidden = "Encounter.Forbidden";
        public const string NameRequired = "Encounter.NameRequired";
        public const string IntelligenceInvalid = "Encounter.IntelligenceInvalid";
        public const string TerrainInvalid = "Encounter.TerrainInvalid";
        public const string ObjectiveInvalid = "Encounter.ObjectiveInvalid";
        public const string DifficultyInvalid = "Encounter.DifficultyInvalid";
        public const string DurationInvalid = "Encounter.DurationInvalid";
    }

    public static class Reward
    {
        public const string NotFound = "Reward.NotFound";
        public const string NameRequired = "Reward.NameRequired";
        public const string CategoryInvalid = "Reward.CategoryInvalid";
        public const string EncounterInvalid = "Reward.EncounterInvalid";
    }

    public static class Combat
    {
        public const string NotFound = "Combat.NotFound";
        public const string NameRequired = "Combat.NameRequired";
        public const string StateRequired = "Combat.StateRequired";
        public const string ConditionInvalid = "Combat.ConditionInvalid";
        public const string EncounterInvalid = "Combat.EncounterInvalid";
    }

    public static class Content
    {
        public const string NotFound = "Content.NotFound";
        public const string NameRequired = "Content.NameRequired";
        public const string ObjectiveTypeInvalid = "Content.ObjectiveTypeInvalid";
        public const string ArcInvalid = "Content.ArcInvalid";
        public const string LinkInvalid = "Content.LinkInvalid";
    }

    public static class Session
    {
        public const string NotFound = "Session.NotFound";
        public const string TitleRequired = "Session.TitleRequired";
    }
}
