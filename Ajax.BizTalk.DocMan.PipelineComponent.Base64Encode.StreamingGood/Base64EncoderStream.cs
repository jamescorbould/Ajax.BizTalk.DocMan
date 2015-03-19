using Microsoft.BizTalk.Streaming;
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
        private Guid callToken { get; set; }
        private VirtualStream s { get; set; }
        private byte[] bufferedBytes { get; set; }
        private int bufferedBytesCount { get; set; }
        private int bytesNumCopiedAlready { get; set; }
        private const int BUFFER_SIZE = 4096;
        public override long Position { get; set; }
        public override long Length { get { return this.s.Length; } }
        public override bool CanWrite { get { return false; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanRead { get { return true; } }

        public Base64EncoderStream(Stream s)
        {
            this.s = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
            s.CopyTo(this.s);
            this.callToken = TraceManager.CustomComponent.TraceIn();
            bufferedBytes = new byte[0];
            bytesNumCopiedAlready = 0;
            bufferedBytesCount = 0;

            TraceManager.CustomComponent.TraceInfo(string.Format("{0} - {1} - Called constructor on Base64EncoderStream class.", System.DateTime.Now, this.callToken));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            TraceManager.CustomComponent.TraceInfo(string.Format("{0} - {1} - Called Read method on Base64EncoderStream class.", System.DateTime.Now, this.callToken));

            var countBytesRead = this.s.Read(buffer, offset, count);

            byte[] bytesRead = new byte[countBytesRead];

            Array.Copy(buffer, bytesRead, countBytesRead);

            string base64 = Convert.ToBase64String(bytesRead);
            byte[] base64EncodedAsBytes = System.Text.Encoding.ASCII.GetBytes(base64);

            int encodedBytesCount = base64EncodedAsBytes.Count();

            if (encodedBytesCount > BUFFER_SIZE)
            {
                int countOfOverflowBytes = (base64EncodedAsBytes.Count() - BUFFER_SIZE);

                byte[] tempBytes = new byte[this.bufferedBytes.Count() + countOfOverflowBytes];

                Array.Copy(bufferedBytes, tempBytes, this.bufferedBytes.Count());

                this.bufferedBytes = new byte[bufferedBytesCount + countOfOverflowBytes];

                Array.Copy(tempBytes, this.bufferedBytes, tempBytes.Count());
                Array.Copy(base64EncodedAsBytes, BUFFER_SIZE, this.bufferedBytes, tempBytes.Count(), countOfOverflowBytes);

                bufferedBytesCount = this.bufferedBytes.Count();

                encodedBytesCount = BUFFER_SIZE;
            }

            Array.Copy(base64EncodedAsBytes, 0, buffer, 0, encodedBytesCount);

            return encodedBytesCount;
        }

        public override void SetLength(long value)
        {
            this.s.SetLength(value);
        }

        public override void Flush()
        {
            this.s.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.s.Seek(offset, origin);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.s.Write(buffer, offset, count);
        }
    }
}