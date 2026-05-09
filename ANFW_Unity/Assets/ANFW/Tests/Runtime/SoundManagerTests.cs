using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ANFW.Sound;

namespace ANFW.Tests
{
    public class SoundManagerTests
    {
        private SoundManager _manager;
        private AudioSource _bgmSource;
        private AudioSource[] _seSources;
        private GameObject _go;
        private CancellationTokenSource _cts;

        [UnitySetUp]
        public IEnumerator SetUp() => UniTask.ToCoroutine(async () =>
        {
            _cts = new CancellationTokenSource();
            _go = new GameObject("SoundManagerTest");
            _bgmSource = _go.AddComponent<AudioSource>();
            _seSources = new[] { _go.AddComponent<AudioSource>() };

            _manager = new SoundManager();
            await _manager.InitializeAsync(_bgmSource, _seSources, _cts.Token);
        });

        [TearDown]
        public void TearDown()
        {
            _manager.Dispose();
            _cts.Cancel();
            _cts.Dispose();
            Object.Destroy(_go);
        }

        [Test]
        public void SetMasterVolume_ClampsAboveOne()
        {
            _manager.SetMasterVolume(1.5f);
            Assert.AreEqual(1f, _manager.MasterVolume);
        }

        [Test]
        public void SetMasterVolume_ClampsBelowZero()
        {
            _manager.SetMasterVolume(-0.5f);
            Assert.AreEqual(0f, _manager.MasterVolume);
        }

        [Test]
        public void SetMasterVolume_UpdatesBGMSourceVolume()
        {
            _manager.SetBGMVolume(0.5f);
            _manager.SetMasterVolume(0.8f);

            Assert.AreEqual(0.5f * 0.8f, _bgmSource.volume, 0.001f);
        }

        [Test]
        public void SetBGMVolume_UpdatesProperty()
        {
            _manager.SetBGMVolume(0.6f);
            Assert.AreEqual(0.6f, _manager.BGMVolume);
        }

        [Test]
        public void SetBGMVolume_UpdatesBGMSourceVolume()
        {
            _manager.SetMasterVolume(0.8f);
            _manager.SetBGMVolume(0.5f);

            Assert.AreEqual(0.8f * 0.5f, _bgmSource.volume, 0.001f);
        }

        [Test]
        public void SetSEVolume_UpdatesProperty()
        {
            _manager.SetSEVolume(0.75f);
            Assert.AreEqual(0.75f, _manager.SEVolume);
        }

        [Test]
        public void SetMasterVolumeEvent_UpdatesMasterVolume()
        {
            EventBus.Emit(new SetMasterVolumeEvent { Volume = 0.3f });
            Assert.AreEqual(0.3f, _manager.MasterVolume);
        }

        [Test]
        public void SetBGMVolumeEvent_UpdatesBGMVolume()
        {
            EventBus.Emit(new SetBGMVolumeEvent { Volume = 0.6f });
            Assert.AreEqual(0.6f, _manager.BGMVolume);
        }

        [Test]
        public void SetSEVolumeEvent_UpdatesSEVolume()
        {
            EventBus.Emit(new SetSEVolumeEvent { Volume = 0.4f });
            Assert.AreEqual(0.4f, _manager.SEVolume);
        }

        [Test]
        public void Dispose_StopsEventBusHandlers()
        {
            _manager.SetMasterVolume(0.5f);
            _manager.Dispose();

            EventBus.Emit(new SetMasterVolumeEvent { Volume = 0.9f });

            Assert.AreEqual(0.5f, _manager.MasterVolume);
        }

        [Test]
        public void StopBGM_WhenNoClipLoaded_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _manager.StopBGM());
        }

        [Test]
        public void CreatePositionalSource_CreatesChildGameObject()
        {
            var parent = new GameObject("Character");

            var source = _manager.CreatePositionalSource(parent.transform);

            Assert.IsNotNull(source);
            Assert.AreEqual(parent.transform, source.transform.parent);
            Assert.AreEqual(1f, source.spatialBlend);

            Object.Destroy(parent);
        }

        [Test]
        public void CreatePositionalSource_VolumeSyncsOnMasterVolumeChange()
        {
            var parent = new GameObject("Character");
            var source = _manager.CreatePositionalSource(parent.transform);

            _manager.SetSEVolume(0.5f);
            _manager.SetMasterVolume(0.8f);

            Assert.AreEqual(0.5f * 0.8f, source.volume, 0.001f);

            Object.Destroy(parent);
        }

        [Test]
        public void CreatePositionalSource_VolumeSyncsOnSEVolumeChange()
        {
            var parent = new GameObject("Character");
            var source = _manager.CreatePositionalSource(parent.transform);

            _manager.SetMasterVolume(0.8f);
            _manager.SetSEVolume(0.5f);

            Assert.AreEqual(0.8f * 0.5f, source.volume, 0.001f);

            Object.Destroy(parent);
        }

        [Test]
        public void CreatePositionalSource_NullReferenceIsCleanedUpOnVolumeChange()
        {
            var parent = new GameObject("Character");
            _manager.CreatePositionalSource(parent.transform);
            Object.DestroyImmediate(parent);

            Assert.DoesNotThrow(() => _manager.SetMasterVolume(0.5f));
        }
    }
}
