namespace FileSystemStorage.Services
{
    public class TeeStream : Stream
    {
        private readonly Stream _source;
        private readonly Stream _copyStream;

        public TeeStream(Stream source, Stream copyStream)
        {
            _source = source;
            _copyStream = copyStream;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int bytesRead = await _source.ReadAsync(buffer, offset, count, cancellationToken);
            if (bytesRead > 0)
            {
                await _copyStream.WriteAsync(buffer, offset, bytesRead, cancellationToken);
            }
            return bytesRead;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        // Required overrides (forward to _source)
        public override bool CanRead => _source.CanRead;
        public override bool CanSeek => false; // No seeking possible
        public override bool CanWrite => false;
        public override long Length => _source.Length;
        public override long Position { get => _source.Position; set => throw new NotSupportedException(); }
        public override void Flush() => _source.Flush();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    }
}
