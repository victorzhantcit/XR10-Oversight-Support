using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Oversight.Clipping
{
    [Serializable]
    public class ImageSelectorDtos
    {
        [JsonProperty("name")]
        public string ImageName;
        [JsonProperty("image")]
        public string ImageBase64;
        [JsonProperty("viewport")]
        public string Viewport;

        public BoxSide GetBoxSide()
        {
            switch (Viewport)
            {
                case "Bottom":
                    return BoxSide.Bottom;
                case "Back":
                    return BoxSide.Back;
                default:
                    return BoxSide.Bottom;
            }
        }

        [JsonConstructor]
        public ImageSelectorDtos() { }

        public ImageSelectorDtos(string imageName, string imageBase64, string viewport)
        {
            ImageName = imageName;
            ImageBase64 = imageBase64;
            Viewport = viewport;
        }
    }
}
