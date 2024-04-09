using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.BaseTypes
{
    /// <summary>
    /// Extends the <see cref="HealthObjectBase"/> by adding implementation of <see cref="IHealthObjectSecurity"/>
    /// </summary>
    public class HealthObjectSecurityBase : HealthObjectBase, IHealthObjectSecurity
    {

        /// <summary>
        /// Permit the object to be read anonymously.
        /// Default: OFF
        /// </summary>
        [JsonPropertyName("permit.AnonRead")]
        public bool PermitAnonymousRead { get; set; }

        /// <summary>
        /// Permit the object to be deleted permanently. 
        /// If unset, will only cause a soft-delete.
        /// Default: OFF
        /// </summary>
        [JsonPropertyName("permit.HardDelete")]
        public bool PermitPermanentDelete { get; set; }


        /// <summary>
        /// When set, audits access to this object.
        /// Default: OFF
        /// </summary>
        [JsonPropertyName("audit.access")]
        public bool AuditAccess { get; set; }


        /// <summary>
        /// Principals (users and groups) allowed to read the object
        /// </summary>
        [JsonPropertyName("acl.read")]
        public List<string> ReadPrincipals { get; }

        /// <summary>
        /// Principals (users and groups) allowed to modify the object. Systems/users should not assume that 
        /// a principal granted Write will necessarily be able to Read the object -- this is an invalid assumption in the Sujay Health System!
        /// </summary>
        [JsonPropertyName("acl.write")]
        public List<string> WritePrincipals { get; }

        /// <summary>
        /// Principals (users and groups) always denied all access to the object
        /// </summary>
        [JsonPropertyName("acl.deny")]
        public List<string> DenyPrincipals { get; }

        /// <summary>
        /// Initialise
        /// </summary>
        protected HealthObjectSecurityBase()
            : base()
        {
            PermitAnonymousRead = false;
            PermitPermanentDelete = false;
            AuditAccess = false;

            ReadPrincipals = new();
            WritePrincipals = new();
            DenyPrincipals = new();
        }


        /// <summary>
        /// Encrypt the information
        /// </summary>
        /// <param name="contextualKey">Key to be used for encryption</param>
        /// <param name="information">Information to be encrypted</param>
        /// <returns>Encrypted information</returns>
        public string Encrypt(string contextualKey, string information)
        {
            byte[] clearBytes = Encoding.UTF8.GetBytes(information);
            Rfc2898DeriveBytes saltBytes = new(contextualKey, 32, 3, HashAlgorithmName.SHA1);

            Aes crypt = Aes.Create();
            crypt.Key = saltBytes.GetBytes(32);
            crypt.IV = saltBytes.GetBytes(16);

            string hash = string.Empty;
            using (MemoryStream m = new())
            {
                using (CryptoStream cs = new(m, crypt.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                }

                hash = Convert.ToBase64String(m.ToArray());
            }

            return hash;
        }

        /// <summary>
        /// Decrypt the information
        /// </summary>
        /// <param name="contextualKey">Key to be used for decryption</param>
        /// <param name="information">Information to be decrypted</param>
        /// <returns>Decrypted information</returns>
        public string Decrypt(string contextualKey, string information)
        {
            byte[] codedBytes = Convert.FromBase64String(information);
            Rfc2898DeriveBytes saltBytes = new(contextualKey, 32, 3, HashAlgorithmName.SHA1);

            Aes crypt = Aes.Create();
            crypt.Key = saltBytes.GetBytes(32);
            crypt.IV = saltBytes.GetBytes(16);

            string hash = string.Empty;
            using (MemoryStream m = new())
            {
                using (CryptoStream cs = new(m, crypt.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(codedBytes, 0, codedBytes.Length);
                }

                hash = Encoding.UTF8.GetString(m.ToArray());
            }

            return hash;
        }



        /// <summary>
        /// Returns if the principal is allowed to read the object
        /// </summary>
        /// <param name="principalName">Name of principal to check</param>
        /// <returns>True if allowed</returns>
        public bool IsAllowedRead(string principalName)
            => string.IsNullOrWhiteSpace(principalName) && PermitAnonymousRead
                    || !DenyPrincipals.Contains(principalName) && ReadPrincipals.Contains(principalName);

        /// <summary>
        /// Returns if the principal is allowed to write/modify the object
        /// </summary>
        /// <param name="principalName">Name of principal to check</param>
        /// <returns>True if allowed</returns>
        public bool IsAllowedWrite(string principalName)
            => !string.IsNullOrWhiteSpace(principalName) && !DenyPrincipals.Contains(principalName) && WritePrincipals.Contains(principalName);
    }
}
