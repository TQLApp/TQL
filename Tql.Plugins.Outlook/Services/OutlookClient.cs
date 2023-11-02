using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Outlook;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Application = Microsoft.Office.Interop.Outlook.Application;

namespace Tql.Plugins.Outlook.Services;

internal class OutlookClient : IDisposable
{
    private readonly ILogger<OutlookClient> _logger;

    // This is a very liberal pattern for matching email addresses. It's
    // only used to discard obviously invalid ones, so it does not need
    // to be fully compliant.
    private static readonly Regex EmailRe =
        new("^[a-z0-9._%+-]+@[a-z0-9.-]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly Application _application;
    private readonly NameSpace _ns;

    public OutlookClient(ILogger<OutlookClient> logger)
    {
        _logger = logger;

        try
        {
            // Only get an actively running Outlook. This prevents the welcome
            // dialog from coming up on machines that don't have Outlook setup.
            _application = (Application)Marshal.GetActiveObject("Outlook.Application");
        }
        catch (System.Exception ex)
        {
            throw new OutlookCacheUpdateException(Labels.OutlookClient_OutlookIsNotRunning, ex);
        }

        _ns = _application.GetNamespace("mapi");
        _ns.Logon(Missing.Value, Missing.Value, false, true);
    }

    public IEnumerable<Person> FindInContactsFolder()
    {
        var contactsFolder = _ns.GetDefaultFolder(OlDefaultFolders.olFolderContacts);
        var contactItems = contactsFolder.Items;

        try
        {
            foreach (var item in contactItems)
            {
                if (item is ContactItem contact)
                {
                    if (MayBeEmailAddress(contact.Email1Address))
                        yield return CreatePerson(contact.FullName, contact.Email1Address);
                    if (MayBeEmailAddress(contact.Email2Address))
                        yield return CreatePerson(contact.FullName, contact.Email2Address);
                    if (MayBeEmailAddress(contact.Email3Address))
                        yield return CreatePerson(contact.FullName, contact.Email3Address);
                }

                Marshal.ReleaseComObject(item);
            }
        }
        finally
        {
            Marshal.ReleaseComObject(contactItems);
            Marshal.ReleaseComObject(contactsFolder);
        }
    }

    private bool MayBeEmailAddress(string? emailAddress) =>
        emailAddress != null && EmailRe.IsMatch(emailAddress);

    public IEnumerable<Person> FindInGlobalAddressList()
    {
        var addressList = _application.Session.GetGlobalAddressList();
        var addressEntries = addressList.AddressEntries;

        try
        {
            AddressEntry addressEntry;

            try
            {
                addressEntry = addressEntries.GetFirst();
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation(ex, "Global address list is not available");
                yield break;
            }

            while (addressEntry != null)
            {
                if (
                    addressEntry.AddressEntryUserType
                        == OlAddressEntryUserType.olExchangeUserAddressEntry
                    || addressEntry.AddressEntryUserType
                        == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry
                )
                {
                    var exchangeUser = addressEntry.GetExchangeUser();

                    if (exchangeUser != null && MayBeEmailAddress(exchangeUser.PrimarySmtpAddress))
                        yield return CreatePerson(
                            exchangeUser.Name,
                            exchangeUser.PrimarySmtpAddress
                        );
                }

                var nextAddressEntry = addressEntries.GetNext();

                Marshal.ReleaseComObject(addressEntry);

                addressEntry = nextAddressEntry;
            }
        }
        finally
        {
            Marshal.ReleaseComObject(addressEntries);
            Marshal.ReleaseComObject(addressList);
        }
    }

    private Person CreatePerson(string? displayName, string emailAddress)
    {
        return new Person(displayName ?? emailAddress, emailAddress);
    }

    public void Dispose()
    {
        Marshal.ReleaseComObject(_ns);
        Marshal.ReleaseComObject(_application);
    }
}
