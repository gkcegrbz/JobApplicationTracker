using System.ComponentModel.DataAnnotations;
namespace JobApplicationTracker.Models;

public class Application
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Firma adı zorunludur")]
    [Display(Name = "Firma Adı")]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pozisyon zorunludur")]
    [Display(Name = "Pozisyon")]
    [StringLength(200)]
    public string Position { get; set; } = string.Empty;

    [Required(ErrorMessage = "Konum zorunludur")]
    [Display(Name = "Konum")]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Display(Name = "Maaş")]
    [StringLength(100)]
    public string? Salary { get; set; }

    [Required(ErrorMessage = "Başvuru tarihi zorunludur")]
    [Display(Name = "Başvuru Tarihi")]
    [DataType(DataType.Date)]
    public DateTime ApplicationDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Durum zorunludur")]
    [Display(Name = "Durum")]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Başvuruldu;

    [Display(Name = "Notlar")]
    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    [Display(Name = "İlgili Kişi")]
    [StringLength(200)]
    public string? ContactPerson { get; set; }

    [Display(Name = "E-posta")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [StringLength(200)]
    public string? ContactEmail { get; set; }

    [Display(Name = "Telefon")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    [StringLength(20)]
    public string? ContactPhone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}

public enum ApplicationStatus
{
    Başvuruldu,
    Mülakat,
    Red,
    Kabul
}