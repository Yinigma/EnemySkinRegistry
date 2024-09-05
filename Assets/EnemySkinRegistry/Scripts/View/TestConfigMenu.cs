using System.Collections.Generic;
using UnityEngine;

namespace AntlerShed.SkinRegistry.View
{
    class TestSkin : Skin
    {
        string Skin.Label => label; 
        private string label;

        string Skin.EnemyId => enemyId;
        private string enemyId;

        string Skin.Id => id;
        private string id;

        Texture2D Skin.Icon => icon;
        private Texture2D icon;

        Skinner Skin.CreateSkinner()
        {
            return null;
        }

        internal TestSkin(string id, string label, string enemyId, Texture2D icon = null)
        {
            this.id = id;
            this.label = label;
            this.enemyId = enemyId;
            this.icon = icon ?? Texture2D.whiteTexture;
        }
    }

    public class TestConfigMenu : MonoBehaviour
    {
        [SerializeField]
        private EnemySkinConfigurationMenu prefab;

        private EnemySkinConfigurationMenu menu;

        void Start()
        {
            Skin[] wigglerSkins = new Skin[]
            {
                new TestSkin("memermods.cosmetic.elmo", "Elmo", "Wiggler"),
                new TestSkin("UrbanPyramid.ColbySkin", "Colby", "Wiggler"),
                new TestSkin("AntlerShed.RhoWiggler", "Rhodope", "Wiggler"),
            };
            Skin[] pinkmanSkins = new Skin[] { new TestSkin("rodeofreak.powerrangersmodpack.blueRanger", "Blue Ranger", "Pinkman") };
            Skin[] brackenSkins = new Skin[]
            {
                new TestSkin("AntlerShed.BitoSkin", "Bito", "Bracken"),
                new TestSkin("GokuBracken", "Goku", "Bracken")
            };
            Skin[] thumperSkins = new Skin[] { };
            ConfigurationViewModel viewModel = new ConfigurationViewModel
            (
                new MoonInfo[]
                {
                    new MoonInfo("Sanctuary", "bimple.CoolMoons.SanctuaryMoon", new HashSet<string>(){ "valley" }),
                    new MoonInfo("Detritus", "bimple.CoolMoons.DetritusMoon", new HashSet<string>(){ "valley" }),
                    new MoonInfo("Filament", "bimple.CoolMoons.FilamentMoon", new HashSet<string>(){ "tundra" }),
                    new MoonInfo("Abnegation", "bimple.CoolMoons.AbnegationMoon", new HashSet<string>(){ "tundra" }),
                    new MoonInfo("Intent", "hideyMan.HaloMoons.Intent", new HashSet<string>(){ "marsh" }),
                    new MoonInfo("Persistence", "gabule.Persistence", new HashSet<string>(){ "tundra" }),
                    new MoonInfo("Gratitude", "gabule.Gratitude", new HashSet<string>(){ "valley" })
                },
                new string[] { "valley", "tundra", "marsh" },
                new Dictionary<string, EnemyInfo>
                {
                    { "Bracken", new EnemyInfo("Bracken", "Bracken", SpawnLocation.INDOOR) },
                    { "Pinkman", new EnemyInfo("Pinkman", "Pinkman", SpawnLocation.INDOOR) },
                    { "Wiggler", new EnemyInfo("Wiggler", "Wiggler", SpawnLocation.OUTDOOR) },
                    { "Thumper", new EnemyInfo("Thumper", "Thumper", SpawnLocation.INDOOR) }
                },
                new Dictionary<string, Skin[]>
                {
                    { "Wiggler", wigglerSkins },
                    { "Pinkman", pinkmanSkins},
                    { "Bracken", brackenSkins},
                    { "Thumper", thumperSkins }
                },
                new string[] { }, 
                new Profile
                (
                    new EnemyConfiguration[]
                    {
                        new EnemyConfiguration("Wiggler", wigglerSkins),
                        new EnemyConfiguration("Thumper", thumperSkins),
                        new EnemyConfiguration("Pinkman", pinkmanSkins),
                        new EnemyConfiguration
                        (
                            "Bracken",
                            new MapConfiguration
                            (
                                0.5f,
                                0.2f,
                                new SkinConfigEntry[]
                                {
                                    new SkinConfigEntry(0.5f, "AntlerShed.BitoSkin")
                                }
                            ),
                            new MapConfiguration[]
                            {
                                new MapConfiguration
                                (
                                    "bimple.CoolMoons.SanctuaryMoon",
                                    0.0f,
                                    0.2f,
                                    new SkinConfigEntry[]
                                    {
                                        new SkinConfigEntry(0.25f, "AntlerShed.BitoSkin")
                                    }
                                ),
                                new MapConfiguration
                                (
                                    "bimple.CoolMoons.AbnegationMoon",
                                    0.3f,
                                    0.2f,
                                    new SkinConfigEntry[]
                                    {
                                        new SkinConfigEntry(1.0f, "AntlerShed.BitoSkin"),
                                        new SkinConfigEntry(0.34f, "GokuBracken")
                                    }
                                ),
                                new MapConfiguration
                                (
                                    "bimple.CoolMoons.FilamentMoon",
                                    0.2f,
                                    0.2f,
                                    new SkinConfigEntry[] { }
                                )
                            },
                            new string[]{ "AntlerShed.BitoSkin" }
                        )
                    }
                ),
                new Dictionary<string, string>()
                {
                    { "AntlerShed.BitoSkin", "Bito" }
                },
                new Dictionary<string, string>() 
                {
                    { "bimple.CoolMoons.FilamentMoon", "Filament" }
                }
                
            );
            menu = Instantiate(prefab, transform);
            menu.Init(viewModel);
        }


    }
}
