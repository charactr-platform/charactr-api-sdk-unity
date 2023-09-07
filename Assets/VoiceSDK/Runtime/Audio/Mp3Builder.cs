using System;
using System.Collections.Generic;
using System.IO;
using NLayer;
using UnityEngine;

namespace Gemelo.Voice.Audio
{
	public class Mp3Builder : AudioClipBuilder
	{
		private readonly MpegFile _mpegFile;
		private readonly MemoryStream _stream;
		
		public Mp3Builder(int sampleRate, byte[] headerData) : base(sampleRate)
		{
			_stream = new MemoryStream();
			_stream.Write(headerData);
			_mpegFile = new MpegFile(_stream);
		}

		public override List<PcmFrame> ToPcmFrames(byte[] bytes)
		{
			_stream.Write(bytes);

			var samplesToRead = (_mpegFile.Length - _mpegFile.Position);
			var samplesCount = 0;

			var buffer = new float[samplesToRead];
			samplesCount += _mpegFile.ReadSamples(buffer, 0, (int) samplesToRead);
			Debug.Log($"Stream L: [{_stream.Length}] P: [{_mpegFile.Position}/{_mpegFile.Length}] , S: [{samplesCount}/{samplesToRead}]");

			var frames = WritePcmFrames(buffer);
			Debug.Log($"Created {frames.Count}");
			return frames;
		}
		
	}
}