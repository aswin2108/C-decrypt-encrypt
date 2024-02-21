using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp1.Data;
using WebApp1.Models;

namespace WebApp1.Controllers
{
    public class StudentsController : Controller
    {
        byte[] key = Encoding.UTF8.GetBytes("12345678912345671234567891234567");
        byte[] iv = Encoding.UTF8.GetBytes("1234567890123456");
        public static string encrypt(string txt, byte[] key, byte[] iv)
        {

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform ict = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {

                    using (CryptoStream cs = new CryptoStream(ms, ict, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(txt);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }

            }

        }

        public static string decrypt(string txt, byte[] key, byte[] iv)
        {
            byte[] arr = Convert.FromBase64String(txt);
            using (Aes aes = Aes.Create())
            {

                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform itct = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(arr))
                {
                    using (CryptoStream cs = new CryptoStream(ms, itct, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
        private readonly WebApp1Context _context;

        public StudentsController(WebApp1Context context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            return View(await _context.Students.ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var students = await _context.Students
                .FirstOrDefaultAsync(m => m.ID == id);
            if (students == null)
            {
                return NotFound();
            }

            return View(students);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Department,Age,Gender,Password")] Students students)
        {
            students.Password = encrypt(students.Password, key, iv);
            if (ModelState.IsValid)
            {
                _context.Add(students);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(students);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var students = await _context.Students.FindAsync(id);
            if (students == null)
            {
                return NotFound();
            }
            students.Password=decrypt(students.Password, key, iv);
            return View(students);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Department,Age,Gender,Password")] Students students)
        {
            if (id != students.ID)
            {
                return NotFound();
            }
            students.Password = encrypt(students.Password, key, iv);
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(students);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentsExists(students.ID))
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
            return View(students);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var students = await _context.Students
                .FirstOrDefaultAsync(m => m.ID == id);
            if (students == null)
            {
                return NotFound();
            }

            return View(students);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var students = await _context.Students.FindAsync(id);
            if (students != null)
            {
                _context.Students.Remove(students);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentsExists(int id)
        {
            return _context.Students.Any(e => e.ID == id);
        }
    }
}
