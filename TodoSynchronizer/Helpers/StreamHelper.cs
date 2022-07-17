using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Helpers
{
    public class StreamHelper
    {

    }

    public class SplitStream : Stream
    {
        private long l, r, pos;
        private Stream stream;

        public long len => r - l + 1;

        public SplitStream(Stream stream, long l, long r)
        {
            this.l = l;
            this.pos = l;
            this.r = r;
            this.stream = stream;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => len;

        public override long Position { 
            get => pos; 
            set { 
                pos = value;
                if (pos > len) pos = len;
                if (pos < 0) pos = 0;
            } 
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            stream.Position = l + pos;
            return stream.Read(buffer, offset, (int)Math.Min(count, r - pos + 1));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    pos = offset;
                    break;
                case SeekOrigin.Current:
                    pos += offset;
                    break;
                case SeekOrigin.End:
                    pos = Length + offset;
                    break;
            }
            if (pos < 0)
                pos = 0;
            if (pos > Length)
                pos = Length;
            return pos;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
