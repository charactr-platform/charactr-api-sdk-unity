﻿using System.Threading.Tasks;
using gemelo.VoiceSDK.Library;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace gemelo.VoiceSDK.Tests
{
	public class Library
	{
		private VoiceLibrary _library;

		[SetUp]
		public void Setup()
		{
			_library = ScriptableObject.CreateInstance<VoiceLibrary>();
		}
		
		[Test]
		public void CreateLibrary_NotNull()
		{
			_library = ScriptableObject.CreateInstance<VoiceLibrary>();
			Assert.NotNull(_library);
			Assert.IsInstanceOf<VoiceLibrary>(_library);
		}

		[Test]
		public void AddNewVoiceItemToLibrary_NotNull_IdNotZero()
		{
			var id = _library.AddNewItem("Hello world!", TestBase.VOICE_ID);
			
			Assert.NotZero(id);
		}

		[Test]
		public async Task GetAudioClipForNewItem_NotNull()
		{
			var id = _library.AddNewItem("Hello world!", TestBase.VOICE_ID);
			Assert.NotZero(id);
			//await _library.AddAudioClip(id);
			
			Assert.IsTrue(_library.GetItemById(id, out var item));
		
			Assert.NotNull(item);
			Assert.IsInstanceOf<VoiceItem>(item);
			Assert.IsTrue(item.IsValid());
			
			var clip = await item.GetAudioClip();
			Assert.NotNull(clip);
			Assert.IsInstanceOf<AudioClip>(clip);
			
			Debug.Log($"Testing GetAudioClip() for item = {item.Id}");
			
			Assert.NotNull(item.AudioClip);
			Assert.IsInstanceOf<AudioClip>(item.AudioClip);
			var path = AssetDatabase.GetAssetPath(item.AudioClip);
			Assert.IsNotEmpty(path);
			Debug.Log($"Asset path: = {path}");
		}

		[Test]
		public void TryLoadExistingLibrary_NotNull()
		{
			var library = Resources.Load<VoiceLibrary>("VoiceLibrary");
			Assert.NotNull(library);
			library.ConvertTextsToAudioClips(null);
		}
	}
}