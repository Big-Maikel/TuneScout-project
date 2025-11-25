using System.Collections.Generic;
using Logic.Models;

namespace Logic.Interfaces
{
    public interface ISongRepository
    {
        IReadOnlyList<Track> GetAll();
    }
}