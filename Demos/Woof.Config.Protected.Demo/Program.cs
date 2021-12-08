using Microsoft.Extensions.Configuration;
using Woof.Config.Protected;
using Woof.Config.Protected.Demo;
using Woof.DataProtection;

var json = new JsonConfigProtected(DataProtectionScope.CurrentUser).Protect();
if (!json.IsProtected) Console.WriteLine("Data protection failed.");
//var config = json.Get<AppConfiguration>();
//Console.WriteLine($"Login: {config.Login}, ApiKey: {config.ApiKey}");