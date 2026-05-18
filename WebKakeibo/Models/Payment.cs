using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebKakeibo.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }   //Navigationプロパティ構築のためには、クラス名＋Id
        [Required]
        [DisplayName]
        [Display(Name = "支払日")]
        public DateTime Posted { get; set; }
        [Required]
        [Display(Name = "品目名(商品名）")]      // 例：食費、交通費、光熱費など
        public string ItemName { get; set; } = string.Empty;
        [Required]
        [Display(Name = "支払先名")]
        public string PaymentName { get; set; } = string.Empty;  // 例：スーパー、コンビニ、電力会社名　等
        [Required]
        [Display(Name = "支払方法")]
        public int PaymentTypeId { get; set; }  // PaymentType クラス（テーブル）とリレーションシップを構築
        public PaymentType? PaymentTypeNavigation { get; set; }  // Navigationプロパティ
        [Required]
        [Display(Name = "金額")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]   //金額を通貨形式で表示するための属性
        public int Amount { get; set; }
        [Required]
        [Display(Name = "科目名")]
        public int SubjectNameId { get; set; }  // SubjectName クラス（テーブル）とリレーションシップを構築
        public SubjectName? SubjectNameNavigation { get; set; } // Navigationプロパティ

        [Required]
        public string? UserId { get; set; }          // IdentityUser クラス（テーブル）とリレーションシップを構築
        [ForeignKey("UserId")]                      // UserId プロパティを外部キーとして指定
        public IdentityUser? User { get; set; }
    }
}
