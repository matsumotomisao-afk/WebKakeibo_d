using System.ComponentModel.DataAnnotations;

namespace WebKakeibo.Models
{
    public class SubjectName
    {
        public int SubjectNameId { get; set; } //Navigationプロパティ構築のためには、クラス名＋Id
        [Required]
        [StringLength(100)]
        [Display(Name = "科目名")]
        public string CourseName { get; set; } = string.Empty;
        [Required]
        [Display(Name = "科目イメージ")]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
