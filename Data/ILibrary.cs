using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOPDS.Data
{
    public interface ILibrary
    {
        event EventHandler LibraryLoaded;
        void LoadAsync();
        string LibraryPath { get; set; }
        bool IsChanged { get; set; }
        bool Contains(string bookPath);
        Book GetBook(string id);
        bool Add(Book book);
        bool Delete(string fileName);
        int Count { get; }
        int FB2Count { get; }
        int EPUBCount { get; }
        List<string> Titles { get; }
        List<string> Authors { get; }
        List<string> Sequences { get; }
        List<Genre> FB2Genres { get; }
        Dictionary<string, string> SoundexedGenres { get; }
        List<Genre> Genres { get; }
        List<string> GetAuthorsByName(string name, bool isOpenSearch);
        List<Book> GetBooksByTitle(string title);
        int GetBooksByAuthorCount(string author);
        List<Book> GetBooksByAuthor(string author);
        List<Book> GetBooksBySequence(string sequence);
        List<Book> GetBooksByGenre(string genre);
        void Load();
        void Save();
        void Append(Book book);
    }
}
