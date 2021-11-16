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
            Console.WriteLine("Beginning run.");
            string vaultAddr = Environment.GetEnvironmentVariable("VAULT_ADDR");
            if(String.IsNullOrEmpty(vaultAddr))
            {
                throw new System.ArgumentNullException("Vault Address", 
                    message: "The VAULT_ADDR environmental variable must be set.");
            }

            string roleName = Environment.GetEnvironmentVariable("VAULT_ROLE");
            if(String.IsNullOrEmpty(roleName))
            {
                throw new System.ArgumentNullException("Vault Role Name",
                    message: "The VAULT_ROLE environmental variable must be set.");
            }
            
            AzureAuth clientFactory = new AzureAuth();
            IVaultClient vault_client = clientFactory.BuildVaultClient(vaultAddr, roleName);
            vault_client.V1.Auth.PerformImmediateLogin();
            Console.WriteLine("The vault client has successfully logged in.");
            
            Secret<SecretData> kv2Secret = null;
            kv2Secret = vault_client.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: "alpha", mountPoint: "kv_auto").Result;

            var password = kv2Secret.Data.Data["wisdom"];

            Console.WriteLine(password.ToString());
        }
    }
}
