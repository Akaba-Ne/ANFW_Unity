using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using ANFW.SaveData;

namespace ANFW.Tests
{
    public class SaveDataManagerTests
    {
        [Serializable]
        private class TestData
        {
            public int Value;
            public string Name;
        }

        private SaveDataManager _manager;
        private string _testDir;

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(Path.GetTempPath(), $"anfw_save_test_{Guid.NewGuid():N}");
            _manager = new SaveDataManager();
            _manager.InitializeAsync(CancellationToken.None, _testDir).GetAwaiter().GetResult();
        }

        [TearDown]
        public void TearDown()
        {
            _manager.Dispose();
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, recursive: true);
        }

        [Test]
        public void Save_AndLoad_RoundTrip()
        {
            _manager.Save(1, "player", new TestData { Value = 42, Name = "Alice" });

            var loaded = _manager.Load<TestData>(1, "player");

            Assert.AreEqual(42, loaded.Value);
            Assert.AreEqual("Alice", loaded.Name);
        }

        [Test]
        public void Load_WhenKeyNotFound_ReturnsDefault()
        {
            var loaded = _manager.Load<TestData>(1, "nonexistent");

            Assert.IsNull(loaded);
        }

        [Test]
        public void HasKey_ReturnsTrueAfterSave()
        {
            _manager.Save(1, "player", new TestData { Value = 1 });

            Assert.IsTrue(_manager.HasKey(1, "player"));
        }

        [Test]
        public void HasKey_ReturnsFalseAfterDelete()
        {
            _manager.Save(1, "player", new TestData { Value = 1 });
            _manager.Delete(1, "player");

            Assert.IsFalse(_manager.HasKey(1, "player"));
        }

        [Test]
        public void DeleteSlot_RemovesAllKeysInSlot()
        {
            _manager.Save(1, "player", new TestData { Value = 1 });
            _manager.Save(1, "progress", new TestData { Value = 2 });

            _manager.DeleteSlot(1);

            Assert.IsFalse(_manager.HasKey(1, "player"));
            Assert.IsFalse(_manager.HasKey(1, "progress"));
        }

        [Test]
        public void DeleteAll_ClearsAllSlots()
        {
            _manager.Save(1, "player", new TestData { Value = 1 });
            _manager.Save(2, "player", new TestData { Value = 2 });

            _manager.DeleteAll();

            Assert.IsFalse(_manager.HasKey(1, "player"));
            Assert.IsFalse(_manager.HasKey(2, "player"));
        }

        [Test]
        public void DifferentSlots_SameKey_AreIndependent()
        {
            _manager.Save(1, "player", new TestData { Value = 10 });
            _manager.Save(2, "player", new TestData { Value = 20 });

            var slot1 = _manager.Load<TestData>(1, "player");
            var slot2 = _manager.Load<TestData>(2, "player");

            Assert.AreEqual(10, slot1.Value);
            Assert.AreEqual(20, slot2.Value);
        }
    }
}
