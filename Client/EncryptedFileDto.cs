namespace Client
{
    public class EncryptedFileDto
    {
        public byte[] Data { get; set; }
        public byte[] Key { get; set; }
        public byte[] Iv { get; set; }
    }
}
