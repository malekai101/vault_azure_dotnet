using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Azure;
using VaultSharp.V1.Commons;

namespace Vault_Azure_Dotnet
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            AzureAuth clientFactory = new AzureAuth();
            IVaultClient vault_client = clientFactory.BuildVaultClient(Environment.GetEnvironmentVariable("VAULT_ADDR"), 
                Environment.GetEnvironmentVariable("VAULT_ROLE"));
            
            Secret<SecretData> kv2Secret = null;
            kv2Secret = vault_client.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: "alpha", mountPoint: "kv_auto").Result;

            var password = kv2Secret.Data.Data["wisdom"];

            Console.WriteLine(password.ToString());
        }
    }
}
