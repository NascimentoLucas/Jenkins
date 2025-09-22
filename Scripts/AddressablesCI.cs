using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace Nascimento.Dev.Build
{
    public static class AddressablesCI
    {
        public static void Build()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                throw new System.Exception("Addressables settings not found. Create them via Window > Asset Management > Addressables > Groups.");
            }

            var profileSettings = settings.profileSettings;
            if (profileSettings == null)
            {
                throw new System.Exception("Addressables profile settings not found or corrupt.");
            }

            if (string.IsNullOrEmpty(settings.activeProfileId))
            {
                throw new System.Exception("No active Addressables profile selected in the Editor.");
            }

            // Do the build
            AddressableAssetSettings.BuildPlayerContent();
            // Optionally: UnityEditor.AssetDatabase.SaveAssets();
        }

        public static void Clean()
        {
            AddressableAssetSettings.CleanPlayerContent();
        }
    }
}
