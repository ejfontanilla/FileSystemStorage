using System.Security.Cryptography;

namespace FileSystemStorage.Services
{
    public class SHA256Stream : Stream
    {
        private readonly Stream _baseStream;
        private readonly SHA256 _sha256;
        public byte[] Hash => _sha256.Hash;

        public SHA256Stream(Stream baseStream)
        {
            _baseStream = baseStream;
            _sha256 = SHA256.Create();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = _baseStream.Read(buffer, offset, count);
            if (bytesRead > 0)
                _sha256.TransformBlock(buffer, offset, bytesRead, buffer, offset);
            return bytesRead;
        }

        public override void Close()
        {
            _sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            base.Close();
        }

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => _baseStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    }
}
