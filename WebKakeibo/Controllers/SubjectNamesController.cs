using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebKakeibo.Data;
using WebKakeibo.Models;

namespace WebKakeibo.Controllers
{
    public class SubjectNamesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectNamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SubjectNames
        public async Task<IActionResult> Index()
        {
            return View(await _context.SubjectName.ToListAsync());
        }

        // GET: SubjectNames/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectName = await _context.SubjectName
                .FirstOrDefaultAsync(m => m.SubjectNameId == id);
            if (subjectName == null)
            {
                return NotFound();
            }

            return View(subjectName);
        }

        // GET: SubjectNames/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SubjectNames/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubjectNameId,CourseName,ImageUrl")] SubjectName subjectName)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subjectName);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subjectName);
        }

        // GET: SubjectNames/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectName = await _context.SubjectName.FindAsync(id);
            if (subjectName == null)
            {
                return NotFound();
            }
            return View(subjectName);
        }

        // POST: SubjectNames/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SubjectNameId,CourseName,ImageUrl")] SubjectName subjectName, string ImageUrlOld, IFormFile ImageUrlNew)
        {
            if (id != subjectName.SubjectNameId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (ImageUrlNew != null && ImageUrlNew.Length > 0)  //新しい画像がアップロードされている場合は、wwwroot/imagesフォルダに保存し、ImageUrlプロパティに保存先のパスを設定する
                {
                    var fileName = Path.GetFileName(ImageUrlNew.FileName);  //ファイル名を取得する
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName); //保存先のパスを作成する
                    using (var stream = new FileStream(filePath, FileMode.Create))  //ファイルを保存するためのストリームを作成する
                    {
                        await ImageUrlNew.CopyToAsync(stream);  //新しい画像を保存する
                    }
                    subjectName.ImageUrl = "images/" + fileName;  //ImageUrlプロパティに保存先のパスを設定する
                }

                try
                {
                    _context.Update(subjectName);
                    //await _context.SaveChangesAsync();
                    int affectedRows = await _context.SaveChangesAsync();  //変更を保存する

                    Console.WriteLine($"{affectedRows} 件の変更を保存しました。");
                    if (affectedRows > 0)
                    {

                        TempData["Message"] = "画像ファイルのアップロードに成功しました。";  //成功メッセージをTempDataに保存する
                        return RedirectToAction(nameof(Details), new { id = subjectName.SubjectNameId });
                        //これは、SubjectNameの詳細ページにリダイレクトする処理、変更後の画像を確認するため。
                    }

                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectNameExists(subjectName.SubjectNameId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //return RedirectToAction(nameof(Index));
            }
            TempData["Message"] = "変更は行われませんでした";  //変更なしをTempDataに保存する
            return View(subjectName);
        }

        // GET: SubjectNames/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectName = await _context.SubjectName
                .FirstOrDefaultAsync(m => m.SubjectNameId == id);
            if (subjectName == null)
            {
                return NotFound();
            }

            return View(subjectName);
        }

        // POST: SubjectNames/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subjectName = await _context.SubjectName.FindAsync(id);
            if (subjectName != null)
            {
                _context.SubjectName.Remove(subjectName);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectNameExists(int id)
        {
            return _context.SubjectName.Any(e => e.SubjectNameId == id);
        }
    }
}
