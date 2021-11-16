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
    public class AzureAuth
    {
        public class InstanceMetadata
        {
            public string name { get; set; }
            public string resourceGroupName { get; set; }
            public string subscriptionId { get; set; }
        }

        const string MetadataEndPoint = "http://169.254.169.254/metadata/instance?api-version=2017-08-01";
        const string AccessTokenEndPoint = "http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=https://management.azure.com/";

        /// <summary>
        /// Builds a Vault client using the Azure MSI.
        /// </summary>
        public IVaultClient BuildVaultClient(string vaultAddr, string roleName)
        {
            string jwt = GetJWT();
            InstanceMetadata metadata = GetMetadata();

            IAuthMethodInfo authMethod = new AzureAuthMethodInfo(roleName: roleName, jwt: jwt, subscriptionId: metadata.subscriptionId, resourceGroupName: metadata.resourceGroupName, virtualMachineName: metadata.name);
            var vaultClientSettings = new VaultClientSettings(vaultAddr, authMethod);

            IVaultClient vaultClient = new VaultClient(vaultClientSettings);
            return vaultClient;
        }

        /// <summary>
        /// Query Azure Resource Manage for metadata about the Azure instance
        /// </summary>
        private InstanceMetadata GetMetadata()
        {
            HttpWebRequest metadataRequest = (HttpWebRequest)WebRequest.Create(MetadataEndPoint);
            metadataRequest.Headers["Metadata"] = "true";
            metadataRequest.Method = "GET";

            HttpWebResponse metadataResponse = (HttpWebResponse)metadataRequest.GetResponse();

            StreamReader streamResponse = new StreamReader(metadataResponse.GetResponseStream());
            string stringResponse = streamResponse.ReadToEnd();
            var resultsDict = JsonConvert.DeserializeObject<Dictionary<string, InstanceMetadata>>(stringResponse);

            return resultsDict["compute"];
        }

        /// <summary>
        /// Query Azure Resource Manager (ARM) for an access token
        /// </summary>
        private string GetJWT()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(AccessTokenEndPoint);
            request.Headers["Metadata"] = "true";
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // Pipe response Stream to a StreamReader and extract access token
            StreamReader streamResponse = new StreamReader(response.GetResponseStream());
            string stringResponse = streamResponse.ReadToEnd();
            var resultsDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringResponse);

            return resultsDict["access_token"];
        }
    }
}
