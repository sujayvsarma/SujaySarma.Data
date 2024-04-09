using System.Collections.Generic;

namespace SujaySarma.Data.Health.BaseTypes
{
    /// <summary>
    /// Defines the security attributes for a health-system object
    /// </summary>
    public interface IHealthObjectSecurity
    {
        /// <summary>
        /// Permit the object to be read anonymously.
        /// Default: OFF
        /// </summary>
        bool PermitAnonymousRead { get; set; }

        /// <summary>
        /// Permit the object to be deleted permanently. 
        /// If unset, will only cause a soft-delete.
        /// Default: OFF
        /// </summary>
        bool PermitPermanentDelete { get; set; }


        /// <summary>
        /// When set, audits access to this object.
        /// Default: OFF
        /// </summary>
        bool AuditAccess { get; set; }

        /// <summary>
        /// Principals (users and groups) allowed to read the object
        /// </summary>
        List<string> ReadPrincipals { get; }

        /// <summary>
        /// Principals (users and groups) allowed to modify the object. Systems/users should not assume that 
        /// a principal granted Write will necessarily be able to Read the object -- this is an invalid assumption in the Sujay Health System!
        /// </summary>
        List<string> WritePrincipals { get; }

        /// <summary>
        /// Principals (users and groups) always denied all access to the object
        /// </summary>
        List<string> DenyPrincipals { get; }

    }
}
