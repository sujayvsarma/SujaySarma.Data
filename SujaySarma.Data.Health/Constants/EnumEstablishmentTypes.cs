using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Constants
{
    /// <summary>
    /// Enumeration of types of actors ("people")
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumEstablishmentTypes
    {
        /// <summary>
        /// Private practice with a single doctor
        /// </summary>
        [Display(Name = "Single doctor private practice", Description = "Private practice with a single doctor")]
        SingleDoctorPractice = 0,

        /// <summary>
        /// Private practice with multiple doctors
        /// </summary>
        [Display(Name = "Multiple doctor private practice", Description = "Private practice with multiple doctors")]
        MultipleDoctorPrivatePractice,

        /// <summary>
        /// Community health centre
        /// </summary>
        [Display(Name = "Community health centre", Description = "A common health centre for the local community")]
        CommunityHealthCentre,

        /// <summary>
        /// General clinic
        /// </summary>
        [Display(Name = "General clinic", Description = "A clinic with general health services (eg: a general practitioner or GP)")]
        GeneralClinic,

        /// <summary>
        /// Specialty clinic
        /// </summary>
        [Display(Name = "Specialty clinic", Description = "A clinic for a specific need or discipline")]
        SpecialityClinic,

        /// <summary>
        /// Multi-specialty clinic
        /// </summary>
        [Display(Name = "Multi-specialty clinic", Description = "A clinic for several specific needs or disciplines")]
        MultiSpecialtyClinic,

        /// <summary>
        /// Hospital
        /// </summary>
        [Display(Name = "Hospital", Description = "A hospital")]
        Hospital,

        /// <summary>
        /// Specialty hospital
        /// </summary>
        [Display(Name = "Specialty hospital", Description = "A hospital for a specific need or discipline")]
        SpecialtyHospital,

        /// <summary>
        /// Multi-specialty hospital
        /// </summary>
        [Display(Name = "Multi-specialty hospital", Description = "A hospital for several specific needs or disciplines")]
        MultiSpecialtyHospital,

        /// <summary>
        /// Emergency centre
        /// </summary>
        [Display(Name = "Emergency centre", Description = "An establishment offering emergency services such as for accident victims")]
        EmergencyCentre,

        /// <summary>
        /// Fire services
        /// </summary>
        [Display(Name = "Fire services", Description = "Fire services to put out fires or attend to other emergencies")]
        FireServices,

        /// <summary>
        /// Law enforcement
        /// </summary>
        [Display(Name = "Law enforcement", Description = "The police or sheriff's department or other law enforcement agency")]
        LawEnforcement,

        /// <summary>
        /// Court or tribunal
        /// </summary>
        [Display(Name = "Judicial services", Description = "A court, tribunal, etc")]
        JudicialServices,

        /// <summary>
        /// Child services
        /// </summary>
        [Display(Name = "Child protection services", Description = "Services to protect the interests of children and minors")]
        ChildProtectionServices,

        /// <summary>
        /// Women services
        /// </summary>
        [Display(Name = "Women services", Description = "Services specific to girls and women")]
        WomenServices,

        /// <summary>
        /// Social services
        /// </summary>
        [Display(Name = "Social services", Description = "Other welfare organisations")]
        SocialServices,

        /// <summary>
        /// Insurance agency
        /// </summary>
        [Display(Name = "Insurance agency", Description = "An agency that offers insurance or underwriting services")]
        InsuranceAgency
    }
}
