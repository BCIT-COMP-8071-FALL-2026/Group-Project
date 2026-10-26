using System;
using System.ComponentModel.DataAnnotations;

namespace OLTPApp.Models
{
    public enum MedicalConditionStatus
    {
        Active,
        Stopped
    }
    public class MedicalConditionItem
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Please enter the condition name.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Please select the diagnosis date.")]
        public DateOnly? DiagnosisDate { get; set; }

        [Required(ErrorMessage = "Please enter the condition description.")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Please select a status.")]
        public MedicalConditionStatus? Status { get; set; }
            = MedicalConditionStatus.Active;
    }
}
