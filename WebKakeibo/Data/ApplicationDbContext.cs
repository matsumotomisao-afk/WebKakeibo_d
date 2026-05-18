using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebKakeibo.Models;


namespace WebKakeibo.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<WebKakeibo.Models.MonthlyBudget> MonthlyBudget { get; set; } = default!;
        public DbSet<WebKakeibo.Models.Payment> Payment { get; set; } = default!;
        public DbSet<WebKakeibo.Models.PaymentType> PaymentType { get; set; } = default!;
        public DbSet<WebKakeibo.Models.SubjectName> SubjectName { get; set; } = default!;

        // 科目ImageDB登録用メソッド
        public void SeedImagesFromWwwroot() // wwwroot/Images フォルダ内のPNGファイルを SubjectName テーブルに登録するメソッド
        {
            try
            {
                // wwwroot/Images の絶対パスを取得
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
                if (!Directory.Exists(imagesPath))
                {
                    Console.WriteLine($"[ERROR] 画像フォルダが存在しません: {imagesPath}");
                    return;
                }
                // PNGファイル一覧を取得
                var pngFiles = Directory.GetFiles(imagesPath, "*.png", SearchOption.TopDirectoryOnly);
                foreach (var filePath in pngFiles)
                {
                    var fileName = Path.GetFileName(filePath);
                    // DBに保存する相対パス（例: "images/sample.png"）
                    var relativePath = Path.Combine("Images", fileName).Replace("\\", "/");
                    // 重複チェック
                    if (!SubjectName.Any(s => s.ImageUrl == relativePath))
                    {
                        var subject = new SubjectName
                        {
                            //SubjectNameIdは自動生成されるため、指定しない
                            CourseName = Path.GetFileNameWithoutExtension(fileName), // ファイル名をName列に設定（拡張子なし）
                            ImageUrl = relativePath
                        };
                        SubjectName.Add(subject);  // DBに追加
                    }
                }
                SaveChanges();                    // 変更を保存
                Console.WriteLine("[INFO] 画像登録が完了しました。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] 登録処理中にエラー: {ex.Message}");
            }
        }
        public void SeedData(ApplicationDbContext db)  // データベースに初期データを挿入するメソッド
        {
            if (db.PaymentType.Any()) return; // データが既に存在する場合はスキップ

            // 起動時に挿入する初期データ
            var paymentTypes = new List<PaymentType>
             {
                 //PaymentTypeIdは自動生成されるため、指定しない
                 new PaymentType {TypeName = "現金" },
                 new PaymentType {TypeName = "モバイル決済" },
                 new PaymentType {TypeName = "クレジットカード" },
                 new PaymentType {TypeName = "電子マネー" },
                 new PaymentType{TypeName = "銀行自動引き落とし"}
             };
            db.PaymentType.AddRange(paymentTypes);// データベースに保存
            db.SaveChanges();                    // 変更を保存
        }

        public void SeedPayment(ApplicationDbContext db)
        {
            if (db.Payment.Any()) return; // データが既に存在する場合はスキップ
            // 起動時に挿入する初期データ
            var payments = new List<Payment>
            {
                //PaymentIdは自動生成されるため、指定しない
                new Payment { Posted = new DateTime(2026, 5, 05), ItemName = "電気代", PaymentName = "中国電力", Amount = 15000, PaymentTypeId = 2, SubjectNameId = 10,UserId = "c6567417-6591-40f2-9f16-d594d541807c" },
                new Payment { Posted = new DateTime(2026, 5, 06), ItemName = "スーツ", PaymentName = "山本洋服店", Amount = 75000, PaymentTypeId = 3, SubjectNameId = 13, UserId = "c6567417-6591-40f2-9f16-d594d541807c" },
                new Payment { Posted = new DateTime(2026, 5, 08), ItemName = "牛肉", PaymentName = "ABCストア", Amount = 1200, PaymentTypeId = 2, SubjectNameId = 16, UserId = "c6567417-6591-40f2-9f16-d594d541807c" },
                new Payment { Posted = new DateTime(2026, 5, 11), ItemName = "住宅ローン", PaymentName = "中国銀行", Amount = 70000, PaymentTypeId = 5, SubjectNameId = 3, UserId = "c6567417-6591-40f2-9f16-d594d541807c" },
                new Payment { Posted = new DateTime(2026, 5, 13), ItemName = "玉ねぎ、にんじん等", PaymentName = "ABCストア", Amount = 2300, PaymentTypeId = 2, SubjectNameId = 16, UserId = "c6567417-6591-40f2-9f16-d594d541807c" },
                new Payment { Posted = new DateTime(2026, 6, 05), ItemName = "ワイン、ビール", PaymentName = "ABCストア", Amount = 4300, PaymentTypeId = 2, SubjectNameId = 16, UserId = "c6567417-6591-40f2-9f16-d594d541807c" },
                new Payment { Posted = new DateTime(2026, 6, 08), ItemName = "ガス代", PaymentName = "ABCガス", Amount = 13000, PaymentTypeId = 5, SubjectNameId = 10, UserId = "c6567417-6591-40f2-9f16-d594d541807c" },
                new Payment { Posted = new DateTime(2026, 6, 11), ItemName = "住宅ローン", PaymentName = "中国銀行", Amount = 75000, PaymentTypeId = 5, SubjectNameId = 3, UserId = "c6567417-6591-40f2-9f16-d594d541807c" },
                new Payment { Posted = new DateTime(2026, 6, 15), ItemName = "キャベツ、醤油他調味料", PaymentName = "ABCストア", Amount = 5500, PaymentTypeId = 1, SubjectNameId = 16, UserId = "c6567417-6591-40f2-9f16-d594d541807c" },
                new Payment { Posted = new DateTime(2026, 6, 18), ItemName = "牛乳、ピーマン、豆腐他", PaymentName = "ABCマーケット", Amount = 6700, PaymentTypeId = 2, SubjectNameId = 16, UserId = "c6567417-6591-40f2-9f16-d594d541807c" }
             };
            db.Payment.AddRange(payments); // データベースに保存c6567417-6591-40f2-9f16-d594d541807c
            db.SaveChanges();             // 変更を保存 PaymentTypeIdとSubjectNameIdは、PaymentTypeテーブルとSubjectNameテーブルの初期データ挿入後に正しいIDを指定する必要があります。

        }
        public void getUsers(ApplicationDbContext db, string userName)
        {
            var users = db.Users.ToList();
            foreach (var user in users)
            {
                if(user.UserName == userName)
                {
                    Console.WriteLine($"ありました！！User ID: {user.Id}, User Name: {user.UserName}");
                }
               
            }
        }
    }
}

