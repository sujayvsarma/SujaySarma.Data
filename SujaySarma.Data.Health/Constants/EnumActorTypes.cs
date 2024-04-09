using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Constants
{
    /// <summary>
    /// Enumeration of types of actors ("people")
    /// </summary>
    [Flags]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumActorTypes
    {
        /// <summary>
        /// Anonymous or unknown person
        /// </summary>
        [Display(Name = "Anonymous", Description = "Anonymous or unknown person")]
        Anonymous = 0,

        /// <summary>
        /// Patient
        /// </summary>
        [Display(Name = "Patient", Description = "Patient")]
        Patient = 1,

        /// <summary>
        /// Guardian to a patient
        /// </summary>
        [Display(Name = "Patient guardian", Description = "Guardian to a patient")]
        PatientGuardian = 2,

        /// <summary>
        /// Dependant of a patient
        /// </summary>
        [Display(Name = "Patient dependant", Description = "Dependant of a patient")]
        PatientDependant = 4,

        /// <summary>
        /// Visitor for a patient
        /// </summary>
        [Display(Name = "Patient visitor", Description = "Visitor for a patient")]
        PatientVisitor = 8,

        /// <summary>
        /// Usually means a doctor
        /// </summary>
        [Display(Name = "Medical practitioner", Description = "Usually means a doctor")]
        MedicalPractitioner = 16,

        /// <summary>
        /// An assistant to a medical practitioner (eg: nurse, secretary, etc)
        /// </summary>
        [Display(Name = "Medical practitioner's assistant", Description = "An assistant to a medical practitioner (eg: nurse, secretary, etc)")]
        MedicalPractitionerAssistant = 32,

        /// <summary>
        /// A nurse (not attached to a medical practitioner)
        /// </summary>
        [Display(Name = "General nurse", Description = "A nurse (not attached to a medical practitioner)")]
        GeneralNurse = 64,

        /// <summary>
        /// A member of the hospital, clinic or other establishment, other than an MedicalPractitioner, MedicalPractitionerAssistant or GeneralNurse.
        /// </summary>
        [Display(Name = "Establishment member", Description = "A member of the hospital, clinic or other establishment, other than an MedicalPractitioner, MedicalPractitionerAssistant or GeneralNurse")]
        EstablishmentMember = 128,

        /// <summary>
        /// A policeman
        /// </summary>
        [Display(Name = "Law enforcement officer", Description = "A policeman")]
        LawEnforcementOfficer = 256,

        /// <summary>
        /// A law-enforcement establishment (police department, sheriff's department, etc)
        /// </summary>
        [Display(Name = "Law enforcement establishment", Description = "A law-enforcement establishment (police department, sheriff's department, etc)")]
        LawEnforcementEstablishment = 512,

        /// <summary>
        /// A lawyer or jurist
        /// </summary>
        [Display(Name = "Lawyer", Description = "Lawyer or advocate")]
        Lawyer = 1024,

        /// <summary>
        /// Office of a lawyer
        /// </summary>
        [Display(Name = "Lawyer's office", Description = "Offices of a lawyer (assistants and aides of a lawyer)")]
        LawyersOffice = 2048,

        /// <summary>
        /// A court, tribunal, etc
        /// </summary>
        [Display(Name = "Judicial establishment", Description = "A court, tribunal, etc")]
        JudicialEstablishment = 4096,

        /// <summary>
        /// Insurance or underwriting agent
        /// </summary>
        [Display(Name = "Insurance or underwriting agent", Description = "Insurance or underwriting agent")]
        InsuranceAgent = 8192,

        /// <summary>
        /// The insurance or underwriting agency
        /// </summary>
        [Display(Name = "Insurance or underwriting agency", Description = "Insurance or underwriting agency")]
        InsuranceEstablishment = 16384,


    }
}
