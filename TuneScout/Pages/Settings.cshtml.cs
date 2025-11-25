using DataAccess.Contexts;
using Logic.Models;
using Logic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace TuneScout.Pages
{
    public class SettingsModel : PageModel
    {
        private readonly UserService _userService;
        private readonly TuneScoutContext _context;

        public SettingsModel(UserService userService, TuneScoutContext context)
        {
            _userService = userService;
            _context = context;
        }

        [BindProperty]
        public bool NoExplicit { get; set; }
        [BindProperty]
        public List<int> SelectedGenreIds { get; set; } = new();

        [BindProperty]
        public List<int> SelectedMoodIds { get; set; } = new();

        [BindProperty]
        public int? SelectedLanguageId { get; set; }

        public List<Genre> Genres { get; set; } = new();
        public List<Mood> Moods { get; set; } = new();
        public List<(int Id, string Name)> Languages { get; set; } = new();

        public string? Message { get; set; }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            Genres = _context.Genres.AsEnumerable().OrderBy(g => g.Name).ToList();
            Moods = _context.Moods.AsEnumerable().OrderBy(m => m.Name).ToList();

            LoadLanguage();

            var user = _userService.GetUserById(userId.Value);
            if (user != null)
            {
                NoExplicit = user.NoExplicit ?? false;
            }

            var pref = _context.Preferences.FirstOrDefault(p => p.UserId == userId.Value);
            if (pref != null)
            {
                SelectedGenreIds = pref.GenreId.HasValue ? new List<int> { pref.GenreId.Value } : new List<int>();
                SelectedMoodIds = pref.MoodId.HasValue ? new List<int> { pref.MoodId.Value } : new List<int>();
                SelectedLanguageId = pref.LanguageId;
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            Genres = _context.Genres.AsEnumerable().OrderBy(g => g.Name).ToList();
            Moods = _context.Moods.AsEnumerable().OrderBy(m => m.Name).ToList();

            LoadLanguage();

            var user = _userService.GetUserById(userId.Value);
            if (user == null)
            {
                return RedirectToPage("/Login");
            }

            SelectedGenreIds = SelectedGenreIds?.Where(i => i > 0).ToList() ?? new List<int>();
            SelectedMoodIds = SelectedMoodIds?.Where(i => i > 0).ToList() ?? new List<int>();

            user.NoExplicit = NoExplicit;
            _userService.UpdateUser(user);

            int? genreToSave = SelectedGenreIds.Any() ? SelectedGenreIds.First() : null;
            int? moodToSave = SelectedMoodIds.Any() ? SelectedMoodIds.First() : null;
            int? languageToSave = SelectedLanguageId;

            if (genreToSave.HasValue && !RecordExists("genre", genreToSave.Value))
                genreToSave = null;
            if (moodToSave.HasValue && !RecordExists("mood", moodToSave.Value))
                moodToSave = null;
            if (languageToSave.HasValue && !RecordExists("language", languageToSave.Value))
                languageToSave = null;

            var pref = _context.Preferences.FirstOrDefault(p => p.UserId == userId.Value);
            if (pref == null)
            {
                pref = new Preference
                {
                    UserId = userId.Value,
                    GenreId = genreToSave,
                    MoodId = moodToSave,
                    LanguageId = languageToSave
                };
                _context.Preferences.Add(pref);
            }
            else
            {
                pref.GenreId = genreToSave;
                pref.MoodId = moodToSave;
                pref.LanguageId = languageToSave;
                _context.Preferences.Update(pref);
            }

            try
            {
                _context.SaveChanges();
                Message = "Voorkeuren opgeslagen.";
            }
            catch (Exception ex)
            {
                Message = "Er is een fout opgetreden bij het opslaan van voorkeuren.";
            }

            return Page();
        }

        private bool RecordExists(string tableName, int id)
        {
            try
            {
                var conn = _context.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT 1 FROM [{tableName}] WHERE id = @id";
                var param = cmd.CreateParameter();
                param.ParameterName = "@id";
                param.Value = id;
                cmd.Parameters.Add(param);

                var result = cmd.ExecuteScalar();
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        private void LoadLanguage()
        {
            Languages.Clear();
            try
            {
                var conn = _context.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT id, name FROM [language] ORDER BY name";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    var name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    if (id > 0)
                        Languages.Add((id, name));
                }

                if (Languages.Count > 0)
                    return;
            }
            catch
            {
                
            }

            Languages = new List<(int, string)>
            {
                (1, "English"),
                (2, "Nederlands"),
                (3, "Español"),
                (4, "Français"),
                (5, "Deutsch")
            };
        }
    }
}