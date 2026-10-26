using System.ComponentModel.DataAnnotations;

namespace OLTPApp.ViewModels.Admin
{
    public class DeactivationChoiceViewModel
    {
        public int SeniorID { get; set; }

        [Required(ErrorMessage = "Please select an action.")]
        public string ActionType { get; set; } = "";
    }

    public class ConfirmDeactivationViewModel
    {
        public int SeniorID { get; set; }

        [Required]
        public string ActionType { get; set; } = "";

        [Required(ErrorMessage = "Please enter required name to confirm.")]
        public string ConfirmationUsername { get; set; } = "";
    }
}
