using Woof.DataProtection;

var plainText = "Hello, World!";
var encryptedText = DP.Protect(plainText);
var decryptedText = DP.Unprotect(encryptedText);
if (decryptedText != plainText || encryptedText == decryptedText)
    throw new InvalidOperationException("Module doesn't work!");
Console.WriteLine("It works.");