using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using Charactr.SDK;
using Charactr.SDK.Wav;
using Charactr.VoiceSDK.SDK;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Charactr.VoiceSDK.Tests
{
	public class Convert : TestBase
	{
		private const string ENDPOINT = "convert";

		[Test]
		public async Task GetConversion_Returns_WAV()
		{
			var wavBytes = await Http.PostAsync(Configuration.API + ENDPOINT, CreateRequest().ToJson());
			
			Assert.NotNull(wavBytes);
			Assert.IsNotEmpty(wavBytes);
		}

		[Test]
		public async Task GetBytesAndConvertToWAV_Returns_WAV()
		{
			var wavBytes = await Http.PostAsync(Configuration.API + ENDPOINT, CreateRequest().ToJson());
			
			Assert.NotNull(wavBytes);
			Assert.IsNotEmpty(wavBytes);
			
			var wav = new WavBuilder(wavBytes);
			Assert.NotNull(wav);
			
			var header = new WavHeaderData(wavBytes);
			Assert.NotNull(header);
			Assert.IsTrue(header.IsExtensibeWav);
			Assert.NotZero(header.AudioFormat);
			Assert.NotZero(header.DataOffset);
			Assert.AreEqual(16,header.BitDepth);

			var clip = wav.CreateAudioClip();
			Assert.NotNull(clip);
			Assert.NotZero(clip.length);
			Assert.AreEqual(130560, clip.samples);
			Assert.AreEqual(32000, clip.frequency);
			Assert.AreEqual(4.08f, clip.length);
			
			var player = CreatePlayerObject();
			player.PlayOneShot(clip);
			await Task.Delay((int)clip.length * 1000);
		}
		
		[UnityTest]
		public IEnumerator PlayConversion_Coroutine_Returns_OK()
		{
			AudioClip audioClip = null;

			var audioPlayer = CreatePlayerObject();
			
			yield return Http.GetAudioClipRoutine(Configuration.API + ENDPOINT, CreateRequest().ToJson(), clip =>
			{
				audioClip = clip;
				audioPlayer.PlayOneShot(clip);
				Debug.Log($"Clip: {clip.frequency} {clip.length} {clip.samples}");
			});
			
			//Give it time to play till end 
			while (audioPlayer.isPlaying)
			{
				yield return null;
			}
			
			Assert.NotNull(audioClip);
			Assert.AreEqual(130560, audioClip.samples);
			Assert.AreEqual(32000, audioClip.frequency);
			Assert.AreEqual(4.08f, audioClip.length);
		}
	}
}