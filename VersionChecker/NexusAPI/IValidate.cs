namespace Straitjacket.Subnautica.Mods.VersionChecker.NexusAPI
{
    internal interface IValidate
    {
        int UserId { get; }
        string ApiKey { get; }
        string Username { get; }
        bool IsPremium { get; }
        bool IsSupporter { get; }
        string Email { get; }
        string ProfileUrl { get; }
    }
}
