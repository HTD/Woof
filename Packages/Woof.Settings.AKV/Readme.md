# Woog.Settings.AKV

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2022 by CodeDog, All rights reserved.

---

## About

An extension of the [`Woof.Settings`](../Woof.Settings/Readme.md) package that
allows some settings values to be not read directly from JSON file,
but from the Azure Key Vault secrets.

This achieves following important goals:
- sensitive data are not distributed with the program or its configuration,
- the access to the vault secrets can be revoked at any time,
- the data can be changed remotely at any time,
- the access to the vault can be limited to the configured end points,
- the access to the data from the code is trivial,
- the AKV configuration is trivial,
- sensitive data stored by the application can be easily encrypted and
  decrypted using a key stored remotely on AKV.

**Security considerations:**

Anyone having the appropriate `.access.json` file can access vault secretes.
That file **CANNOT** be made public or accessible to anyone but intended user.

To mitigate the vulnerability additional steps must be taken.

In order not to store that sensitive file in plain text,
the data protection service is used.

Windows systems have such service built in that allows to encrypt the sensitive
data with either the user's private key, or the machine private key.

The data can be decrypted only by that user, or on the same machine.

On Linux systems there is .NET support for data protection, but the keys
must be created and managed by the application.

The keys are also sensitive data that must not be accessible for unauthorized
users.

## Usage

The user of this package should have an active Microsoft Azure subscription
and some knowledge about what the Azure Key Vault is and how it works.

If the main application executable file is named `test`, then
the AKV settings should be placed in `test.access.json` file.

It is referred as `.access.json` further.

The empty file should have following structure:
```json
{
    "VaultUri": "",
    "DirectoryId": "",
    "ApplicationId": "",
    "ClientSecret": "",
    "CredentialsEncodingKeyName": ""
}
```

### Azure Active Directory configuration

- Click `App registrations`.
- Click `New registration`.
- Enter the name of the application.
- Select support account types (not important here).
- Click `Register`.
- Copy `Application ID` to the `.access.json` file as `ApplicationId`.
- Click `Certificates and secrets`.
- Click `New client secret`.
- Copy the generated secret to the `.access.json` file as `ClientSecret`

### Azure Key Vault configuration

- Create a new Key Vault.
- Click `Access policies`.
- Click `Add access policy`.
- Select principal as the application registered in previous step.
- Add `Get Sercret Permission`.
- From `Overview` section copy `Vault Uri` and `Directory ID` to
  `VaultUri` and `DirectoryId` `.access.json` properties.
- Click `Secrets`.
- Click `Generate/Import`.
- Enter the secret's name, referred later as `CredentialsEncodingKey`.
- In Visual Studio C# Interactive window enter:
  ```cs
  Convert.ToBase64String(System.Security.Cryptography.Aes.Create().Key)
  ```
- Copy the result to the clipboard.
- Paste it as the secret's value in AKV panel.
- Copy the `CredentialsEncodingKey` name to the `CredentialsEncodingKeyName`
  property of the `.access.json` file.

### C# code

- Create the program configuration as a record, for example:
  ```cs
  internal class Settings : JsonSettingsAkv<Settings> {

      public static Settings Default { get; } = new Settings();
      
      public string MyDirectValue { get; init; }
      
      [AKV("SecretName")]
      public string MySensitiveValue { get; init; }

  }
  ```
- Create program configuration JSON, for example:
  ```json
  {
      "myDirectValue": "test"
  }
  ```
- Load the configuration like this:
  ```cs
  await Settings.Default.LoadAsync();
  ```
- If the secret with name "SecretName" is created in AKV, its value can be
  accessed as `Settings.Default.MySensitiveValue`.
- If anything is done wrong, an exception will be thrown or the values will
  be `null`.

For advanced features like credentials encryption see the demo provided.
Also read the documentation of the `Woof.Settings`, `Woof.DataProtection`
and `Woof.DataProtection.Linux`.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.