namespace Tql.Plugins.Outlook.Services.Interop;

internal enum OlAddressEntryUserType
{
    olExchangeUserAddressEntry = 0,
    olExchangeDistributionListAddressEntry = 1,
    olExchangePublicFolderAddressEntry = 2,
    olExchangeAgentAddressEntry = 3,
    olExchangeOrganizationAddressEntry = 4,
    olExchangeRemoteUserAddressEntry = 5,
    olOutlookContactAddressEntry = 10, // 0x0000000A
    olOutlookDistributionListAddressEntry = 11, // 0x0000000B
    olLdapAddressEntry = 20, // 0x00000014
    olSmtpAddressEntry = 30, // 0x0000001E
    olOtherAddressEntry = 40, // 0x00000028
}
