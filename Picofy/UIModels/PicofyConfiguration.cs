using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Picofy.TorshifyHelper;

namespace Picofy.UIModels
{
    public class PicofyConfiguration
    {
        [JsonProperty]
        private Dictionary<string, PlaylistSorting> _playlistSorts = new Dictionary<string, PlaylistSorting>();

        private PicofyConfiguration() { }

        private static PicofyConfiguration _currentConfiguration;

        public static PicofyConfiguration CurrentConfiguration
        {
            get
            {
                if (_currentConfiguration == null)
                {
                    if (!File.Exists(ConfigurationPath))
                    {
                        new PicofyConfiguration().SaveChanges();
                    }

                    return _currentConfiguration = JsonConvert.DeserializeObject<PicofyConfiguration>(File.ReadAllText(ConfigurationPath));
                }

                return _currentConfiguration;
            }
        }

        public static string ConfigurationPath 
        {
            get
            {
                return Path.Combine(Constants.SettingsFolder, "picofy_settings.json");
            }
        }

        public void SetSortingForPlaylist(string playlistName, PlaylistSorting sorting)
        {
            _playlistSorts[playlistName] = sorting;
            SaveChanges();
        }

        public PlaylistSorting GetSortingForPlaylist(string playlistName)
        {
            if (_playlistSorts.ContainsKey(playlistName))
            {
                if (_playlistSorts[playlistName].SortDirection == null)
                {
                    return null;
                }

                return _playlistSorts[playlistName];
            }

            return null;
        }

        public void SaveChanges()
        {
            File.WriteAllText(ConfigurationPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
