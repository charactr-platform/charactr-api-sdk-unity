using System;
using System.Collections.Generic;
using Gemelo.Voice.Audio;

namespace Gemelo.Voice.Streaming
{
	internal class PcmDataProvider
	{
		private readonly Queue<byte[]> _dataQueue;
		private readonly Queue<PcmFrame> _pcmFrames;
			
		private PcmFrame _currentPcmFrame;
		
		public PcmDataProvider()
		{
			_dataQueue = new Queue<byte[]>();
			_pcmFrames = new Queue<PcmFrame>();
			CreateNewPcmFrame();
		}

		public void AddRawData(byte[] data)
		{
			lock (_dataQueue)
			{
				_dataQueue.Enqueue(data);
			}
		}

		public bool HasData()
		{
			lock (_dataQueue)
			{
				return _dataQueue.Count > 0;
			}
		}

		public void ReadHeaderData(out byte[] header)
		{
			lock (_dataQueue)
			{
				header = _dataQueue.ToArray()[0];
			}
		}
			
		public bool FillPcmFramesBuffer(out List<PcmFrame> pcmFrames)
		{
			bool framesFound = false;
			pcmFrames = new List<PcmFrame>();
				
			lock (_dataQueue)
			{
				while (HasData())
				{
					CreateFrameData(_dataQueue.Dequeue());
						
					for (int i = 0; i < _pcmFrames.Count; i++)
					{
						if (_pcmFrames.TryDequeue(out var frame))
							pcmFrames.Add(frame);
					}

					framesFound = pcmFrames.Count > 0;
				}

				return framesFound;
			}
		}
			
			
		private void CreateFrameData(Span<byte> data)
		{
			if (!_currentPcmFrame.AddData(data.ToArray(), out var overflow))
				return;
			
			_pcmFrames.Enqueue(_currentPcmFrame);
			
			CreateNewPcmFrame();
			CreateFrameData(overflow);
		}
			
		private void CreateNewPcmFrame()
		{
#if UNITY_WEBGL && !UNITY_EDITOR
			_currentPcmFrame = new PcmFrame(WebGlAudioBufferProcessor.BufferSize);
#else
			_currentPcmFrame = new PcmFrame();
#endif
		}

		public bool ReadLastFrame(out PcmFrame frame)
		{
			frame = null;

			if (!_currentPcmFrame.HasData)
				return false;
			
			_currentPcmFrame.WriteSamples(true);
			frame = _currentPcmFrame;
			return true;
		}

		public void Dispose()
		{
			lock (_dataQueue)
			{
				_dataQueue.Clear();
				_pcmFrames.Clear();
			}
		}
	}
}