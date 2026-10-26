using System;
using System.ComponentModel.DataAnnotations;

namespace OLTPApp.Models
{
    public enum MedicationStatus
    {
        Active,
        Stopped
    }

    public class MedicationItem
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Please enter the medication name.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Please enter the dosage.")]
        public string Dose { get; set; } = "";

        [Required(ErrorMessage = "Please enter the frequency.")]
        public string Frequency { get; set; } = "";

        [Required(ErrorMessage = "Please enter the prescriber's name.")]
        public string PrescribedBy { get; set; } = "";

        [Required(ErrorMessage = "Please select a start date.")]
        public DateOnly? StartDate { get; set; }

        [Required(ErrorMessage = "Please select the next refill date.")]
        public DateOnly? NextRefillDate { get; set; }

        [Required(ErrorMessage = "Please select a medication status.")]
        public MedicationStatus? Status { get; set; } = MedicationStatus.Active;
    }
}