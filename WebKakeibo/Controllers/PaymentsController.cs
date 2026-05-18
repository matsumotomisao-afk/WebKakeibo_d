using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebKakeibo.Data;
using WebKakeibo.Models;
using Microsoft.AspNetCore.Authorization; // Authorize属性を使用するための名前空間
using System.Security.Claims;  // ユーザーのクレームを扱うための名前空間

namespace WebKakeibo.Controllers
{
    [Authorize] // 認証が必要なコントローラーであることを示す属性
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public void GenerateChartData(IQueryable<Payment> payments) 
        {
            var xData = payments
                .Where(x => x.SubjectNameNavigation != null)      // SubjectNameNavigationがnullでないデータに絞り込む
                .GroupBy(x => x.SubjectNameNavigation!.CourseName)  // SubjectNameNavigationのCourseNameでグループ化する
                .Select(g => new                                     // グループ化したデータから、ラベルと合計金額を抽出する
                {
                    labels = g.Key,                           // グループ化のキー（CourseName）がラベルになる
                    data = g.Sum(x => x.Amount)               // グループ内のAmountを合計してデータにする
                })
                .ToList();
            // ラベルとデータを分離
            var xlabels = xData.Select(x => x.labels).ToList();  //各要素 x から labels プロパティだけを取り出す。
            var xdata = xData.Select(x => x.data).ToList();   // xdata には、xData の全要素から data だけを集めたリストが入ります。
                                                              //それぞれの.ToList()は抽出結果を List型 に変換。
            ViewBag.Labels = xlabels;                        //ViewBagにxlabelsを格納する
            ViewBag.Data = xdata;                            //ViewBagにxdataを格納する
        }

        // GET: Payments
        public async Task<IActionResult> Index(int? num)
        {
            var today = DateTime.Today;                //今日の日付

            //-------------------------------------------------------For Generate LinkButton
                                                       //Paymentテーブルのposte列の月名をDistinctする（重複排除：例：８月に３件、９月に２件あっても、８，９、が出力される）

            var dstnctPost = _context.Payment
                             .Where(p => p.Posted.Year == today.Year) //今年のデータを抽出する
                             .Select(p => p.Posted.Month)             //月名だけ抽出(Monthのデータ）
                             .Distinct()               //重複を排除
                             .OrderBy(m => m)            //昇順ソート
                             .ToList();                //List化する


            if (dstnctPost != null)
            {

                ViewBag.DstnctPost = dstnctPost;   //本チュートリアルでは、Listの中に１，２、が入っている。
            }
            //-------------------------------------------------------End For Generate LinkButton


            if (num == null)
            {
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1); //今日の日付から今月の1日を取得
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1); //今月の末日を取得

                ViewBag.CurrentMonth = today.Month; //現在の月をViewBagに格納する（例：８月）

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // 現在のユーザーのIDを取得する
                var applicationDbContext = _context.Payment
                    .Include(p => p.PaymentTypeNavigation)
                    .Include(p => p.SubjectNameNavigation)
                    .Include(p => p.User)
                    .Where(p => p.UserId == userId) // 現在のユーザーの支払いのみを取得する
                   .Where(p => p.Posted >= firstDayOfMonth && p.Posted <= lastDayOfMonth);  //さらに、支払日が今月の1日から末日までのデータに絞り込む
                GenerateChartData(applicationDbContext);
                //-----------------For Chart
                //var xData = applicationDbContext
                //    .Where(x => x.SubjectNameNavigation != null)      // SubjectNameNavigationがnullでないデータに絞り込む
                //    .GroupBy(x => x.SubjectNameNavigation!.CourseName)  // SubjectNameNavigationのCourseNameでグループ化する
                //    .Select(g => new                                     // グループ化したデータから、ラベルと合計金額を抽出する
                //    {
                //        labels = g.Key,                           // グループ化のキー（CourseName）がラベルになる
                //        data = g.Sum(x => x.Amount)               // グループ内のAmountを合計してデータにする
                //    })
                //    .ToList();
                //// ラベルとデータを分離
                //var xlabels = xData.Select(x => x.labels).ToList();  //各要素 x から labels プロパティだけを取り出す。
                //var xdata = xData.Select(x => x.data).ToList();   // xdata には、xData の全要素から data だけを集めたリストが入ります。
                //                                                  //それぞれの.ToList()は抽出結果を List型 に変換。
                //ViewBag.Labels = xlabels;                        //ViewBagにxlabelsを格納する
                //ViewBag.Data = xdata;                            //ViewBagにxdataを格納する

