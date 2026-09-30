using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace AndroidLibrary
{
    public class AndroidLibrarySettings : ObservableObject, ISettings
    {
        private readonly AndroidLibrary plugin;

        private string installedAppsFilePath = string.Empty;
        public string InstalledAppsFilePath { get => installedAppsFilePath; set => SetValue(ref installedAppsFilePath, value); }


        public AndroidLibrarySettings() { }
        public AndroidLibrarySettings(AndroidLibrary plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<AndroidLibrarySettings>();
            
            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                InstalledAppsFilePath = savedSettings.InstalledAppsFilePath;
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(this);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return errors.Count == 0;
        }
    }
}