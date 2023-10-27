﻿using Microsoft.Office.Interop.Outlook;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Application = Microsoft.Office.Interop.Outlook.Application;

namespace Tql.Plugins.Outlook.Services;

internal class OutlookClient : IDisposable
{
    // This is a very liberal pattern for matching email addresses. It's
    // only used to discard obviously invalid ones, so it does not need
    // to be fully compliant.
    private static readonly Regex EmailRe =
        new("^[a-z0-9._%+-]+@[a-z0-9.-]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly Application _application;
    private readonly NameSpace _ns;

    public OutlookClient()
    {
        // Only get an actively running Outlook. This prevents the welcome
        // dialog from coming up on machines that don't have Outlook setup.
        _application = (Application)Marshal.GetActiveObject("Outlook.Application");

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
                        yield return new Person(contact.FullName, contact.Email1Address);
                    if (MayBeEmailAddress(contact.Email2Address))
                        yield return new Person(contact.FullName, contact.Email2Address);
                    if (MayBeEmailAddress(contact.Email3Address))
                        yield return new Person(contact.FullName, contact.Email3Address);
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
            var addressEntry = addressEntries.GetFirst();

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

                    if (exchangeUser != null)
                        yield return new Person(exchangeUser.Name, exchangeUser.PrimarySmtpAddress);
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

    public void Dispose()
    {
        Marshal.ReleaseComObject(_ns);
        Marshal.ReleaseComObject(_application);
    }
}