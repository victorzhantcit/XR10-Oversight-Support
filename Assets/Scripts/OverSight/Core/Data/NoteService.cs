using Oversight.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Extensions;

namespace Oversight.Core
{
    public class NoteService
    {
        // Local storage
        private readonly string _noteFile = "Notes.json";
            
        public NoteService()
        {
            LocalFileSystem.CheckOrCreateFile(_noteFile, new List<NoteDto>());
        }

        public async Task<List<NoteDto>> GetLocalNotes()
            => await LocalFileSystem.GetLocalDataAsync<List<NoteDto>>(_noteFile);

        public void SaveNotesData(List<NoteDto> notes)
            => LocalFileSystem.SaveData(notes, _noteFile);
    }
}
