using System.Collections.Generic;
using System.Linq;
using Logic.Interfaces;
using Logic.Models;
using DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class SongRepository : ISongRepository
    {
        private readonly TuneScoutContext _context;

        public SongRepository(TuneScoutContext context)
        {
            _context = context;
        }

        public IReadOnlyList<Track> GetAll()
        {
            return _context.Tracks
                .AsNoTracking()
                .OrderBy(t => t.Id)
                .ToList();
        }
    }
}