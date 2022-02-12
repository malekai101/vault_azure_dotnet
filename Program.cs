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
using VaultSharp.V1.SecretsEngines.PKI;

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
            
            /*
            Secret<SecretData> kv2Secret = null;
            kv2Secret = vault_client.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: "funcdata", mountPoint: "kv_func").Result;

            var password = kv2Secret.Data.Data["universe"];
            Console.WriteLine(password.ToString());
            */
            
            string certCommonName = Environment.GetEnvironmentVariable("VAULT_COMMON_NAME");
            if(String.IsNullOrEmpty(certCommonName))
            {
                Console.WriteLine("The VAULT_COMMON_NAME variable is not set.  Using alpha.");
                certCommonName = "alpha";
            }

            const string pkiRoleName = "app";
            var certificateCredentialsRequestOptions = new CertificateCredentialsRequestOptions();
            certificateCredentialsRequestOptions.CommonName = $"{certCommonName}.acme-app.com";
            try
            {
                var certSecret = vault_client.V1.Secrets.PKI.GetCredentialsAsync(pkiRoleName, certificateCredentialsRequestOptions);
                string privateKeyContent = certSecret.Result.Data.CertificateContent;
                Console.Write("Certificate content:");
                Console.WriteLine(privateKeyContent);
            }
            catch (Exception e)
            {
                Console.Write("Certificate generation failed.");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
