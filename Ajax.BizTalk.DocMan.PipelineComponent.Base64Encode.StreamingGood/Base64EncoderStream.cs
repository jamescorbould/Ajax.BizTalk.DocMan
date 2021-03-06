﻿using Microsoft.BizTalk.Streaming;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.BizTalk.CAT.BestPractices.Framework.Instrumentation;

namespace Ajax.BizTalk.DocMan.PipelineComponent
{
    public class Base64EncoderStream : Stream, IDisposable
    {
        private Guid _callToken { get; set; }
        private VirtualStream _vs { get; set; }
        private List<char> _bufferedBase64Chars { get; set; }
        private int _bufferedBase64CharsCount { get; set; }
        private int _bytesNumCopiedAlready { get; set; }
        private const int BUFFER_SIZE = 4096;
        public override long Position { get; set; }
        public override long Length { get { return this._vs.Length; } }
        public override bool CanWrite { get { return false; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanRead { get { return true; } }

        public Base64EncoderStream(Stream s, int? bufferSize)
        {
            if (bufferSize == null)
            {
                // Default buffer size for VirtualStream is 10240 bytes.
                // This constructor will default to use VirtualStream.MemoryFlag.AutoOverFlowToDisk enum value.
                _vs = new VirtualStream();
            }
            else
            {
                // This constructor will default to use VirtualStream.MemoryFlag.AutoOverFlowToDisk enum value.
                _vs = new VirtualStream(Convert.ToInt32(bufferSize), Convert.ToInt32(bufferSize));
            }

            s.CopyTo(_vs);
            _callToken = TraceManager.CustomComponent.TraceIn();
            _bufferedBase64Chars = new List<char>();
            _bytesNumCopiedAlready = 0;
            _bufferedBase64CharsCount = 0;

            TraceManager.CustomComponent.TraceInfo(string.Format("{0} - {1} - Called constructor on Base64EncoderStream class.", System.DateTime.Now, _callToken));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            TraceManager.CustomComponent.TraceInfo(string.Format("{0} - {1} - Called Read method on Base64EncoderStream class.", System.DateTime.Now, _callToken));

            try
            {
                var countBytesRead = _vs.Read(buffer, offset, count);
                byte[] bytesRead = new byte[countBytesRead];
                string base64Read = String.Empty;
                int countBytesWritten = 0;

                Array.Copy(buffer, bytesRead, countBytesRead);

                base64Read = Convert.ToBase64String(bytesRead);

                TraceManager.CustomComponent.TraceInfo(string.Format("{0} - {1} - Count of bytes read from stream = {2}", System.DateTime.Now, _callToken, countBytesRead));

                // Check if any pre-existing bytes exist in the local buffer from previous reads.
                // These need to be written to the output buffer first.
                // 1 char == 1 byte.

                TraceManager.CustomComponent.TraceInfo(string.Format("{0} - {1} - Number of bytes in local buffer = {2}", System.DateTime.Now, _callToken, _bufferedBase64CharsCount));

                if (_bufferedBase64CharsCount > 0)
                {
                    int length = Math.Min(BUFFER_SIZE, _bufferedBase64CharsCount);
                    Array.Copy(System.Text.Encoding.ASCII.GetBytes(_bufferedBase64Chars.ToArray<char>()), buffer, length);
                    _bufferedBase64Chars.RemoveRange(0, length);
                    _bufferedBase64CharsCount = _bufferedBase64Chars.Count();
                    countBytesWritten += length;

                    TraceManager.CustomComponent.TraceInfo(string.Format("{0} - {1} - Wrote {2} to local buffer.", System.DateTime.Now, _callToken, countBytesWritten));
                }

                int bufferSpaceLeft = BUFFER_SIZE - countBytesWritten;

                TraceManager.CustomComponent.TraceInfo(string.Format("{0} - {1} - Space left in buffer = {2}", System.DateTime.Now, _callToken, bufferSpaceLeft));

                // Check and write any overflow from **this** read to the local buffer.
                if (base64Read.Count() > bufferSpaceLeft)
                {
                    _bufferedBase64Chars.AddRange(base64Read.ToList<char>().GetRange(bufferSpaceLeft, (base64Read.Count() - bufferSpaceLeft)));
                    TraceManager.CustomComponent.TraceInfo(string.Format("{0} - {1} - Wrote to buffer.  Start index = {2}; end index = {3}", System.DateTime.Now, _callToken, bufferSpaceLeft, (base64Read.Count() - bufferSpaceLeft)));
                    base64Read = base64Read.Remove(bufferSpaceLeft, (base64Read.Count() - bufferSpaceLeft));
                    _bufferedBase64CharsCount = _bufferedBase64Chars.Count();
                }

                // Write bytes from **this** read, if any bytes can fit in the output buffer.
                Array.Copy(System.Text.Encoding.ASCII.GetBytes(base64Read.ToArray<char>()), buffer, base64Read.Length);
                countBytesWritten += base64Read.Length;

                TraceManager.CustomComponent.TraceInfo(string.Format("{0} - {1} - Count of bytes written = {2}", System.DateTime.Now, _callToken, countBytesWritten));

                return countBytesWritten;
            }
            catch (Exception ex)
            {
                TraceManager.CustomComponent.TraceError(ex, true, _callToken);
                throw;
            }
        }

        public override void SetLength(long value)
        {
            _vs.SetLength(value);
        }

        public override void Flush()
        {
            _vs.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _vs.Seek(offset, origin);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _vs.Write(buffer, offset, count);
        }
    }
}