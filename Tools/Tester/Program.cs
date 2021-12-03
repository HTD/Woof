using Woof.LinuxAdmin;

var serviceUser = UserInfo.FromName("service");
var path = "~/.net/dpapi";
var resolved = Linux.ResolveUserPath(path, serviceUser!);
Console.WriteLine(resolved);