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

        [JsonProperty]
        private float _volume;

        public float Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                if (value > 0 && value <= 1)
                {
                    _volume = value;
                    SaveChanges();
                }
            }
        }

        private PicofyConfiguration() { }

        private static PicofyConfiguration _current;

        public static PicofyConfiguration Current
        {
            get
            {
                if (_current != null) return _current;

                if (!File.Exists(ConfigurationPath))
                {
                    new PicofyConfiguration().SaveChanges();
                }

                return _current = JsonConvert.DeserializeObject<PicofyConfiguration>(File.ReadAllText(ConfigurationPath));
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
