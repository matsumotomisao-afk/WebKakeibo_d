using System.ComponentModel.DataAnnotations;

namespace WebKakeibo.Models
{
    public class PaymentType
    {
        public int PaymentTypeId { get; set; } //Navigationプロパティ構築のためには、クラス名＋Id
        [Required]
        [Display(Name = "決済名")]
        [StringLength(50)]
        public string TypeName { get; set; } = string.Empty;  // 例：現金、クレジットカード、モバイル決済
    }
}
