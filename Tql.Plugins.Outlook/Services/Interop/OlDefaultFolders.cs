namespace Tql.Plugins.Outlook.Services.Interop;

internal enum OlDefaultFolders
{
    olFolderDeletedItems = 3,
    olFolderOutbox = 4,
    olFolderSentMail = 5,
    olFolderInbox = 6,
    olFolderCalendar = 9,
    olFolderContacts = 10, // 0x0000000A
    olFolderJournal = 11, // 0x0000000B
    olFolderNotes = 12, // 0x0000000C
    olFolderTasks = 13, // 0x0000000D
    olFolderDrafts = 16, // 0x00000010
    olPublicFoldersAllPublicFolders = 18, // 0x00000012
    olFolderConflicts = 19, // 0x00000013
    olFolderSyncIssues = 20, // 0x00000014
    olFolderLocalFailures = 21, // 0x00000015
    olFolderServerFailures = 22, // 0x00000016
    olFolderJunk = 23, // 0x00000017
    olFolderRssFeeds = 25, // 0x00000019
    olFolderToDo = 28, // 0x0000001C
    olFolderManagedEmail = 29, // 0x0000001D
    olFolderSuggestedContacts = 30, // 0x0000001E
}
