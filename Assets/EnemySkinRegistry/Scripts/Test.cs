using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace AntlerShed.SkinRegistry
{
    [TestFixture]
    class ProfileTests
    {
        private class FakeSkin : Skin
        {
            public string Label { get; }

            public string Id { get; }

            public string EnemyId { get; }

            public Texture2D Icon => null;

            public Skinner CreateSkinner()
            {
                return null;
            }

            public FakeSkin(string label, string id, string enemyId)
            {
                Label = label;
                Id = id;
                EnemyId = enemyId;
            }
        }

        private Profile testProfile;

        [SetUp]
        public void MakeConfig()
        {
            testProfile = new Profile
            (
                new EnemyConfiguration[]
                {
                    new EnemyConfiguration
                    (
                        "yornix",
                        new MapConfiguration
                        (
                            1.0f,
                            1.0f,
                            new SkinConfigEntry[]
                            {
                                new SkinConfigEntry(0.5f, "mormonPete.holyYornix"),
                                new SkinConfigEntry(0.5f, "dandelleon.fabulousYornix"),
                                new SkinConfigEntry(1.0f, "someIdiot.peepeeSkin")
                            }
                        ),
                        new MapConfiguration[]
                        {
                            new MapConfiguration
                            (
                                "60 Tumtum",
                                0.0f, 0.0f,
                                new SkinConfigEntry[]
                                {
                                    new SkinConfigEntry(1.0f, "mormonPete.holyYornix"),
                                    new SkinConfigEntry(0.1f, "gatoSludge.burpMaster"),
                                }
                            ),
                            new MapConfiguration
                            (
                                "20 Gilgamesh",
                                0.0f, 0.0f,
                                new SkinConfigEntry[]
                                {
                                    new SkinConfigEntry(1.0f, "mormonPete.holyYornix"),
                                    new SkinConfigEntry(0.1f, "gatoSludge.burpMaster"),
                                }
                            )
                        },
                        new string[] { "mormonPete.holyYornix", "gatoSludge.burpMaster", "dandelleon.fabulousYornix", "someIdiot.peepeeSkin" }
                    ),
                    new EnemyConfiguration
                    (
                        "snipe",
                        new MapConfiguration
                        (
                            1.0f,
                            1.0f,
                            new SkinConfigEntry[]
                            {

                            }
                        ),
                        new MapConfiguration[]
                        {

                        },
                        new string[] { }
                    ),
                }
            );
        }

        [Test]
        public void TestSanitizeProfile()
        {
            EnemyInfo[] testEnemies = new EnemyInfo[] 
            { 
                new EnemyInfo("Yornix", "yornix", SpawnLocation.OUTDOOR),
                new EnemyInfo("Shimbler", "shimbler", SpawnLocation.INDOOR)
            };
            Skin[] testSkins = new Skin[]
            {
                new FakeSkin("Holy Yornix", "mormonPete.holyYornix", "yornix"),
                new FakeSkin("Burp Master", "gatoSludge.burpMaster", "yornix"),
                new FakeSkin("Holy Yornix", "dandelleon.fabulousYornix", "yornix"),
            };
            string[] testMoons = new string[] { "20 Gilgamesh", "44 Peanut" };
            string[] testMoonTags = new string[] { "Valley", "Marsh", "Tundra" };
            testProfile.SyncWithLoadedMods
            (
                testEnemies,
                testSkins,
                testMoons,
                testMoonTags
            );
            Assert.IsEmpty(testProfile.ConfigData.Where((EnemyConfiguration cfg)=>!testEnemies.Any((EnemyInfo info) => info.Id.Equals(cfg.EnemyId))));
            Assert.IsEmpty(testProfile.ConfigData.Where((EnemyConfiguration cfg) => cfg.MapConfigs.Any((MapConfiguration mapCfg)=> !testMoons.Any((string moonId)=> moonId.Equals(mapCfg.Id)))));
            Assert.IsEmpty(testProfile.ConfigData.Where((EnemyConfiguration cfg) => cfg.MapConfigs.Any((MapConfiguration mapCfg) => mapCfg.Distribution.Any((SkinConfigEntry skinCfg)=> !testSkins.Any((Skin skin) => skin.Id.Equals(skinCfg.SkinId))))));
            Assert.IsEmpty(testProfile.ConfigData.Where((EnemyConfiguration cfg) => cfg.DefaultConfiguration.Distribution.Any((SkinConfigEntry skinCfg) => !testSkins.Any((Skin skin) => skin.Id.Equals(skinCfg.SkinId)))));
            Assert.IsTrue(testProfile.ConfigData.All((EnemyConfiguration cfg) => testEnemies.Any((EnemyInfo info)=> info.Id.Equals(cfg.EnemyId))));
        }

        [Test]
        public void TestDefaultSkinConfigApplication()
        {
            testProfile.ApplyDefaultSkinConfig
            (
                new DefaultSkinConfiguration
                (
                    "dandelleon.fabulousYornix",
                    new DefaultSkinConfigEntry[]
                    { 
                        new DefaultSkinConfigEntry
                        (
                            "20 Gilgamesh",
                            0.6f,
                            1.0f
                        ),
                        new DefaultSkinConfigEntry
                        (
                            "44 Peanut",
                            0.0f,
                            0.5f
                        )
                    },
                    1.0f,
                    0.0f,
                    0.3f,
                    0.6f
                ),
                "yornix",
                new string [] { "44 Peanut", "20 Gilgamesh" },
                new string[0]
            );
            Assert.AreEqual(testProfile.GetEnemyConfig("yornix").GetMoonConfig("44 Peanut").Distribution.First((entry) => entry.SkinId.Equals("dandelleon.fabulousYornix")).IndoorFrequency, 0.5f);
            Assert.AreEqual(testProfile.GetEnemyConfig("yornix").GetMoonConfig("44 Peanut").Distribution.First((entry) => entry.SkinId.Equals("dandelleon.fabulousYornix")).OutdoorFrequency, 0.0f);
            Assert.AreEqual(testProfile.GetEnemyConfig("yornix").GetMoonConfig("20 Gilgamesh").Distribution.First((entry) => entry.SkinId.Equals("dandelleon.fabulousYornix")).IndoorFrequency, 1.0f);
            Assert.AreEqual(testProfile.GetEnemyConfig("yornix").GetMoonConfig("20 Gilgamesh").Distribution.First((entry) => entry.SkinId.Equals("dandelleon.fabulousYornix")).OutdoorFrequency, 0.6f);
            Assert.AreEqual(testProfile.GetEnemyConfig("yornix").GetMoonConfig("44 Peanut").IndoorVanillaFrequency, 0.6f);
            Assert.AreEqual(testProfile.GetEnemyConfig("yornix").GetMoonConfig("44 Peanut").OutdoorVanillaFrequency, 0.0f);
            Assert.AreEqual(testProfile.GetEnemyConfig("yornix").GetMoonConfig("20 Gilgamesh").IndoorVanillaFrequency, 0.0f);
            Assert.AreEqual(testProfile.GetEnemyConfig("yornix").GetMoonConfig("20 Gilgamesh").OutdoorVanillaFrequency, 0.0f);
            Assert.AreEqual(testProfile.GetEnemyConfig("yornix").DefaultConfiguration.Distribution.First((entry) => entry.SkinId.Equals("dandelleon.fabulousYornix")).OutdoorFrequency, 1.0f);
            Assert.AreEqual(testProfile.GetEnemyConfig("yornix").DefaultConfiguration.Distribution.First((entry) => entry.SkinId.Equals("dandelleon.fabulousYornix")).IndoorFrequency, 0.3f);
        }

        [Test]
        public void TestDefaultMoonConfigApplication()
        {
            testProfile.ApplyDefaultMoonConfig
            (
                new DefaultMapConfiguration
                (
                    "44 Peanut",
                    new DefaultMapConfigEntry[]
                    {
                        new DefaultMapConfigEntry
                        (
                            "yornix",
                            0.0f,
                            new SkinConfigEntry[]
                            {
                                new SkinConfigEntry
                                (
                                    0.5f,
                                    "mormonPete.holyYornix"
                                ),
                                new SkinConfigEntry
                                (
                                    0.2f,
                                    "fakerooni.evilYornix"
                                )
                            }
                        )
                    }
                ),
                new string[] 
                { 
                    "mormonPete.holyYornix",
                    "gatoSludge.burpMaster",
                    "dandelleon.fabulousYornix" 
                }
            );
            Assert.AreEqual(testProfile.GetEnemyConfig("yornix").GetMoonConfig("44 Peanut").Distribution.First((entry) => entry.SkinId.Equals("mormonPete.holyYornix")).IndoorFrequency, 0.5f);
            Assert.IsEmpty(testProfile.GetEnemyConfig("yornix").GetMoonConfig("44 Peanut").Distribution.Where((entry) => entry.SkinId.Equals("fakerooni.evilYornix")));
        }
    }
}
