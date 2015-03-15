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

            TraceManager.CustomComponent.TraceInfo(string.Format("{0} - {1} - Called constructor on Base64EncoderStream class.", System.DateTime.Now, this.callToken));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            TraceManager.CustomComponent.TraceInfo(string.Format("{0} - {1} - Called Read method on Base64EncoderStream class.", System.DateTime.Now, this.callToken));

            string base64 = Convert.ToBase64String(buffer);
            byte[] bytes = Convert.FromBase64String(base64);

            var bytesread = this.s.Read(bytes, 0, bytes.Length);

            return bytesread;
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