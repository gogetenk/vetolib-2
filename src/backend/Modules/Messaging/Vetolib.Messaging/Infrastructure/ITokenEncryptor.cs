namespace Vetolib.Messaging.Infrastructure;

internal interface ITokenEncryptor
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
