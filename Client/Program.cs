using Client;
using Cryptography;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

var httpClient = new HttpClient();

var rsa = System.Security.Cryptography.RSA.Create();
await SendRsaParameters();

bool programIsRunning = true;
while (programIsRunning)
{
    Console.WriteLine("Enter command:");
    string command = Console.ReadLine() ?? String.Empty;
    var commandWords = command.Split(' ');
    switch (commandWords[0])
    {
        case "genkey":
            rsa = System.Security.Cryptography.RSA.Create();
            await SendRsaParameters();
            Console.WriteLine("New key is generated!");
            break;
        case "file":
            if (commandWords.Length != 2)
            {
                Console.WriteLine("Enter valid command");
                continue;
            }

            string fileName = commandWords[1];
            EncryptedFileDto? encryptedFileDto;
            try
            {
                encryptedFileDto = await httpClient.GetFromJsonAsync<EncryptedFileDto>($"https://localhost:7037/files/{fileName}");
            }
            catch (Exception e)
            {
                encryptedFileDto = null;
            }

            if (encryptedFileDto == null)
            {
                Console.WriteLine("Failed to load file from server");
                continue;
            }

            var key = rsa.Decrypt(encryptedFileDto.Key, System.Security.Cryptography.RSAEncryptionPadding.OaepSHA256);
            var iv = rsa.Decrypt(encryptedFileDto.Iv, System.Security.Cryptography.RSAEncryptionPadding.OaepSHA256);
            var aes = new Aes(key, iv);
            var decryptedData = aes.DecryptOFB(encryptedFileDto.Data);
            Console.WriteLine(Encoding.UTF8.GetString(decryptedData));
            break;
        case "exit":
            programIsRunning = false;
            break;
        default:
            Console.WriteLine("Enter valid command");
            break;
    }
}

async Task SendRsaParameters()
{
    var rsaKeyInfo = rsa.ExportParameters(false);
    await httpClient.PostAsJsonAsync("https://localhost:7037/files/", JsonConvert.SerializeObject(rsaKeyInfo));
}
