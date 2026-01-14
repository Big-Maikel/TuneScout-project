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
using TuneScout.Models;

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
        public SettingsViewModel ViewModel { get; set; } = new();

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            ViewModel.Genres = _context.Genres.AsEnumerable().OrderBy(g => g.Name).ToList();
            ViewModel.Moods = _context.Moods.AsEnumerable().OrderBy(m => m.Name).ToList();

            LoadLanguages();

            var user = _userService.GetUserById(userId.Value);
            if (user != null)
            {
                ViewModel.NoExplicit = user.NoExplicit ?? false;
            }

            var prefs = _context.Preferences.Where(p => p.UserId == userId.Value).ToList();
            
            ViewModel.SelectedGenreIds = prefs.Where(p => p.GenreId.HasValue)
                                             .Select(p => p.GenreId!.Value)
                                             .Distinct()
                                             .ToList();
            
            ViewModel.SelectedMoodIds = prefs.Where(p => p.MoodId.HasValue)
                                            .Select(p => p.MoodId!.Value)
                                            .Distinct()
                                            .ToList();
            
            ViewModel.SelectedLanguageId = prefs.FirstOrDefault()?.LanguageId;

            return Page();
        }

        public IActionResult OnPost()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            ViewModel.Genres = _context.Genres.AsEnumerable().OrderBy(g => g.Name).ToList();
            ViewModel.Moods = _context.Moods.AsEnumerable().OrderBy(m => m.Name).ToList();

            LoadLanguages();

            var user = _userService.GetUserById(userId.Value);
            if (user == null)
            {
                return RedirectToPage("/Login");
            }

            ViewModel.SelectedGenreIds = ViewModel.SelectedGenreIds?.Where(i => i > 0).ToList() ?? new List<int>();
            ViewModel.SelectedMoodIds = ViewModel.SelectedMoodIds?.Where(i => i > 0).ToList() ?? new List<int>();

            user.NoExplicit = ViewModel.NoExplicit;
            _userService.UpdateUser(user);

            var validGenreIds = ViewModel.SelectedGenreIds.Where(id => RecordExists("genre", id)).ToList();
            var validMoodIds = ViewModel.SelectedMoodIds.Where(id => RecordExists("mood", id)).ToList();
            
            int? languageToSave = ViewModel.SelectedLanguageId;
            if (languageToSave.HasValue && !RecordExists("language", languageToSave.Value))
                languageToSave = null;

            var existingPrefs = _context.Preferences.Where(p => p.UserId == userId.Value).ToList();
            _context.Preferences.RemoveRange(existingPrefs);

            if (validGenreIds.Any() && validMoodIds.Any())
            {
                foreach (var genreId in validGenreIds)
                {
                    foreach (var moodId in validMoodIds)
                    {
                        _context.Preferences.Add(new Preference
                        {
                            UserId = userId.Value,
                            GenreId = genreId,
                            MoodId = moodId,
                            LanguageId = languageToSave
                        });
                    }
                }
            }
            else if (validGenreIds.Any())
            {
                foreach (var genreId in validGenreIds)
                {
                    _context.Preferences.Add(new Preference
                    {
                        UserId = userId.Value,
                        GenreId = genreId,
                        MoodId = null,
                        LanguageId = languageToSave
                    });
                }
            }
            else if (validMoodIds.Any())
            {
                foreach (var moodId in validMoodIds)
                {
                    _context.Preferences.Add(new Preference
                    {
                        UserId = userId.Value,
                        GenreId = null,
                        MoodId = moodId,
                        LanguageId = languageToSave
                    });
                }
            }
            else
            {
                _context.Preferences.Add(new Preference
                {
                    UserId = userId.Value,
                    GenreId = null,
                    MoodId = null,
                    LanguageId = languageToSave
                });
            }

            try
            {
                _context.SaveChanges();
                ViewModel.Message = "Voorkeuren opgeslagen.";
            }
            catch (Exception ex)
            {
                ViewModel.Message = "Er is een fout opgetreden bij het opslaan van voorkeuren.";
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

        private void LoadLanguages()
        {
            ViewModel.Languages.Clear();
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
                        ViewModel.Languages.Add((id, name));
                }

                if (ViewModel.Languages.Count > 0)
                    return;
            }
            catch
            {
                
            }

            ViewModel.Languages = new List<(int, string)>
            {
                (1, "Nederlands"),
                (2, "Engels"),
                (3, "Duits"),
                (4, "Frans"),
                (5, "Spaans")
            };
        }
    }
}