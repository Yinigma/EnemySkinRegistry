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
                    new MoonInfo("Sanctuary", "bimple.CoolMoons.SanctuaryMoon"),
                    new MoonInfo("Detritus", "bimple.CoolMoons.DetritusMoon"),
                    new MoonInfo("Filament", "bimple.CoolMoons.FilamentMoon"),
                    new MoonInfo("Abnegation", "bimple.CoolMoons.AbnegationMoon"),
                    new MoonInfo("Intent", "hideyMan.HaloMoons.Intent"),
                    new MoonInfo("Persistence", "gabule.Persistence"),
                    new MoonInfo("Gratitude", "gabule.Gratitude")
                },
                new Dictionary<string, EnemyInfo>
                {
                    { "Bracken", new EnemyInfo("Bracken", "Bracken") },
                    { "Pinkman", new EnemyInfo("Pinkman", "Pinkman") },
                    { "Wiggler", new EnemyInfo("Wiggler", "Wiggler") },
                    { "Thumper", new EnemyInfo("Thumper", "Thumper") }
                },
                new Dictionary<string, Skin[]>
                {
                    { "Wiggler", wigglerSkins },
                    { "Pinkman", pinkmanSkins},
                    { "Bracken", brackenSkins},
                    { "Thumper", thumperSkins }
                },
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
                                new SkinConfigEntry[]
                                {
                                    new SkinConfigEntry(0.25f, "AntlerShed.BitoSkin")
                                }
                            ),
                            new MapConfiguration
                            (
                                "bimple.CoolMoons.AbnegationMoon",
                                0.3f,
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
                                new SkinConfigEntry[] { }
                            )
                        },
                        new string[]{ "AntlerShed.BitoSkin" }
                    )
                }
            );
            menu = Instantiate(prefab, transform);
            menu.Init(viewModel);
        }


    }
}