                //-----------------End For Chart
                //-------現在月の予算（Budget）を取得,チャート用データ
                var budget = _context.MonthlyBudget
                      .Where(b => b.UserId == userId)
                      .Where(b => b.YearNum == today.Year && b.MonthNum == today.Month) //今年、今月でフィルターする
                      .Select(b => b.BudgetAmount)    //今年、今月の予算額を取得
                      .FirstOrDefault();
                ViewBag.ThisMonthBudget = budget;
                //-----------End 予算取得


                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                var firstDayOfMonth = new DateTime(today.Year, num.Value, 1);    //num月の1日を取得
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);  //num月の末日を取得
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);    //現在のログインユーザーを取得

                ViewBag.CurrentMonth = num.Value; 　　　　//LinkButtonの表示月をViewBagに格納する

                var applicationDbContext = _context.Payment
                    .Include(p => p.PaymentTypeNavigation)   //PaymentテーブルからPaymentTypeテーブル、を結合してデータを取得する
                    .Include(p => p.SubjectNameNavigation)  //Paymentテーブルから、SubjectNameテーブル、を結合してデータを取得する
                    .Include(p => p.User)          //Paymentテーブルから、AspNetUsersテーブルを結合してデータを取得する
                    .Where(p => p.UserId == userId)   //現在のログインユーザーのデータに絞り込む
                    .Where(p => p.Posted >= firstDayOfMonth && p.Posted <= lastDayOfMonth);  //さらに、支払日がnumヶ月前の1日から末日までのデータに絞り込む
                GenerateChartData(applicationDbContext);
                //--------------------------------For Chart
                //var xData = applicationDbContext
                //   .Where(x => x.SubjectNameNavigation != null)
                //   .GroupBy(x => x.SubjectNameNavigation!.CourseName)
                //   .Select(g => new
                //   {
                //       labels = g.Key,
                //       data = g.Sum(x => x.Amount)
                //   })
                //   .ToList();
                //// ラベルとデータを分離
                //var xlabels = xData.Select(x => x.labels).ToList();
                //var xdata = xData.Select(x => x.data).ToList();

                //ViewBag.Labels = xlabels;
                //ViewBag.Data = xdata;
                //-------------------------End For Chart
                //-------選択月の予算（Budget）を取得,チャート用データ
                var budget = _context.MonthlyBudget
                      .Where(b => b.UserId == userId)
                      .Where(b => b.YearNum == today.Year && b.MonthNum == num.Value)
                      .Select(b => b.BudgetAmount)
                      .FirstOrDefault();
                ViewBag.ThisMonthBudget = budget;
                //-----------End 予算取得


                return View(await applicationDbContext.ToListAsync());
            }
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payment
                .Include(p => p.PaymentTypeNavigation)
                .Include(p => p.SubjectNameNavigation)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PaymentId == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Payments/Create
        public IActionResult Create()
        {
            ///////////////////////////////////////////////////////今月の支払い履歴を表示するためのコード
            var today = DateTime.Today;                //今日の日付
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1); //今年の今月の1日を取得
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1); //今月の末日を取得

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);  //現在のログインユーザーを取得
            var applicationDbContext = _context.Payment
                .Include(p => p.SubjectNameNavigation)
                .Include(p => p.PaymentTypeNavigation)
                .Include(p => p.User)                   //Paymentテーブルから、SubjectNameテーブル、PaymentTypeテーブル、AspNetUsersテーブルを結合してデータを取得する
                .Where(p => p.UserId == userId)         //現在のログインユーザーのデータに絞り込む
                .Where(p => p.Posted >= firstDayOfMonth && p.Posted <= lastDayOfMonth);  //さらに、支払日が今月の1日から末日までのデータに絞り込む

            ViewData["PaymentHistory"] = applicationDbContext.ToList();//ToList() を呼び出すとSQLが実行されます取得したデータをViewData[PaymentHistory]にobject化する

            //////////////////////////////////////////今月の支払い履歴を表示するためのコード　終了
            ///
            ViewData["PaymentTypeId"] = new SelectList(_context.Set<PaymentType>(), "PaymentTypeId", "TypeName");// PaymentTypeクラスのPaymentTypeIdプロパティを値として、TypeNameプロパティを表示テキストとして使用するSelectListを作成し、ViewDataに格納する
            ViewData["SubjectNameId"] = new SelectList(_context.Set<SubjectName>(), "SubjectNameId", "CourseName");// SubjectNameクラスのSubjectNameIdプロパティを値として、CourseNameプロパティを表示テキストとして使用するSelectListを作成し、ViewDataに格納する
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");// IdentityUserクラスのIdプロパティを値として、Idプロパティを表示テキストとして使用するSelectListを作成し、ViewDataに格納する
            return View();
        }

        // POST: Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PaymentId,Posted,ItemName,PaymentName,PaymentTypeId,Amount,SubjectNameId,UserId")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentTypeId"] = new SelectList(_context.Set<PaymentType>(), "PaymentTypeId", "TypeName", payment.PaymentTypeId);
            ViewData["SubjectNameId"] = new SelectList(_context.Set<SubjectName>(), "SubjectNameId", "CourseName", payment.SubjectNameId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", payment.UserId);
            return View(payment);
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payment.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            ViewData["PaymentTypeId"] = new SelectList(_context.Set<PaymentType>(), "PaymentTypeId", "TypeName", payment.PaymentTypeId);
            ViewData["SubjectNameId"] = new SelectList(_context.Set<SubjectName>(), "SubjectNameId", "CourseName", payment.SubjectNameId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", payment.UserId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaymentId,Posted,ItemName,PaymentName,PaymentTypeId,Amount,SubjectNameId,UserId")] Payment payment)
        {
            if (id != payment.PaymentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.PaymentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentTypeId"] = new SelectList(_context.Set<PaymentType>(), "PaymentTypeId", "TypeName", payment.PaymentTypeId);
            ViewData["SubjectNameId"] = new SelectList(_context.Set<SubjectName>(), "SubjectNameId", "CourseName", payment.SubjectNameId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", payment.UserId);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payment
                .Include(p => p.PaymentTypeNavigation)
                .Include(p => p.SubjectNameNavigation)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PaymentId == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payment.FindAsync(id);
            if (payment != null)
            {
                _context.Payment.Remove(payment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payment.Any(e => e.PaymentId == id);
        }
    }
}
