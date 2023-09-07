using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Gemelo.Voice.Audio
{
	public class PcmFrame: IDisposable
	{
		public static int BlockSize = sizeof(short);//16bit per sample 
		public int SamplesSize => _samplesSize;
		public float[] Samples => _samples.ToArray();
		public bool HasData => _samples.Count > 0;
		
		private List<float> _samples;
		private readonly int _samplesSize;

		public PcmFrame(int samplesCount = 4096)
		{
			_samplesSize = samplesCount;
			_samples = new List<float>();
		}
			
		public bool AddPcmData(ArraySegment<byte> data, out byte[] overflow)
		{
			float[] samples = null;

			var initialSize = _samples.Count;
			var offset = (SamplesSize * BlockSize) - (initialSize * BlockSize);
			
			if (data.Count >= offset)
			{
				ConvertByteToFloat(data.Slice(0,offset).ToArray(), out samples);
				overflow = data.Slice(offset, data.Count - offset).ToArray();
				AddPcmData(samples, out _);
				return true;
			}
			
			ConvertByteToFloat(data.ToArray(), out samples);
			overflow = null;
			AddPcmData(samples, out _);
			
			return false;
		}

		public bool AddPcmData(ArraySegment<float> pcm, out float[] overflow)
		{
			var initialSize = _samples.Count;
			var offset = SamplesSize - initialSize;
			
			if (pcm.Count >= offset)
			{
				_samples.AddRange(pcm.Slice(0, offset));

				overflow = pcm.Slice(offset, pcm.Count - offset).ToArray();
			
				Debug.Log($"[AddPcmData ] Samples size: {initialSize} -> {_samples.Count}, Offset data: {overflow.Length}, PcmIn: {pcm.Count},  Offset: {offset}");
				return true;
			}
			
			_samples.AddRange(pcm);

			overflow = Array.Empty<float>();
			return false;
		}
		
		public static void ConvertByteToFloat(byte[] data, out float[] waveData, int offset = 0)
		{
			var i = 0;
			
			var samplesSize = (data.Length - offset) / BlockSize;
			waveData = new float[samplesSize];

			while (i < samplesSize)
			{
				var pos = (i * BlockSize) + offset;
				var v = (float)BitConverter.ToInt16(data, pos) / short.MaxValue;
				waveData[i] = v;
				++i;
			}

			data = null;
		}

		public void Dispose()
		{
			_samples = null;
			GC.Collect();
		}
	}
}