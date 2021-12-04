new Woof.TestTerminal.Terminal {
    IsMaximized = true,
    Projects = new() {
        ["Test Service"] = "Woof.ServiceInstaller.TestService",
    }
}.Start(asAdministrator: true, "testsvc --help");