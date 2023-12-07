using Cryptography;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server;
using System.Text;
using RSA = System.Security.Cryptography.RSA;
using RSAParameters = System.Security.Cryptography.RSAParameters;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

RSAParameters rsaEncryptionParameters = new RSAParameters();

app.MapGet("/files/{fileName}", async (string fileName) =>
{
    if (!File.Exists($"TextFiles\\{fileName}"))
    {
        return Results.BadRequest("No such file");
    }

    string fileText = await File.ReadAllTextAsync($"TextFiles\\{fileName}");
    var key = GenerateBytes(16);
    var iv = GenerateBytes(16);
    var aes = new Aes(key, iv);

    var rsa = RSA.Create();
    rsa.ImportParameters(rsaEncryptionParameters);
    key = rsa.Encrypt(key, System.Security.Cryptography.RSAEncryptionPadding.OaepSHA256);
    iv = rsa.Encrypt(iv, System.Security.Cryptography.RSAEncryptionPadding.OaepSHA256);

    byte[] encryptedFileData = aes.EncryptOFB(Encoding.ASCII.GetBytes(fileText));
    return Results.Ok(new EncryptedFileDto { Data = encryptedFileData, Key = key, Iv = iv });
});

app.MapPost("/files", ([FromBody] string rsaParametersJson) =>
{
    rsaEncryptionParameters = JsonConvert.DeserializeObject<RSAParameters>(rsaParametersJson);
    return Results.Ok();
});

app.Run();

byte[] GenerateBytes(int amount)
{
    var result = new byte[amount];
    var random = new Random();
    random.NextBytes(result);
    return result;
}
