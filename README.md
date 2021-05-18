# Watcher
Built a tool that redirects incoming email attachments to the appropriate team based on the email title. It is a C# console application based on .NET Framework 4.5. Incoming email attachments were automatically downloaded to the file system and I used FileSystemWatcher to monitor the changes in the directory. Then the tool processed the email and the file and automatically notified the team about new incoming files.