using fNbt;
using System.Collections.Generic;
using TiledSharp;
using VEMC.Parts;

namespace VEMC.PartBuilders
{
    public static partial class MapPartBuilder
    {
        public static void BuildSfx(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType)
        {
            mode = BuilderMode.BuildingSfx;

            MapPart audioSfx = new MapPart("audsfx", true);
            objectsByType.TryGetValue("sfx", out List<TmxObjectGroup.TmxObject> audioSfxObjects);

            if (audioSfxObjects != null)
            {
                foreach (TmxObjectGroup.TmxObject sfxObj in audioSfxObjects)
                {
                    MapPart sfxPart = new MapPart(false);
                    sfxPart.Add("x", sfxObj.X);
                    sfxPart.Add("y", sfxObj.Y);
                    sfxPart.Add("w", sfxObj.Width);
                    sfxPart.Add("h", sfxObj.Height);
                    sfxPart.AddFromDictionary<short>("flag", sfxObj.Properties, "playFlag", 0);
                    sfxPart.AddFromDictionary<short>("interval", sfxObj.Properties, "interval", 0);
                    sfxPart.AddFromDictionary<bool>("loop", sfxObj.Properties, "loop", true);
                    sfxPart.AddFromDictionary<string>("sfx", sfxObj.Properties, "sfx");
                    audioSfx.Add(sfxPart);
                }
            }

            if (audioSfx.Tags.Count > 0)
            {
                Debug.Log($"Total SFX: {audioSfx.Tags.Count}");

                mapCompound.Add(audioSfx.Tag);
                allMapParts.Add(audioSfx);
            }
        }

        public static void BuildBackgroundAudio(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType)
        {
            mode = BuilderMode.BuildingBackgroundAudio;

            MapPart bgm = new MapPart("audbgm", true);
            objectsByType.TryGetValue("bgm", out List<TmxObjectGroup.TmxObject> bgmObjectList);

            if (bgmObjectList != null)
            {
                foreach (TmxObjectGroup.TmxObject bgmObj in bgmObjectList)
                {
                    MapPart bgmPart = new MapPart(false);
                    bgmPart.Add("x", bgmObj.X);
                    bgmPart.Add("y", bgmObj.Y);
                    bgmPart.Add("w", bgmObj.Width);
                    bgmPart.Add("h", bgmObj.Height);
                    bgmPart.AddFromDictionary<short>("flag", bgmObj.Properties, "playflag", 0);
                    bgmPart.AddFromDictionary<bool>("loop", bgmObj.Properties, "loop", true);
                    bgmPart.AddFromDictionary<string>("bgm", bgmObj.Properties, "bgm");
                    bgm.Add(bgmPart);
                }
            }
            if (bgm.Tags.Count > 0)
            {
                Debug.Log($"Total audio: {bgm.Tags.Count}. This probably shouldn't be more than one (1).");

                mapCompound.Add(bgm.Tag);
                allMapParts.Add(bgm);
            }
        }

    }
}
